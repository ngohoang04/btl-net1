using MauiApp1.ViewModels;

namespace MauiApp1.Views;

public partial class RegisterPage : ContentPage
{
    // Inject ViewModel vào Constructor
    public RegisterPage(RegisterViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    // Thêm constructor mặc định để tránh lỗi nếu gọi new RegisterPage() không tham số ở đâu đó
    public RegisterPage() : this(Application.Current.Handler.MauiContext.Services.GetService<RegisterViewModel>())
    {
    }
}