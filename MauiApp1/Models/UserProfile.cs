namespace MauiApp1.Models;

public class UserProfile
{
    public string UserId { get; set; } // ID để định danh
    public string FullName { get; set; }
    public string Email { get; set; }
    public string AvatarBase64 { get; set; } // Lưu ảnh dạng chuỗi mã hóa cho đơn giản
    public DateTime DateOfBirth { get; set; }
    public string TimeZone { get; set; }
}