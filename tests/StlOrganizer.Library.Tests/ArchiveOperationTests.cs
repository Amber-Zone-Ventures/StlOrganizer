using Shouldly;

namespace StlOrganizer.Library.Tests;

public class ArchiveOperationTests
{
    [Fact]
    public void DecompressFiles_WhenCreated_ShouldHaveValue()
    {
        ArchiveOperation.DecompressArchives.Id.ShouldBe(1);
        ArchiveOperation.DecompressArchives.Name.ShouldBe("Decompress files");
    }

    [Fact]
    public void CompressFolder_WhenCreated_ShouldHaveValue()
    {
        ArchiveOperation.CompressFolder.Id.ShouldBe(2);
        ArchiveOperation.CompressFolder.Name.ShouldBe("Compress folder");
    }

    [Fact]
    public void ExtractImages_WhenCreated_ShouldHaveValue()
    {
        ArchiveOperation.ExtractImages.Id.ShouldBe(3);
        ArchiveOperation.ExtractImages.Name.ShouldBe("Extract images");
    }
}