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

        public async Task<(List<EmbroideryColor> colors, int[,] colorMatrix)> ProcessImageWithMatrix(
          string imagePath, int targetWidth, int targetHeight)
        {
            return await Task.Run(() =>
            {
                using (var originalBitmap = SKBitmap.Decode(imagePath))
                {
                    if (originalBitmap == null)
                        throw new Exception("Не удалось загрузить изображение");

                    // Изменяем размер до целевого
                    var resizedBitmap = ResizeBitmap(originalBitmap, targetWidth, targetHeight);

                    // Извлекаем все цвета
                    var colorDict = ExtractColorsWithPositions(resizedBitmap);

                    // Уменьшаем количество цветов (максимум 50)
                    var reducedColors = ReduceColors(colorDict.Values.ToList(), maxColors: 50);

                    // Назначаем символы
                    var embroideryColors = AssignSymbols(reducedColors);

                    // Создаем карту цветов
                    var colorMap = new Dictionary<SKColor, int>();
                    for (int i = 0; i < embroideryColors.Count; i++)
                    {
                        var color = new SKColor(
                            (byte)embroideryColors[i].Red,
                            (byte)embroideryColors[i].Green,
                            (byte)embroideryColors[i].Blue);
                        colorMap[color] = i;
                    }

                    // Создаем матрицу индексов цветов
                    var colorMatrix = new int[targetWidth, targetHeight];
                    for (int x = 0; x < targetWidth; x++)
                    {
                        for (int y = 0; y < targetHeight; y++)
                        {
                            var pixelColor = resizedBitmap.GetPixel(x, y);
                            var simplifiedColor = SimplifyColor(pixelColor);

                            if (colorMap.ContainsKey(simplifiedColor))
                            {
                                colorMatrix[x, y] = colorMap[simplifiedColor];
                            }
                            else
                            {
                                // Находим ближайший цвет
                                int closestIndex = FindClosestColorIndex(simplifiedColor, embroideryColors);
                                colorMatrix[x, y] = closestIndex;
                            }
                        }
                    }

                    return (embroideryColors, colorMatrix);
                }
            });
        }
        // Извлечение цветов с позициями
        private Dictionary<SKColor, QuantizedColor> ExtractColorsWithPositions(SKBitmap bitmap)
        {
            var colorDict = new Dictionary<SKColor, QuantizedColor>();

            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    var pixelColor = bitmap.GetPixel(x, y);
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
                }
            }

            return colorDict;
        }

        // Поиск ближайшего цвета
        private int FindClosestColorIndex(SKColor color, List<EmbroideryColor> colors)
        {
            int closestIndex = 0;
            int minDistance = int.MaxValue;

            for (int i = 0; i < colors.Count; i++)
            {
                int distance = Math.Abs(color.Red - colors[i].Red) +
                              Math.Abs(color.Green - colors[i].Green) +
                              Math.Abs(color.Blue - colors[i].Blue);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }

        // Сохранение матрицы в файл
        public string SaveColorMatrix(int[,] matrix, string patternName)
        {
            var safeName = string.Join("_", patternName.Split(Path.GetInvalidFileNameChars()));
            var fileName = $"matrix_{safeName}_{Guid.NewGuid()}.dat";
            var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            using (var writer = new BinaryWriter(File.OpenWrite(filePath)))
            {
                // Записываем размеры
                writer.Write(matrix.GetLength(0)); // width
                writer.Write(matrix.GetLength(1)); // height

                // Записываем матрицу индексов
                for (int y = 0; y < matrix.GetLength(1); y++)
                {
                    for (int x = 0; x < matrix.GetLength(0); x++)
                    {
                        writer.Write(matrix[x, y]);
                    }
                }
            }

            return fileName;
        }

        // Загрузка матрицы из файла
        public (int[,] matrix, int width, int height) LoadColorMatrix(string fileName)
        {
            var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Файл матрицы не найден");

            using (var reader = new BinaryReader(File.OpenRead(filePath)))
            {
                int width = reader.ReadInt32();
                int height = reader.ReadInt32();
                var matrix = new int[width, height];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        matrix[x, y] = reader.ReadInt32();
                    }
                }

                return (matrix, width, height);
            }
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


        //private SKBitmap ResizeBitmap(SKBitmap original, int width, int height)
        //{
        //    var resized = new SKBitmap(width, height);
        //    using (var canvas = new SKCanvas(resized))
        //    {
        //        var destRect = new SKRect(0, 0, width, height);
        //        var srcRect = new SKRect(0, 0, original.Width, original.Height);
        //        canvas.DrawBitmap(original, srcRect, destRect);
        //    }
        //    return resized;
        //}

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

        /// <summary>
        /// Сохраняет схему вышивки в PNG файл (ОПТИМИЗИРОВАННАЯ ВЕРСИЯ)
        /// </summary>
        public string SavePatternAsPng(
            EmbroideryPattern pattern,
            List<EmbroideryColor> colors,
            int[,] colorMatrix,
            int cellSize = 25,
            bool showSymbols = true,
            bool showGrid = true)
        {
            int width = pattern.Width * cellSize;
            int height = pattern.Height * cellSize;

            // Предварительно создаем карту цветов SKColor для быстрого доступа
            var skColors = new SKColor[colors.Count];
            for (int i = 0; i < colors.Count; i++)
            {
                skColors[i] = new SKColor((byte)colors[i].Red, (byte)colors[i].Green, (byte)colors[i].Blue);
            }

            // Предварительно создаем массив индексов для быстрого доступа к матрице
            // Сжимаем 2D матрицу в 1D массив для быстрого доступа
            int totalCells = pattern.Width * pattern.Height;
            var colorIndices = new int[totalCells];
            for (int y = 0; y < pattern.Height; y++)
            {
                for (int x = 0; x < pattern.Width; x++)
                {
                    colorIndices[y * pattern.Width + x] = colorMatrix[x, y];
                }
            }

            using (var bitmap = new SKBitmap(width, height))
            {
                // БЛОЧНАЯ ОТРИСОВКА - заполняем целыми строками цветов
                using (var canvas = new SKCanvas(bitmap))
                {
                    // Оптимизация: рисуем прямоугольниками вместо попиксельной заливки
                    for (int y = 0; y < pattern.Height; y++)
                    {
                        for (int x = 0; x < pattern.Width; x++)
                        {
                            int colorIndex = colorIndices[y * pattern.Width + x];
                            var color = skColors[colorIndex];

                            using (var paint = new SKPaint { Color = color, IsAntialias = false })
                            {
                                float left = x * cellSize;
                                float top = y * cellSize;
                                var rect = SKRect.Create(left, top, cellSize, cellSize);
                                canvas.DrawRect(rect, paint);
                            }
                        }
                    }
                }

                // Рисуем символы (если нужно) - только при большом размере клетки
                if (showSymbols && cellSize >= 20)
                {
                    using (var canvas = new SKCanvas(bitmap))
                    {
                        // Предварительно создаем краски для текста
                        var textPaints = new SKPaint[colors.Count];
                        for (int i = 0; i < colors.Count; i++)
                        {
                            var contrastColor = GetContrastColorForPng(skColors[i]);
                            textPaints[i] = new SKPaint
                            {
                                Color = contrastColor,
                                TextSize = cellSize * 0.45f,
                                TextAlign = SKTextAlign.Center,
                                IsAntialias = true,
                                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
                            };
                        }

                        for (int y = 0; y < pattern.Height; y++)
                        {
                            for (int x = 0; x < pattern.Width; x++)
                            {
                                int colorIndex = colorIndices[y * pattern.Width + x];
                                var symbol = colors[colorIndex].Symbol;

                                if (!string.IsNullOrEmpty(symbol))
                                {
                                    float centerX = x * cellSize + cellSize / 2;
                                    float centerY = y * cellSize + cellSize / 2 + textPaints[colorIndex].TextSize * 0.35f;
                                    canvas.DrawText(symbol, centerX, centerY, textPaints[colorIndex]);
                                }
                            }
                        }

                        // Освобождаем ресурсы
                        foreach (var paint in textPaints)
                        {
                            paint?.Dispose();
                        }
                    }
                }

                // Рисуем сетку (если нужно и размер клетки не слишком маленький)
                if (showGrid && cellSize >= 8)
                {
                    using (var canvas = new SKCanvas(bitmap))
                    using (var gridPaint = new SKPaint
                    {
                        Color = SKColors.Black,
                        StrokeWidth = Math.Max(0.5f, cellSize / 30f),
                        IsStroke = true,
                        IsAntialias = false  // Отключаем сглаживание для скорости
                    })
                    {
                        // Рисуем только вертикальные линии
                        for (int x = 0; x <= pattern.Width; x++)
                        {
                            float xPos = x * cellSize;
                            canvas.DrawLine(xPos, 0, xPos, height, gridPaint);
                        }

                        // Рисуем только горизонтальные линии
                        for (int y = 0; y <= pattern.Height; y++)
                        {
                            float yPos = y * cellSize;
                            canvas.DrawLine(0, yPos, width, yPos, gridPaint);
                        }
                    }
                }

                // Сохраняем в PNG с максимальной скоростью сжатия
                var safeName = string.Join("_", pattern.Name.Split(Path.GetInvalidFileNameChars()));
                var fileName = $"{safeName}_схема_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

                using (var image = SKImage.FromBitmap(bitmap))
                using (var data = image.Encode(SKEncodedImageFormat.Png, 85)) // Уменьшил качество до 85% для скорости
                {
                    using (var stream = File.Create(filePath))
                    {
                        data.SaveTo(stream);
                    }
                }

                return filePath;
            }
        }

        private SKColor GetContrastColorForPng(SKColor color)
        {
            // Для PNG используем чёрный или белый текст
            double luminance = (0.299 * color.Red + 0.587 * color.Green + 0.114 * color.Blue);
            return luminance > 128 ? SKColors.Black : SKColors.White;
        }
    }
}
