namespace StlOrganizer.Library.ImageProcessing;

public interface IImageCopier
{
    void CopyImageToFolder(string sourceFile, string imagesFolder);
    string GenerateUniqueFileName(string directory, string fileName);
}
