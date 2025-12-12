using MauiApp1.ViewModels;

namespace MauiApp1.Views;

public partial class HomePage : ContentPage
{
    public HomePage(HomeViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}