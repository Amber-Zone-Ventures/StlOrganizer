using FakeItEasy;
using Shouldly;
using StlOrganizer.Gui.OperationSelection;
using StlOrganizer.Library.Compression;
using StlOrganizer.Library.Decompression;
using StlOrganizer.Library.ImageProcessing;
using StlOrganizer.Library.SystemAdapters.AsyncWork;

namespace StlOrganizer.Gui.Tests;

public class MainWindowViewModelTests
{
    [Fact]
    public void OperationMap_HasOperations()
    {
        var sut = CreateSut();

        sut.OperationMap.ShouldNotBeNull();
    }

    [Fact]
    public void IsBusy_NothingIsHappening_IsFalse()
    {
        var sut = CreateSut();
        
        sut.IsBusy.ShouldBeFalse();
    }

    [Fact]
    public void Ctor_Defaults_AreSet()
    {
        var sut = CreateSut();

        sut.SelectedOperation.ShouldBe(ArchiveOperation.DecompressArchives);
        sut.Title.ShouldBe("Stl Organizer");
        sut.StatusMessage.ShouldBe("Directory is required.");
        sut.IsBusy.ShouldBeFalse();
    }

    [Fact]
    public void ChangeTitleCommand_Updates_Title()
    {
        var sut = CreateSut();

        sut.ChangeTitleCommand.Execute(null);

        sut.Title.ShouldBe("Stl Organizer - Updated");
    }

    [Fact]
    public void SelectedDirectory_WhenSet_ClearsValidation_AndSetsReady()
    {
        var sut = CreateSut();

        sut.SelectedDirectory = "C:\\temp";

        sut.StatusMessage.ShouldBe("Ready");
        sut.HasErrors.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteOperationAsync_ExtractImages_CallsOrganizer_AndSucceeds()
    {
        var imageOrganizer = A.Fake<IImageOrganizer>();
        var sut = CreateSut(imageOrganizer: imageOrganizer);
        sut.SelectedDirectory = "C:\\data";
        sut.SelectedOperation = ArchiveOperation.ExtractImages;

        await sut.ExecuteOperationCommand.ExecuteAsync(null);

        A.CallTo(() => imageOrganizer.OrganizeImagesAsync(
                "C:\\data",
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        sut.IsBusy.ShouldBeFalse();
        sut.StatusMessage.ShouldBe("Operation completed successfully.");
    }

    [Fact]
    public async Task ExecuteOperationAsync_CompressFolder_CallsCompressor_AndSucceeds()
    {
        var compressor = A.Fake<ICompressor>();
        var sut = CreateSut(compressor: compressor);
        sut.SelectedDirectory = "C:\\archive";
        sut.SelectedOperation = ArchiveOperation.CompressFolder;

        await sut.ExecuteOperationCommand.ExecuteAsync(null);

        A.CallTo(() => compressor.Compress(
                "C:\\archive",
                "C:\\archive.zip",
                A<IProgress<CompressProgress>>._,
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        sut.IsBusy.ShouldBeFalse();
        sut.StatusMessage.ShouldBe("Operation completed successfully.");
    }

    [Fact]
    public async Task ExecuteOperationAsync_DecompressFolder_CallsWorkflow_AndSucceeds()
    {
        var workflow = A.Fake<IDecompressionWorkflow>();
        var sut = CreateSut(decompressionWorkflow: workflow);
        sut.SelectedDirectory = "C:\\incoming";
        sut.SelectedOperation = ArchiveOperation.DecompressArchives;

        await sut.ExecuteOperationCommand.ExecuteAsync(null);

        A.CallTo(() => workflow.Execute(
                "C:\\incoming",
                A<IProgress<DecompressionProgress>>._,
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        sut.IsBusy.ShouldBeFalse();
        sut.StatusMessage.ShouldBe("Operation completed successfully.");
    }

    [Fact]
    public async Task CancelCommand_WhenOperationObservesCancellation_SetsCanceledMessage()
    {
        var workflow = A.Fake<IDecompressionWorkflow>();
        A.CallTo(() => workflow.Execute(
                A<string>._,
                A<IProgress<DecompressionProgress>>._,
                A<CancellationToken>._))
            .ReturnsLazily((string _, IProgress<DecompressionProgress> _, CancellationToken ct) =>
                Task.Delay(Timeout.Infinite, ct));
        var ctsProvider = new DefaultCtsProvider();
        var sut = CreateSut(decompressionWorkflow: workflow, ctsProvider: ctsProvider);
        sut.SelectedDirectory = "C:\\incoming";
        sut.SelectedOperation = ArchiveOperation.DecompressArchives;

        var executeTask = sut.ExecuteOperationCommand.ExecuteAsync(null);

        // Give the command a brief moment to start and acquire the CTS
        await Task.Delay(10);
        sut.CancelCommand.Execute(null);

        await executeTask;

        sut.IsBusy.ShouldBeFalse();
        sut.StatusMessage.ShouldBe("Operation canceled.");
    }

    private static MainWindowViewModel CreateSut(
        IDecompressionWorkflow? decompressionWorkflow = null,
        ICompressor? compressor = null,
        IImageOrganizer? imageOrganizer = null,
        ICancellationTokenSourceProvider? ctsProvider = null)
    {
        decompressionWorkflow ??= A.Fake<IDecompressionWorkflow>();
        compressor ??= A.Fake<ICompressor>();
        imageOrganizer ??= A.Fake<IImageOrganizer>();
        ctsProvider ??= new DefaultCtsProvider();

        return new MainWindowViewModel(
            decompressionWorkflow,
            compressor,
            imageOrganizer,
            ctsProvider);
    }

    private sealed class DefaultCtsProvider : ICancellationTokenSourceProvider
    {
        public CancellationTokenSource Create() => new();
    }
}
    