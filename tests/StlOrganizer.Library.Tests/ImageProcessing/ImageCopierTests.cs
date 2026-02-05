using FakeItEasy;
using Shouldly;
using StlOrganizer.Library.ImageProcessing;
using StlOrganizer.Library.SystemAdapters.FileSystem;

namespace StlOrganizer.Library.Tests.ImageProcessing;

public class ImageCopierTests
{
    private readonly IFileOperations fileOperations = A.Fake<IFileOperations>();
    private readonly ImageCopier sut;

    public ImageCopierTests()
    {
        sut = new ImageCopier(fileOperations);
    }

    [Fact]
    public void CopyImageToFolder_NoExistingFile_CopiesToTarget()
    {
        const string source = @"C:\Root\photo.jpg";
        const string imagesFolder = @"C:\Root\Images";

        A.CallTo(() => fileOperations.FileExists(@"C:\Root\Images\photo.jpg")).Returns(false);

        sut.CopyImageToFolder(source, imagesFolder);

        A.CallTo(() => fileOperations.CopyFile(source, @"C:\Root\Images\photo.jpg", false))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void CopyImageToFolder_WhenFileExists_GeneratesUniqueNameAndCopies()
    {
        const string source = @"C:\Root\photo.jpg";
        const string imagesFolder = @"C:\Root\Images";
        const string existing = @"C:\Root\Images\photo.jpg";
        const string unique = @"C:\Root\Images\photo_1.jpg";

        A.CallTo(() => fileOperations.FileExists(existing)).Returns(true);
        A.CallTo(() => fileOperations.FileExists(unique)).Returns(false);

        sut.CopyImageToFolder(source, imagesFolder);

        A.CallTo(() => fileOperations.CopyFile(source, unique, false))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void GenerateUniqueFileName_IncrementsCounterUntilAvailable()
    {
        const string imagesFolder = @"C:\Root\Images";
        const string fileName = "photo.jpg";

        A.CallTo(() => fileOperations.FileExists(@"C:\Root\Images\photo_1.jpg")).Returns(true);
        A.CallTo(() => fileOperations.FileExists(@"C:\Root\Images\photo_2.jpg")).Returns(true);
        A.CallTo(() => fileOperations.FileExists(@"C:\Root\Images\photo_3.jpg")).Returns(false);

        var result = sut.GenerateUniqueFileName(imagesFolder, fileName);

        result.ShouldBe(@"C:\Root\Images\photo_3.jpg");
    }
}
