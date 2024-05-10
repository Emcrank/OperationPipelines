using System;
using System.Diagnostics;

namespace Pipeliner.Net;

/// <summary>
/// A stopwatch that will utilizes the IDisposable scope to start and stop automatically.
/// </summary>
internal class ScopeStopwatch : IDisposable
{
    private Action<DateTimeOffset>? onStart;
    private readonly Stopwatch stopwatch;

    /// <summary>
    /// Gets an action to be invoked just before the stopwatch starts. <see cref="DateTimeOffset.Now"/> is passed as a parameter.
    /// </summary>
    public Action<DateTimeOffset>? OnStart
    {
        get => onStart;
        init
        {
            onStart = value;
            onStart?.Invoke(DateTimeOffset.Now);
        }
    }

    /// <summary>
    /// Gets an action to be invoked after the stopwatch stop's. <see cref="Stopwatch.Elapsed"/>'s <see cref="TimeSpan"/> is passed as a parameter.
    /// </summary>
    public Action<TimeSpan>? OnComplete { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="ScopeStopwatch"/> and starts measuring elapsed time.
    /// </summary>
    internal ScopeStopwatch()
    {
        stopwatch = Stopwatch.StartNew();
    }

    /// <summary>
    /// Stops the <see cref="ScopeStopwatch"/> measuring elapsed time.
    /// </summary>
    public void Dispose()
    {
        stopwatch.Stop();
        OnComplete?.Invoke(stopwatch.Elapsed);
    }
}