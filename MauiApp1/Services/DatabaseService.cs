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


           // System.Diagnostics.Debug.WriteLine($"DB Path: {_dbPath}"); //показывает, где создается бд

            // Создаем папку для изображений, если её нет
            if (!Directory.Exists(_imagesPath))
            {
                Directory.CreateDirectory(_imagesPath);
            }
        }




        //что бы удалить старую бд при модификациях бд
        public void DeleteDatabase()
        {
            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
            }
        }
        //









        private async Task Init()
        {
            if (_database != null)
                return;

            _database = new SQLiteAsyncConnection(_dbPath);
            await _database.CreateTableAsync<User>();
            await _database.CreateTableAsync<EmbroideryPattern>();
            await _database.CreateTableAsync<EmbroideryColor>();
            await _database.CreateTableAsync<EmbroideryProgress>();

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

        // Добавьте в класс DatabaseService новые методы

        // Сохранение цветов схемы
        public async Task<bool> SavePatternColors(int patternId, List<EmbroideryColor> colors)
        {
            try
            {
                await Init();

                foreach (var color in colors)
                {
                    color.PatternId = patternId;
                    await _database.InsertAsync(color);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        //// Получение цветов схемы
        //public async Task<List<EmbroideryColor>> GetPatternColors(int patternId)
        //{
        //    await Init();
        //    return await _database.Table<EmbroideryColor>()
        //        .Where(c => c.PatternId == patternId)
        //        .OrderBy(c => c.Symbol)
        //        .ToListAsync();
        //}

        //// Обновление количества цветов в схеме
        //public async Task UpdatePatternColorCount(int patternId, int colorCount)
        //{
        //    await Init();
        //    var pattern = await _database.Table<EmbroideryPattern>()
        //        .FirstOrDefaultAsync(p => p.Id == patternId);

        //    if (pattern != null)
        //    {
        //        pattern.ColorCount = colorCount;
        //        await _database.UpdateAsync(pattern);
        //    }
        //}
        // Получение цветов схемы
        public async Task<List<EmbroideryColor>> GetPatternColors(int patternId)
        {
            await Init();
            return await _database.Table<EmbroideryColor>()
                .Where(c => c.PatternId == patternId)
                .OrderBy(c => c.Symbol)
                .ToListAsync();
        }

        // Обновление количества цветов в схеме
        public async Task UpdatePatternColorCount(int patternId, int colorCount)
        {
            await Init();
            var pattern = await _database.Table<EmbroideryPattern>()
                .FirstOrDefaultAsync(p => p.Id == patternId);  // Исправлено: p вместо c, и добавлено Async

            if (pattern != null)
            {
                pattern.ColorCount = colorCount;
                await _database.UpdateAsync(pattern);  // Исправлено: _database вместо database
            }
        }


        #region Методы для работы со схемами вышивки

        // Сохранение схемы
        // Сохранение схемы
        public async Task<(bool success, string message, int patternId)> SavePattern(EmbroideryPattern pattern, Stream imageStream, int userId)
        {
            try
            {
                await Init();

                var safeName = string.Join("_", pattern.Name.Split(Path.GetInvalidFileNameChars()));
                var fileName = $"{Guid.NewGuid()}_{safeName}.jpg";
                var filePath = Path.Combine(_imagesPath, fileName);

                using (var fileStream = File.Create(filePath))
                {
                    if (imageStream.CanSeek)
                    {
                        imageStream.Position = 0;
                    }
                    await imageStream.CopyToAsync(fileStream);
                    await fileStream.FlushAsync();
                }

                pattern.ImageFileName = fileName;
                pattern.CreatedAt = DateTime.Now;
                pattern.UserId = userId;  // ← Привязываем схему к пользователю

                await _database.InsertAsync(pattern);

                return (true, "Схема успешно сохранена", pattern.Id);
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка: {ex.Message}", 0);
            }
        }

        // Получение всех схем ТОЛЬКО для конкретного пользователя
        public async Task<List<EmbroideryPattern>> GetUserPatterns(int userId)
        {
            await Init();
            return await _database.Table<EmbroideryPattern>()
                .Where(p => p.UserId == userId)  // ← Фильтр по пользователю
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        // Получение одной схемы с проверкой принадлежности пользователю
        public async Task<EmbroideryPattern> GetPatternById(int patternId, int userId)
        {
            await Init();
            return await _database.Table<EmbroideryPattern>()
                .FirstOrDefaultAsync(p => p.Id == patternId && p.UserId == userId);
        }

        // Получение всех схем
        public async Task<List<EmbroideryPattern>> GetAllPatterns()
        {
            await Init();
            return await _database.Table<EmbroideryPattern>().OrderByDescending(p => p.CreatedAt).ToListAsync();
        }


        // Удаление схемы (с проверкой прав)
        // Удаление схемы (с удалением файла матрицы)
        public async Task<bool> DeletePattern(int patternId, int userId)
        {
            try
            {
                await Init();

                var pattern = await GetPatternById(patternId, userId);
                if (pattern == null)
                    return false;

                // Удаляем файл матрицы цветов
                if (!string.IsNullOrEmpty(pattern.PatternDataFile))
                {
                    var matrixPath = Path.Combine(FileSystem.AppDataDirectory, pattern.PatternDataFile);
                    if (File.Exists(matrixPath))
                        File.Delete(matrixPath);
                }

                // Удаляем файл изображения
                var imagePath = GetImagePath(pattern.ImageFileName);
                if (File.Exists(imagePath))
                    File.Delete(imagePath);

                // Удаляем цвета из БД
                var colors = await GetPatternColors(patternId, userId);
                foreach (var color in colors)
                    await _database.DeleteAsync(color);

                // Удаляем схему
                await _database.DeleteAsync(pattern);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Получение цветов схемы (с проверкой прав)
        public async Task<List<EmbroideryColor>> GetPatternColors(int patternId, int userId)
        {
            await Init();

            // Проверяем, принадлежит ли схема пользователю
            var pattern = await GetPatternById(patternId, userId);
            if (pattern == null)
                return new List<EmbroideryColor>();

            return await _database.Table<EmbroideryColor>()
                .Where(c => c.PatternId == patternId)
                .OrderBy(c => c.Symbol)
                .ToListAsync();
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




        #region Методы для работы с прогрессом вышивки

        // Сохранение или обновление прогресса
        public async Task<bool> SaveProgress(int patternId, int userId, HashSet<(int x, int y)> completedStitches)
        {
            try
            {
                await Init();

                var progress = await _database.Table<EmbroideryProgress>()
                    .FirstOrDefaultAsync(p => p.PatternId == patternId && p.UserId == userId);

                // Преобразуем HashSet в строку
                var stitchesString = string.Join(";", completedStitches.Select(s => $"{s.x},{s.y}"));

                if (progress == null)
                {
                    progress = new EmbroideryProgress
                    {
                        PatternId = patternId,
                        UserId = userId,
                        CompletedStitches = stitchesString,
                        LastUpdated = DateTime.Now,
                        PercentComplete = completedStitches.Count * 100 / (patternId > 0 ? 1 : 1) // Временно, обновим позже
                    };
                    await _database.InsertAsync(progress);
                }
                else
                {
                    progress.CompletedStitches = stitchesString;
                    progress.LastUpdated = DateTime.Now;
                    await _database.UpdateAsync(progress);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        // Получение прогресса для схемы
        public async Task<HashSet<(int x, int y)>> GetProgress(int patternId, int userId)
        {
            try
            {
                await Init();

                var progress = await _database.Table<EmbroideryProgress>()
                    .FirstOrDefaultAsync(p => p.PatternId == patternId && p.UserId == userId);

                if (progress == null || string.IsNullOrEmpty(progress.CompletedStitches))
                    return new HashSet<(int x, int y)>();

                var result = new HashSet<(int x, int y)>();
                var stitches = progress.CompletedStitches.Split(';', StringSplitOptions.RemoveEmptyEntries);

                foreach (var stitch in stitches)
                {
                    var parts = stitch.Split(',');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
                    {
                        result.Add((x, y));
                    }
                }

                return result;
            }
            catch
            {
                return new HashSet<(int x, int y)>();
            }
        }

        // Обновление процента выполнения
        public async Task UpdatePercentComplete(int patternId, int userId, int totalStitches, int completedStitches)
        {
            try
            {
                await Init();

                var progress = await _database.Table<EmbroideryProgress>()
                    .FirstOrDefaultAsync(p => p.PatternId == patternId && p.UserId == userId);

                if (progress != null)
                {
                    progress.PercentComplete = completedStitches * 100 / totalStitches;
                    await _database.UpdateAsync(progress);
                }
            }
            catch { }
        }

        #endregion
    }
}
