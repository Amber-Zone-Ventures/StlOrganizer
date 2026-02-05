using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using StlOrganizer.Gui.OperationSelection;
using StlOrganizer.Library.Compression;
using StlOrganizer.Library.Decompression;
using StlOrganizer.Library.ImageProcessing;
using StlOrganizer.Library.SystemAdapters.AsyncWork;

namespace StlOrganizer.Gui;

public partial class MainWindowViewModel : ObservableValidator
{
    private readonly ICancellationTokenSourceProvider cancellationTokenSourceProvider;
    private readonly ICompressor compressor;
    private readonly IDecompressionWorkflow decompressionWorkflow;
    private readonly IImageOrganizer imageOrganizer;

    [ObservableProperty] private ObservableCollection<object> availableOperations =
    [
        ArchiveOperation.DecompressArchives,
        ArchiveOperation.CompressFolder,
        ArchiveOperation.ExtractImages
    ];

    private CancellationTokenSource? cancellationToken;

    [ObservableProperty] private bool isBusy;

    [ObservableProperty] private int progress;

    [ObservableProperty] [Required(ErrorMessage = "Directory is required.")]
    private string selectedDirectory = string.Empty;

    [ObservableProperty] private ArchiveOperation selectedOperation;

    [ObservableProperty] private string statusMessage = string.Empty;

    [ObservableProperty] private string textFieldValue = string.Empty;

    [ObservableProperty] private string title = "Stl Organizer";

    public MainWindowViewModel(
        IDecompressionWorkflow decompressionWorkflow,
        ICompressor compressor,
        IImageOrganizer imageOrganizer,
        ICancellationTokenSourceProvider cancellationTokenSourceProvider)
    {
        this.decompressionWorkflow = decompressionWorkflow;
        this.compressor = compressor;
        this.imageOrganizer = imageOrganizer;
        this.cancellationTokenSourceProvider = cancellationTokenSourceProvider;
        SelectedOperation = ArchiveOperation.DecompressArchives;

        ValidateAllProperties();
        UpdateStatusMessageFromValidation();
    }

    public Dictionary<ArchiveOperation, Func<Task>> OperationMap => new()
    {
        { ArchiveOperation.CompressFolder, CompressFolder },
        { ArchiveOperation.DecompressArchives, DecompressFolder },
        { ArchiveOperation.ExtractImages, ExtractImages }
    };

    [RelayCommand]
    private void ChangeTitle()
    {
        Title = "Stl Organizer - Updated";
    }

    [RelayCommand]
    private void SelectDirectory()
    {
        var dialog = new OpenFolderDialog();

        if (dialog.ShowDialog() == true) SelectedDirectory = dialog.FolderName;
    }

    partial void OnSelectedDirectoryChanged(string value)
    {
        ValidateProperty(value, nameof(SelectedDirectory));
        UpdateStatusMessageFromValidation();
    }

    private void UpdateStatusMessageFromValidation()
    {
        if (HasErrors)
        {
            var error = GetErrors(nameof(SelectedDirectory)).FirstOrDefault();
            StatusMessage = error?.ErrorMessage ?? "Validation error.";
        }
        else if (StatusMessage == "Directory is required." || string.IsNullOrWhiteSpace(StatusMessage) ||
                 StatusMessage == "Please select a directory first.")
        {
            StatusMessage = "Ready";
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        cancellationToken?.Cancel();
    }

    [RelayCommand]
    private async Task ExecuteOperationAsync()
    {
        ValidateAllProperties();
        UpdateStatusMessageFromValidation();

        if (HasErrors)
            return;

        cancellationToken = cancellationTokenSourceProvider.Create();

        try
        {
            IsBusy = true;
            StatusMessage = $"Executing {SelectedOperation.Name}...";

            await OperationMap.First(o => SelectedOperation == o.Key)
                .Value();

            StatusMessage = "Operation completed successfully.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Operation canceled.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            cancellationToken.Dispose();
            cancellationToken = null;
        }
    }

    private async Task ExtractImages()
    {
        await imageOrganizer.OrganizeImagesAsync(
            SelectedDirectory,
            cancellationToken!.Token);
    }

    private async Task DecompressFolder()
    {
        await decompressionWorkflow.Execute(
            SelectedDirectory,
            new Progress<DecompressionProgress>(o =>
            {
                Progress = o.Progress;
                StatusMessage = $"Decompressing {o.Message}";
            }),
            cancellationToken!.Token);
    }

    private async Task CompressFolder()
    {
        await compressor.Compress(
            SelectedDirectory,
            SelectedDirectory + ".zip",
            new Progress<CompressProgress>(o =>
            {
                Progress = o.Percent;
                StatusMessage = $"Compressing {o.LastFile}";
            }),
            cancellationToken!.Token);
    }
}
