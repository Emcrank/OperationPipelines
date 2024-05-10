using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Sinks.TestCorrelator;

namespace Pipeliner.Net.UnitTests;

public partial class OperationPipelineTests : IDisposable
{
    private readonly ITestCorrelatorContext testContext;
    private readonly SerilogLoggerFactory loggerFactory;

    public OperationPipelineTests()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.TestCorrelator()
            .Enrich.FromLogContext()
            .MinimumLevel.Debug()
            .CreateLogger();

        loggerFactory = new SerilogLoggerFactory(Log.Logger);
        testContext = TestCorrelator.CreateContext();
    }

    public void Dispose()
    {
        testContext.Dispose();
    }
}