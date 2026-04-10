using SQLite;
using MauiApp1.Models;  // Исправлено с MauiAuthApp на MauiApp1
using System.Security.Cryptography;
using System.Text;

namespace MauiApp1.Services  // Исправлено пространство имен
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;
        private readonly string _dbPath;

        public DatabaseService()
        {
            _dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "authapp.db3");
        }

        private async Task Init()
        {
            if (_database != null)
                return;

            _database = new SQLiteAsyncConnection(_dbPath);
            await _database.CreateTableAsync<User>();
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public async Task<(bool success, string message)> RegisterUser(string login, string email, string password, string confirmPassword)
        {
            try
            {
                await Init();

                if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
                {
                    return (false, "Все поля обязательны для заполнения");
                }

                if (!IsValidEmail(email))
                {
                    return (false, "Введите корректный email адрес");
                }

                if (password.Length < 6)
                {
                    return (false, "Пароль должен содержать минимум 6 символов");
                }

                if (password != confirmPassword)
                {
                    return (false, "Пароли не совпадают");
                }

                var existingUser = await _database.Table<User>()
                    .Where(u => u.Login == login || u.Email == email)
                    .FirstOrDefaultAsync();

                if (existingUser != null)
                {
                    if (existingUser.Login == login)
                        return (false, "Пользователь с таким логином уже существует");
                    else
                        return (false, "Пользователь с таким email уже зарегистрирован");
                }

                var user = new User
                {
                    Login = login,
                    Email = email,
                    PasswordHash = HashPassword(password),
                    CreatedAt = DateTime.Now
                };

                await _database.InsertAsync(user);
                return (true, "Регистрация успешно завершена");
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка: {ex.Message}");
            }
        }

        public async Task<(bool success, string message, User user)> LoginUser(string loginOrEmail, string password)
        {
            try
            {
                await Init();

                if (string.IsNullOrWhiteSpace(loginOrEmail) || string.IsNullOrWhiteSpace(password))
                {
                    return (false, "Введите логин/email и пароль", null);
                }

                var user = await _database.Table<User>()
                    .Where(u => u.Login == loginOrEmail || u.Email == loginOrEmail)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return (false, "Пользователь не найден", null);
                }

                var passwordHash = HashPassword(password);
                if (user.PasswordHash != passwordHash)
                {
                    return (false, "Неверный пароль", null);
                }

                return (true, "Вход выполнен успешно", user);
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка: {ex.Message}", null);
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<User>> GetAllUsers()
        {
            await Init();
            return await _database.Table<User>().ToListAsync();
        }
    }
}