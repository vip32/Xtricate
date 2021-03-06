﻿using Serilog;
using Xunit;
using Xtricate.Core.Common;

namespace Xtricate.IntegrationTests
{
    public class SeqTests
    {
        public SeqTests()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Trace(outputTemplate: "{Timestamp:u} [{Level}] {SourceContext}:: {CorrelationId} {Message}{NewLine}{Exception}")
                .WriteTo.ColoredConsole(outputTemplate: "{Timestamp:u} [{Level}] {SourceContext}:: {CorrelationId} {Message}{NewLine}{Exception}")
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        [Fact]
        public void Seq1()
        {
            using (new Seq("CLIENT", "make a service request", enabled: true, loggingEnabled: true))
            {
                Seq.Self("filter grid");
                var sut = new Service(new Repo());
                sut.Get();
            }

            //Seq.Steps.ForEach(s => Log.Debug(s.Dump()));
            Log.Debug(Seq.Render());
            var name = Seq.RenderDiagram();
            Log.Debug(name);
        }

        [Fact]
        public void Seq2()
        {
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

            //Seq.Steps.ForEach(s => Log.Debug(s.Dump()));
            Log.Debug(Seq.Render());
            var name = Seq.RenderDiagram();
            Log.Debug(name);
        }
    }

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
}
