using CommunityToolkit.Maui; // Đã có using
using MauiApp1.Services;
using MauiApp1.Views;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using CommunityToolkit.Maui;
namespace MauiApp1
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit() // <--- BẠN CẦN THÊM DÒNG NÀY VÀO ĐÂY
                .UseLocalNotification()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // 1. Đăng ký Service
            builder.Services.AddSingleton<Services.AuthService>();
            builder.Services.AddSingleton<Services.UserService>();
            builder.Services.AddSingleton<Services.CalendarService>();

            // 2. Đăng ký ViewModel và View
            builder.Services.AddTransient<ViewModels.LoginViewModel>();
            builder.Services.AddTransient<Views.LoginPage>();

            builder.Services.AddTransient<ViewModels.AddEventViewModel>();
            builder.Services.AddTransient<Views.AddEventPage>();

            builder.Services.AddTransient<ViewModels.HomeViewModel>();
            builder.Services.AddTransient<Views.HomePage>();

            builder.Services.AddTransient<ViewModels.RegisterViewModel>();
            builder.Services.AddTransient<Views.RegisterPage>();

            builder.Services.AddTransient<ViewModels.ProfileViewModel>();
            builder.Services.AddTransient<Views.ProfilePage>();

            builder.Services.AddTransient<Views.TodoPage>();
            builder.Services.AddTransient<ViewModels.TodoViewModel>();

            builder.Services.AddTransient<ViewModels.TodoDetailViewModel>();
            builder.Services.AddTransient<Views.TodoDetailPage>();
            return builder.Build();
        }
    }
}