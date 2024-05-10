using Serilog.Events;
using Serilog.Sinks.TestCorrelator;

namespace Pipeliner.Net.UnitTests;

public partial class OperationPipelineTests
{
    [Fact]
    public async Task RunAsync_EmbeddedPipeline_Success()
    {
        var logger = loggerFactory.CreateLogger(nameof(OperationPipelineTests));

        var innerPipeline = new OperationPipeline<int, int>(logger)
            .AddOperation<int, int>(x => x + 5);

        var pipeline = new OperationPipeline<string, int>(logger)
            .AddOperation<string, int>(Convert.ToInt32)
            .AddPipeline(innerPipeline);

        int result = await pipeline.RunAsync("50");
        var logEvents = TestCorrelator.GetLogEventsFromCurrentContext();

        Assert.Equal(55, result);
        Assert.Equal(10, logEvents.Count(x => x.Level == LogEventLevel.Information));
    }

    [Fact]
    public async Task RunAsync_ExceptionOccursAndHandlerHandlesIt_Success()
    {
        var logger = loggerFactory.CreateLogger(nameof(OperationPipelineTests));

        var pipeline = new OperationPipeline<int, int>(logger)
            // ReSharper disable once IntDivisionByZero - test
            .AddOperation<int, int>(x => x / 0, onExceptionHandler: exception => exception is DivideByZeroException);

        int result = await pipeline.RunAsync(10);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task RunAsync_ExceptionOccursNoHandler_Success()
    {
        var logger = loggerFactory.CreateLogger(nameof(OperationPipelineTests));

        var pipeline = new OperationPipeline<int, int>(logger)
            // ReSharper disable once IntDivisionByZero - test for exception.
            .AddOperation<int, int>(x => x / 0);

        await Assert.ThrowsAsync<DivideByZeroException>(async () => await pipeline.RunAsync(10));
    }

    [Fact]
    public async Task RunAsync_WithExplicitResult_Success()
    {
        var logger = loggerFactory.CreateLogger(nameof(OperationPipelineTests));

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
        Assert.Equal(6, logEvents.Count(x => x.Level == LogEventLevel.Information));
    }

    [Fact]
    public async Task RunAsync_WithImplicitResult_Success()
    {
        var logger = loggerFactory.CreateLogger(nameof(OperationPipelineTests));

        var pipeline = new OperationPipeline<string, int>(logger)
            .AddOperation<string, int>(Convert.ToInt32)
            .AddOperation<int, int>(param => param + 5);

        int result = await pipeline.RunAsync("50");
        var logEvents = TestCorrelator.GetLogEventsFromCurrentContext();

        Assert.Equal(55, result);
        Assert.Equal(6, logEvents.Count(x => x.Level == LogEventLevel.Information));
    }
}