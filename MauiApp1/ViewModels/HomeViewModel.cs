using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1.Services;
using MauiApp1.Views;

namespace MauiApp1.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly AuthService _authService;

    public HomeViewModel(AuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    async Task Logout()
    {
        bool confirm = await Shell.Current.DisplayAlert("Đăng xuất", "Bạn có chắc chắn muốn đăng xuất?", "Có", "Không");
        if (confirm)
        {
            _authService.Logout();
            // Quay về màn hình đăng nhập
            Application.Current.MainPage = new NavigationPage(new LoginPage(new LoginViewModel(_authService)));
        }
    }
}