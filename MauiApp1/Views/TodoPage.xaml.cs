// File: Views/TodoPage.xaml.cs
using MauiApp1.ViewModels; // <-- Nhớ dòng này

namespace MauiApp1.Views;

public partial class TodoPage : ContentPage
{
    // Cần thêm tham số vm vào trong ngoặc
    public TodoPage(TodoViewModel vm)
    {
        InitializeComponent();

        // --- QUAN TRỌNG NHẤT: DÒNG NÀY ---
        BindingContext = vm;
    }
}