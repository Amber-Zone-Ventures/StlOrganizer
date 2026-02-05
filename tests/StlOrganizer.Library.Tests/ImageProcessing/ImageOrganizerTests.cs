using FakeItEasy;
using Serilog;
using Shouldly;
using StlOrganizer.Library.ImageProcessing;
using StlOrganizer.Library.SystemAdapters.FileSystem;

namespace StlOrganizer.Library.Tests.ImageProcessing;

public class ImageOrganizerTests
{
    private readonly IFileSystem fileSystem;
    private readonly IImageCopier imageCopier;
    private readonly IImageFinder imageFinder;
    private readonly ImageOrganizer organizer;

    public ImageOrganizerTests()
    {
        fileSystem = A.Fake<IFileSystem>();
        imageFinder = A.Fake<IImageFinder>();
        imageCopier = A.Fake<IImageCopier>();
        var logger1 = A.Fake<ILogger>();
        organizer = new ImageOrganizer(fileSystem, imageFinder, imageCopier, logger1);
    }

    [Fact]
    public async Task OrganizeImagesAsync_WhenDirectoryDoesNotExist_ThrowsDirectoryNotFoundException()
    {
        const string rootPath = @"C:\NonExistent";
        A.CallTo(() => fileSystem.DirectoryExists(rootPath)).Returns(false);

        await Should.ThrowAsync<DirectoryNotFoundException>(
            async () => await organizer.OrganizeImagesAsync(rootPath));
    }

    [Fact]
    public async Task OrganizeImagesAsync_CreatesImagesFolder()
    {
        const string rootPath = @"C:\TestDir";
        const string imagesFolder = @"C:\TestDir\Images";

        A.CallTo(() => fileSystem.DirectoryExists(rootPath)).Returns(true);
        A.CallTo(() => imageFinder.GetAllImageFiles(rootPath, A<CancellationToken>._))
            .Returns([]);

        await organizer.OrganizeImagesAsync(rootPath);

        A.CallTo(() => fileSystem.CreateDirectory(imagesFolder)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task OrganizeImagesAsync_WhenNoImageFiles_DoesNotCopy()
    {
        const string rootPath = @"C:\TestDir";

        A.CallTo(() => fileSystem.DirectoryExists(rootPath)).Returns(true);
        A.CallTo(() => imageFinder.GetAllImageFiles(rootPath, A<CancellationToken>._))
            .Returns([]);

        await organizer.OrganizeImagesAsync(rootPath);

        A.CallTo(() => imageCopier.CopyImageToFolder(A<string>._, A<string>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task OrganizeImagesAsync_WithJpgFile_CopiesImage()
    {
        const string rootPath = @"C:\TestDir";
        const string imageFile = @"C:\TestDir\photo.jpg";

        A.CallTo(() => fileSystem.DirectoryExists(rootPath)).Returns(true);
        A.CallTo(() => imageFinder.GetAllImageFiles(rootPath, A<CancellationToken>._))
            .Returns(new List<string> { imageFile });

        await organizer.OrganizeImagesAsync(rootPath);

        A.CallTo(() => imageCopier.CopyImageToFolder(imageFile, @"C:\TestDir\Images"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task OrganizeImagesAsync_WithMultipleImageTypes_CopiesAllImages()
    {
        const string rootPath = @"C:\TestDir";
        const string jpgFile = @"C:\TestDir\photo.jpg";
        const string pngFile = @"C:\TestDir\image.png";
        const string gifFile = @"C:\TestDir\animation.gif";

        A.CallTo(() => fileSystem.DirectoryExists(rootPath)).Returns(true);
        A.CallTo(() => imageFinder.GetAllImageFiles(rootPath, A<CancellationToken>._))
            .Returns(new List<string> { jpgFile, pngFile, gifFile });

        await organizer.OrganizeImagesAsync(rootPath);

        A.CallTo(() => imageCopier.CopyImageToFolder(jpgFile, @"C:\TestDir\Images"))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => imageCopier.CopyImageToFolder(pngFile, @"C:\TestDir\Images"))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => imageCopier.CopyImageToFolder(gifFile, @"C:\TestDir\Images"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task OrganizeImagesAsync_DelegatesCopyToCopier()
    {
        const string rootPath = @"C:\TestDir";
        const string imageFile = @"C:\TestDir\photo.jpg";

        A.CallTo(() => fileSystem.DirectoryExists(rootPath)).Returns(true);
        A.CallTo(() => imageFinder.GetAllImageFiles(rootPath, A<CancellationToken>._))
            .Returns(new List<string> { imageFile });

        await organizer.OrganizeImagesAsync(rootPath);

        A.CallTo(() => imageCopier.CopyImageToFolder(imageFile, @"C:\TestDir\Images"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task OrganizeImagesAsync_WithSubdirectories_CopiesAll()
    {
        const string rootPath = @"C:\TestDir";
        const string imageInRoot = @"C:\TestDir\photo1.jpg";
        const string imageInSub = @"C:\TestDir\SubDir\photo2.jpg";

        A.CallTo(() => fileSystem.DirectoryExists(rootPath)).Returns(true);
        A.CallTo(() => imageFinder.GetAllImageFiles(rootPath, A<CancellationToken>._))
            .Returns(new List<string> { imageInRoot, imageInSub });

        await organizer.OrganizeImagesAsync(rootPath);

        A.CallTo(() => imageCopier.CopyImageToFolder(imageInRoot, @"C:\TestDir\Images"))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => imageCopier.CopyImageToFolder(imageInSub, @"C:\TestDir\Images"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task OrganizeImagesAsync_CopiesImagesEvenIfAlreadyInImagesFolder()
    {
        const string rootPath = @"C:\TestDir";
        const string imagesFolder = @"C:\TestDir\Images";
        const string imageInRoot = @"C:\TestDir\photo.jpg";
        const string imageInImages = @"C:\TestDir\Images\already.jpg";

        A.CallTo(() => fileSystem.DirectoryExists(rootPath)).Returns(true);
        A.CallTo(() => imageFinder.GetAllImageFiles(rootPath, A<CancellationToken>._))
            .Returns(new List<string> { imageInRoot, imageInImages });

        await organizer.OrganizeImagesAsync(rootPath);

        A.CallTo(() => imageCopier.CopyImageToFolder(imageInRoot, imagesFolder))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => imageCopier.CopyImageToFolder(imageInImages, imagesFolder))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task OrganizeImagesAsync_WhenCopyFails_ContinuesWithOtherFiles()
    {
        const string rootPath = @"C:\TestDir";
        const string image1 = @"C:\TestDir\photo1.jpg";
        const string image2 = @"C:\TestDir\photo2.jpg";

        A.CallTo(() => fileSystem.DirectoryExists(rootPath)).Returns(true);
        A.CallTo(() => imageFinder.GetAllImageFiles(rootPath, A<CancellationToken>._))
            .Returns(new List<string> { image1, image2 });
        A.CallTo(() => imageCopier.CopyImageToFolder(image1, @"C:\TestDir\Images"))
            .Throws(new IOException("Access denied"));

        await organizer.OrganizeImagesAsync(rootPath);
        A.CallTo(() => imageCopier.CopyImageToFolder(image2, @"C:\TestDir\Images"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task OrganizeImagesAsync_SupportsAllCommonImageFormats()
    {
        const string rootPath = @"C:\TestDir";
        string[] imageFiles =
        [
            @"C:\TestDir\file.jpg",
            @"C:\TestDir\file.jpeg",
            @"C:\TestDir\file.png",
            @"C:\TestDir\file.gif",
            @"C:\TestDir\file.bmp",
            @"C:\TestDir\file.tiff",
            @"C:\TestDir\file.webp"
        ];

        A.CallTo(() => fileSystem.DirectoryExists(rootPath)).Returns(true);
        A.CallTo(() => imageFinder.GetAllImageFiles(rootPath, A<CancellationToken>._))
            .Returns(imageFiles);

        await organizer.OrganizeImagesAsync(rootPath);
        foreach (var file in imageFiles)
        {
            A.CallTo(() => imageCopier.CopyImageToFolder(file, @"C:\TestDir\Images"))
                .MustHaveHappenedOnceExactly();
        }
    }
}
