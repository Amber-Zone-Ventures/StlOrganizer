namespace StlOrganizer.Library.SystemAdapters.FileSystem;

public class FileSystemAdapter(IFileOperations fileOperations) : IFileSystem
{
    public bool DirectoryExists(string path) => Directory.Exists(path);

    public IEnumerable<string> GetFiles(string path, string searchPattern, SearchOption searchOption)
        => Directory.GetFiles(path, searchPattern, searchOption);

    public string[] GetDirectories(string path) => Directory.GetDirectories(path);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    public string GetFolderName(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));

        if (fileOperations.FileExists(path))
            path = Path.GetDirectoryName(path)!;

        path = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        return Path.GetFileName(path);
    }
}
