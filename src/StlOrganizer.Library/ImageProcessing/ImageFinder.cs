using StlOrganizer.Library.SystemAdapters.FileSystem;

namespace StlOrganizer.Library.ImageProcessing;

public class ImageFinder(IFileSystem fileSystem) : IImageFinder
{
    private static readonly string[] ImageExtensions =
    [
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif", ".webp", ".svg"
    ];

    public IReadOnlyList<string> GetAllImageFiles(string path, CancellationToken cancellationToken = default)
        => fileSystem.GetFiles(path, "*.*", SearchOption.AllDirectories)
            .Where(IsImageFile)
            .ToList();

    private static bool IsImageFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return ImageExtensions.Contains(extension);
    }
}
