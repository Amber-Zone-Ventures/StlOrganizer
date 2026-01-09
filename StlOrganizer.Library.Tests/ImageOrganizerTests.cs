using FakeItEasy;
using Serilog;
using Shouldly;

namespace StlOrganizer.Library.Tests;

public class ImageOrganizerTests
{
    private readonly IFileSystem fileSystem;
    private readonly IFileOperations fileOperations;
    private readonly ILogger logger;
    private readonly ImageOrganizer organizer;

    public ImageOrganizerTests()
    {
        fileSystem = A.Fake<IFileSystem>();
        fileOperations = A.Fake<IFileOperations>();
        logger = A.Fake<ILogger>();
        organizer = new ImageOrganizer(fileSystem, fileOperations, logger);
    }

    [Fact]
    public async Task OrganizeImagesAsync_WhenDirectoryDoesNotExist_ThrowsDirectoryNotFoundException()
    {
        const string rootPath = "C:\\NonExistent";
        A.CallTo(() => fileSystem.DirectoryExists(rootPath)).Returns(false);

        await Should.ThrowAsync<DirectoryNotFoundException>(
            async () => await organizer.OrganizeImagesAsync(rootPath));
    }

    [Fact]
    public async Task OrganizeImagesAsync_CreatesImagesFolder()
    {
        const string rootPath = "C:\\TestDir";
        const string imagesFolder = "C:\\TestDir\\Images";

        A.CallTo(() => fileSystem.DirectoryExists(rootPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFiles(A<string>._, A<string>._, A<SearchOption>._))
            .Returns(Array.Empty<string>());
        A.CallTo(() => fileSystem.GetDirectories(A<string>._)).Returns(Array.Empty<string>());

        await organizer.OrganizeImagesAsync(rootPath);

        A.CallTo(() => fileSystem.CreateDirectory(imagesFolder)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task OrganizeImagesAsync_WhenNoImageFiles_ReturnsZero()
    {
        const string rootPath = "C:\\TestDir";

        A.CallTo(() => fileSystem.DirectoryExists(rootPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFiles(A<string>._, A<string>._, A<SearchOption>._))
            .Returns(new[] { "C:\\TestDir\\file.txt", "C:\\TestDir\\document.pdf" });
        A.CallTo(() => fileSystem.GetExtension("C:\\TestDir\\file.txt")).Returns(".txt");
        A.CallTo(() => fileSystem.GetExtension("C:\\TestDir\\document.pdf")).Returns(".pdf");
        A.CallTo(() => fileSystem.GetDirectories(A<string>._)).Returns(Array.Empty<string>());

        var result = await organizer.OrganizeImagesAsync(rootPath);

        result.ShouldBe(0);
    }

    [Fact]
    public async Task OrganizeImagesAsync_WithJpgFile_CopiesImage()
    {
        const string rootPath = "C:\\TestDir";
        const string imageFile = "C:\\TestDir\\photo.jpg";

        A.CallTo(() => fileSystem.DirectoryExists(rootPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFiles(rootPath, "*.*", SearchOption.TopDirectoryOnly))
            .Returns(new[] { imageFile });
        A.CallTo(() => fileSystem.GetExtension(imageFile)).Returns(".jpg");
        A.CallTo(() => fileOperations.GetFileName(imageFile)).Returns("photo.jpg");
        A.CallTo(() => fileOperations.FileExists(A<string>._)).Returns(false);
        A.CallTo(() => fileSystem.GetDirectories(A<string>._)).Returns(Array.Empty<string>());

        var result = await organizer.OrganizeImagesAsync(rootPath);

        result.ShouldBe(1);
        A.CallTo(() => fileOperations.CopyFile(imageFile, "C:\\TestDir\\Images\\photo.jpg", false))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task OrganizeImagesAsync_WithMultipleImageTypes_CopiesAllImages()
    {
        const string rootPath = "C:\\TestDir";
        const string jpgFile = "C:\\TestDir\\photo.jpg";
        const string pngFile = "C:\\TestDir\\image.png";
        const string gifFile = "C:\\TestDir\\animation.gif";

        A.CallTo(() => fileSystem.DirectoryExists(rootPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFiles(rootPath, "*.*", SearchOption.TopDirectoryOnly))
            .Returns(new[] { jpgFile, pngFile, gifFile });
        A.CallTo(() => fileSystem.GetExtension(jpgFile)).Returns(".jpg");
        A.CallTo(() => fileSystem.GetExtension(pngFile)).Returns(".png");
        A.CallTo(() => fileSystem.GetExtension(gifFile)).Returns(".gif");
        A.CallTo(() => fileOperations.GetFileName(jpgFile)).Returns("photo.jpg");
        A.CallTo(() => fileOperations.GetFileName(pngFile)).Returns("image.png");
        A.CallTo(() => fileOperations.GetFileName(gifFile)).Returns("animation.gif");
        A.CallTo(() => fileOperations.FileExists(A<string>._)).Returns(false);
        A.CallTo(() => fileSystem.GetDirectories(A<string>._)).Returns(Array.Empty<string>());

        var result = await organizer.OrganizeImagesAsync(rootPath);

        result.ShouldBe(3);
    }

    [Fact]
    public async Task OrganizeImagesAsync_IsCaseInsensitive()
    {
        const string rootPath = "C:\\TestDir";
        const string imageFile = "C:\\TestDir\\PHOTO.JPG";

        A.CallTo(() => fileSystem.DirectoryExists(rootPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFiles(rootPath, "*.*", SearchOption.TopDirectoryOnly))
            .Returns(new[] { imageFile });
        A.CallTo(() => fileSystem.GetExtension(imageFile)).Returns(".JPG");
        A.CallTo(() => fileOperations.GetFileName(imageFile)).Returns("PHOTO.JPG");
        A.CallTo(() => fileOperations.FileExists(A<string>._)).Returns(false);
        A.CallTo(() => fileSystem.GetDirectories(A<string>._)).Returns(Array.Empty<string>());

        var result = await organizer.OrganizeImagesAsync(rootPath);

        result.ShouldBe(1);
    }

    [Fact]
    public async Task OrganizeImagesAsync_WithDuplicateFileName_GeneratesUniqueFileName()
    {
        const string rootPath = "C:\\TestDir";
        const string imageFile = "C:\\TestDir\\photo.jpg";
        const string existingFile = "C:\\TestDir\\Images\\photo.jpg";
        const string uniqueFile = "C:\\TestDir\\Images\\photo_1.jpg";

        A.CallTo(() => fileSystem.DirectoryExists(rootPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFiles(rootPath, "*.*", SearchOption.TopDirectoryOnly))
            .Returns(new[] { imageFile });
        A.CallTo(() => fileSystem.GetExtension(imageFile)).Returns(".jpg");
        A.CallTo(() => fileOperations.GetFileName(imageFile)).Returns("photo.jpg");
        A.CallTo(() => fileOperations.FileExists(existingFile)).Returns(true);
        A.CallTo(() => fileOperations.FileExists(uniqueFile)).Returns(false);
        A.CallTo(() => fileSystem.GetDirectories(A<string>._)).Returns(Array.Empty<string>());

        await organizer.OrganizeImagesAsync(rootPath);

        A.CallTo(() => fileOperations.CopyFile(imageFile, uniqueFile, false))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task OrganizeImagesAsync_WithSubdirectories_ScansRecursively()
    {
        const string rootPath = "C:\\TestDir";
        const string subDir = "C:\\TestDir\\SubDir";
        const string imageInRoot = "C:\\TestDir\\photo1.jpg";
        const string imageInSub = "C:\\TestDir\\SubDir\\photo2.jpg";

        A.CallTo(() => fileSystem.DirectoryExists(rootPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFiles(rootPath, "*.*", SearchOption.TopDirectoryOnly))
            .Returns(new[] { imageInRoot });
        A.CallTo(() => fileSystem.GetFiles(subDir, "*.*", SearchOption.TopDirectoryOnly))
            .Returns(new[] { imageInSub });
        A.CallTo(() => fileSystem.GetDirectories(rootPath)).Returns(new[] { subDir });
        A.CallTo(() => fileSystem.GetDirectories(subDir)).Returns(Array.Empty<string>());
        A.CallTo(() => fileSystem.GetExtension(A<string>._)).Returns(".jpg");
        A.CallTo(() => fileOperations.GetFileName(imageInRoot)).Returns("photo1.jpg");
        A.CallTo(() => fileOperations.GetFileName(imageInSub)).Returns("photo2.jpg");
        A.CallTo(() => fileOperations.FileExists(A<string>._)).Returns(false);

        var result = await organizer.OrganizeImagesAsync(rootPath);

        result.ShouldBe(2);
    }

    [Fact]
    public async Task OrganizeImagesAsync_DoesNotScanImagesFolderItself()
    {
        const string rootPath = "C:\\TestDir";
        const string imagesFolder = "C:\\TestDir\\Images";
        const string imageInRoot = "C:\\TestDir\\photo.jpg";

        A.CallTo(() => fileSystem.DirectoryExists(rootPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFiles(rootPath, "*.*", SearchOption.TopDirectoryOnly))
            .Returns(new[] { imageInRoot });
        A.CallTo(() => fileSystem.GetDirectories(rootPath)).Returns(new[] { imagesFolder });
        A.CallTo(() => fileSystem.GetExtension(imageInRoot)).Returns(".jpg");
        A.CallTo(() => fileOperations.GetFileName(imageInRoot)).Returns("photo.jpg");
        A.CallTo(() => fileOperations.FileExists(A<string>._)).Returns(false);

        await organizer.OrganizeImagesAsync(rootPath);

        A.CallTo(() => fileSystem.GetFiles(imagesFolder, A<string>._, A<SearchOption>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task OrganizeImagesAsync_WhenCopyFails_ContinuesWithOtherFiles()
    {
        const string rootPath = "C:\\TestDir";
        const string image1 = "C:\\TestDir\\photo1.jpg";
        const string image2 = "C:\\TestDir\\photo2.jpg";

        A.CallTo(() => fileSystem.DirectoryExists(rootPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFiles(rootPath, "*.*", SearchOption.TopDirectoryOnly))
            .Returns(new[] { image1, image2 });
        A.CallTo(() => fileSystem.GetExtension(A<string>._)).Returns(".jpg");
        A.CallTo(() => fileOperations.GetFileName(image1)).Returns("photo1.jpg");
        A.CallTo(() => fileOperations.GetFileName(image2)).Returns("photo2.jpg");
        A.CallTo(() => fileOperations.FileExists(A<string>._)).Returns(false);
        A.CallTo(() => fileOperations.CopyFile(image1, A<string>._, A<bool>._))
            .Throws(new IOException("Access denied"));
        A.CallTo(() => fileSystem.GetDirectories(A<string>._)).Returns(Array.Empty<string>());

        var result = await organizer.OrganizeImagesAsync(rootPath);

        result.ShouldBe(1);
        A.CallTo(() => fileOperations.CopyFile(image2, A<string>._, A<bool>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task OrganizeImagesAsync_SupportsAllCommonImageFormats()
    {
        const string rootPath = "C:\\TestDir";
        var imageFiles = new[]
        {
            "C:\\TestDir\\file.jpg",
            "C:\\TestDir\\file.jpeg",
            "C:\\TestDir\\file.png",
            "C:\\TestDir\\file.gif",
            "C:\\TestDir\\file.bmp",
            "C:\\TestDir\\file.tiff",
            "C:\\TestDir\\file.webp"
        };

        A.CallTo(() => fileSystem.DirectoryExists(rootPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFiles(rootPath, "*.*", SearchOption.TopDirectoryOnly))
            .Returns(imageFiles);
        A.CallTo(() => fileSystem.GetExtension("C:\\TestDir\\file.jpg")).Returns(".jpg");
        A.CallTo(() => fileSystem.GetExtension("C:\\TestDir\\file.jpeg")).Returns(".jpeg");
        A.CallTo(() => fileSystem.GetExtension("C:\\TestDir\\file.png")).Returns(".png");
        A.CallTo(() => fileSystem.GetExtension("C:\\TestDir\\file.gif")).Returns(".gif");
        A.CallTo(() => fileSystem.GetExtension("C:\\TestDir\\file.bmp")).Returns(".bmp");
        A.CallTo(() => fileSystem.GetExtension("C:\\TestDir\\file.tiff")).Returns(".tiff");
        A.CallTo(() => fileSystem.GetExtension("C:\\TestDir\\file.webp")).Returns(".webp");
        A.CallTo(() => fileOperations.GetFileName(A<string>._)).ReturnsLazily((string path) => Path.GetFileName(path));
        A.CallTo(() => fileOperations.FileExists(A<string>._)).Returns(false);
        A.CallTo(() => fileSystem.GetDirectories(A<string>._)).Returns(Array.Empty<string>());

        var result = await organizer.OrganizeImagesAsync(rootPath);

        result.ShouldBe(7);
    }
}
