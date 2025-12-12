using MauiApp1.ViewModels;
namespace MauiApp1.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage(ProfileViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}