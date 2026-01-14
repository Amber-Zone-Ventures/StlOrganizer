namespace StlOrganizer.Library.SystemAdapters.AsyncWork;

public class CancellationTokenSourceProvider : ICancellationTokenSourceProvider
{
    public CancellationTokenSource Create() => new CancellationTokenSource();
}
