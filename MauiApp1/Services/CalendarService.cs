using Firebase.Database;
using Firebase.Database.Query;
using MauiApp1.Models;
using Newtonsoft.Json;

namespace MauiApp1.Services;

public class CalendarService
{
    // Lưu ý: Đảm bảo URL kết thúc bằng dấu /
    private const string DatabaseUrl = "https://smartcalendarapp-b60fb-default-rtdb.asia-southeast1.firebasedatabase.app/";
    private readonly FirebaseClient _firebaseClient;

    public CalendarService()
    {
        _firebaseClient = new FirebaseClient(DatabaseUrl, new FirebaseOptions
        {
            AuthTokenAsyncFactory = () => SecureStorage.Default.GetAsync("auth_token")
        });
    }

    // --- 1. LẤY DANH SÁCH (GET) ---
    public async Task<List<CalendarEvent>> GetEventsAsync(string userId)
    {
        try
        {
            // SỬA: Trỏ thẳng vào thư mục của User đó để lấy cho nhanh
            var events = await _firebaseClient
                .Child("calendars")
                .Child(userId)
                .OnceAsync<CalendarEvent>();

            return events.Select(x =>
            {
                // Gán Key từ Firebase vào ID của object để sau này còn Xóa/Sửa được
                x.Object.Id = x.Key;
                x.Object.UserId = userId; // Đảm bảo UserId luôn đúng
                return x.Object;
            }).ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi GetEvents: {ex.Message}");
            return new List<CalendarEvent>();
        }
    }

    // --- 2. THÊM MỚI (ADD) ---
    public async Task<bool> AddEventAsync(CalendarEvent evt)
    {
        try
        {
            // PostAsync: Tạo ID ngẫu nhiên
            var result = await _firebaseClient
                .Child("calendars")
                .Child(evt.UserId)
                .PostAsync(evt);

            // Cập nhật lại ID vào object nếu cần dùng ngay
            evt.Id = result.Key;

            return true;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi Thêm", ex.Message, "OK");
            return false;
        }
    }

    // --- 3. CẬP NHẬT (UPDATE / EDIT) ---
    public async Task<bool> UpdateEventAsync(CalendarEvent evt)
    {
        try
        {
            if (string.IsNullOrEmpty(evt.Id)) return false;

            // PutAsync: Ghi đè vào đúng ID cũ
            await _firebaseClient
                .Child("calendars")
                .Child(evt.UserId)
                .Child(evt.Id)
                .PutAsync(evt);

            return true;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi Sửa", ex.Message, "OK");
            return false;
        }
    }

    // --- 4. XÓA (DELETE) ---
    public async Task DeleteEventAsync(string userId, CalendarEvent evt)
    {
        try
        {
            if (string.IsNullOrEmpty(evt.Id)) return;

            await _firebaseClient
                .Child("calendars")
                .Child(userId)
                .Child(evt.Id)
                .DeleteAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi Xóa", ex.Message, "OK");
        }
    }
    // Thêm vào trong class CalendarService
    public async Task<List<TodoItem>> GetTodosAsync(string userId)
    {
        try
        {
            var items = await _firebaseClient
                .Child("todos")
                .Child(userId)
                .OnceAsync<TodoItem>();

            return items.Select(x =>
            {
                x.Object.Id = x.Key;
                x.Object.UserId = userId;
                return x.Object;
            }).ToList();
        }
        catch { return new List<TodoItem>(); }
    }

    public async Task<bool> AddTodoAsync(TodoItem item)
    {
        try
        {
            var result = await _firebaseClient
                .Child("todos")
                .Child(item.UserId)
                .PostAsync(item);
            item.Id = result.Key;
            return true;
        }
        catch { return false; }
    }

    public async Task UpdateTodoAsync(TodoItem item)
    {
        if (string.IsNullOrEmpty(item.Id)) return;
        await _firebaseClient
            .Child("todos")
            .Child(item.UserId)
            .Child(item.Id)
            .PutAsync(item);
    }

    public async Task DeleteTodoAsync(string userId, string todoId)
    {
        await _firebaseClient
            .Child("todos")
            .Child(userId)
            .Child(todoId)
            .DeleteAsync();
    }
}