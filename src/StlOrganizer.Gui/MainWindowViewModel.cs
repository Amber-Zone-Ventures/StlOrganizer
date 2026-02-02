using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using StlOrganizer.Gui.Compression;
using StlOrganizer.Gui.Home;
using StlOrganizer.Gui.Images;
using Wpf.Ui.Controls;

namespace StlOrganizer.Gui;

public partial class MainWindowViewModel(IServiceProvider serviceProvider) : ObservableValidator
{
    private readonly IServiceProvider serviceProvider = serviceProvider;

    [ObservableProperty] private ObservableCollection<object> menuItems =
    [
        new NavigationViewItem("Home", SymbolRegular.Home24, typeof(HomePage)),
        new NavigationViewItem("Compression", SymbolRegular.Archive24, typeof(CompressionPage)),
        new NavigationViewItem("Images", SymbolRegular.Image24, typeof(ImagesPage))
    ];
}
