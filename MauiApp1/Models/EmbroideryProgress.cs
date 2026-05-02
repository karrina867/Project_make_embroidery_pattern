using SQLite;

namespace MauiApp1.Models
{
    [Table("EmbroideryProgress")]
    public class EmbroideryProgress
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int PatternId { get; set; }

        [Indexed]
        public int UserId { get; set; }

        // Храним отмеченные клетки в виде строки: "x1,y1;x2,y2;x3,y3"
        public string CompletedStitches { get; set; } = "";

        public DateTime LastUpdated { get; set; }

        // Процент выполнения (для быстрого отображения)
        public int PercentComplete { get; set; }
    }
}