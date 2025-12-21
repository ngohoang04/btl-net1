using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1.Models;
using MauiApp1.Services; // Nhớ using Services

namespace MauiApp1.ViewModels;

[QueryProperty(nameof(Item), "Item")]
public partial class TodoDetailViewModel : ObservableObject
{
    private readonly CalendarService _service;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotEditing))] // Khi IsEditing đổi thì IsNotEditing cũng đổi theo
    bool isEditing = false;

    // Biến phụ để dùng cho giao diện (ngược lại với IsEditing)
    public bool IsNotEditing => !IsEditing;

    [ObservableProperty]
    TodoItem item;

    // Danh sách độ ưu tiên cho Picker chọn
    public List<string> Priorities { get; } = new() { "Cao", "Trung bình", "Thấp" };

    // Bắt buộc phải inject Service vào Constructor
    public TodoDetailViewModel(CalendarService service)
    {
        _service = service;
    }

    [RelayCommand]
    void EnableEdit()
    {
        IsEditing = true;
    }

    [RelayCommand]
    async Task Save()
    {
        if (Item == null) return;

        // 1. Cập nhật màu sắc mới nếu user đổi độ ưu tiên
        Item.Color = Item.Priority switch
        {
            "Cao" => "#FF4500",
            "Trung bình" => "#FFD700",
            _ => "#32CD32"
        };

        // 2. Gọi Service cập nhật lên Firebase
        await _service.UpdateTodoAsync(Item);

        // 3. Tắt chế độ sửa
        IsEditing = false;

        await Shell.Current.DisplayAlert("Thành công", "Đã cập nhật công việc", "OK");
    }

    [RelayCommand]
    async Task GoBack()
    {
        await Shell.Current.GoToAsync("..");
    }
}