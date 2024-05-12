# Pipeliner.Net
A library for creating reusable pipelines of operations that can be executed as a whole.

## ![CI Build]([https://github.com/github/docs/actions/workflows/main.yml/badge.svg](https://github.com/Emcrank/Pipeliner.Net/actions/workflows/ci.yml/badge.svg))

```csharp
var pipeline = new OperationPipeline<string, int>(logger)
    .AddOperation<string, int>(Convert.ToInt32)
    .AddOperation<int, int>(param => param + 5);

int result = await pipeline.RunAsync("50");

Console.WriteLine(result);

// Prints 55
```
