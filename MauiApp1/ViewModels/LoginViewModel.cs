using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1.Services;
using MauiApp1.Views;

namespace MauiApp1.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly AuthService _authService;

    public LoginViewModel(AuthService authService)
    {
        _authService = authService;
    }

    [ObservableProperty]
    private string email;

    [ObservableProperty]
    private string password;

    [ObservableProperty]
    private bool isLoading;

    [RelayCommand]
    async Task Login()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            // ✅ SỬA: Dùng Application.Current.MainPage để tránh lỗi Null nếu chưa có Shell
            await Application.Current.MainPage.DisplayAlert("Thông báo", "Vui lòng nhập đủ thông tin", "OK");
            return;
        }

        IsLoading = true;

        // Gọi hàm đăng nhập
        var error = await _authService.LoginAsync(Email, Password);

        IsLoading = false;

        if (string.IsNullOrEmpty(error))
        {
            // 1. Hiện thông báo chúc mừng
            await Application.Current.MainPage.DisplayAlert("Tuyệt vời", "Đăng nhập thành công rồi!", "OK");

            // 2. Sau đó mới chuyển trang
            Application.Current.MainPage = new AppShell();
        }
    }
    [RelayCommand]
    async Task GoToRegister()
    {
        // Cách 1: Giải pháp nhanh nhất (Service Locator)
        // Lấy trang RegisterPage đã được cấu hình sẵn từ hệ thống
        var registerPage = Application.Current.Handler.MauiContext.Services.GetService<Views.RegisterPage>();
        await Application.Current.MainPage.Navigation.PushAsync(registerPage);
    }
    [RelayCommand]
    async Task ForgotPassword()
    {
        // Hiện hộp thoại hỏi Email
        string emailInput = await Application.Current.MainPage.DisplayPromptAsync(
            "Quên Mật Khẩu",
            "Nhập email của bạn để nhận link đặt lại mật khẩu:",
            "Gửi",
            "Hủy",
            keyboard: Keyboard.Email);

        // Nếu người dùng có nhập và bấm Gửi
        if (!string.IsNullOrWhiteSpace(emailInput))
        {
            IsLoading = true;
            var error = await _authService.ResetPasswordAsync(emailInput);
            IsLoading = false;

            if (string.IsNullOrEmpty(error))
            {
                await Application.Current.MainPage.DisplayAlert("Đã Gửi", "Vui lòng kiểm tra email (cả hộp thư rác) để đặt lại mật khẩu.", "OK");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", error, "OK");
            }
        }
    }
}