﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Crank.OperationPipelines;

public class OperationPipeline<TParam, TResult>
{
    private readonly List<IUntypedOperation> operations = [];
    private Func<TResult?>? resultFactory;
    private volatile bool earlyExitSpecified;

    public OperationPipeline(ILogger? logger = null)
    {
        Logger = logger;
    }

    public string Id { get; set; } = Guid.NewGuid().ToString("D");

    public string Name { get; set; } = "Unnamed_Pipeline";

    protected ILogger? Logger { get; }

    protected TParam? Parameter { get; private set; }

    private void Reset()
    {
        resultFactory = null;
        earlyExitSpecified = false;
    }

    public TResult? Run(TParam parameter)
    {
        if (parameter == null)
            throw new ArgumentNullException(nameof(parameter));

        return RunInternal(CancellationToken.None);
    }

    /// <exception cref="OperationCanceledException" />
    public async Task<TResult?> RunAsync(TParam parameter, CancellationToken cancellationToken = default)
    {
        if (parameter == null)
            throw new ArgumentNullException(nameof(parameter));

        Parameter = parameter;

        if (operations.Count == 0)
            throw new InvalidOperationException("Must have 1 or more operations configured.");

        using (new ScopedStopwatch(
                   elapsed => Logger?.LogInformation("Pipeline [{PipelineId}](`{PipelineName}`) ended in {ElapsedMs}ms", Id, Name, elapsed.TotalMilliseconds)))
        {
            Logger?.LogInformation("Pipeline [{PipelineId}](`{PipelineName}`) starting...", Id, Name);

            try
            {
                return await Task.Run(() => RunInternal(cancellationToken), cancellationToken).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                throw new OperationCanceledException("Pipeline was cancelled.");
            }
            finally
            {
                Reset();
            }
        }
    }

    private TResult? RunInternal(CancellationToken cancellationToken = default)
    {
        var firstOperation = operations.First();
        var lastOperation = operations.Last();

        object? operationResult = default(TResult?);

        foreach (var operation in operations.Where(x => x.CanExecute()))
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (new ScopedStopwatch(
                       elapsed => Logger?.LogInformation(
                           "Pipeline [{PipelineId}] operation `{OperationName}` ended in {ElapsedMs}ms",
                           Id,
                           operation.Name,
                           elapsed.TotalMilliseconds)))
            {
                try
                {
                    // Determine the parameter for the operation.
                    object? operationParameter = operation == firstOperation
                        ? Parameter
                        : operationResult;

                    // Run the operation; using its input factory as a parameter.
                    operationResult = operation.UntypedExecution(operationParameter);

                    // Operation was completed, so invoke the completion handler if there is one.
                    operation.UntypedOnCompletionHandler?.Invoke(operationResult);

                    // Exit early conditions were met, so break.
                    if (earlyExitSpecified)
                        break;
                }
                catch (Exception ex)
                {
                    // If the operation has an exception handler defined,
                    // If it doesn't have one - propagate the exception.
                    // If it does have one - call it to see if it handles the exception, if it doesn't, we propagate it.
                    if (!operation.OnExceptionHandler?.Invoke(ex) ?? true)
                        throw;
                }
                finally
                {
                    // If last operation and result has not been explicitly set.
                    // Use the last operation's result.
                    if (resultFactory == null && operation == lastOperation)
                    {
                        // ReSharper disable once AccessToModifiedClosure
                        SetResult(() => (TResult?)operationResult);
                    }
                }
            }
        }

        return resultFactory!();
    }

    public OperationPipeline<TParam, TResult> SetResult(Func<TResult?> pipelineResultFactory)
    {
        if (resultFactory != null)
            throw new InvalidOperationException("You can only set the result once.");

        resultFactory = pipelineResultFactory;
        return this;
    }

    public OperationPipeline<TParam, TResult> AddOperation<TInput, TOutput>(Operation<TInput, TOutput> operation)
    {
        if (operation == null)
            throw new ArgumentNullException(nameof(operation));

        operations.Add(operation);
        return this;
    }

    public OperationPipeline<TParam, TResult> AddOperation<TInput, TOutput>(Func<TInput, TOutput> execution, string? operationName = null,
        Action<TOutput>? onCompletionHandler = null, Func<Exception, bool>? onExceptionHandler = null)
    {
        AddOperation(new DelegateOperation<TInput, TOutput>(execution, operationName, null, onCompletionHandler, onExceptionHandler));
        return this;
    }

    public OperationPipeline<TParam, TResult> AddConditionalOperation<TInput, TOutput>(Func<bool> ifTrueCondition, Func<TInput, TOutput> execution,
        string? operationName = null,
        Action<TOutput>? onCompletionHandler = null, Func<Exception, bool>? onExceptionHandler = null)
    {
        AddOperation(new DelegateOperation<TInput, TOutput>(execution, operationName, ifTrueCondition, onCompletionHandler, onExceptionHandler));
        return this;
    }

    public OperationPipeline<TParam, TResult> AddConditionalExit<TInput, TOutput>(Func<bool> ifTrueCondition, Func<TResult?> resultFactory,
        string? operationName = null)
    {
        AddOperation(
            new DelegateOperation<object, object>(
                _ =>
                {
                    earlyExitSpecified = true;
                    SetResult(resultFactory);
                    return new object();
                },
                operationName,
                ifTrueCondition));
        return this;
    }

    public OperationPipeline<TParam, TResult> AddPipeline<TInput, TOutput>(OperationPipeline<TInput, TOutput> pipeline, string? operationName = null,
    Action<TOutput>? onCompletionHandler = null, Func<Exception, bool>? onExceptionHandler = null)
    {
        AddOperation(new DelegateOperation<TInput, TOutput>(pipeline.Run, $"Pipeline `{pipeline.Name}`"));
        return this;
    }

    public OperationPipeline<TParam, TResult> RemoveOperationsByName(string operationName)
    {
        operations.RemoveAll(x => x.Name == operationName);
        return this;
    }

    public OperationPipeline<TParam, TResult> RemoveAllOperations()
    {
        operations.Clear();
        return this;
    }
}