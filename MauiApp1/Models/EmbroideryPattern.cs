using SQLite;

namespace MauiApp1.Models
{
    [Table("Patterns")]
    public class EmbroideryPattern
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }           // Название схемы
        public string ImageFileName { get; set; }  // Имя файла картинки
        public DateTime CreatedAt { get; set; }    // Дата создания
        public int Width { get; set; }             // Ширина в крестиках
        public int Height { get; set; }            // Высота в крестиках
        public int ColorCount { get; set; }        // Количество цветов
    }
}