namespace StlOrganizer.Library.Decompression;

public sealed class DecompressionProgress : Progress<DecompressionProgress>
{
    public int Progress { get; init; }
    public string? Message { get; init; } = "";
}
