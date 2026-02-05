namespace StlOrganizer.Library.SystemAdapters.FileSystem;

public interface IFileOperations
{
    bool FileExists(string path);
    bool DirectoryExists(string path);
    void CopyFile(string sourceFileName, string destFileName, bool overwrite);
}
