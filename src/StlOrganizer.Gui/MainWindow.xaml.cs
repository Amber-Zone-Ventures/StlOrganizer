using System;
using Microsoft.Extensions.DependencyInjection;

namespace StlOrganizer.Gui;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly IServiceProvider serviceProvider;

    public MainWindow(MainWindowViewModel viewModel, IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
        InitializeComponent();
        DataContext = viewModel;

        NavigationView.SetServiceProvider(serviceProvider);
    }
}
