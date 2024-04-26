using System;

namespace Pipeliner.Net;

internal interface IUntypedOperation
{
    string Name { get; }

    Func<object?, object> UntypedExecution { get; }

    Action<object>? UntypedOnCompletionHandler { get; }

    Func<bool> CanExecute { get; }

    Func<Exception, bool>? OnExceptionHandler { get; }
}