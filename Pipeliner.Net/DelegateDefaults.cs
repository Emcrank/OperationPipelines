using System;

namespace Pipeliner.Net;

internal static class DelegateDefaults
{
    internal static class CanExecute
    {
        internal static Func<bool> Always { get; } = () => true;
    }

    internal static class OnCompletionHandler<T>
    {
        internal static Action<T> Empty { get; } = _ => { };
    }

    internal static class OnExceptionHandler
    {
        internal static Func<Exception, bool> Empty { get; } = _ => false;
    }
}