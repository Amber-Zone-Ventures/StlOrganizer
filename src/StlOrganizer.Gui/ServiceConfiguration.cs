using Microsoft.Extensions.DependencyInjection;
using Serilog;
using StlOrganizer.Gui.Compression;
using StlOrganizer.Gui.Home;
using StlOrganizer.Gui.Images;
using StlOrganizer.Library.Compression;
using StlOrganizer.Library.Decompression;
using StlOrganizer.Library.ImageProcessing;
using StlOrganizer.Library.OperationSelection;
using StlOrganizer.Library.SystemAdapters.AsyncWork;
using StlOrganizer.Library.SystemAdapters.FileSystem;

namespace StlOrganizer.Gui;

public static class ServiceConfiguration
{
    public static void ConfigureServices(ServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        services.AddSingleton(Log.Logger);
        services.AddSingleton<IFileSystem, FileSystemAdapter>();
        services.AddSingleton<IFileOperations, FileOperationsAdapter>();
        services.AddSingleton<IDirectoryService, DirectoryServiceAdapter>();
        services.AddSingleton<IDecompressor, Decompressor>();
        services.AddSingleton<IFolderScanner, FolderScanner>();
        services.AddSingleton<IFolderFlattener, FolderFlattener>();
        services.AddSingleton<IDecompressionWorkflow, DecompressionWorkflow>();
        services.AddSingleton<IImageOrganizer, ImageOrganizer>();
        services.AddSingleton<ICompressor, Compressor>();
        services.AddSingleton<IArchiveOperationSelector, ArchiveOperationSelector>();
        services.AddSingleton<ICancellationTokenSourceProvider, CancellationTokenSourceProvider>();

        // ViewModels
        services.AddTransient<CompressionViewModel>();
        services.AddTransient<MainWindowViewModel>();

        // Pages
        services.AddSingleton<CompressionPage>();
        services.AddSingleton<HomePage>();
        services.AddSingleton<ImagesPage>();

        // Windows
        services.AddTransient<MainWindow>();
    }
}
