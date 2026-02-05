using Serilog;
using StlOrganizer.Library.SystemAdapters.FileSystem;

namespace StlOrganizer.Library.ImageProcessing;

public class ImageOrganizer(
    IFileSystem fileSystem,
    IImageFinder imageFinder,
    IImageCopier imageCopier,
    ILogger logger) : IImageOrganizer
{
    private const string ImagesFolderName = "Images";

    public async Task OrganizeImagesAsync(
        string rootPath,
        CancellationToken cancellationToken = default)
    {
        if (!fileSystem.DirectoryExists(rootPath))
            throw new DirectoryNotFoundException($"Directory not found: {rootPath}");

        await Task.Run(() =>
        {
            var imagesFolder = Path.Combine(rootPath, ImagesFolderName);
            fileSystem.CreateDirectory(imagesFolder);

            var copiedCount = 0;
            var allImages = imageFinder.GetAllImageFiles(rootPath, cancellationToken);

            foreach (var file in allImages)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    imageCopier.CopyImageToFolder(file, imagesFolder);
                    copiedCount++;
                    logger.Debug("Copied image {FileName} to Images folder", Path.GetFileName(file));
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Failed to copy image {FileName}", file);
                }
            }
            logger.Information("Organized {CopiedCount} image(s) into {ImagesFolder}", copiedCount, imagesFolder);
        }, cancellationToken);
    }
}
