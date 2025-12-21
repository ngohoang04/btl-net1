using MauiApp1.ViewModels;

namespace MauiApp1.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _vm;

    public HomePage(HomeViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Load lại dữ liệu mỗi khi vào trang
        if (_vm.RefreshCommand.CanExecute(null))
        {
            _vm.RefreshCommand.Execute(null);
        }
    }

    // (Đã xóa hàm Calendar_DayTapped vì không cần nữa)
}