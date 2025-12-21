using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1.Models;
using MauiApp1.Services;
using Plugin.LocalNotification; // <-- Nhớ cài thư viện này và thêm dòng này

namespace MauiApp1.ViewModels;

[QueryProperty(nameof(EventToEdit), "EventData")]
public partial class AddEventViewModel : ObservableObject
{
    private readonly CalendarService _calendarService;

    [ObservableProperty]
    CalendarEvent eventToEdit;

    // Các thuộc tính binding
    [ObservableProperty] string title;
    [ObservableProperty] string description;
    [ObservableProperty] string location;
    [ObservableProperty] DateTime startDate = DateTime.Now;
    [ObservableProperty] TimeSpan startTime = DateTime.Now.TimeOfDay;
    [ObservableProperty] DateTime endDate = DateTime.Now;
    [ObservableProperty] TimeSpan endTime = DateTime.Now.AddHours(1).TimeOfDay;
    [ObservableProperty] string selectedCategory;

    // --- MỚI: Binding cho Picker nhắc nhở ---
    [ObservableProperty] string selectedReminder = "Không nhắc";

    private bool _isEditMode = false;
    private string _currentEventId = null;

    public List<string> Categories { get; } = new() { "Công việc", "Gia đình", "Sức khỏe", "Hẹn hò", "Khác" };

    // --- MỚI: Danh sách tùy chọn nhắc nhở cho View binding (nếu cần) ---
    public List<string> ReminderOptions { get; } = new() { "Không nhắc", "Trước 15 phút", "Trước 30 phút", "Trước 1 giờ", "Trước 1 ngày" };

    public AddEventViewModel(CalendarService calendarService)
    {
        _calendarService = calendarService;
    }

    // --- MỚI: Hàm chuyển đổi ---
    private int GetReminderMinutes(string selection)
    {
        return selection switch
        {
            "Trước 15 phút" => 15,
            "Trước 30 phút" => 30,
            "Trước 1 giờ" => 60,
            "Trước 1 ngày" => 1440,
            _ => 0
        };
    }

    private string GetSelectionFromMinutes(int minutes)
    {
        return minutes switch
        {
            15 => "Trước 15 phút",
            30 => "Trước 30 phút",
            60 => "Trước 1 giờ",
            1440 => "Trước 1 ngày",
            _ => "Không nhắc"
        };
    }

    partial void OnEventToEditChanged(CalendarEvent value)
    {
        if (value != null)
        {
            _isEditMode = true;
            _currentEventId = value.Id;

            Title = value.Title;
            Description = value.Description;
            Location = value.Location;
            SelectedCategory = value.Category;

            StartDate = value.StartTime.Date;
            StartTime = value.StartTime.TimeOfDay;
            EndDate = value.EndTime.Date;
            EndTime = value.EndTime.TimeOfDay;

            // --- MỚI: Hiển thị lại mức nhắc nhở cũ ---
            // (Bạn cần thêm thuộc tính ReminderMinutes vào Model CalendarEvent trước nhé)
            SelectedReminder = GetSelectionFromMinutes(value.ReminderMinutes);
        }
    }

    [RelayCommand]
    async Task SaveEvent()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            await Application.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng nhập tiêu đề", "OK");
            return;
        }

        // --- MỚI: Kiểm tra quyền thông báo (bắt buộc cho Android 13+) ---
        if (await LocalNotificationCenter.Current.AreNotificationsEnabled() == false)
        {
            await LocalNotificationCenter.Current.RequestNotificationPermission();
        }

        var startDateTime = StartDate.Date + StartTime;
        var endDateTime = EndDate.Date + EndTime;

        if (endDateTime < startDateTime)
        {
            await Application.Current.MainPage.DisplayAlert("Lỗi", "Thời gian kết thúc phải sau thời gian bắt đầu", "OK");
            return;
        }

        var userId = await SecureStorage.Default.GetAsync("user_email");
        userId = userId?.Replace(".", "_").Replace("@", "_");

        string colorHex = SelectedCategory switch
        {
            "Công việc" => "#FF5733",
            "Gia đình" => "#33FF57",
            "Sức khỏe" => "#3357FF",
            _ => "#808080"
        };

        // --- MỚI: Xử lý ID Thông báo ---
        // Nếu là sửa và đã có ID thông báo thì dùng lại, nếu không thì tạo random
        int notiId = (_isEditMode && eventToEdit.NotificationId != 0)
                     ? eventToEdit.NotificationId
                     : new Random().Next(100000, 999999);

        int remMinutes = GetReminderMinutes(SelectedReminder);

        var eventToSave = new CalendarEvent
        {
            UserId = userId,
            Title = Title,
            Description = Description,
            Location = Location,
            StartTime = startDateTime,
            EndTime = endDateTime,
            Category = SelectedCategory ?? "Khác",
            Color = colorHex,
            Id = _isEditMode ? _currentEventId : null,

            // --- MỚI: Lưu thông tin nhắc nhở vào Database ---
            ReminderMinutes = remMinutes,
            NotificationId = notiId
        };

        bool success;
        if (_isEditMode)
            success = await _calendarService.UpdateEventAsync(eventToSave);
        else
            success = await _calendarService.AddEventAsync(eventToSave);

        if (success)
        {
            // --- MỚI: LÊN LỊCH THÔNG BÁO ---
            try
            {
                // 1. Hủy thông báo cũ (để tránh trùng lặp hoặc nếu người dùng chọn "Không nhắc")
                LocalNotificationCenter.Current.Cancel(notiId);

                // 2. Nếu có chọn nhắc nhở
                if (remMinutes > 0)
                {
                    var notifyTime = startDateTime.AddMinutes(-remMinutes);

                    // Chỉ báo nếu thời gian đó chưa trôi qua
                    if (notifyTime > DateTime.Now)
                    {
                        var request = new NotificationRequest
                        {
                            NotificationId = notiId,
                            Title = $"Sắp diễn ra: {Title}",
                            Description = $"{Title} bắt đầu lúc {startDateTime:HH:mm}",
                            Schedule = new NotificationRequestSchedule
                            {
                                NotifyTime = notifyTime
                            }
                        };
                        await LocalNotificationCenter.Current.Show(request);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi tạo thông báo: " + ex.Message);
            }

            await Application.Current.MainPage.DisplayAlert("Thành công", _isEditMode ? "Đã cập nhật" : "Đã thêm", "OK");
            await Shell.Current.GoToAsync("..");
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert("Lỗi", "Không thể lưu dữ liệu", "OK");
        }
    }

    [RelayCommand]
    async Task Cancel() => await Shell.Current.GoToAsync("..");
}