namespace StlOrganizer.Library.SystemAdapters.AsyncWork;

public interface ICancellationTokenSourceProvider
{
    CancellationTokenSource Create();
}