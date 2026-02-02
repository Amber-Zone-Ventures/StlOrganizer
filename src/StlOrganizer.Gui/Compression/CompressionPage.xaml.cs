namespace StlOrganizer.Gui.Compression;

public partial class CompressionPage
{
    public CompressionPage(CompressionViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
