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

    // --- TRẠNG THÁI ---
    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                // Khi IsBusy thay đổi, lệnh SaveProfileCommand sẽ được kiểm tra lại xem có được bấm hay không
                SaveProfileCommand.NotifyCanExecuteChanged();
            }
        }
    }

    // --- DỮ LIỆU ---
    private string _fullName;
    public string FullName
    {
        get => _fullName;
        set => SetProperty(ref _fullName, value);
    }

    private string _userEmail;
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

    private string _userTimeZone;
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

        DateOfBirth = DateTime.Now;
        UserTimeZone = "Asia/Ho_Chi_Minh";

        LoadUserProfile();
    }

    private async void LoadUserProfile()
    {
        if (IsBusy) return;
        IsBusy = true; // Bật trạng thái tải

        try
        {
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
                    _base64ImageString = profile.AvatarBase64;
                    byte[] imageBytes = Convert.FromBase64String(profile.AvatarBase64);
                    AvatarSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                }
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Lỗi", "Không tải được hồ sơ: " + ex.Message, "OK");
        }
        finally
        {
            IsBusy = false; // Tắt trạng thái tải
        }
    }

    [RelayCommand]
    async Task PickImage()
    {
        try
        {
            var result = await MediaPicker.Default.PickPhotoAsync();
            if (result != null)
            {
                // Đọc luồng để hiển thị
                using var stream1 = await result.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream1.CopyToAsync(memoryStream);

                byte[] imageBytes = memoryStream.ToArray();

                // 1. Hiển thị lên UI
                AvatarSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));

                // 2. Lưu chuỗi Base64 để chuẩn bị upload
                _base64ImageString = Convert.ToBase64String(imageBytes);
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Lỗi ảnh", ex.Message, "OK");
        }
    }

    // Kiểm tra: Chỉ cho phép bấm Lưu khi KHÔNG Busy
    [RelayCommand(CanExecute = nameof(CanSave))]
    async Task SaveProfile()
    {
        if (IsBusy || string.IsNullOrEmpty(_currentUserId)) return;
        IsBusy = true;

        try
        {
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
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Không thể lưu dữ liệu vào hệ thống.", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Lỗi", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Điều kiện để nút Lưu sáng lên: Không Busy
    private bool CanSave() => !IsBusy;

    [RelayCommand]
    async Task ChangePassword()
    {
        string newPass = await Application.Current.MainPage.DisplayPromptAsync(
            "Đổi Mật Khẩu", "Nhập mật khẩu mới:", "Đổi", "Hủy", placeholder: "Min 6 ký tự");

        if (!string.IsNullOrWhiteSpace(newPass))
        {
            IsBusy = true;
            var error = await _authService.ChangePasswordAsync(newPass);
            IsBusy = false;

            if (string.IsNullOrEmpty(error))
                await Application.Current.MainPage.DisplayAlert("Thành công", "Mật khẩu đã đổi.", "OK");
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