using Firebase.Auth;
using Firebase.Auth.Providers; // Bắt buộc dòng này để dùng EmailProvider
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace MauiApp1.Services;

public class AuthService
{
    // 1. Thay API Key của bạn
    private const string ApiKey = "AIzaSyBjhAMieMYZURXJhydygp80mpc-bnY5VwI";

    // 2. Thay Project ID lấy từ ảnh bạn gửi (smartcalendarapp-b60fb)
    private const string AuthDomain = "smartcalendarapp-b60fb.firebaseapp.com";

    // Ở bản mới, tên biến là FirebaseAuthClient
    private readonly FirebaseAuthClient _authClient;

    public AuthService()
    {
        var config = new FirebaseAuthConfig
        {
            ApiKey = ApiKey,
            AuthDomain = AuthDomain,
            Providers = new FirebaseAuthProvider[]
            {
                // Bắt buộc phải khai báo Provider muốn dùng (Email/Password)
                new EmailProvider()
            }
        };

        _authClient = new FirebaseAuthClient(config);
    }

    public async Task<string> LoginAsync(string email, string password)
    {
        try
        {
            // Lệnh đăng nhập ở bản mới
            var userCredential = await _authClient.SignInWithEmailAndPasswordAsync(email, password);

            // Lấy Token
            var token = await userCredential.User.GetIdTokenAsync();

            // Lưu token vào máy
            await SecureStorage.Default.SetAsync("auth_token", token);
            await SecureStorage.Default.SetAsync("user_email", email);
            return ""; // Thành công
        }
        catch (Exception ex)
        {
            // Gợi ý: Bạn có thể đặt breakpoint ở đây để xem lỗi chi tiết
            return "Sai email hoặc mật khẩu";
        }
    }
    public async Task<string> RegisterAsync(string email, string password)
    {
        try
        {
            // Gọi hàm tạo user của Firebase
            await _authClient.CreateUserWithEmailAndPasswordAsync(email, password);
            return ""; // Không có lỗi
        }
        catch (Exception ex)
        {
            // Xử lý lỗi (ví dụ: Email đã tồn tại, mật khẩu quá yếu...)
            if (ex.Message.Contains("EMAIL_EXISTS"))
                return "Email này đã được sử dụng.";
            else if (ex.Message.Contains("WEAK_PASSWORD"))
                return "Mật khẩu phải có ít nhất 6 ký tự.";
            else
                return "Đăng ký thất bại: " + ex.Message;
        }
    }

    public async Task<bool> CheckLoginStatusAsync()
    {
        var token = await SecureStorage.Default.GetAsync("auth_token");
        return !string.IsNullOrEmpty(token);
    }

    public void Logout()
    {
        // 1. Quan trọng nhất: Xóa token lưu trong máy để lần sau bắt đăng nhập lại
        SecureStorage.Default.Remove("auth_token");

        // 2. Cố gắng đăng xuất trên thư viện (nếu có thể)
        try
        {
            // Kiểm tra xem User có tồn tại không trước khi gọi SignOut
            if (_authClient?.User != null)
            {
                _authClient.SignOut();
            }
        }
        catch
        {
            // Nếu lỗi (do User null hoặc thư viện lỗi) thì bỏ qua luôn, 
            // vì ta đã xóa token ở bước 1 rồi là đủ an toàn.
        }
    }
    public async Task<string> ResetPasswordAsync(string email)
    {
        try
        {
            // Firebase sẽ gửi 1 email chứa link đổi mật khẩu cho người dùng
            await _authClient.ResetEmailPasswordAsync(email);
            return ""; // Thành công
        }
        catch (Exception ex)
        {
            return "Lỗi: " + ex.Message;
        }
    }
    public async Task<string> ChangePasswordAsync(string newPassword)
    {
        try
        {
            // Lấy user hiện tại đang đăng nhập
            var user = _authClient.User;

            if (user != null)
            {
                await user.ChangePasswordAsync(newPassword);
                return ""; // Thành công
            }
            return "Bạn chưa đăng nhập hoặc phiên đăng nhập hết hạn.";
        }
        catch (Exception ex)
        {
            return "Đổi mật khẩu thất bại: " + ex.Message;
        }
    }
}