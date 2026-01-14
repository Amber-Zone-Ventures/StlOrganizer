using FakeItEasy;
using Shouldly;
using StlOrganizer.Gui.ViewModels;
using StlOrganizer.Library;
using StlOrganizer.Library.SystemAdapters;
using StlOrganizer.Library.SystemAdapters.AsyncWork;

namespace StlOrganizer.Gui.Tests.ViewModels;

public class MainViewModelTests
{
    private readonly ICancellationTokenSourceProvider cancellationTokenSourceProvider;
    private readonly IArchiveOperationSelector archiveOperationSelector;
    private readonly MainViewModel viewModel;

    public MainViewModelTests()
    {
        archiveOperationSelector = A.Fake<IArchiveOperationSelector>();
        cancellationTokenSourceProvider = A.Fake<ICancellationTokenSourceProvider>();
        A.CallTo(() => cancellationTokenSourceProvider.Create()).ReturnsLazily(() => new CancellationTokenSource());
        viewModel = new MainViewModel(archiveOperationSelector, cancellationTokenSourceProvider);
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
        viewModel.AvailableOperations.Count.ShouldBe(3);
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

    [Fact]
    public async Task ExecuteOperationAsync_WithValidDirectory_CallsOperationSelector()
    {
        const string directory = @"C:\TestDir";
        const string expectedResult = "Operation completed successfully";
        viewModel.SelectedDirectory = directory;
        viewModel.SelectedOperation = ArchiveOperation.ExtractImages;

        A.CallTo(() =>
                archiveOperationSelector.ExecuteOperationAsync(ArchiveOperation.ExtractImages, directory, A<CancellationToken>._))
            .Returns(Task.FromResult(expectedResult));

        await viewModel.ExecuteOperationCommand.ExecuteAsync(null);

        A.CallTo(() =>
                archiveOperationSelector.ExecuteOperationAsync(ArchiveOperation.ExtractImages, directory, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        viewModel.StatusMessage.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task ExecuteOperationAsync_SetsIsExecutingDuringOperation()
    {
        const string directory = @"C:\TestDir";
        viewModel.SelectedDirectory = directory;
        var executionStarted = false;

        A.CallTo(() => archiveOperationSelector.ExecuteOperationAsync(A<ArchiveOperation>._, A<string>._, A<CancellationToken>._))
            .ReturnsLazily(() =>
            {
                executionStarted = viewModel.IsBusy;
                return Task.FromResult("Success");
            });

        await viewModel.ExecuteOperationCommand.ExecuteAsync(null);

        executionStarted.ShouldBeTrue();
    }

    [Fact]
    public async Task ExecuteOperationAsync_UsesCancellationTokenSourceFromProvider()
    {
        const string directory = @"C:\TestDir";
        viewModel.SelectedDirectory = directory;
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        A.CallTo(() => cancellationTokenSourceProvider.Create()).Returns(cts);

        await viewModel.ExecuteOperationCommand.ExecuteAsync(null);

        A.CallTo(() => cancellationTokenSourceProvider.Create()).MustHaveHappenedOnceExactly();
        A.CallTo(() => archiveOperationSelector.ExecuteOperationAsync(A<ArchiveOperation>._, A<string>._, token))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteOperationAsync_SetsStatusMessageDuringOperation()
    {
        const string directory = @"C:\TestDir";
        viewModel.SelectedDirectory = directory;
        viewModel.SelectedOperation = ArchiveOperation.CompressFolder;

        A.CallTo(() => archiveOperationSelector.ExecuteOperationAsync(A<ArchiveOperation>._, A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult("Done"));

        await viewModel.ExecuteOperationCommand.ExecuteAsync(null);

        viewModel.StatusMessage.ShouldBe("Done");
    }

    [Fact]
    public async Task ExecuteOperationAsync_WithException_SetsErrorMessage()
    {
        const string directory = @"C:\TestDir";
        const string exceptionMessage = "Test exception";
        viewModel.SelectedDirectory = directory;

        A.CallTo(() => archiveOperationSelector.ExecuteOperationAsync(A<ArchiveOperation>._, A<string>._, A<CancellationToken>._))
            .Throws(new InvalidOperationException(exceptionMessage));

        await viewModel.ExecuteOperationCommand.ExecuteAsync(null);

        viewModel.StatusMessage.ShouldBe($"Error: {exceptionMessage}");
        viewModel.IsBusy.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteOperationAsync_WithException_ResetsIsExecuting()
    {
        const string directory = @"C:\TestDir";
        viewModel.SelectedDirectory = directory;

        A.CallTo(() => archiveOperationSelector.ExecuteOperationAsync(A<ArchiveOperation>._, A<string>._, A<CancellationToken>._))
            .Throws(new Exception("Test error"));

        await viewModel.ExecuteOperationCommand.ExecuteAsync(null);

        viewModel.IsBusy.ShouldBeFalse();
    }

    [Fact]
    public void SelectedOperation_CanBeChanged()
    {
        viewModel.SelectedOperation = ArchiveOperation.CompressFolder;
        viewModel.SelectedOperation.ShouldBe(ArchiveOperation.CompressFolder);

        viewModel.SelectedOperation = ArchiveOperation.ExtractImages;
        viewModel.SelectedOperation.ShouldBe(ArchiveOperation.ExtractImages);
    }

    [Fact]
    public void SelectedDirectory_CanBeChanged()
    {
        const string directory = @"C:\TestDirectory";

        viewModel.SelectedDirectory = directory;

        viewModel.SelectedDirectory.ShouldBe(directory);
    }

    [Fact]
    public void TextFieldValue_CanBeChanged()
    {
        const string value = "Test Value";

        viewModel.TextFieldValue = value;

        viewModel.TextFieldValue.ShouldBe(value);
    }

    [Fact]
    public async Task ExecuteOperationAsync_WithDifferentOperationTypes_PassesCorrectType()
    {
        const string directory = @"C:\TestDir";
        viewModel.SelectedDirectory = directory;

        // Test FileDecompressor
        viewModel.SelectedOperation = ArchiveOperation.DecompressArchives;
        await viewModel.ExecuteOperationCommand.ExecuteAsync(null);
        A.CallTo(() =>
                archiveOperationSelector.ExecuteOperationAsync(ArchiveOperation.DecompressArchives, directory,
                    A<CancellationToken>._))
            .MustHaveHappened();

        // Test FolderCompressor
        viewModel.SelectedOperation = ArchiveOperation.CompressFolder;
        await viewModel.ExecuteOperationCommand.ExecuteAsync(null);
        A.CallTo(() =>
                archiveOperationSelector.ExecuteOperationAsync(ArchiveOperation.CompressFolder, directory,
                    A<CancellationToken>._))
            .MustHaveHappened();

        // Test ImageOrganizer
        viewModel.SelectedOperation = ArchiveOperation.ExtractImages;
        await viewModel.ExecuteOperationCommand.ExecuteAsync(null);
        A.CallTo(() =>
                archiveOperationSelector.ExecuteOperationAsync(ArchiveOperation.ExtractImages, directory, A<CancellationToken>._))
            .MustHaveHappened();
    }
}