// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NpgsqlTypes;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.PostgreSQL;
using Serilog.Sinks.PostgreSQL.ColumnWriters;

Console.WriteLine("Hello, World!");
var connectionString = @"Host=dbserver;Port=5432;database=Logging;password=MySuperSecretPassword;username=God";

string tableName = "logs";

IDictionary<string, ColumnWriterBase> columnWriters = new Dictionary<string, ColumnWriterBase>
{
    { "raise_date", new TimestampColumnWriter(NpgsqlDbType.Timestamp) },
    { "level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
    { "message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
    { "message_template", new MessageTemplateColumnWriter(NpgsqlDbType.Text) },
    { "exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
    { "properties", new LogEventSerializedColumnWriter(NpgsqlDbType.Jsonb) },
    { "props_test", new PropertiesColumnWriter(NpgsqlDbType.Jsonb) },
    { "machine_name", new SinglePropertyColumnWriter("MachineName", PropertyWriteMethod.ToString, NpgsqlDbType.Text, "l") }
};



using Logger? psLogger = new LoggerConfiguration()
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
psLogger.Dispose();
logMgr.LogInformation("Sleep ended");


