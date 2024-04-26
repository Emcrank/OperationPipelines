# Pipeliner.Net
A library for creating reusable pipelines of operations that can be executed as a whole.

```csharp
var pipeline = new OperationPipeline<string, int>(logger)
    .AddOperation<string, int>(Convert.ToInt32)
    .AddOperation<int, int>(param => param + 5);

int result = await pipeline.RunAsync("50");

Console.WriteLine(result);

// Prints 55
```