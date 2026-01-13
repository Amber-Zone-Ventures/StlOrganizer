using System.Threading;

namespace StlOrganizer.Library;

public class CancellationTokenSourceProvider : ICancellationTokenSourceProvider
{
    public CancellationTokenSource Create() => new CancellationTokenSource();
}
