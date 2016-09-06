using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Xtricate.Common
{
    public class Seq : IDisposable
    {
        private static Stack<string> _froms = new Stack<string>();
        private static string _title;
        private static bool _enabled;
        public static List<SeqStep> Steps = new List<SeqStep>();
        private readonly string _returnDescription;

        public Seq(string from, string description = null, string returnDescription = null, string title = null,
            bool? enabled = null)
        {
            if (enabled.HasValue) _enabled = enabled.Value;
            if (!_enabled) return;

            _returnDescription = returnDescription;
            if (!string.IsNullOrEmpty(title)) _title = title;
            if (_froms.Count >= 1)
                Steps.Add(new SeqStep
                {
                    Type = _froms.Peek() != from ? SeqStepType.Call : SeqStepType.CallSelf,
                    From = _froms.Peek(),
                    To = from,
                    Description = description
                });
            _froms.Push(from);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public static bool IsEnabled()
        {
            return _enabled;
        }

        public static Seq Call(string from, string description = null, string returnDescription = null,
            string title = null)
        {
            if (!_enabled) return null;
            return new Seq(from, description, returnDescription, title);
        }

        public static void Self(string description)
        {
            if (!_enabled) return;
            Steps.Add(new SeqStep
            {
                Type = SeqStepType.Self,
                From = _froms.Peek(),
                To = _froms.Peek(),
                Description = description
            });
        }

        public static void Note(string description)
        {
            if (!_enabled) return;
            Steps.Add(new SeqStep
            {
                Type = SeqStepType.Note,
                Description = description,
                From = _froms.Peek(),
                To = _froms.Peek()
            });
        }

        public static void Reset()
        {
            _froms = new Stack<string>();
            Steps = new List<SeqStep>();
            _title = null;
        }

        public static string Render()
        {
            var sb = new StringBuilder();
            sb.Append($"\ntitle {_title}\n");
            if (Steps != null)
                foreach (var s in Steps)
                {
                    sb.Append(s.Render());
                }
            return sb.ToString();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            if (!_enabled) return;

            if (_froms.Count >= 2)
            {
                var from = _froms.Pop();
                if (from != _froms.Peek())
                    Steps.Add(new SeqStep
                    {
                        Type = SeqStepType.Return,
                        From = from,
                        To = _froms.Peek(),
                        Description = _returnDescription
                    });
            }
        }

        /// <summary>
        ///     Given a WSD description, produces a sequence diagram PNG.
        /// </summary>
        /// <param name="name">Name of the file.</param>
        /// <param name="path">The path.</param>
        /// <param name="style">One of the valid styles for the diagram</param>
        /// <param name="format">The output format requested. Must be one of the valid format supported</param>
        /// <returns>
        ///     The full path of the downloaded image
        /// </returns>
        /// <exception cref="System.Exception">
        ///     Unexpected HTTP status from server:  + response.StatusCode + :  + response.StatusDescription
        ///     or
        ///     Error parsing response from server:  + jsonObject
        ///     or
        ///     Server reported HTTP error during image fetch:  + response.StatusCode + :  + response.StatusDescription
        ///     or
        ///     Exception while saving image to temp file:  + e.Message
        /// </exception>
        /// <exception cref="Exception">If an error occurred during the request</exception>
        /// This method uses the WebSequenceDiagrams.com public API to query an image and stored in a local
        /// temporary directory on the file system.
        /// You can easily change it to return the stream to the image requested instead of a file.
        /// To invoke it:
        /// ..
        /// using System.Web;
        /// ...
        /// string name = grabSequenceDiagram("a-&gt;b: Hello", "qsd", "png");
        /// ..
        /// You need to add the assembly "System.Web" to your reference list (that by default is not
        /// added to new projects)
        /// Questions / suggestions: fabriziobertocci@gmail.com
        public static string RenderDiagram(string name, string path = null, string style = "modern-blue" /*"qsd"*/,
            string format = "png")
        {
            // Websequence diagram API:
            // prepare a POST body containing the required properties
            if (string.IsNullOrEmpty(path)) path = AppDomain.CurrentDomain.BaseDirectory;
            var wsd = Render();
            var sb = new StringBuilder("style=");
            sb.Append(style).Append("&apiVersion=1&format=").Append(format).Append("&message=");
            sb.Append(System.Net.WebUtility.UrlEncode(System.Net.WebUtility.UrlDecode(wsd)));
            var postBytes = Encoding.ASCII.GetBytes(sb.ToString());

            // Typical Microsoft crap here: the HttpWebRequest by default always append the header
            //          "Expect: 100-Continue"
            // to every request. Some web servers (including www.websequencediagrams.com) chockes on that
            // and respond with a 417 error.
            // Disable it permanently:
            ServicePointManager.Expect100Continue = false;

            // set up request object
            // The following command might throw UriFormatException
            var request = WebRequest.Create("http://www.websequencediagrams.com/index.php") as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postBytes.Length;
            //request.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
            //request.Credentials = new NetworkCredential("", "", "de");

            // add post data to request
            var postStream = request.GetRequestStream();
            postStream.Write(postBytes, 0, postBytes.Length);
            postStream.Close();

            var response = request.GetResponse() as HttpWebResponse;

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Unexpected HTTP status from server: " + response.StatusCode + ": " +
                                    response.StatusDescription);
            }

            var stream = new StreamReader(response.GetResponseStream());
            var jsonObject = stream.ReadToEnd();
            stream.Close();

            // Expect response like this one: {"img": "?png=mscKTO107", "errors": []}
            // Instead of using a full JSON parser, do a simple parsing of the response
            var components = jsonObject.Split('"');
            // Ensure component #1 is 'img':
            if (components[1].Equals("img") == false)
            {
                throw new Exception("Error parsing response from server: " + jsonObject);
            }

            var uri = components[3];

            // Now download the image
            request = WebRequest.Create("http://www.websequencediagrams.com/" + uri) as HttpWebRequest;
            request.Method = "GET";

            response = request.GetResponse() as HttpWebResponse;

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Server reported HTTP error during image fetch: " + response.StatusCode + ": " +
                                    response.StatusDescription);
            }
            try
            {
                var srcStream = response.GetResponseStream();
                if (string.IsNullOrEmpty(name))
                {
                    name = Path.GetTempFileName();
                    name = name.Replace(".tmp", "." + format);
                }
                else
                {
                    name = Path.Combine(path, name) + "." + format;
                }
                var dstStream = new FileStream(name, FileMode.Create);

                // Copy streams
                var buffer = new byte[1024];
                int read;
                while ((read = srcStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    dstStream.Write(buffer, 0, read);
                }
                dstStream.Close();
                srcStream.Close();

                //Log.Debug(wsd);
                //Log.Debug("created: " + name);

                return name;
            }
            catch (Exception e)
            {
                throw new Exception("Exception while saving image to temp file: " + e.Message);
            }
        }
    }
}

//public class Start : IDisposable
//{
//    private readonly string _returnDescription;

//    public Start(string title, string name, string returnDescription)
//    {
//        Sequence.Start(title, name);
//        _returnDescription = returnDescription;
//    }
//    public void Dispose()
//    {
//        Dispose(true);
//        GC.SuppressFinalize(this);
//    }

//    /// <summary>
//    /// Called when the object is cleaned up, to close the scope
//    /// </summary>
//    protected virtual void Dispose(bool disposing)
//    {
//        if (!disposing) return;
//        //Sequence.Return(_returnDescription);
//    }
//}

//public static class Sequence
//{
//    private static string _title;
//    private static string _activeTo;
//    private static string _activeFrom;
//    private static string _activeDescription;
//    private static readonly Stack<string> Froms = new Stack<string>();
//    public static List<Step> Steps = new List<Step>();

//    public static void Start(string title, string name)
//    {
//        _title = title;
//        Steps = new List<Step>();
//        _activeFrom = name;
//        Froms.Push(name);
//        _activeDescription = null;
//        _activeTo = null;
//    }

//    public static void From(string name, string description = null)
//    {
//        _activeFrom = name;
//        Froms.Push(name);
//        if (!string.IsNullOrEmpty(description)) _activeDescription = description;
//    }

//    public static void To(string name, string description)
//    {
//        _activeTo = name;
//        if (string.IsNullOrEmpty(_activeFrom)) From(name, description);
//        if (!string.IsNullOrEmpty(description)) _activeDescription = description;
//        Steps.Add(new Step { Type = StepType.Call, From = _activeFrom, To = name, Description = _activeDescription });
//    }

//    public static void ToSelf(string name, string description)
//    {
//        //_activeTo = name;
//        //if (!string.IsNullOrEmpty(description)) _activeDescription = description;
//        Steps.Add(new Step { Type = StepType.Self, From = name, To = name, Description = description });
//    }

//    public static void ToSelf(string description)
//    {
//        //_activeTo = name;
//        //if (!string.IsNullOrEmpty(description)) _activeDescription = description;
//        Steps.Add(new Step { Type = StepType.Self, From = _activeTo, To = _activeTo, Description = description });
//    }

//    public static void AddNote(string description)
//    {
//        Steps.Add(new Step { Type = StepType.Note, Description = description, From = _activeTo, To = _activeTo });
//    }

//    public static void Return(string description)
//    {
//        _activeFrom = Froms.Pop();
//        Steps.Add(new Step { Type = StepType.Return, From = _activeTo, To = _activeFrom, Description = description });
//        _activeTo = _activeFrom;
//    }

//    public static string Render()
//    {
//        var sb = new StringBuilder();
//        sb.Append(string.Format("\ntitle {0}\n", _title));
//        if (Steps != null)
//            foreach (var s in Steps)
//            {
//                sb.Append(s.Render());
//            }
//        return sb.ToString();
//    }
//}