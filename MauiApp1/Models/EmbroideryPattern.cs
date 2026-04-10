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
        public int Width { get; set; }      // Ширина в крестиках
        public int Height { get; set; }     // Высота в крестиках
        public int ColorCount { get; set; }  // Количество цветов
        public int CellSize { get; set; }   // Размер ячейки в пикселях (для отображения)
    }
}