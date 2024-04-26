using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Sinks.TestCorrelator;

namespace Pipeliner.Net.UnitTests;

public class OperationPipelineTests : IDisposable
{
    public OperationPipelineTests()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.TestCorrelator()
            .Enrich.FromLogContext()
            .MinimumLevel.Debug()
            .CreateLogger();
        testContext = TestCorrelator.CreateContext();
    }

    public void Dispose()
    {
        testContext.Dispose();
    }

    private readonly ITestCorrelatorContext testContext;

    [Fact]
    public async Task RunAsync_ExceptionOccursAndHandlerHandlesIt_Success()
    {
        var logger = new SerilogLoggerFactory().CreateLogger(nameof(OperationPipelineTests));

        var pipeline = new OperationPipeline<int, int>(logger)
            // ReSharper disable once IntDivisionByZero - test
            .AddOperation<int, int>(x => x / 0, onExceptionHandler: exception => exception is DivideByZeroException);

        int result = await pipeline.RunAsync(10);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task RunAsync_ExceptionOccursNoHandler_Success()
    {
        var logger = new SerilogLoggerFactory().CreateLogger(nameof(OperationPipelineTests));

        var pipeline = new OperationPipeline<int, int>()
            // ReSharper disable once IntDivisionByZero - test for exception.
            .AddOperation<int, int>(x => x / 0);

        await Assert.ThrowsAsync<DivideByZeroException>(async () => await pipeline.RunAsync(10));
    }

    [Fact]
    public async Task RunAsync_WithExplicitResult_Success()
    {
        var logger = new SerilogLoggerFactory().CreateLogger(nameof(OperationPipelineTests));

        int final = 0;

        var pipeline = new OperationPipeline<string, int>(logger)
            .AddOperation<string, int>(Convert.ToInt32)
            .AddOperation<int, int>(
                param =>
                {
                    final = param + 5;
                    return param;
                })
            .SetResult(() => final);

        double result = await pipeline.RunAsync("50");
        var logEvents = TestCorrelator.GetLogEventsFromCurrentContext();

        Assert.Equal(55, result);
        Assert.Equal(4, logEvents.Count(x => x.Level == LogEventLevel.Information));
    }

    [Fact]
    public async Task RunAsync_WithImplicitResult_Success()
    {
        var logger = new SerilogLoggerFactory().CreateLogger(nameof(OperationPipelineTests));

        var pipeline = new OperationPipeline<string, int>(logger)
            .AddOperation<string, int>(Convert.ToInt32)
            .AddOperation<int, int>(param => param + 5);

        int result = await pipeline.RunAsync("50");
        var logEvents = TestCorrelator.GetLogEventsFromCurrentContext();

        Assert.Equal(55, result);
        Assert.Equal(4, logEvents.Count(x => x.Level == LogEventLevel.Information));
    }
}