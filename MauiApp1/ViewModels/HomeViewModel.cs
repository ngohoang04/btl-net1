using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1.Models;
using MauiApp1.Services;
using MauiApp1.Views;
using Plugin.Maui.Calendar.Models; // Quan trọng
using System.Collections.ObjectModel;

namespace MauiApp1.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly CalendarService _calendarService;

    // Collection đặc biệt của thư viện Calendar để hiển thị dấu chấm sự kiện
    public EventCollection Events { get; set; } = new EventCollection();

    [ObservableProperty]
    private ObservableCollection<CalendarEvent> selectedDateEvents = new();

    [ObservableProperty]
    private DateTime selectedDate = DateTime.Today;

    public HomeViewModel(AuthService authService, CalendarService calendarService)
    {
        _authService = authService;
        _calendarService = calendarService;

        // Tải dữ liệu khi mở màn hình
        LoadEvents();
    }

    // ViewModels/HomeViewModel.cs

    // ViewModels/HomeViewModel.cs

    public async void LoadEvents()
    {
        try
        {
            // 1. Lấy User ID
            var userId = await SecureStorage.Default.GetAsync("user_email");
            if (string.IsNullOrEmpty(userId)) return;
            userId = userId.Replace(".", "_").Replace("@", "_");

            // 2. Tải dữ liệu từ Firebase
            var list = await _calendarService.GetEventsAsync(userId);

            // MẸO DEBUG: Hiện thông báo để biết chắc chắn có tải được dữ liệu hay không
            // (Nếu hiện số 0 thì là do chưa lưu được hoặc sai UserID)
            // await Application.Current.MainPage.DisplayAlert("Debug", $"Tải được {list.Count} sự kiện", "OK");

            var newEvents = new EventCollection();

            foreach (var evt in list)
            {
                // --- KHẮC PHỤC 1: CHUYỂN VỀ GIỜ ĐỊA PHƯƠNG ---
                // Firebase trả về có thể là UTC, ta cần ép về giờ máy người dùng
                var localStartTime = evt.StartTime.ToLocalTime();
                var localEndTime = evt.EndTime.ToLocalTime();

                // Cập nhật lại vào object để hiển thị ra list bên dưới cho đúng giờ
                evt.StartTime = localStartTime;
                evt.EndTime = localEndTime;

                // Dùng ngày địa phương để làm Key cho Calendar
                if (!newEvents.ContainsKey(localStartTime.Date))
                {
                    newEvents[localStartTime.Date] = new List<object>();
                }
                (newEvents[localStartTime.Date] as List<object>).Add(evt);
            }

            // --- KHẮC PHỤC 2: BẮT BUỘC CẬP NHẬT TRÊN MAIN THREAD ---
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Events = newEvents;
                OnPropertyChanged(nameof(Events));

                // Refresh lại list bên dưới nếu đang chọn ngày có sự kiện
                SelectedDateChanged(SelectedDate);
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi LoadEvents: {ex.Message}");
        }
    }
    [RelayCommand]
    void SelectedDateChanged(DateTime date)
    {
        SelectedDateEvents.Clear();
        if (Events.ContainsKey(date.Date))
        {
            var list = Events[date.Date] as List<object>;
            foreach (CalendarEvent evt in list)
            {
                SelectedDateEvents.Add(evt);
            }
        }
    }

    [RelayCommand]
    async Task GoToAddEvent()
    {
        await Shell.Current.GoToAsync(nameof(AddEventPage));
    }

    // Thêm hàm Refresh để gọi lại khi quay lại từ trang Add
    [RelayCommand]
    void Refresh() => LoadEvents();

    // (Giữ lại hàm Logout cũ của bạn)
    [RelayCommand]
    async Task Logout()
    {
        bool confirm = await Shell.Current.DisplayAlert("Đăng xuất", "Bạn có chắc chắn muốn đăng xuất?", "Có", "Không");
        if (confirm)
        {
            _authService.Logout();
            Application.Current.MainPage = new NavigationPage(new LoginPage(new LoginViewModel(_authService)));
        }
    }

    [RelayCommand]
    public async Task DeleteEvent(CalendarEvent eventToDelete)
    {
        if (eventToDelete == null) return;

        bool answer = await Shell.Current.DisplayAlert("Xác nhận", "Bạn có chắc muốn xóa sự kiện này?", "Có", "Không");
        if (answer)
        {
            try
            {
                // --- BƯỚC 1: XÓA TRÊN FIREBASE (QUAN TRỌNG) ---
                // Lấy UserID giống như lúc LoadEvents
                var userId = await SecureStorage.Default.GetAsync("user_email");
                if (!string.IsNullOrEmpty(userId))
                {
                    userId = userId.Replace(".", "_").Replace("@", "_");

                    // Gọi Service để xóa trên server
                    // Bạn cần đảm bảo hàm này tồn tại trong CalendarService
                    await _calendarService.DeleteEventAsync(userId, eventToDelete);
                }

                // --- BƯỚC 2: XÓA KHỎI UI (DANH SÁCH BÊN DƯỚI) ---
                SelectedDateEvents.Remove(eventToDelete);

                // --- BƯỚC 3: XÓA KHỎI LỊCH (DẤU CHẤM) ---
                if (Events.ContainsKey(SelectedDate))
                {
                    // Lưu ý: Thư viện Calendar thường lưu Value là List<object>, ép kiểu List<CalendarEvent> sẽ bị null
                    var list = Events[SelectedDate] as List<object>;

                    if (list != null)
                    {
                        list.Remove(eventToDelete);

                        // Nếu ngày đó hết sự kiện thì xóa luôn key để mất dấu chấm
                        if (list.Count == 0)
                        {
                            Events.Remove(SelectedDate);
                        }

                        // Cập nhật lại UI để lịch vẽ lại dấu chấm
                        // Cách nhanh nhất để ép giao diện vẽ lại collection
                        var temp = Events;
                        Events = null;
                        Events = temp;
                        OnPropertyChanged(nameof(Events));
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", "Không thể xóa sự kiện: " + ex.Message, "OK");
            }
        }
    }

    [RelayCommand]
    public async Task EditEvent(CalendarEvent eventToEdit)
    {
        // Chuyển sang trang AddEventPage và gửi kèm dữ liệu
        var navParam = new Dictionary<string, object>
    {
        { "EventData", eventToEdit }
    };
        await Shell.Current.GoToAsync(nameof(AddEventPage), navParam);
    }
}