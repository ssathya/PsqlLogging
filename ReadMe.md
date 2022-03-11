# Using PostgreSQL for Serilog

I struggled for a few hours and couldn't get PostgreSQL sync to work for my application. I also Googled/Binged for samples but there was not full example that I could copy and paste and make it work. 

This is a working example but below is  the journey I went through until I realized what was happening.

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
## Attempt 4 & 5
Eventually this is how I'll have to use the application. The logger object will be created 
when the application is created and injected to other classes via dependency injection. 
For *attempt 4* I did not call the Dispose and nothing was written to the database.  *Attempt 5* 
uncommented Dispose and as expected everything until "Starting to sleep" was inserted but "Sleep ended" 
did not. 

To summarize, **Logger object needs to be disposed at the end of the application** to have logs saved in your database.