using Firebase.Database;
using Firebase.Database.Query;
using MauiApp1.Models;

namespace MauiApp1.Services;

public class UserService
{
    // Dán URL Database bạn vừa copy ở Bước 1 vào đây
    private const string DatabaseUrl = "https://console.firebase.google.com/u/0/project/smartcalendarapp-b60fb/database/smartcalendarapp-b60fb-default-rtdb/data/~2F";
    private readonly FirebaseClient _firebaseClient;

    public UserService()
    {
        _firebaseClient = new FirebaseClient(DatabaseUrl);
    }

    // 1. Lưu hoặc Cập nhật thông tin
    public async Task<bool> SaveUserProfileAsync(UserProfile profile)
    {
        try
        {
            // Lưu vào nhánh: Users -> [UserId] -> Thông tin
            await _firebaseClient
                .Child("Users")
                .Child(profile.UserId)
                .PutAsync(profile);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // 2. Lấy thông tin người dùng
    public async Task<UserProfile> GetUserProfileAsync(string userId)
    {
        try
        {
            var profile = await _firebaseClient
                .Child("Users")
                .Child(userId)
                .OnceSingleAsync<UserProfile>();
            return profile;
        }
        catch
        {
            return null;
        }
    }
}