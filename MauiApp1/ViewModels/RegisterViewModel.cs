using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1.Services;

namespace MauiApp1.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    private readonly AuthService _authService;

    public RegisterViewModel(AuthService authService)
    {
        _authService = authService;
    }

    [ObservableProperty] string email;
    [ObservableProperty] string password;
    [ObservableProperty] string confirmPassword; // Trường nhập lại mật khẩu
    [ObservableProperty] bool isLoading;

    [RelayCommand]
    async Task Register()
    {
        // 1. Kiểm tra nhập liệu
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            await Application.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng nhập đủ thông tin", "OK");
            return;
        }

        if (Password != ConfirmPassword)
        {
            await Application.Current.MainPage.DisplayAlert("Lỗi", "Mật khẩu nhập lại không khớp", "OK");
            return;
        }

        IsLoading = true;

        // 2. Gọi Service đăng ký
        var error = await _authService.RegisterAsync(Email, Password);

        IsLoading = false;

        if (string.IsNullOrEmpty(error))
        {
            // Thành công -> Thông báo và quay lại trang đăng nhập
            await Application.Current.MainPage.DisplayAlert("Thành công", "Tạo tài khoản thành công! Vui lòng đăng nhập.", "OK");

            // Quay lại trang trước (Login)
            await Application.Current.MainPage.Navigation.PopAsync();
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert("Thất bại", error, "OK");
        }
    }

    [RelayCommand]
    async Task GoBack()
    {
        // Quay lại trang Login nếu bấm nút "Hủy" hoặc "Quay lại"
        await Application.Current.MainPage.Navigation.PopAsync();
    }
}