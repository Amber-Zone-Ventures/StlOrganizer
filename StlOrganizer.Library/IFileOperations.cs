namespace StlOrganizer.Library;

public interface IFileOperations
{
    bool FileExists(string path);
    void CopyFile(string sourceFileName, string destFileName, bool overwrite);
    void DeleteFile(string path);
    string GetFileName(string path);
}
