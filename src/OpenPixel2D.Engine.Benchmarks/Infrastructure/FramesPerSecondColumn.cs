using System.Globalization;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace OpenPixel2D.Engine.Benchmarks.Infrastructure;

internal sealed class FramesPerSecondColumn : IColumn
{
    public string Id => nameof(FramesPerSecondColumn);
    public string ColumnName => "FPS";
    public string Legend => "Derived frames per second based on the benchmark mean frame time.";
    public bool AlwaysShow => true;
    public ColumnCategory Category => ColumnCategory.Custom;
    public int PriorityInCategory => 0;
    public bool IsNumeric => true;
    public UnitType UnitType => UnitType.Dimensionless;
    public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;
    public bool IsAvailable(Summary summary) => true;
    public bool IsRowByRow => false;

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
    {
        return GetValue(summary, benchmarkCase, SummaryStyle.Default);
    }

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style)
    {
        double? framesPerSecond = GetFramesPerSecond(summary, benchmarkCase);

        if (framesPerSecond is null)
        {
            return "NA";
        }

        return framesPerSecond.Value.ToString("N2", CultureInfo.InvariantCulture);
    }

    public bool IsBetter(Summary summary, BenchmarkCase benchmarkCase, BenchmarkCase baselineBenchmarkCase)
    {
        double? current = GetFramesPerSecond(summary, benchmarkCase);
        double? baseline = GetFramesPerSecond(summary, baselineBenchmarkCase);

        if (current is null || baseline is null)
        {
            return false;
        }

        return current.Value > baseline.Value;
    }

    private static double? GetFramesPerSecond(Summary summary, BenchmarkCase benchmarkCase)
    {
        BenchmarkReport? report = summary.Reports.FirstOrDefault(candidate => candidate.BenchmarkCase == benchmarkCase);
        double? meanNanoseconds = report?.ResultStatistics?.Mean;

        if (meanNanoseconds is null || meanNanoseconds <= 0d)
        {
            return null;
        }

        return 1_000_000_000d / meanNanoseconds.Value;
    }
}
