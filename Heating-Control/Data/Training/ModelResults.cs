namespace Heating_Control.Data.Training;
public sealed class ModelResults
{
    public string? ReadableDuration { get; set; } = null;
    public TimeSpan? Duration { get; set; } = null;
    public float Median { get; set; }
    public float Average { get; set; }
    public float MaxDeviation { get; set; }
    public float MinDeviation { get; set; }
    public Dictionary<string, float> EvaluateResults { get; set; } = [];
}
