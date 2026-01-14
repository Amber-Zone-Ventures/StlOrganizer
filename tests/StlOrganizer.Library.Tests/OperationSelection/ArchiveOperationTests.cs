using Shouldly;
using StlOrganizer.Library.OperationSelection;

namespace StlOrganizer.Library.Tests.OperationSelection;

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
    
    [Fact]
    public void FromId_WhenPassedDecompressId_ReturnsDecompressArchives()
    {
        ArchiveOperation.FromId(1).ShouldBe(ArchiveOperation.DecompressArchives);
    }
    
    [Fact]
    public void FromId_WhenPassedDecompressId_ReturnsCompressFolder()
    {
        ArchiveOperation.FromId(2).ShouldBe(ArchiveOperation.CompressFolder);
    }

    [Fact]
    public void FromId_WhenPassedDecompressId_ReturnsExtractImages()
    {
        ArchiveOperation.FromId(3).ShouldBe(ArchiveOperation.ExtractImages);
    }

}