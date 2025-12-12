using MauiApp1.Services;
using MauiApp1.Views;
using MauiApp1.ViewModels;

namespace MauiApp1;

public partial class App : Application
{
    private readonly AuthService _authService;
    private readonly IServiceProvider _serviceProvider;

    public App(AuthService authService, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _authService = authService;
        _serviceProvider = serviceProvider;

        // Kiểm tra trạng thái đăng nhập khi mở App
        CheckLogin();
    }

    private async void CheckLogin()
    {
        // Kiểm tra xem có token trong máy không
        bool isLoggedIn = await _authService.CheckLoginStatusAsync();

        if (isLoggedIn)
        {
            // Đã đăng nhập -> Vào thẳng AppShell
            MainPage = new AppShell();
        }
        else
        {
            // Chưa đăng nhập -> Vào màn hình Login
            // Lấy LoginViewModel từ ServiceProvider để đảm bảo AuthService được tiêm vào đúng
            var loginVm = _serviceProvider.GetService<LoginViewModel>();
            MainPage = new NavigationPage(new LoginPage(loginVm));
        }
    }
}