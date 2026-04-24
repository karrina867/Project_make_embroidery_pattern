using SkiaSharp;
using MauiApp1.Models;

namespace MauiApp1.Services
{
    public class ImageProcessor
    {
        // Класс для хранения информации о цвете
        public class QuantizedColor
        {
            public SKColor Color { get; set; }
            public int Count { get; set; }
            public List<SKPointI> Positions { get; set; } = new List<SKPointI>();
        }

        public async Task<List<EmbroideryColor>> ProcessImage(string imagePath, int targetWidth, int targetHeight)
        {
            return await Task.Run(() =>
            {
                using (var originalBitmap = SKBitmap.Decode(imagePath))
                {
                    if (originalBitmap == null)
                        throw new Exception("Не удалось загрузить изображение");

                    // 1. Изменяем размер изображения
                    var resizedBitmap = ResizeBitmap(originalBitmap, targetWidth, targetHeight);

                    // 2. Извлекаем все цвета
                    var colors = ExtractColors(resizedBitmap);

                    // 3. Уменьшаем количество цветов (оставляем максимум 50)
                    var reducedColors = ReduceColors(colors, maxColors: 50);

                    // 4. Назначаем символы цветам
                    var embroideryColors = AssignSymbols(reducedColors);

                    return embroideryColors;
                }
            });
        }

        private SKBitmap ResizeBitmap(SKBitmap original, int width, int height)
        {
            var resized = new SKBitmap(width, height);
            using (var canvas = new SKCanvas(resized))
            {
                var destRect = new SKRect(0, 0, width, height);
                var srcRect = new SKRect(0, 0, original.Width, original.Height);
                canvas.DrawBitmap(original, srcRect, destRect);
            }
            return resized;
        }

        private List<QuantizedColor> ExtractColors(SKBitmap bitmap)
        {
            var colorDict = new Dictionary<SKColor, QuantizedColor>();

            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    var pixelColor = bitmap.GetPixel(x, y);
                    // Упрощаем цвет для уменьшения оттенков
                    var simplifiedColor = SimplifyColor(pixelColor);

                    if (!colorDict.ContainsKey(simplifiedColor))
                    {
                        colorDict[simplifiedColor] = new QuantizedColor
                        {
                            Color = simplifiedColor,
                            Count = 0
                        };
                    }
                    colorDict[simplifiedColor].Count++;
                    colorDict[simplifiedColor].Positions.Add(new SKPointI(x, y));
                }
            }

            return colorDict.Values.OrderByDescending(c => c.Count).ToList();
        }

        private SKColor SimplifyColor(SKColor color, int step = 32)
        {
            // Округляем компоненты цвета до шага (квантование)
            byte r = (byte)((color.Red / step) * step);
            byte g = (byte)((color.Green / step) * step);
            byte b = (byte)((color.Blue / step) * step);
            return new SKColor(r, g, b);
        }

        private List<QuantizedColor> ReduceColors(List<QuantizedColor> colors, int maxColors)
        {
            if (colors.Count <= maxColors)
                return colors;

            // Оставляем только самые частые цвета
            return colors.Take(maxColors).ToList();
        }

        private List<EmbroideryColor> AssignSymbols(List<QuantizedColor> colors)
        {
            var symbols = new string[] {
        "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
        "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
        "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m",
        "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z",
        "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
        "●", "▲", "■", "♥", "♦", "♣", "♠", "★", "☆", "☀", "☁", "☂"
    };

            var result = new List<EmbroideryColor>();

            for (int i = 0; i < colors.Count && i < symbols.Length; i++)
            {
                var color = colors[i].Color;
                result.Add(new EmbroideryColor
                {
                    ColorHex = ColorToHex(color),
                    Red = color.Red,
                    Green = color.Green,
                    Blue = color.Blue,
                    Symbol = symbols[i],  // Теперь это string
                    PixelCount = colors[i].Count,
                    DMCNumber = FindClosestDMC(color)
                });
            }

            return result;
        }

        // Сохранение матрицы цветов в файл
        public async Task<string> SavePatternMatrix(int[,] colorIndices, List<EmbroideryColor> colors)
        {
            return await Task.Run(() =>
            {
                var fileName = $"pattern_{Guid.NewGuid()}.pmat";
                var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

                using (var writer = new StreamWriter(filePath))
                {
                    // Записываем размеры
                    writer.WriteLine($"{colorIndices.GetLength(0)},{colorIndices.GetLength(1)}");

                    // Записываем индексы цветов
                    for (int y = 0; y < colorIndices.GetLength(1); y++)
                    {
                        for (int x = 0; x < colorIndices.GetLength(0); x++)
                        {
                            writer.Write($"{colorIndices[x, y]},");
                        }
                        writer.WriteLine();
                    }
                }

                return fileName;
            });
        }

        private string ColorToHex(SKColor color)
        {
            return $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
        }

        private string FindClosestDMC(SKColor color)
        {
            // База DMC цветов (можно расширить)
            var dmcColors = new Dictionary<string, SKColor>
            {
                // Белые и кремовые
                { "BLANC", new SKColor(255, 255, 255) },
                { "ECRU", new SKColor(248, 240, 227) },
                
                // Черные и серые
                { "310", new SKColor(0, 0, 0) },
                { "317", new SKColor(108, 108, 108) },
                { "318", new SKColor(163, 163, 163) },
                { "415", new SKColor(204, 204, 204) },
                
                // Красные
                { "321", new SKColor(255, 60, 60) },
                { "666", new SKColor(220, 20, 20) },
                { "304", new SKColor(180, 30, 30) },
                { "498", new SKColor(150, 20, 20) },
                
                // Зеленые
                { "700", new SKColor(0, 150, 0) },
                { "701", new SKColor(0, 120, 0) },
                { "702", new SKColor(0, 180, 0) },
                { "703", new SKColor(0, 210, 0) },
                { "704", new SKColor(0, 200, 0) },
                
                // Синие
                { "796", new SKColor(0, 60, 200) },
                { "797", new SKColor(0, 80, 230) },
                { "798", new SKColor(0, 100, 250) },
                { "799", new SKColor(0, 130, 255) },
                { "800", new SKColor(0, 50, 150) },
                
                // Желтые и оранжевые
                { "972", new SKColor(255, 200, 0) },
                { "973", new SKColor(255, 150, 0) },
                { "742", new SKColor(255, 180, 50) },
                { "743", new SKColor(255, 200, 100) },
                
                // Фиолетовые
                { "550", new SKColor(80, 0, 120) },
                { "552", new SKColor(100, 30, 140) },
                { "553", new SKColor(130, 60, 160) },
                { "554", new SKColor(160, 90, 180) },
                
                // Розовые
                { "605", new SKColor(255, 130, 150) },
                { "606", new SKColor(255, 80, 100) },
                { "602", new SKColor(220, 60, 90) },
                { "603", new SKColor(200, 50, 80) },
                
                // Коричневые
                { "300", new SKColor(80, 40, 20) },
                { "301", new SKColor(100, 50, 25) },
                { "302", new SKColor(120, 60, 30) },
                { "303", new SKColor(140, 70, 35) },
            };

            // Находим ближайший цвет по сумме разниц RGB
            var closest = dmcColors.OrderBy(dmc =>
                Math.Abs(dmc.Value.Red - color.Red) +
                Math.Abs(dmc.Value.Green - color.Green) +
                Math.Abs(dmc.Value.Blue - color.Blue))
                .FirstOrDefault();

            return closest.Key;
        }
    }
}