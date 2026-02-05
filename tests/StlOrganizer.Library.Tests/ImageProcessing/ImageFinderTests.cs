using FakeItEasy;
using Shouldly;
using StlOrganizer.Library.ImageProcessing;
using StlOrganizer.Library.SystemAdapters.FileSystem;

namespace StlOrganizer.Library.Tests.ImageProcessing;

public class ImageFinderTests
{
    private readonly IFileSystem fileSystem = A.Fake<IFileSystem>();
    private readonly ImageFinder sut;

    public ImageFinderTests()
    {
        sut = new ImageFinder(fileSystem);
    }

    [Fact]
    public void GetAllImageFiles_WhenNoImages_ReturnsEmptyList()
    {
        const string root = @"C:\Empty";
        var files = new List<string>
        {
            @"C:\Empty\readme.md",
            @"C:\Empty\note.TXT",
            @"C:\Empty\data.csv"
        };

        A.CallTo(() => fileSystem.GetFiles(root, "*.*", SearchOption.AllDirectories))
            .Returns(files);

        var result = sut.GetAllImageFiles(root);

        result.ShouldBeEmpty();
    }

    [Fact]
    public void GetAllImageFiles_ShouldBeCaseInsensitiveOnExtensions()
    {
        const string root = @"C:\Pics";
        var files = new List<string>
        {
            @"C:\Pics\upper.JPG",
            @"C:\Pics\mixed.JpEg",
            @"C:\Pics\vector.SVG"
        };

        A.CallTo(() => fileSystem.GetFiles(root, "*.*", SearchOption.AllDirectories))
            .Returns(files);

        var result = sut.GetAllImageFiles(root);

        result.ShouldBe(files);
    }

    [Fact]
    public void GetAllImageFiles_FindsAllSupportedExtensions()
    {
        const string root = @"C:\AllTypes";
        var files = new List<string>
        {
            @"C:\AllTypes\photo.jpg",
            @"C:\AllTypes\photo.jpeg",
            @"C:\AllTypes\image.png",
            @"C:\AllTypes\anim.gif",
            @"C:\AllTypes\bitmap.bmp",
            @"C:\AllTypes\scan.tiff",
            @"C:\AllTypes\scan.tif",
            @"C:\AllTypes\modern.webp",
            @"C:\AllTypes\vector.svg"
        };

        A.CallTo(() => fileSystem.GetFiles(root, "*.*", SearchOption.AllDirectories))
            .Returns(files);

        var result = sut.GetAllImageFiles(root);

        result.ShouldBe(files);
    }
}
