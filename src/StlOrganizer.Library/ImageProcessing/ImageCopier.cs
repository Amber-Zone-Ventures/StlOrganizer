using StlOrganizer.Library.SystemAdapters.FileSystem;

namespace StlOrganizer.Library.ImageProcessing;

public class ImageCopier(IFileOperations fileOperations) : IImageCopier
{
    public void CopyImageToFolder(string sourceFile, string imagesFolder)
    {
        var fileName = Path.GetFileName(sourceFile);
        var destinationPath = Path.Combine(imagesFolder, fileName);

        if (fileOperations.FileExists(destinationPath))
            destinationPath = GenerateUniqueFileName(imagesFolder, fileName);

        fileOperations.CopyFile(sourceFile, destinationPath, false);
    }

    public string GenerateUniqueFileName(string directory, string fileName)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);
        var counter = 1;

        string newFileName;
        do
        {
            newFileName = Path.Combine(directory, $"{fileNameWithoutExtension}_{counter}{extension}");
            counter++;
        } while (fileOperations.FileExists(newFileName));

        return newFileName;
    }
}
