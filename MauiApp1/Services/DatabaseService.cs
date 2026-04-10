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
        private readonly string _imagesPath;

        public DatabaseService()
        {
            _dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "embroidery.db3");
            _imagesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PatternImages");

            // Создаем папку для изображений, если её нет
            if (!Directory.Exists(_imagesPath))
            {
                Directory.CreateDirectory(_imagesPath);
            }
        }

        private async Task Init()
        {
            if (_database != null)
                return;

            _database = new SQLiteAsyncConnection(_dbPath);
            await _database.CreateTableAsync<User>();
            await _database.CreateTableAsync<EmbroideryPattern>();
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
        #region Методы для работы со схемами вышивки

        // Сохранение схемы
        // Сохранение схемы
        public async Task<(bool success, string message, int patternId)> SavePattern(EmbroideryPattern pattern, Stream imageStream)
        {
            try
            {
                await Init();

                // Генерируем уникальное имя для файла
                var safeName = string.Join("_", pattern.Name.Split(Path.GetInvalidFileNameChars()));
                var fileName = $"{Guid.NewGuid()}_{safeName}.jpg";
                var filePath = Path.Combine(_imagesPath, fileName);

                // Сохраняем изображение - важно: не закрываем исходный поток
                using (var fileStream = File.Create(filePath))
                {
                    // Сбрасываем позицию потока в начало
                    if (imageStream.CanSeek)
                    {
                        imageStream.Position = 0;
                    }
                    await imageStream.CopyToAsync(fileStream);
                    await fileStream.FlushAsync();
                }

                // Сохраняем информацию в БД
                pattern.ImageFileName = fileName;
                pattern.CreatedAt = DateTime.Now;

                await _database.InsertAsync(pattern);

                return (true, "Схема успешно сохранена", pattern.Id);
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка: {ex.Message}", 0);
            }
        }

        // Получение всех схем
        public async Task<List<EmbroideryPattern>> GetAllPatterns()
        {
            await Init();
            return await _database.Table<EmbroideryPattern>().OrderByDescending(p => p.CreatedAt).ToListAsync();
        }

        // Получение пути к изображению схемы
        public string GetImagePath(string imageFileName)
        {
            return Path.Combine(_imagesPath, imageFileName);
        }

        // Удаление схемы
        public async Task<bool> DeletePattern(EmbroideryPattern pattern)
        {
            try
            {
                await Init();

                // Удаляем файл изображения
                var imagePath = GetImagePath(pattern.ImageFileName);
                if (File.Exists(imagePath))
                {
                    File.Delete(imagePath);
                }

                // Удаляем запись из БД
                await _database.DeleteAsync(pattern);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
