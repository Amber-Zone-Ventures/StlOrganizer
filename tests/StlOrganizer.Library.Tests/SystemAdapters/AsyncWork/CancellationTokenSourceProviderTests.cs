using Shouldly;
using StlOrganizer.Library.SystemAdapters.AsyncWork;

namespace StlOrganizer.Library.Tests.SystemAdapters.AsyncWork;

public class CancellationTokenSourceProviderTests
{
    [Fact]
    public void Create_ReturnsTokenSource()
    {
        new CancellationTokenSourceProvider()
            .Create()
            .ShouldBeOfType<CancellationTokenSource>();
    }
}
