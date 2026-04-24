using SQLite;

namespace MauiApp1.Models
{
    [Table("Patterns")]
    public class EmbroideryPattern
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
        public string ImageFileName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int ColorCount { get; set; }
        public int CellSize { get; set; }

        // НОВОЕ ПОЛЕ: ID пользователя, которому принадлежит схема
        [Indexed]  // Добавляем индекс для быстрого поиска
        public int UserId { get; set; }
        public string PatternDataFile { get; set; } // Имя файла с матрицей цветов




    }
}