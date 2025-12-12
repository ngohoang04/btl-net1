using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1.Services;
using MauiApp1.Views;
using MauiApp1.Models;
using System.IO;

namespace MauiApp1.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly UserService _userService;

    // --- KHAI BÁO THỦ CÔNG (Đảm bảo không lỗi) ---

    private string _fullName;
    public string FullName
    {
        get => _fullName;
        set => SetProperty(ref _fullName, value);
    }

    private string _userEmail; // Đặt tên khác biến Email của hệ thống
    public string UserEmail
    {
        get => _userEmail;
        set => SetProperty(ref _userEmail, value);
    }

    private DateTime _dateOfBirth;
    public DateTime DateOfBirth
    {
        get => _dateOfBirth;
        set => SetProperty(ref _dateOfBirth, value);
    }

    private string _userTimeZone; // Đặt tên khác biến TimeZone của hệ thống
    public string UserTimeZone
    {
        get => _userTimeZone;
        set => SetProperty(ref _userTimeZone, value);
    }

    private ImageSource _avatarSource = "dotnet_bot.png";
    public ImageSource AvatarSource
    {
        get => _avatarSource;
        set => SetProperty(ref _avatarSource, value);
    }

    private string _currentUserId;
    private string _base64ImageString;

    public ProfileViewModel(AuthService authService, UserService userService)
    {
        _authService = authService;
        _userService = userService;

        // Khởi tạo giá trị mặc định
        DateOfBirth = DateTime.Now;
        UserTimeZone = "Asia/Ho_Chi_Minh";

        LoadUserProfile();
    }

    private async void LoadUserProfile()
    {
        var token = await SecureStorage.Default.GetAsync("auth_token");
        var rawEmail = await SecureStorage.Default.GetAsync("user_email");

        if (string.IsNullOrEmpty(rawEmail)) return;

        UserEmail = rawEmail;
        _currentUserId = rawEmail.Replace(".", "_").Replace("@", "_");

        var profile = await _userService.GetUserProfileAsync(_currentUserId);
        if (profile != null)
        {
            FullName = profile.FullName;
            DateOfBirth = profile.DateOfBirth;
            UserTimeZone = profile.TimeZone;

            if (!string.IsNullOrEmpty(profile.AvatarBase64))
            {
                byte[] imageBytes = Convert.FromBase64String(profile.AvatarBase64);
                AvatarSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                _base64ImageString = profile.AvatarBase64;
            }
        }
    }

    [RelayCommand]
    async Task PickImage()
    {
        var result = await MediaPicker.Default.PickPhotoAsync();
        if (result != null)
        {
            var stream = await result.OpenReadAsync();
            AvatarSource = ImageSource.FromStream(() => stream);

            using var memoryStream = new MemoryStream();
            var newStream = await result.OpenReadAsync();
            await newStream.CopyToAsync(memoryStream);
            byte[] imageBytes = memoryStream.ToArray();
            _base64ImageString = Convert.ToBase64String(imageBytes);
        }
    }

    [RelayCommand]
    async Task SaveProfile()
    {
        if (string.IsNullOrEmpty(_currentUserId)) return;

        var userProfile = new UserProfile
        {
            UserId = _currentUserId,
            Email = UserEmail,
            FullName = FullName,
            DateOfBirth = DateOfBirth,
            TimeZone = UserTimeZone,
            AvatarBase64 = _base64ImageString
        };

        var success = await _userService.SaveUserProfileAsync(userProfile);
        if (success)
            await Application.Current.MainPage.DisplayAlert("Thành công", "Đã cập nhật hồ sơ!", "OK");
        else
            await Application.Current.MainPage.DisplayAlert("Lỗi", "Không thể lưu dữ liệu.", "OK");
    }

    [RelayCommand]
    async Task ChangePassword()
    {
        string newPass = await Application.Current.MainPage.DisplayPromptAsync(
            "Đổi Mật Khẩu",
            "Nhập mật khẩu mới:",
            "Đổi",
            "Hủy",
            placeholder: "Mật khẩu mới (tối thiểu 6 ký tự)");

        if (!string.IsNullOrWhiteSpace(newPass))
        {
            var error = await _authService.ChangePasswordAsync(newPass);

            if (string.IsNullOrEmpty(error))
                await Application.Current.MainPage.DisplayAlert("Thành công", "Mật khẩu đã được cập nhật.", "OK");
            else
                await Application.Current.MainPage.DisplayAlert("Lỗi", error, "OK");
        }
    }

    [RelayCommand]
    async Task Logout()
    {
        bool confirm = await Application.Current.MainPage.DisplayAlert("Đăng xuất", "Bạn có chắc chắn muốn đăng xuất?", "Có", "Không");
        if (confirm)
        {
            _authService.Logout();
            Application.Current.MainPage = new NavigationPage(new LoginPage(new LoginViewModel(_authService)));
        }
    }
}