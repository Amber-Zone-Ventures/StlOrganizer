using FakeItEasy;
using Shouldly;
using StlOrganizer.Library.Decompression;
using StlOrganizer.Library.SystemAdapters.FileSystem;

namespace StlOrganizer.Library.Tests.Decompression;

public class FolderScannerTests
{
    private readonly IDecompressor decompressor = A.Fake<IDecompressor>();
    private readonly IFileSystem fileSystem = A.Fake<IFileSystem>();
    private readonly FolderScanner sut;

    public FolderScannerTests()
    {
        sut = new FolderScanner(
            fileSystem,
            decompressor);
    }

    [Fact]
    public async Task FindAndDecompress_WhenPathIsEmpty_ShouldThrowException()
    {
        A.CallTo(() => fileSystem.GetFiles(A<string>._, "*.zip", A<SearchOption>._))
            .Returns(new List<string>());

        await sut.FindAndDecompress(
                string.Empty,
                new Progress<DecompressionProgress>(),
                CancellationToken.None)
            .ShouldThrowAsync<NoArchivesFoundException>();
    }

    [Fact]
    public async Task FindAndDecompress_WhenZipFilesAreFound_ShouldDecompressThem()
    {
        const string folder = @"C:\TestDir";
        const string file = @"C:\TestDir\archive.zip";
        const string destination = @"C:\TestDir\archive";

        A.CallTo(() => fileSystem.GetFiles(folder, "*.zip", A<SearchOption>._))
            .Returns(new List<string> { file });
        var progress = A.Fake<IProgress<DecompressionProgress>>();

        await sut.FindAndDecompress(
            folder,
            progress,
            CancellationToken.None);

        A.CallTo(() => decompressor.DecompressAsync(file, destination, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task FindAndDecompress_WhenZipFilesAreFound_ShouldReportProgress()
    {
        const string folder = @"C:\TestDir";
        const string file = @"C:\TestDir\archive.zip";

        A.CallTo(() => fileSystem.GetFiles(folder, "*.zip", A<SearchOption>._))
            .Returns(new List<string> { file });
        var progress = A.Fake<IProgress<DecompressionProgress>>();

        await sut.FindAndDecompress(
            folder,
            progress,
            CancellationToken.None);

        A.CallTo(() => progress.Report(A<DecompressionProgress>._))
            .MustHaveHappenedOnceExactly();
    }
}
