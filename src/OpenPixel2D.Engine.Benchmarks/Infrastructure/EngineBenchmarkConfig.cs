using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Toolchains.InProcess;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;

namespace OpenPixel2D.Engine.Benchmarks.Infrastructure;

public class EngineBenchmarkConfig : ManualConfig
{
    public EngineBenchmarkConfig()
    {
        AddJob(CreateJob());
        AddDiagnoser(MemoryDiagnoser.Default);
        this.WithOrderer(new DefaultOrderer(SummaryOrderPolicy.Method, MethodOrderPolicy.Declared));

        if (UseInProcessFallback())
        {
            AddValidator(InProcessValidator.DontFailOnError);
        }
    }

    private static Job CreateJob()
    {
        Job job = Job.ShortRun;

        if (UseInProcessFallback())
        {
            return job.WithToolchain(InProcessNoEmitToolchain.Instance).WithId("ShortRunInProcess");
        }

        return job.WithId("ShortRun");
    }

    private static bool UseInProcessFallback()
    {
        string? value = Environment.GetEnvironmentVariable("OPENPIXEL2D_BENCHMARK_INPROCESS");
        return string.Equals(value, "1", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }
}
