using System;
using System.Diagnostics;

namespace Crank.OperationPipelines;

internal class ScopedStopwatch : IDisposable
{
    private readonly Action<TimeSpan> onComplete;
    private readonly Stopwatch stopwatch;

    internal ScopedStopwatch(Action<TimeSpan> onComplete)
    {
        this.onComplete = onComplete;
        stopwatch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        stopwatch.Stop();
        onComplete(stopwatch.Elapsed);
    }
}