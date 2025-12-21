using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1.Models;
using MauiApp1.Services;
using System.Collections.ObjectModel;
using MauiApp1.Views; // Đảm bảo có dòng này

namespace MauiApp1.ViewModels;

public partial class TodoViewModel : ObservableObject
{
    private readonly CalendarService _service;

    public ObservableCollection<TodoItem> Todos { get; set; } = new();

    // Các biến nhập liệu
    [ObservableProperty] string newTitle;
    [ObservableProperty] DateTime newDeadline = DateTime.Today;
    [ObservableProperty] string newPriority = "Trung bình";
    [ObservableProperty] string newContent;

    // --- MỚI: Biến để quản lý trạng thái Sửa ---
    [ObservableProperty] bool isEditMode = false; // Đang sửa hay đang thêm?
    [ObservableProperty] string buttonText = "Thêm"; // Chữ trên nút bấm
    private TodoItem _itemToEdit; // Lưu tạm món đang sửa

    public List<string> Priorities { get; } = new() { "Cao", "Trung bình", "Thấp" };

    public TodoViewModel(CalendarService service)
    {
        _service = service;
        LoadTodos();
    }

    public async void LoadTodos()
    {
        var userId = await SecureStorage.Default.GetAsync("user_email");
        if (string.IsNullOrEmpty(userId)) return;
        userId = userId.Replace(".", "_").Replace("@", "_");

        var list = await _service.GetTodosAsync(userId);

        var sortedList = list.Where(x => !x.IsCompleted)
                             .OrderBy(x => x.Deadline)
                             .ToList();

        Todos.Clear();
        foreach (var item in sortedList) Todos.Add(item);
    }

    // --- MỚI: Hàm chuẩn bị dữ liệu để Sửa ---
    [RelayCommand]
    void PrepareEdit(TodoItem item)
    {
        if (item == null) return;

        // 1. Đưa dữ liệu cũ lên khung nhập
        NewTitle = item.Title;
        NewContent = item.Content;
        NewDeadline = item.Deadline;
        NewPriority = item.Priority;

        // 2. Lưu lại món đang sửa
        _itemToEdit = item;

        // 3. Đổi trạng thái sang "Sửa"
        IsEditMode = true;
        ButtonText = "Lưu";
    }

    // --- MỚI: Hàm Hủy Sửa (nếu người dùng đổi ý) ---
    [RelayCommand]
    void CancelEdit()
    {
        // Reset về trạng thái thêm mới
        NewTitle = string.Empty;
        NewContent = string.Empty;
        NewDeadline = DateTime.Today;
        NewPriority = "Trung bình";

        _itemToEdit = null;
        IsEditMode = false;
        ButtonText = "Thêm";
    }

    // --- CẬP NHẬT: Hàm AddTodo giờ xử lý cả Thêm và Sửa ---
    [RelayCommand]
    async Task SaveTodo() // Đổi tên từ AddTodo thành SaveTodo cho đúng ngữ cảnh
    {
        if (string.IsNullOrWhiteSpace(NewTitle)) return;

        var userId = await SecureStorage.Default.GetAsync("user_email");
        userId = userId?.Replace(".", "_").Replace("@", "_");

        string color = NewPriority switch
        {
            "Cao" => "#FF4500",
            "Trung bình" => "#FFD700",
            _ => "#32CD32"
        };

        if (IsEditMode && _itemToEdit != null)
        {
            // --- LOGIC SỬA (UPDATE) ---

            // 1. Cập nhật dữ liệu vào object đang sửa
            _itemToEdit.Title = NewTitle;
            _itemToEdit.Content = NewContent;
            _itemToEdit.Deadline = NewDeadline;
            _itemToEdit.Priority = NewPriority;
            _itemToEdit.Color = color;
            _itemToEdit.UserId = userId; // Đảm bảo ID user

            // 2. Gọi Service cập nhật lên Firebase
            await _service.UpdateTodoAsync(_itemToEdit);

            // 3. Reset form
            CancelEdit();
        }
        else
        {
            // --- LOGIC THÊM MỚI (ADD) ---
            var newItem = new TodoItem
            {
                UserId = userId,
                Title = NewTitle,
                Deadline = NewDeadline,
                Priority = NewPriority,
                IsCompleted = false,
                Color = color,
                Content = NewContent
            };

            if (await _service.AddTodoAsync(newItem))
            {
                Todos.Add(newItem);
                // Reset form thủ công (hoặc gọi CancelEdit cũng được)
                NewTitle = string.Empty;
                NewContent = string.Empty;
            }
        }
    }

    // ... Các hàm ToggleComplete, DeleteTodo, GoToDetails giữ nguyên ...
    [RelayCommand]
    async Task ToggleComplete(TodoItem item)
    {
        await _service.UpdateTodoAsync(item);
        if (item.IsCompleted)
        {
            await Task.Delay(1000);
            Todos.Remove(item);
        }
    }

    [RelayCommand]
    async Task GoToDetails(TodoItem item)
    {
        if (item == null) return;
        var navParam = new Dictionary<string, object> { { "Item", item } };
        await Shell.Current.GoToAsync(nameof(TodoDetailPage), navParam);
    }

    [RelayCommand]
    async Task DeleteTodo(TodoItem item)
    {
        bool confirm = await Shell.Current.DisplayAlert("Xóa", "Xóa công việc này?", "Có", "Không");
        if (confirm)
        {
            await _service.DeleteTodoAsync(item.UserId, item.Id);
            Todos.Remove(item);
        }
    }
}