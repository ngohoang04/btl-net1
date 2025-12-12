using Microsoft.Extensions.Logging;
using MauiApp1.Services;
namespace MauiApp1
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // --- THÊM ĐOẠN NÀY ---
            // 1. Đăng ký Service
            builder.Services.AddSingleton<Services.AuthService>();
            builder.Services.AddSingleton<Services.UserService>();
            // 2. Đăng ký ViewModel và View
            builder.Services.AddTransient<ViewModels.LoginViewModel>();
            builder.Services.AddTransient<Views.LoginPage>();
            // ---------------------
            builder.Services.AddTransient<ViewModels.HomeViewModel>();
            builder.Services.AddTransient<Views.HomePage>();
            // Đăng ký cho màn hình Đăng ký
            builder.Services.AddTransient<ViewModels.RegisterViewModel>();
            builder.Services.AddTransient<Views.RegisterPage>();

            builder.Services.AddTransient<ViewModels.ProfileViewModel>();
            builder.Services.AddTransient<Views.ProfilePage>();
            return builder.Build();
        }
    }
}
