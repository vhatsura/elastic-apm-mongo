# MongoDB auto instrumentation for Elastic Apm

[![Build Status](https://dev.azure.com/vadimhatsura/elastic-apm-mongo/_apis/build/status/vhatsura.elastic-apm-mongo?branchName=master)](https://dev.azure.com/vadimhatsura/elastic-apm-mongo/_build/latest?definitionId=4&branchName=master)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=elastic-apm-mongo&metric=alert_status)](https://sonarcloud.io/dashboard?id=elastic-apm-mongo)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=elastic-apm-mongo&metric=coverage)](https://sonarcloud.io/dashboard?id=elastic-apm-mongo)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=elastic-apm-mongo&metric=sqale_rating)](https://sonarcloud.io/dashboard?id=elastic-apm-mongo)
[![NuGet Badge](https://buildstats.info/nuget/ElasticApm.MongoDB)](https://www.nuget.org/packages/ElasticApm.MongoDB/)

An auto instrumentation of [MongoDB](https://github.com/mongodb/mongo-csharp-driver) events for [Elastic Apm solution](https://github.com/elastic/apm-agent-dotnet).

## Installation

```powershell
Install-Package ElasticApm.MongoDB
```

## Usage

MongoDB.Driver uses own event notification approach instead of commonly used [System.Diagnostics.DiagnosticSource](https://github.com/dotnet/corefx/blob/master/src/System.Diagnostics.DiagnosticSource/src/DiagnosticSourceUsersGuide.md). By this reason, before `Elastic.Apm` configuration we need to configure `MongoClient`. It can be done with `ClusterConfigurator` action in `MongoClientSettings`:

```csharp
var settings = MongoClientSettings.FromConnectionString(mongoConnectionString);
settings.ClusterConfigurator = builder => builder.Subscribe(new MongoEventSubscriber());

var mongoClient = new MongoClient(settings);
```

`MongoEventSubscriber` allows to spread command events from MongoDB.Driver in application via `DiagnosticSource`.
Now we can configure `Elastic.Apm` agent:

```csharp
Agent.Subscribe(new MongoDiagnosticsSubscriber());
```

or in case of ASP.NET Core application:

```csharp
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        app.UseElasticApm(Configuration, new MongoDiagnosticsSubscriber());

        //Rest of the Configure() method...
    }
}
```

NOTE: don't forget to include other subscribers.

## Roadmap

## Contributing
