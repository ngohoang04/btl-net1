using MauiApp1.ViewModels;
namespace MauiApp1.Views;

public partial class TodoDetailPage : ContentPage
{
    public TodoDetailPage(TodoDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}