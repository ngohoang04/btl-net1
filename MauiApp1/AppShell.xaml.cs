namespace MauiApp1
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // SỬA LẠI NHƯ SAU: Tách thành 2 dòng riêng biệt

            // 1. Đăng ký đường dẫn cho trang Đăng ký
            Routing.RegisterRoute("RegisterPage", typeof(Views.RegisterPage));

            // 2. Đăng ký đường dẫn cho trang Thêm sự kiện
            // (Mẹo: Dùng nameof() để lấy tên class làm tên đường dẫn cho chính xác, tránh gõ sai chính tả)
            Routing.RegisterRoute(nameof(Views.AddEventPage), typeof(Views.AddEventPage));
            Routing.RegisterRoute(nameof(Views.TodoDetailPage), typeof(Views.TodoDetailPage));
        }
    }
}