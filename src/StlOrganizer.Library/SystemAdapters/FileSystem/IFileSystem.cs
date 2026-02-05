namespace StlOrganizer.Library.SystemAdapters.FileSystem;

public interface IFileSystem
{
    bool DirectoryExists(string path);
    IEnumerable<string> GetFiles(string path, string searchPattern, SearchOption searchOption);
    string[] GetDirectories(string path);
    void CreateDirectory(string path);
}
