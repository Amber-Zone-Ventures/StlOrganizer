namespace StlOrganizer.Library.ImageProcessing;

public interface IImageFinder
{
    IReadOnlyList<string> GetAllImageFiles(string path, CancellationToken cancellationToken = default);
}
