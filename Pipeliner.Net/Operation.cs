using System;

namespace Pipeliner.Net;

public abstract class Operation<TInput, TResult> : IUntypedOperation
{
    public abstract Func<TInput, TResult?> Execution { get; }

    public virtual Action<TResult> OnCompletionHandler => DelegateDefaults.OnCompletionHandler<TResult>.Empty;

    public virtual string Name => "Unnamed_Operation";

    public virtual Func<Exception, bool> OnExceptionHandler => DelegateDefaults.OnExceptionHandler.Empty;

    public virtual Func<bool> CanExecute => DelegateDefaults.CanExecute.Always;

    Func<object?, object> IUntypedOperation.UntypedExecution => x => Execution((TInput)x!)!;

    Action<object> IUntypedOperation.UntypedOnCompletionHandler => x => OnCompletionHandler.Invoke((TResult)x);
}