namespace StlOrganizer.Library.OperationSelection;

public sealed class OrganizerProgress : Progress<OrganizerProgress>
{
    public int Progress { get; init; }
    public string? Message { get; init; } = "";
}
