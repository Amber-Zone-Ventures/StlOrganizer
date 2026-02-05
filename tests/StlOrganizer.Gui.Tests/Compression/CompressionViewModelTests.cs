using FakeItEasy;
using Shouldly;
using StlOrganizer.Gui.Compression;
using StlOrganizer.Gui.OperationSelection;
using StlOrganizer.Library.SystemAdapters.AsyncWork;

namespace StlOrganizer.Gui.Tests.Compression;

public class CompressionViewModelTests
{
    private readonly CompressionViewModel viewModel;

    public CompressionViewModelTests()
    {
        var cancellationTokenSourceProvider1 = A.Fake<ICancellationTokenSourceProvider>();
        A.CallTo(() => cancellationTokenSourceProvider1.Create()).ReturnsLazily(() => new CancellationTokenSource());
        viewModel = new CompressionViewModel(cancellationTokenSourceProvider1);
    }

    [Fact]
    public void Constructor_InitializesProperties()
    {
        viewModel.Title.ShouldBe("Stl Organizer");
        viewModel.TextFieldValue.ShouldBe(string.Empty);
        viewModel.SelectedDirectory.ShouldBe(string.Empty);
        viewModel.IsBusy.ShouldBeFalse();
        viewModel.StatusMessage.ShouldBe("Directory is required.");
    }

    [Fact]
    public void Constructor_InitializesAvailableOperations()
    {
        viewModel.AvailableOperations.ShouldNotBeNull();
        viewModel.AvailableOperations.ShouldContain(ArchiveOperation.DecompressArchives);
        viewModel.AvailableOperations.ShouldContain(ArchiveOperation.CompressFolder);
        viewModel.AvailableOperations.ShouldContain(ArchiveOperation.ExtractImages);
    }

    [Fact]
    public void Constructor_SetsDefaultSelectedOperation()
    {
        viewModel.SelectedOperation.ShouldBe(ArchiveOperation.DecompressArchives);
    }

    [Fact]
    public void ChangeTitle_UpdatesTitle()
    {
        viewModel.ChangeTitleCommand.Execute(null);

        viewModel.Title.ShouldBe("Stl Organizer - Updated");
    }

    [Fact]
    public async Task ExecuteOperationAsync_WithEmptyDirectory_SetsErrorMessage()
    {
        viewModel.SelectedDirectory = string.Empty;

        await viewModel.ExecuteOperationCommand.ExecuteAsync(null);

        viewModel.StatusMessage.ShouldBe("Directory is required.");
        viewModel.IsBusy.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteOperationAsync_WithWhitespaceDirectory_SetsErrorMessage()
    {
        viewModel.SelectedDirectory = "   ";

        await viewModel.ExecuteOperationCommand.ExecuteAsync(null);

        viewModel.StatusMessage.ShouldBe("Directory is required.");
        viewModel.IsBusy.ShouldBeFalse();
    }
}
