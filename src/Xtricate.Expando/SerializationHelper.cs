﻿using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Xtricate.Dynamic
{
    public static class SerializationHelper
    {
        /// <summary>
        ///     Serializes an object instance to a file.
        /// </summary>
        /// <param name="instance">the object instance to serialize</param>
        /// <param name="fileName"></param>
        /// <param name="binarySerialization">determines whether XML serialization or binary serialization is used</param>
        /// <returns></returns>
        public static bool SerializeObject(object instance, string fileName, bool binarySerialization)
        {
            var retVal = true;

            if (!binarySerialization)
            {
                XmlTextWriter writer = null;
                try
                {
                    var serializer = new XmlSerializer(instance.GetType());

                    // Create an XmlTextWriter using a FileStream.
                    Stream fs = new FileStream(fileName, FileMode.Create);
                    writer = new XmlTextWriter(fs, new UTF8Encoding())
                    {
                        Formatting = Formatting.Indented,
                        IndentChar = ' ',
                        Indentation = 3
                    };

                    // Serialize using the XmlTextWriter.
                    serializer.Serialize(writer, instance);
                }
                catch (Exception ex)
                {
                    Debug.Write("SerializeObject failed with : " + ex.Message, "");
                    retVal = false;
                }
                finally
                {
                    if (writer != null)
                        writer.Close();
                }
            }
            else
            {
                Stream fs = null;
                try
                {
                    var serializer = new BinaryFormatter();
                    fs = new FileStream(fileName, FileMode.Create);
                    serializer.Serialize(fs, instance);
                }
                catch
                {
                    retVal = false;
                }
                finally
                {
                    if (fs != null)
                        fs.Close();
                }
            }

            return retVal;
        }

        /// <summary>
        ///     Overload that supports passing in an XML TextWriter.
        /// </summary>
        /// <remarks>
        ///     Note the Writer is not closed when serialization is complete
        ///     so the caller needs to handle closing.
        /// </remarks>
        /// <param name="instance">object to serialize</param>
        /// <param name="writer">XmlTextWriter instance to write output to</param>
        /// <param name="throwExceptions">Determines whether false is returned on failure or an exception is thrown</param>
        /// <returns></returns>
        public static bool SerializeObject(object instance, XmlTextWriter writer, bool throwExceptions)
        {
            var retVal = true;

            try
            {
                var serializer = new XmlSerializer(instance.GetType());

                // Create an XmlTextWriter using a FileStream.
                writer.Formatting = Formatting.Indented;
                writer.IndentChar = ' ';
                writer.Indentation = 3;

                // Serialize using the XmlTextWriter.
                serializer.Serialize(writer, instance);
            }
            catch (Exception ex)
            {
                Debug.Write(
                    "SerializeObject failed with : " + ex.GetBaseException().Message + "\r\n" +
                    (ex.InnerException != null ? ex.InnerException.Message : ""), "");

                if (throwExceptions)
                    throw;

                retVal = false;
            }

            return retVal;
        }

        /// <summary>
        ///     Serializes an object into an XML string variable for easy 'manual' serialization
        /// </summary>
        /// <param name="instance">object to serialize</param>
        /// <param name="xmlResultString">resulting XML string passed as an out parameter</param>
        /// <returns>true or false</returns>
        public static bool SerializeObject(object instance, out string xmlResultString)
        {
            return SerializeObject(instance, out xmlResultString, false);
        }

        /// <summary>
        ///     Serializes an object into a string variable for easy 'manual' serialization
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="xmlResultString">Out parm that holds resulting XML string</param>
        /// <param name="throwExceptions">If true causes exceptions rather than returning false</param>
        /// <returns></returns>
        public static bool SerializeObject(object instance, out string xmlResultString, bool throwExceptions)
        {
            xmlResultString = string.Empty;
            var ms = new MemoryStream();

            var writer = new XmlTextWriter(ms, new UTF8Encoding());

            if (!SerializeObject(instance, writer, throwExceptions))
            {
                ms.Close();
                return false;
            }

            xmlResultString = Encoding.UTF8.GetString(ms.ToArray(), 0, (int) ms.Length);

            ms.Close();
            writer.Close();

            return true;
        }

        /// <summary>
        ///     Serializes an object instance to a file.
        /// </summary>
        /// <param name="instance">the object instance to serialize</param>
        /// <param name="resultBuffer">The result buffer.</param>
        /// <param name="throwExceptions">if set to <c>true</c> [throw exceptions].</param>
        /// <returns></returns>
        /// <summary>
        ///     Serializes an object to an XML string. Unlike the other SerializeObject overloads
        ///     this methods *returns a string* rather than a bool result!
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="throwExceptions">Determines if a failure throws or returns null</param>
        /// <returns>
        ///     null on error otherwise the Xml String.
        /// </returns>
        /// <remarks>
        ///     If null is passed in null is also returned so you might want
        ///     to check for null before calling this method.
        /// </remarks>
        /// <summary>
        ///     Deserializes an object from file and returns a reference.
        /// </summary>
        /// <param name="fileName">name of the file to serialize to</param>
        /// <param name="objectType">The Type of the object. Use typeof(yourobject class)</param>
        /// <param name="binarySerialization">determines whether we use Xml or Binary serialization</param>
        /// <returns>Instance of the deserialized object or null. Must be cast to your object type</returns>
        /// <summary>
        ///     Deserializes an object from file and returns a reference.
        /// </summary>
        /// <param name="fileName">name of the file to serialize to</param>
        /// <param name="objectType">The Type of the object. Use typeof(yourobject class)</param>
        /// <param name="binarySerialization">determines whether we use Xml or Binary serialization</param>
        /// <param name="throwExceptions">determines whether failure will throw rather than return null on failure</param>
        /// <returns>Instance of the deserialized object or null. Must be cast to your object type</returns>
        /// <summary>
        ///     Deserialize an object from an XmlReader object.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public static object DeSerializeObject(XmlReader reader, Type objectType)
        {
            var serializer = new XmlSerializer(objectType);
            var instance = serializer.Deserialize(reader);
            reader.Close();

            return instance;
        }

        public static object DeSerializeObject(string xml, Type objectType)
        {
            var reader = new XmlTextReader(xml, XmlNodeType.Document, null);
            return DeSerializeObject(reader, objectType);
        }

        /// <summary>
        ///     Deseializes a binary serialized object from  a byte array
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="objectType"></param>
        /// <param name="throwExceptions"></param>
        /// <returns></returns>
        public static object DeSerializeObject(byte[] buffer, Type objectType, bool throwExceptions = false)
        {
            MemoryStream ms = null;
            object instance;

            try
            {
                var serializer = new BinaryFormatter();
                ms = new MemoryStream(buffer);
                instance = serializer.Deserialize(ms);
            }
            catch
            {
                if (throwExceptions)
                    throw;

                return null;
            }
            finally
            {
                if (ms != null)
                    ms.Close();
            }

            return instance;
        }
    }
}