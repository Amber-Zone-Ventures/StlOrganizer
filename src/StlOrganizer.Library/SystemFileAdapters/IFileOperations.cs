namespace StlOrganizer.Library.SystemFileAdapters;

public interface IFileOperations
{
    bool FileExists(string path);
    bool DirectoryExists(string path);
    void CopyFile(string sourceFileName, string destFileName, bool overwrite);
    void DeleteFile(string path);
    string GetFileName(string path);
}
