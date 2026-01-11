using FakeItEasy;
using Serilog;
using Shouldly;
using StlOrganizer.Library.Decompression;
using StlOrganizer.Library.SystemFileAdapters;

namespace StlOrganizer.Library.Tests.Decompression;

public class DecompressionWorkflowTests
{
    private readonly IFileDecompressor fileDecompressor;
    private readonly IFolderFlattener folderFlattener;
    private readonly IFileOperations fileOperations;
    private readonly ILogger logger;
    private readonly DecompressionWorkflow workflow;

    public DecompressionWorkflowTests()
    {
        fileDecompressor = A.Fake<IFileDecompressor>();
        folderFlattener = A.Fake<IFolderFlattener>();
        fileOperations = A.Fake<IFileOperations>();
        logger = A.Fake<ILogger>();
        workflow = new DecompressionWorkflow(fileDecompressor, folderFlattener, fileOperations, logger);
    }

    [Fact]
    public async Task ExecuteAsync_CallsFileDecompressorFirst()
    {
        const string directoryPath = @"C:\TestDir";
        var extractedFiles = new List<string> { "file1.txt", "file2.txt" };
        var result = new DecompressionResult(extractedFiles, []);

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .Returns(result);

        await workflow.ExecuteAsync(directoryPath);

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_CallsFolderFlattenerAfterDecompression()
    {
        const string directoryPath = @"C:\TestDir";
        var result = new DecompressionResult(["file1.txt"], []);

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .Returns(result);

        await workflow.ExecuteAsync(directoryPath);

        A.CallTo(() => folderFlattener.RemoveNestedFolders(directoryPath))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_CallsOperationsInCorrectOrder()
    {
        const string directoryPath = @"C:\TestDir";
        var result = new DecompressionResult(["file1.txt"], []);
        var callOrder = new List<string>();

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .Invokes(() => callOrder.Add("decompress"))
            .Returns(result);

        A.CallTo(() => folderFlattener.RemoveNestedFolders(directoryPath))
            .Invokes(() => callOrder.Add("flatten"));

        await workflow.ExecuteAsync(directoryPath);

        callOrder.ShouldBe(new[] { "decompress", "flatten" });
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsExtractedFilesFromDecompressor()
    {
        const string directoryPath = @"C:\TestDir";
        var extractedFiles = new List<string> { "file1.txt", "file2.txt", "file3.txt" };
        var result = new DecompressionResult(extractedFiles, []);

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .Returns(result);

        var returnedFiles = await workflow.ExecuteAsync(directoryPath);

        returnedFiles.ShouldBe(extractedFiles);
    }

    [Fact]
    public async Task ExecuteAsync_PassesCorrectDirectoryPathToDecompressor()
    {
        const string directoryPath = @"C:\MyDirectory";
        var result = new DecompressionResult(["file1.txt"], []);

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .Returns(result);

        await workflow.ExecuteAsync(directoryPath);

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_PassesCorrectDirectoryPathToFlattener()
    {
        const string directoryPath = @"C:\MyDirectory";
        var result = new DecompressionResult(["file1.txt"], []);

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .Returns(result);

        await workflow.ExecuteAsync(directoryPath);

        A.CallTo(() => folderFlattener.RemoveNestedFolders(directoryPath))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoFilesExtracted_StillCallsFlattener()
    {
        const string directoryPath = @"C:\TestDir";
        var result = new DecompressionResult([], []);

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .Returns(result);

        var returnedFiles = await workflow.ExecuteAsync(directoryPath);

        A.CallTo(() => folderFlattener.RemoveNestedFolders(directoryPath))
            .MustHaveHappenedOnceExactly();
        returnedFiles.ShouldBeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_LogsStartOfWorkflow()
    {
        const string directoryPath = @"C:\TestDir";
        var result = new DecompressionResult(["file1.txt"], []);

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .Returns(result);

        await workflow.ExecuteAsync(directoryPath);

        A.CallTo(() => logger.Information(
                A<string>.That.Contains("Starting decompression workflow"),
                directoryPath))
            .MustHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_LogsDecompressionCompletion()
    {
        const string directoryPath = @"C:\TestDir";
        var result = new DecompressionResult(["file1.txt", "file2.txt"], []);

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .Returns(result);

        await workflow.ExecuteAsync(directoryPath);

        A.CallTo(() => logger.Information(
                A<string>.That.Contains("Decompressed"),
                2))
            .MustHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_LogsFlattenerCompletion()
    {
        const string directoryPath = @"C:\TestDir";
        var result = new DecompressionResult(["file1.txt"], []);

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .Returns(result);

        await workflow.ExecuteAsync(directoryPath);

        A.CallTo(() => logger.Information(
                A<string>.That.Contains("Completed folder flattening"),
                directoryPath))
            .MustHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WhenDecompressorThrows_DoesNotCallFlattener()
    {
        const string directoryPath = @"C:\TestDir";

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .Throws<DirectoryNotFoundException>();

        await Should.ThrowAsync<DirectoryNotFoundException>(
            async () => await workflow.ExecuteAsync(directoryPath));

        A.CallTo(() => folderFlattener.RemoveNestedFolders(A<string>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WhenDeleteOriginalFilesFalse_DoesNotDeleteFiles()
    {
        const string directoryPath = @"C:\TestDir";
        var compressedFiles = new List<string> { "archive.zip", "file.gz" };
        var result = new DecompressionResult(["file1.txt"], compressedFiles);

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .Returns(result);

        await workflow.ExecuteAsync(directoryPath, deleteOriginalFiles: false);

        A.CallTo(() => fileOperations.DeleteFile(A<string>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WhenDeleteOriginalFilesTrue_DeletesCompressedFiles()
    {
        const string directoryPath = @"C:\TestDir";
        const string compressedFile1 = "archive.zip";
        const string compressedFile2 = "file.gz";
        var compressedFiles = new List<string> { compressedFile1, compressedFile2 };
        var result = new DecompressionResult(["file1.txt"], compressedFiles);

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .Returns(result);

        await workflow.ExecuteAsync(directoryPath, deleteOriginalFiles: true);

        A.CallTo(() => fileOperations.DeleteFile(compressedFile1))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => fileOperations.DeleteFile(compressedFile2))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_WhenDeleteOriginalFilesTrue_LogsDeletedFiles()
    {
        const string directoryPath = @"C:\TestDir";
        const string compressedFile = "archive.zip";
        var result = new DecompressionResult(["file1.txt"], [compressedFile]);

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .Returns(result);

        await workflow.ExecuteAsync(directoryPath, deleteOriginalFiles: true);

        A.CallTo(() => logger.Information(
                A<string>.That.Contains("Deleted compressed file"),
                compressedFile))
            .MustHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WhenDeleteFileFails_LogsError()
    {
        const string directoryPath = @"C:\TestDir";
        const string compressedFile = "archive.zip";
        var result = new DecompressionResult(["file1.txt"], [compressedFile]);
        var exception = new IOException("File is locked");

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .Returns(result);
        A.CallTo(() => fileOperations.DeleteFile(compressedFile))
            .Throws(exception);

        await workflow.ExecuteAsync(directoryPath, deleteOriginalFiles: true);

        A.CallTo(() => logger.Error(
                exception,
                A<string>.That.Contains("Failed to delete compressed file"),
                compressedFile))
            .MustHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WhenDeleteFileFails_ContinuesProcessing()
    {
        const string directoryPath = @"C:\TestDir";
        const string failingFile = "archive1.zip";
        const string successFile = "archive2.zip";
        var result = new DecompressionResult(["file1.txt"], [failingFile, successFile]);

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .Returns(result);
        A.CallTo(() => fileOperations.DeleteFile(failingFile))
            .Throws<IOException>();

        await workflow.ExecuteAsync(directoryPath, deleteOriginalFiles: true);

        A.CallTo(() => fileOperations.DeleteFile(successFile))
            .MustHaveHappenedOnceExactly();
    }
}
