using System.Diagnostics.CodeAnalysis;

namespace StlOrganizer.Library.SystemAdapters.FileSystem;

public class DirectoryServiceAdapter : IDirectoryService
{
    [ExcludeFromCodeCoverage]
    public bool Exists(string path) => Directory.Exists(path);

    [ExcludeFromCodeCoverage]
    public string[] GetDirectories(string path) => Directory.GetDirectories(path);

    [ExcludeFromCodeCoverage]
    public void Move(string sourcePath, string destinationPath)
    {
        foreach (var file in Directory.GetFiles(sourcePath))
        {
            var fileName = Path.GetFileName(file);
            var destFile = Path.Combine(destinationPath, fileName);
            File.Move(file, destFile);
        }

        foreach (var dir in Directory.GetDirectories(sourcePath))
        {
            var dirName = Path.GetFileName(dir);
            var destDir = Path.Combine(destinationPath, dirName);
            Directory.Move(dir, destDir);
        }
    }

    [ExcludeFromCodeCoverage]
    public void Delete(string path, bool recursive) => Directory.Delete(path, recursive);

    public string GetDirectoryName(string path) => Path.GetFileName(path);
}
