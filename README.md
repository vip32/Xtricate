# Xtricate [![Build status](https://ci.appveyor.com/api/projects/status/8dnddawd6bb3dxn9?svg=true&retina=true)](https://ci.appveyor.com/project/vip32/xtricate-docset) [![CodeFactor](https://www.codefactor.io/repository/github/vip32/xtricate/badge/master)](https://www.codefactor.io/repository/github/vip32/xtricate/overview/master)

| | | |
|-------|------|------|
|Package|net4|netstandard1.3|
|Xtricate.Common|[![NuGet downloads](https://img.shields.io/nuget/v/Xtricate.Common.svg)](http://www.nuget.org/packages/Xtricate.Common)| [![NuGet downloads](https://img.shields.io/nuget/vpre/Xtricate.Core.Common.svg)](http://www.nuget.org/packages/Xtricate.Core.Common)|
|Xtricate.Configuration|[![NuGet downloads](https://img.shields.io/nuget/v/Xtricate.Configuration.svg)](http://www.nuget.org/packages/Xtricate.Configuration)||
|Xtricate.DocSet|[![NuGet downloads](https://img.shields.io/nuget/v/Xtricate.DocSet.svg)](http://www.nuget.org/packages/Xtricate.DocSet)||
|Xtricate.DocSet.Sqlite|[![NuGet downloads](https://img.shields.io/nuget/v/Xtricate.DocSet.Sqlite.svg)](http://www.nuget.org/packages/Xtricate.DocSet.Sqlite)||
|Xtricate.DocSet.Serilog|[![NuGet downloads](https://img.shields.io/nuget/v/Xtricate.DocSet.Serilog.svg)](http://www.nuget.org/packages/Xtricate.Serilog.Sqlite)||
|Xtricate.Dynamic|[![NuGet downloads](https://img.shields.io/nuget/v/Xtricate.Dynamic.svg)](http://www.nuget.org/packages/Xtricate.Dynamic)||
|Xtricate.Tmpl|[![NuGet downloads](https://img.shields.io/nuget/v/Xtricate.Templ.svg)](http://www.nuget.org/packages/Xtricate.Dynamic)||
|Xtricate.Web.Dashboard|[![NuGet downloads](https://img.shields.io/nuget/v/Xtricate.Dynamic.svg)](http://www.nuget.org/packages/Xtricate.Dynamic)||

## Configuration (Xtricate.Configuration)
TODO

## DocSet (Xtricate.DocSet)
#### introduction
DocSet provides document like storage with basic querying on Microsoft SQLServer. 
Documents are stored as JSON with keys and tags. The keys and tags are used to retrieve documents.
By using an optional indexmap basic querying becomes possible. Indexed fields are stored in seperate table columns for performance reasons. 
All database assets are created automaticly when needed (database/tables/columns/indexes).  
#### examples
* setup the storage
```c
    var options = new StorageOptions(new ConnectionStrings().Get(connectionName), databaseName: databaseName, schemaName: schemaName);
    var connectionFactory = new SqlConnectionFactory();
    var indexMap = TestDocumentIndexMap;
    var storage = new DocStorage<TestDocument>(
        connectionFactory, options, new SqlBuilder(), 
        new JsonNetSerializer(), new Md5Hasher(), indexMap);
```
* instantiate a test document
```c
    var document = new Customer
    {
        FirstName = "John",
        LastName = "Doe"
    }
```

* insert or update document value with key and tags
```c
    storage.Upsert("mykey", document);
    storage.Upsert("mykey", document, new[] {"en-US"});
```

* insert or update document data with key and tags
```c
    var stream = File.OpenRead(@".\cat.jpg");
    storage.Upsert("mykey", stream);
    storage.Upsert("mykey", document, stream);
    storage.Upsert("mykey", stream, new[] {"en-US", "cat"});
```

* query document values
```c
    var document = storage.LoadValues("mykey");
    var document = storage.LoadValues(new[] {"en-US"});
    var document = storage.LoadValues("mykey", new[] {"en-US"});
````

* query document data
```c
    var stream = storage.LoadData("mykey");
    var stream = storage.LoadData(new[] {"en-US"});
    var stream = storage.LoadData("mykey", new[] {"en-US"});
````

* remove documents
```c
    storage.Delete("mykey");
    storage.Delete( new[] {"en-US"});
    storage.Delete("mykey", new[] {"en-US"});
```

#### table structure
* uid (uniqueidentifier)
* id (int)
* key (nvarchar:512)
* tags (nvarchar:1024)
* hash (nvarchar:128)
* timestamp (datetime)
* value (ntext) contains the JSON document
* data (varbinary:max) contains the binary data
* ???_idx (nvarchar:2048)

#### Sqlserver
TODO

#### Sqlite
TODO

#### Serilog
TODO

## Expando (Xtricate.Dynamic)
TODO

## Templ (Xtricate.Templ)
TODO

## Web.Dashboard (Xtricate.Web.Dashboard)
TODO

## Seq (Xtricate.Common/ Xtricate.Core.Common)
#### introduction
Create nice sequence diagrams with [https://www.websequencediagrams.com/](https://www.websequencediagrams.com/)
#### examples
* with using blocks
```C
    using (new Seq("CLIENT", enabled: true, loggingEnabled: true))
    {
        using (new Seq("SERVICE", "handle get request", "return json"))
        {
            using (new Seq("REPO", "get entities", "return entities"))
            {
                // do nothing
            }
        }
    }
    var name = Seq.RenderDiagram();
    // name equals the local filename containing the sequence diagram as a image (png)
```

* integrated with actual code blocks
```C
    using (new Seq("CLIENT", "make a service request", enabled: true, loggingEnabled: true))
    {
        Seq.Self("filter grid");
        var service = new Service(new Repo());
        service.Get();
    }
    var name = Seq.RenderDiagram();
    // name equals the local filename containing the sequence diagram as a image (png)

    public class Service
    {
        private readonly Repo _repo;

        public Service(Repo repo)
        {
            _repo = repo;
        }

        public void Get()
        {
            using (Seq.Call("SERVICE", "handle GET request", "return json"))
            {
                Validate();
                MapQuery();
                _repo.GetData();
            }
        }

        private void MapQuery()
        {
            Seq.Self("parse querystring");
        }

        private void Validate()
        {
            Seq.Note("fluentvalidation");
            using (Seq.Call("SERVICE", "validate"))
            {
            }
        }
    }

    public class Repo
    {
        public void GetData()
        {
            using (Seq.Call("REPO", "get entities", "return entities"))
            {
                Seq.Self("get connection string");
                Seq.Note("make connection");
                Seq.Note("execute sql");
                Seq.Note("map to entity");
            }
        }
    }
```

## Bugs and feature requests
Do you have a bug or a feature request? Please use the [issue tracker](https://github.com/vip32/xtricate.docset/issues) and search for existing and closed issues. If your problem or request isn't addressed yet, go ahead and [open a new issue](https://github.com/vip32/xtricate.docset/issues/new). 

## Contributing
You can also get involved and [fork the repository](https://github.com/vip32/xtricate.docset/fork) to submit your own pull requests. 

## Versioning
For transparency and to maintain backward compatibility (as much as possible), this project uses the [Semantic Versioning guidelines](http://semver.org/).

## Creators
* [vip32](https://github.com/vip32) - for the initial conception and core contributor.
