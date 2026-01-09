namespace StlOrganizer.Library;

public interface IFileOperations
{
    bool FileExists(string path);
    void CopyFile(string sourceFileName, string destFileName, bool overwrite);
    string GetFileName(string path);
}
