using SQLite;

namespace MauiApp1.Models
{
    [Table("PatternColors")]
    public class EmbroideryColor
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int PatternId { get; set; }

        public string ColorHex { get; set; }
        public int Red { get; set; }
        public int Green { get; set; }
        public int Blue { get; set; }

        // ИЗМЕНЕНО: char -> string
        public string Symbol { get; set; }  // Символ для схемы (например, "A", "B", "●")

        public string DMCNumber { get; set; }
        public int PixelCount { get; set; }
    }
}