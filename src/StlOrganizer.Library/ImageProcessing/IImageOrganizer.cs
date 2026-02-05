namespace StlOrganizer.Library.ImageProcessing;

public interface IImageOrganizer
{
    Task OrganizeImagesAsync(string rootPath, CancellationToken cancellationToken = default);
}
