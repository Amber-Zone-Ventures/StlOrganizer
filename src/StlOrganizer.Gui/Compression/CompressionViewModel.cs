using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using StlOrganizer.Gui.OperationSelection;
using StlOrganizer.Library.SystemAdapters.AsyncWork;

namespace StlOrganizer.Gui.Compression;

public partial class CompressionViewModel : ObservableValidator
{
    private readonly ICancellationTokenSourceProvider cancellationTokenSourceProvider;

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

    public CompressionViewModel(
        ICancellationTokenSourceProvider cancellationTokenSourceProvider)
    {
        this.cancellationTokenSourceProvider = cancellationTokenSourceProvider;
        SelectedOperation = ArchiveOperation.DecompressArchives;

        ValidateAllProperties();
        UpdateStatusMessageFromValidation();
    }

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
    private Task ExecuteOperationAsync()
    {
        ValidateAllProperties();
        UpdateStatusMessageFromValidation();

        if (HasErrors)
            return Task.CompletedTask;

        cancellationToken = cancellationTokenSourceProvider.Create();

        try
        {
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
        return Task.CompletedTask;
    }
}
