using MauiApp1.ViewModels;

namespace MauiApp1.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm; // Kết nối giao diện với logic
    }
}