namespace StlOrganizer.Library;

public interface ICancellationTokenSourceProvider
{
    CancellationTokenSource Create();
}