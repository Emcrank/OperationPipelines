using System;

namespace Crank.OperationPipelines;

public class DelegateOperation<TInput, TOutput> : Operation<TInput, TOutput>
{
    public DelegateOperation(Func<TInput, TOutput?> execution,
        string? operationName = null,
        Func<bool>? canExecute = null,
        Action<TOutput>? onCompletionHandler = null,
        Func<Exception, bool>? onExceptionHandler = null)
    {
        Execution = execution;
        Name = operationName ?? base.Name;
        CanExecute = canExecute ?? base.CanExecute;
        OnCompletionHandler = onCompletionHandler ?? base.OnCompletionHandler;
        OnExceptionHandler = onExceptionHandler ?? base.OnExceptionHandler;
    }

    public override Func<TInput, TOutput?> Execution { get; }

    public override string Name { get; }

    public override Func<bool> CanExecute { get; }

    public override Action<TOutput> OnCompletionHandler { get; }

    public override Func<Exception, bool> OnExceptionHandler { get; }
}