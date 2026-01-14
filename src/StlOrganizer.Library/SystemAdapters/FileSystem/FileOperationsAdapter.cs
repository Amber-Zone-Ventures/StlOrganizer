namespace StlOrganizer.Library.SystemAdapters.FileSystem;

public class FileOperationsAdapter : IFileOperations
{
    public bool FileExists(string path) => File.Exists(path);
    
    public bool DirectoryExists(string path) => Directory.Exists(path);

    public void CopyFile(string sourceFileName, string destFileName, bool overwrite) 
        => File.Copy(sourceFileName, destFileName, overwrite);

    public void DeleteFile(string path) => File.Delete(path);

    public string GetFileName(string path) => Path.GetFileName(path);
}
