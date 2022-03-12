# Using PostgreSQL for Serilog

I searched and search. Bing, DuckDuckGo, Google, You, etc. Nope. No one gave me a link where I could find a 
working example of Serilog that uses *Serilog.Sinks.Postgresql.Alternative* as the sink - no - no one likes this Sink. Did find a handful that uses *Serilog.Sinks.Postgresql*  but its not the one I wanted. 

This is a working example but below is in [this](https://github.com/ssathya/PsqlLogging) repository. Below is the journey I took and during its course realized what I was missing.

## Attempt 1:
```cs
Logger? psLogger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.PostgreSQL(connectionString, tableName, columnWriters
    , batchSizeLimit: 10
    //, period: TimeSpan.FromSeconds(1)
    , needAutoCreateTable: true)
    .CreateLogger();
logMgr.LogCritical("Critical");
```

**No joy. Table was created but nothing was written to database.**

## Attempt 2
```cs
using Logger? psLogger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.PostgreSQL(connectionString, tableName, columnWriters
    , batchSizeLimit: 10
    //, period: TimeSpan.FromSeconds(1)
    , needAutoCreateTable: true)
    .CreateLogger();
logMgr.LogCritical("Critical");
```
**Yes, it worked.**  Realized the logger was a disposable object and Dispose method needed to be called.

## Attempt 3
```cs
Logger? psLogger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.PostgreSQL(connectionString, tableName, columnWriters
    , batchSizeLimit: 10
    , period: TimeSpan.FromSeconds(1)
    , needAutoCreateTable: true)
    .CreateLogger();


IServiceCollection serviceCollection = new ServiceCollection();
serviceCollection.AddLogging(c =>
{
    c.SetMinimumLevel(LogLevel.Information);
    c.AddSerilog(psLogger, true);
});
var collection = serviceCollection.BuildServiceProvider();
ILogger<Program>? logMgr = collection.GetRequiredService<ILogger<Program>>();
logMgr.LogDebug("Debug");
logMgr.LogInformation("Information");
logMgr.LogCritical("Critical");
logMgr.LogTrace("Trace");
logMgr.LogError("Error");

psLogger.Warning("After messing");

logMgr.LogInformation("Starting to sleep");
Thread.Sleep(5000);
//psLogger.Dispose();
logMgr.LogInformation("Sleep ended");
```
### Actually 3 & 4
Eventually this is how I'd use the sink in my application. The logger object will be created 
when the application is launched and injected to other classes via DI. For *attempt 3* I did not call 
Dispose and nothing was written to the database.  *Attempt 4* uncommented Dispose and as expected everything until "Starting to sleep" was in my log table but "Sleep ended" was sent to ether not to my table!

To summarize, **Logger object needs to be disposed at the end of the application** to flush your logs into database.
