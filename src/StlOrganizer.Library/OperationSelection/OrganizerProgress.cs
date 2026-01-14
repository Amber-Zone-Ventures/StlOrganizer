namespace StlOrganizer.Library.OperationSelection;

public sealed class OrganizerProgress : Progress<OrganizerProgress>
{
    public int Progress { get; init; } = 0;
    public string? Message { get; init; } = "";
}
