using MauiApp1.Data;
using MauiApp1.Models;
using SkiaSharp;

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

        //ProcessImageWithMatrix
        public async Task<List<EmbroideryColor>> ProcessImage(string imagePath, int targetWidth, int targetHeight)
        {
            return await Task.Run(() =>
            {
                using (var originalBitmap = FixOrientation(imagePath))
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
                using (var originalBitmap = FixOrientation(imagePath))
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

        //// var bitmap = SKBitmap.Decode(filePath);

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

        public SKBitmap FixOrientation(string filePath)
        {
            // 1. Читаем файл через кодек, чтобы узнать его реальную EXIF-ориентацию
            using (var codec = SKCodec.Create(filePath))
            {
                if (codec == null) return SKBitmap.Decode(filePath);

                // Декодируем базовое изображение
                var bitmap = SKBitmap.Decode(filePath);
                if (bitmap == null) return null;

                // Получаем метку ориентации
                var orientation = codec.EncodedOrigin;

                // Если картинка изначально ориентирована правильно (TopLeft), просто возвращаем её
                if (orientation == SKEncodedOrigin.TopLeft)
                {
                    return bitmap;
                }

                // Вычисляем, на сколько градусов и как нужно повернуть
                int degrees = 0;
                bool flip = false;

                switch (orientation)
                {
                    case SKEncodedOrigin.TopRight: // Отразить по горизонтали
                        flip = true;
                        break;
                    case SKEncodedOrigin.BottomRight: // Повернуть на 180 градусов
                        degrees = 180;
                        break;
                    case SKEncodedOrigin.BottomLeft: // Повернуть на 180 и отразить
                        degrees = 180;
                        flip = true;
                        break;
                    case SKEncodedOrigin.LeftTop: // Повернуть на 90 по часовой и отразить
                        degrees = 90;
                        flip = true;
                        break;
                    case SKEncodedOrigin.RightTop: // Повернуть на 90 по часовой
                        degrees = 90;
                        break;
                    case SKEncodedOrigin.RightBottom: // Повернуть на 270 по часовой и отразить
                        degrees = 270;
                        flip = true;
                        break;
                    case SKEncodedOrigin.LeftBottom: // Повернуть на 270 по часовой
                        degrees = 270;
                        break;
                }

                if (degrees == 0 && !flip) return bitmap;

                // Создаем новый битмап с учетом измененных размеров (если поворот на 90 или 270 градусов)
                bool isSwapped = degrees == 90 || degrees == 270;
                int newWidth = isSwapped ? bitmap.Height : bitmap.Width;
                int newHeight = isSwapped ? bitmap.Width : bitmap.Height;

                var rotatedBitmap = new SKBitmap(newWidth, newHeight);

                using (var canvas = new SKCanvas(rotatedBitmap))
                {
                    canvas.Clear(SKColors.Transparent);

                    // Сдвигаем холст в центр для корректного поворота
                    canvas.Translate(newWidth / 2f, newHeight / 2f);
                    canvas.RotateDegrees(degrees);

                    if (flip)
                    {
                        canvas.Scale(-1, 1);
                    }

                    // Рисуем старую картинку на новый повернутый холст
                    canvas.DrawBitmap(bitmap, -bitmap.Width / 2f, -bitmap.Height / 2f);
                }

                bitmap.Dispose(); // Освобождаем память от старого перевернутого битмапа
                return rotatedBitmap;
            }
        }
        //bitmap = SKBitmap

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

                // Поиск DMC и Gamma кодов
                var (dmcCode, gammaCode) = FindFlossCodes(color);

                result.Add(new EmbroideryColor
                {
                    ColorHex = ColorToHex(color),
                    Red = color.Red,
                    Green = color.Green,
                    Blue = color.Blue,
                    Symbol = symbols[i],
                    PixelCount = colors[i].Count,
                    DMCNumber = dmcCode,
                    GammaNumber = gammaCode
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

        //private string FindClosestDMC(SKColor color)
        //{
        //    // База DMC цветов (можно расширить)
        //    var dmcColors = new Dictionary<string, SKColor>
        //    {
        //        // Белые и кремовые
        //        { "BLANC", new SKColor(255, 255, 255) },
        //        { "ECRU", new SKColor(248, 240, 227) },

        //        // Черные и серые
        //        { "310", new SKColor(0, 0, 0) },
        //        { "317", new SKColor(108, 108, 108) },
        //        { "318", new SKColor(163, 163, 163) },
        //        { "415", new SKColor(204, 204, 204) },

        //        // Красные
        //        { "321", new SKColor(255, 60, 60) },
        //        { "666", new SKColor(220, 20, 20) },
        //        { "304", new SKColor(180, 30, 30) },
        //        { "498", new SKColor(150, 20, 20) },

        //        // Зеленые
        //        { "700", new SKColor(0, 150, 0) },
        //        { "701", new SKColor(0, 120, 0) },
        //        { "702", new SKColor(0, 180, 0) },
        //        { "703", new SKColor(0, 210, 0) },
        //        { "704", new SKColor(0, 200, 0) },

        //        // Синие
        //        { "796", new SKColor(0, 60, 200) },
        //        { "797", new SKColor(0, 80, 230) },
        //        { "798", new SKColor(0, 100, 250) },
        //        { "799", new SKColor(0, 130, 255) },
        //        { "800", new SKColor(0, 50, 150) },

        //        // Желтые и оранжевые
        //        { "972", new SKColor(255, 200, 0) },
        //        { "973", new SKColor(255, 150, 0) },
        //        { "742", new SKColor(255, 180, 50) },
        //        { "743", new SKColor(255, 200, 100) },

        //        // Фиолетовые
        //        { "550", new SKColor(80, 0, 120) },
        //        { "552", new SKColor(100, 30, 140) },
        //        { "553", new SKColor(130, 60, 160) },
        //        { "554", new SKColor(160, 90, 180) },

        //        // Розовые
        //        { "605", new SKColor(255, 130, 150) },
        //        { "606", new SKColor(255, 80, 100) },
        //        { "602", new SKColor(220, 60, 90) },
        //        { "603", new SKColor(200, 50, 80) },

        //        // Коричневые
        //        { "300", new SKColor(80, 40, 20) },
        //        { "301", new SKColor(100, 50, 25) },
        //        { "302", new SKColor(120, 60, 30) },
        //        { "303", new SKColor(140, 70, 35) },
        //    };

        //    // Находим ближайший цвет по сумме разниц RGB
        //    var closest = dmcColors.OrderBy(dmc =>
        //        Math.Abs(dmc.Value.Red - color.Red) +
        //        Math.Abs(dmc.Value.Green - color.Green) +
        //        Math.Abs(dmc.Value.Blue - color.Blue))
        //        .FirstOrDefault();

        //    return closest.Key;
        //}

      

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
        /// <summary>
        /// Сохраняет большую схему в несколько PNG файлов (постранично)
        /// </summary>
        public List<string> SavePatternAsPages(
            EmbroideryPattern pattern,
            List<EmbroideryColor> colors,
            int[,] colorMatrix,
            int pageWidth = 50,   // Ширина страницы в крестиках
            int pageHeight = 50,  // Высота страницы в крестиках
            int cellSize = 25,
            bool showSymbols = true,
            bool showGrid = true)
        {
            var result = new List<string>();

            // Вычисляем количество страниц
            int pagesX = (int)Math.Ceiling((double)pattern.Width / pageWidth);
            int pagesY = (int)Math.Ceiling((double)pattern.Height / pageHeight);

            int totalPages = pagesX * pagesY;
            int currentPage = 0;

            System.Diagnostics.Debug.WriteLine($"Разделение схемы {pattern.Width}x{pattern.Height} на {pagesX}x{pagesY} = {totalPages} страниц");

            // Предварительная подготовка данных
            var skColors = new SKColor[colors.Count];
            for (int i = 0; i < colors.Count; i++)
            {
                skColors[i] = new SKColor((byte)colors[i].Red, (byte)colors[i].Green, (byte)colors[i].Blue);
            }

            // Сжимаем 2D матрицу в 1D массив
            int totalCells = pattern.Width * pattern.Height;
            var colorIndices = new int[totalCells];
            for (int y = 0; y < pattern.Height; y++)
            {
                for (int x = 0; x < pattern.Width; x++)
                {
                    colorIndices[y * pattern.Width + x] = colorMatrix[x, y];
                }
            }

            // Создаем каждую страницу
            for (int pageY = 0; pageY < pagesY; pageY++)
            {
                for (int pageX = 0; pageX < pagesX; pageX++)
                {
                    currentPage++;

                    // Определяем границы текущей страницы
                    int startX = pageX * pageWidth;
                    int startY = pageY * pageHeight;
                    int endX = Math.Min(startX + pageWidth, pattern.Width);
                    int endY = Math.Min(startY + pageHeight, pattern.Height);

                    int pageWidthPx = (endX - startX) * cellSize;
                    int pageHeightPx = (endY - startY) * cellSize;

                    using (var bitmap = new SKBitmap(pageWidthPx, pageHeightPx))
                    using (var canvas = new SKCanvas(bitmap))
                    {
                        // Очищаем белым
                        canvas.Clear(SKColors.White);

                        // Рисуем клетки
                        for (int y = startY; y < endY; y++)
                        {
                            for (int x = startX; x < endX; x++)
                            {
                                int colorIndex = colorIndices[y * pattern.Width + x];
                                var color = skColors[colorIndex];

                                using (var paint = new SKPaint { Color = color, IsAntialias = false })
                                {
                                    float left = (x - startX) * cellSize;
                                    float top = (y - startY) * cellSize;
                                    var rect = SKRect.Create(left, top, cellSize, cellSize);
                                    canvas.DrawRect(rect, paint);
                                }
                            }
                        }

                        // Рисуем символы
                        if (showSymbols && cellSize >= 20)
                        {
                            var textPaints = new SKPaint[colors.Count];
                            for (int i = 0; i < colors.Count; i++)
                            {
                                var contrastColor = GetContrastColorForPng(skColors[i]);
                                textPaints[i] = new SKPaint
                                {
                                    Color = contrastColor,
                                    TextSize = cellSize * 0.45f,
                                    TextAlign = SKTextAlign.Center,
                                    IsAntialias = true
                                };
                            }

                            for (int y = startY; y < endY; y++)
                            {
                                for (int x = startX; x < endX; x++)
                                {
                                    int colorIndex = colorIndices[y * pattern.Width + x];
                                    var symbol = colors[colorIndex].Symbol;

                                    if (!string.IsNullOrEmpty(symbol))
                                    {
                                        float centerX = (x - startX) * cellSize + cellSize / 2;
                                        float centerY = (y - startY) * cellSize + cellSize / 2 + textPaints[colorIndex].TextSize * 0.35f;
                                        canvas.DrawText(symbol, centerX, centerY, textPaints[colorIndex]);
                                    }
                                }
                            }

                            foreach (var paint in textPaints)
                            {
                                paint?.Dispose();
                            }
                        }

                        // Рисуем сетку
                        if (showGrid && cellSize >= 8)
                        {
                            using (var gridPaint = new SKPaint
                            {
                                Color = SKColors.Black,
                                StrokeWidth = Math.Max(0.5f, cellSize / 30f),
                                IsStroke = true,
                                IsAntialias = false
                            })
                            {
                                for (int x = 0; x <= (endX - startX); x++)
                                {
                                    float xPos = x * cellSize;
                                    canvas.DrawLine(xPos, 0, xPos, pageHeightPx, gridPaint);
                                }

                                for (int y = 0; y <= (endY - startY); y++)
                                {
                                    float yPos = y * cellSize;
                                    canvas.DrawLine(0, yPos, pageWidthPx, yPos, gridPaint);
                                }
                            }
                        }

                        // Добавляем информацию о странице
                        using (var textPaint = new SKPaint
                        {
                            Color = SKColors.Black,
                            TextSize = cellSize * 0.8f,
                            IsAntialias = true
                        })
                        {
                            string pageInfo = $"Страница {currentPage} из {totalPages} | Строка {pageY + 1} из {pagesY} | Столбец {pageX + 1} из {pagesX}";
                            canvas.DrawText(pageInfo, 10, pageHeightPx - 10, textPaint);
                        }

                        // Сохраняем страницу
                        var safeName = string.Join("_", pattern.Name.Split(Path.GetInvalidFileNameChars()));
                        var fileName = $"{safeName}_стр{pageY + 1}_кол{pageX + 1}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                        var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

                        using (var image = SKImage.FromBitmap(bitmap))
                        using (var data = image.Encode(SKEncodedImageFormat.Png, 90))
                        {
                            using (var stream = File.Create(filePath))
                            {
                                data.SaveTo(stream);
                            }
                        }

                        result.Add(filePath);
                    }
                }
            }

            return result;
        }




        ///// <summary>
        ///// Сохраняет схему вышивки в черно-белый PNG файл (оттенки серого)
        ///// </summary>
        //public string SavePatternAsBlackAndWhitePng(
        //    EmbroideryPattern pattern,
        //    List<EmbroideryColor> colors,
        //    int[,] colorMatrix,
        //    int cellSize = 25,
        //    bool showSymbols = true,
        //    bool showGrid = true)
        //{
        //    int width = pattern.Width * cellSize;
        //    int height = pattern.Height * cellSize;

        //    // Предварительно создаем карту цветов SKColor для быстрого доступа
        //    var skColors = new SKColor[colors.Count];
        //    for (int i = 0; i < colors.Count; i++)
        //    {
        //        skColors[i] = new SKColor((byte)colors[i].Red, (byte)colors[i].Green, (byte)colors[i].Blue);
        //    }

        //    // Предварительно вычисляем яркость для каждого цвета (оттенок серого)
        //    var grayValues = new byte[colors.Count];
        //    for (int i = 0; i < colors.Count; i++)
        //    {
        //        // Формула яркости для оттенков серого
        //        byte gray = (byte)(skColors[i].Red * 0.299 + skColors[i].Green * 0.587 + skColors[i].Blue * 0.114);
        //        grayValues[i] = gray;
        //    }

        //    // Сжимаем 2D матрицу в 1D массив для быстрого доступа
        //    int totalCells = pattern.Width * pattern.Height;
        //    var colorIndices = new int[totalCells];
        //    for (int y = 0; y < pattern.Height; y++)
        //    {
        //        for (int x = 0; x < pattern.Width; x++)
        //        {
        //            colorIndices[y * pattern.Width + x] = colorMatrix[x, y];
        //        }
        //    }

        //    using (var bitmap = new SKBitmap(width, height))
        //    {
        //        // Отрисовка черно-белых клеток
        //        using (var canvas = new SKCanvas(bitmap))
        //        {
        //            for (int y = 0; y < pattern.Height; y++)
        //            {
        //                for (int x = 0; x < pattern.Width; x++)
        //                {
        //                    int colorIndex = colorIndices[y * pattern.Width + x];
        //                    byte gray = grayValues[colorIndex];
        //                    var grayColor = new SKColor(gray, gray, gray);

        //                    using (var paint = new SKPaint { Color = grayColor, IsAntialias = false })
        //                    {
        //                        float left = x * cellSize;
        //                        float top = y * cellSize;
        //                        var rect = SKRect.Create(left, top, cellSize, cellSize);
        //                        canvas.DrawRect(rect, paint);
        //                    }
        //                }
        //            }
        //        }

        //        // Рисуем символы (белые или черные для контраста)
        //        if (showSymbols && cellSize >= 20)
        //        {
        //            using (var canvas = new SKCanvas(bitmap))
        //            {
        //                for (int y = 0; y < pattern.Height; y++)
        //                {
        //                    for (int x = 0; x < pattern.Width; x++)
        //                    {
        //                        int colorIndex = colorIndices[y * pattern.Width + x];
        //                        var symbol = colors[colorIndex].Symbol;
        //                        byte gray = grayValues[colorIndex];

        //                        if (!string.IsNullOrEmpty(symbol))
        //                        {
        //                            // Выбираем контрастный цвет: на темном фоне - белый, на светлом - черный
        //                            var textColor = gray < 128 ? SKColors.White : SKColors.Black;

        //                            using (var textPaint = new SKPaint
        //                            {
        //                                Color = textColor,
        //                                TextSize = cellSize * 0.45f,
        //                                TextAlign = SKTextAlign.Center,
        //                                IsAntialias = true
        //                            })
        //                            {
        //                                float centerX = x * cellSize + cellSize / 2;
        //                                float centerY = y * cellSize + cellSize / 2 + textPaint.TextSize * 0.35f;
        //                                canvas.DrawText(symbol, centerX, centerY, textPaint);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        // Рисуем сетку
        //        if (showGrid && cellSize >= 8)
        //        {
        //            using (var canvas = new SKCanvas(bitmap))
        //            using (var gridPaint = new SKPaint
        //            {
        //                Color = SKColors.Black,
        //                StrokeWidth = Math.Max(0.5f, cellSize / 30f),
        //                IsStroke = true,
        //                IsAntialias = false
        //            })
        //            {
        //                for (int x = 0; x <= pattern.Width; x++)
        //                {
        //                    float xPos = x * cellSize;
        //                    canvas.DrawLine(xPos, 0, xPos, height, gridPaint);
        //                }

        //                for (int y = 0; y <= pattern.Height; y++)
        //                {
        //                    float yPos = y * cellSize;
        //                    canvas.DrawLine(0, yPos, width, yPos, gridPaint);
        //                }
        //            }
        //        }

        //        // Сохраняем в PNG
        //        var safeName = string.Join("_", pattern.Name.Split(Path.GetInvalidFileNameChars()));
        //        var fileName = $"{safeName}_черно-белая_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        //        var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

        //        using (var image = SKImage.FromBitmap(bitmap))
        //        using (var data = image.Encode(SKEncodedImageFormat.Png, 90))
        //        {
        //            using (var stream = File.Create(filePath))
        //            {
        //                data.SaveTo(stream);
        //            }
        //        }

        //        return filePath;
        //    }
        //}
        /// <summary>
        /// Сохраняет схему в контурном режиме (только сетка и символы, без заливки)
        /// </summary>
        public string SavePatternAsOutlinePng(
            EmbroideryPattern pattern,
            List<EmbroideryColor> colors,
            int[,] colorMatrix,
            int cellSize = 25,
            bool showSymbols = true,
            bool showGrid = true)
        {
            int width = pattern.Width * cellSize;
            int height = pattern.Height * cellSize;

            using (var bitmap = new SKBitmap(width, height))
            using (var canvas = new SKCanvas(bitmap))
            {
                // Заливаем всё белым фоном
                canvas.Clear(SKColors.White);

                // Рисуем символы
                if (showSymbols && cellSize >= 15)
                {
                    for (int y = 0; y < pattern.Height; y++)
                    {
                        for (int x = 0; x < pattern.Width; x++)
                        {
                            int colorIndex = colorMatrix[x, y];
                            var symbol = colors[colorIndex].Symbol;

                            if (!string.IsNullOrEmpty(symbol))
                            {
                                using (var textPaint = new SKPaint
                                {
                                    Color = SKColors.Black,
                                    TextSize = cellSize * 0.5f,
                                    TextAlign = SKTextAlign.Center,
                                    IsAntialias = true,
                                    Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
                                })
                                {
                                    float centerX = x * cellSize + cellSize / 2;
                                    float centerY = y * cellSize + cellSize / 2 + textPaint.TextSize * 0.35f;
                                    canvas.DrawText(symbol, centerX, centerY, textPaint);
                                }
                            }
                        }
                    }
                }

                // Рисуем сетку
                if (showGrid && cellSize >= 8)
                {
                    using (var gridPaint = new SKPaint
                    {
                        Color = SKColors.Black,
                        StrokeWidth = Math.Max(0.5f, cellSize / 30f),
                        IsStroke = true,
                        IsAntialias = false
                    })
                    {
                        // Вертикальные линии
                        for (int x = 0; x <= pattern.Width; x++)
                        {
                            float xPos = x * cellSize;
                            canvas.DrawLine(xPos, 0, xPos, height, gridPaint);
                        }

                        // Горизонтальные линии
                        for (int y = 0; y <= pattern.Height; y++)
                        {
                            float yPos = y * cellSize;
                            canvas.DrawLine(0, yPos, width, yPos, gridPaint);
                        }
                    }

                    // Толстые линии каждые 10 клеток
                    if (pattern.Width >= 10 || pattern.Height >= 10)
                    {
                        using (var thickPaint = new SKPaint
                        {
                            Color = SKColors.Black,
                            StrokeWidth = Math.Max(1.5f, cellSize / 15f),
                            IsStroke = true,
                            IsAntialias = false
                        })
                        {
                            for (int x = 10; x < pattern.Width; x += 10)
                            {
                                float xPos = x * cellSize;
                                canvas.DrawLine(xPos, 0, xPos, height, thickPaint);
                            }

                            for (int y = 10; y < pattern.Height; y += 10)
                            {
                                float yPos = y * cellSize;
                                canvas.DrawLine(0, yPos, width, yPos, thickPaint);
                            }
                        }
                    }
                }

                // Сохраняем в PNG
                var safeName = string.Join("_", pattern.Name.Split(Path.GetInvalidFileNameChars()));
                var fileName = $"{safeName}_контурная_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

                using (var image = SKImage.FromBitmap(bitmap))
                using (var data = image.Encode(SKEncodedImageFormat.Png, 90))
                {
                    using (var stream = File.Create(filePath))
                    {
                        data.SaveTo(stream);
                    }
                }

                return filePath;
            }
        }


        /// <summary>
        /// Сохраняет контурную схему постранично
        /// </summary>
        public List<string> SavePatternAsOutlinePages(
            EmbroideryPattern pattern,
            List<EmbroideryColor> colors,
            int[,] colorMatrix,
            int pageWidth = 50,
            int pageHeight = 50,
            int cellSize = 25,
            bool showSymbols = true,
            bool showGrid = true)
        {
            var result = new List<string>();

            int pagesX = (int)Math.Ceiling((double)pattern.Width / pageWidth);
            int pagesY = (int)Math.Ceiling((double)pattern.Height / pageHeight);
            int totalPages = pagesX * pagesY;
            int currentPage = 0;

            for (int pageY = 0; pageY < pagesY; pageY++)
            {
                for (int pageX = 0; pageX < pagesX; pageX++)
                {
                    currentPage++;

                    int startX = pageX * pageWidth;
                    int startY = pageY * pageHeight;
                    int endX = Math.Min(startX + pageWidth, pattern.Width);
                    int endY = Math.Min(startY + pageHeight, pattern.Height);

                    int pageWidthPx = (endX - startX) * cellSize;
                    int pageHeightPx = (endY - startY) * cellSize;

                    using (var bitmap = new SKBitmap(pageWidthPx, pageHeightPx))
                    using (var canvas = new SKCanvas(bitmap))
                    {
                        // Белый фон
                        canvas.Clear(SKColors.White);

                        // Рисуем символы
                        if (showSymbols && cellSize >= 15)
                        {
                            for (int y = startY; y < endY; y++)
                            {
                                for (int x = startX; x < endX; x++)
                                {
                                    int colorIndex = colorMatrix[x, y];
                                    var symbol = colors[colorIndex].Symbol;

                                    if (!string.IsNullOrEmpty(symbol))
                                    {
                                        using (var textPaint = new SKPaint
                                        {
                                            Color = SKColors.Black,
                                            TextSize = cellSize * 0.5f,
                                            TextAlign = SKTextAlign.Center,
                                            IsAntialias = true
                                        })
                                        {
                                            float centerX = (x - startX) * cellSize + cellSize / 2;
                                            float centerY = (y - startY) * cellSize + cellSize / 2 + textPaint.TextSize * 0.35f;
                                            canvas.DrawText(symbol, centerX, centerY, textPaint);
                                        }
                                    }
                                }
                            }
                        }

                        // Рисуем сетку
                        if (showGrid && cellSize >= 8)
                        {
                            using (var gridPaint = new SKPaint
                            {
                                Color = SKColors.Black,
                                StrokeWidth = Math.Max(0.5f, cellSize / 30f),
                                IsStroke = true,
                                IsAntialias = false
                            })
                            {
                                int cols = endX - startX;
                                int rows = endY - startY;

                                for (int x = 0; x <= cols; x++)
                                {
                                    float xPos = x * cellSize;
                                    canvas.DrawLine(xPos, 0, xPos, pageHeightPx, gridPaint);
                                }

                                for (int y = 0; y <= rows; y++)
                                {
                                    float yPos = y * cellSize;
                                    canvas.DrawLine(0, yPos, pageWidthPx, yPos, gridPaint);
                                }
                            }
                        }

                        // Добавляем номер страницы
                        using (var textPaint = new SKPaint
                        {
                            Color = SKColors.Black,
                            TextSize = cellSize * 0.8f,
                            IsAntialias = true
                        })
                        {
                            string pageInfo = $"Стр. {currentPage} из {totalPages} | {startX + 1}-{endX} x {startY + 1}-{endY}";
                            canvas.DrawText(pageInfo, 10, pageHeightPx - 10, textPaint);
                        }

                        // Сохраняем
                        var safeName = string.Join("_", pattern.Name.Split(Path.GetInvalidFileNameChars()));
                        var fileName = $"{safeName}_контурная_стр{pageY + 1}_кол{pageX + 1}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                        var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

                        using (var image = SKImage.FromBitmap(bitmap))
                        using (var data = image.Encode(SKEncodedImageFormat.Png, 90))
                        {
                            using (var stream = File.Create(filePath))
                            {
                                data.SaveTo(stream);
                            }
                        }

                        result.Add(filePath);
                    }
                }
            }

            return result;
        }
        /// <summary>
        /// Сохраняет схему на страницы A4 (легенда на отдельной странице, все страницы одинакового размера)
        /// </summary>
        public List<string> SavePatternAsA4Pages(
            EmbroideryPattern pattern,
            List<EmbroideryColor> colors,
            int[,] colorMatrix,
            int cellSize = 25,
            bool showSymbols = true,
            bool showGrid = true,
            int stitchesPerPage = 50)
        {
            var result = new List<string>();

            // Размер A4 в пикселях (при 96 DPI: 794x1123)
            int a4WidthPx = 794;
            int a4HeightPx = 1123;

            // Отступы на странице (поля)
            int margin = 40;
            int availableWidth = a4WidthPx - margin * 2;
            int availableHeight = a4HeightPx - margin * 2;

            // Количество страниц по горизонтали и вертикали
            int pagesX = (int)Math.Ceiling((double)pattern.Width / stitchesPerPage);
            int pagesY = (int)Math.Ceiling((double)pattern.Height / stitchesPerPage);

            // Размер одной страницы в крестиках (одинаковый для всех)
            int pageWidthStitches = stitchesPerPage;
            int pageHeightStitches = stitchesPerPage;

            int totalPages = pagesX * pagesY;
            int currentPage = 0;

            // Легенда будет на отдельной странице в конце
            bool hasLegendPage = true;

            // Безопасное имя для файлов
            var safeName = string.Join("_", pattern.Name.Split(Path.GetInvalidFileNameChars()));

            // Подготавливаем данные
            var skColors = new SKColor[colors.Count];
            for (int i = 0; i < colors.Count; i++)
            {
                skColors[i] = new SKColor((byte)colors[i].Red, (byte)colors[i].Green, (byte)colors[i].Blue);
            }

            // Сжимаем 2D матрицу в 1D массив
            int totalCells = pattern.Width * pattern.Height;
            var colorIndices = new int[totalCells];
            for (int y = 0; y < pattern.Height; y++)
            {
                for (int x = 0; x < pattern.Width; x++)
                {
                    colorIndices[y * pattern.Width + x] = colorMatrix[x, y];
                }
            }

            // 1. Сохраняем все страницы со схемой (одинакового размера)
            for (int pageY = 0; pageY < pagesY; pageY++)
            {
                for (int pageX = 0; pageX < pagesX; pageX++)
                {
                    currentPage++;

                    int startX = pageX * pageWidthStitches;
                    int startY = pageY * pageHeightStitches;
                    int endX = Math.Min(startX + pageWidthStitches, pattern.Width);
                    int endY = Math.Min(startY + pageHeightStitches, pattern.Height);

                    int actualPageWidthPx = (endX - startX) * cellSize;
                    int actualPageHeightPx = (endY - startY) * cellSize;

                    using (var bitmap = new SKBitmap(a4WidthPx, a4HeightPx))
                    using (var canvas = new SKCanvas(bitmap))
                    {
                        canvas.Clear(SKColors.White);

                        // Рисуем рамку страницы (для обрезки при склейке)
                        using (var cropPaint = new SKPaint { Color = SKColors.LightGray, StrokeWidth = 1, IsStroke = true })
                        {
                            canvas.DrawRect(SKRect.Create(2, 2, a4WidthPx - 4, a4HeightPx - 4), cropPaint);
                        }

                        // Центрируем схему на странице
                        float offsetX = margin + (availableWidth - actualPageWidthPx) / 2;
                        float offsetY = margin + (availableHeight - actualPageHeightPx) / 2;

                        canvas.Save();
                        canvas.Translate(offsetX, offsetY);

                        // Рисуем клетки
                        for (int y = startY; y < endY; y++)
                        {
                            for (int x = startX; x < endX; x++)
                            {
                                int colorIndex = colorIndices[y * pattern.Width + x];
                                var color = skColors[colorIndex];

                                float left = (x - startX) * cellSize;
                                float top = (y - startY) * cellSize;
                                var rect = SKRect.Create(left, top, cellSize, cellSize);

                                using (var paint = new SKPaint { Color = color, IsAntialias = false })
                                {
                                    canvas.DrawRect(rect, paint);
                                }
                            }
                        }

                        // Рисуем символы
                        if (showSymbols && cellSize >= 15)
                        {
                            for (int y = startY; y < endY; y++)
                            {
                                for (int x = startX; x < endX; x++)
                                {
                                    int colorIndex = colorIndices[y * pattern.Width + x];
                                    var symbol = colors[colorIndex].Symbol;
                                    var color = skColors[colorIndex];

                                    if (!string.IsNullOrEmpty(symbol))
                                    {
                                        using (var textPaint = new SKPaint
                                        {
                                            Color = GetContrastColorForPng(color),
                                            TextSize = cellSize * 0.45f,
                                            TextAlign = SKTextAlign.Center,
                                            IsAntialias = true,
                                            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
                                        })
                                        {
                                            float centerX = (x - startX) * cellSize + cellSize / 2;
                                            float centerY = (y - startY) * cellSize + cellSize / 2 + textPaint.TextSize * 0.35f;
                                            canvas.DrawText(symbol, centerX, centerY, textPaint);
                                        }
                                    }
                                }
                            }
                        }

                        // Рисуем сетку
                        if (showGrid && cellSize >= 8)
                        {
                            using (var gridPaint = new SKPaint
                            {
                                Color = SKColors.Black,
                                StrokeWidth = Math.Max(0.5f, cellSize / 30f),
                                IsStroke = true,
                                IsAntialias = false
                            })
                            {
                                int cols = endX - startX;
                                int rows = endY - startY;

                                for (int x = 0; x <= cols; x++)
                                {
                                    float xPos = x * cellSize;
                                    canvas.DrawLine(xPos, 0, xPos, actualPageHeightPx, gridPaint);
                                }

                                for (int y = 0; y <= rows; y++)
                                {
                                    float yPos = y * cellSize;
                                    canvas.DrawLine(0, yPos, actualPageWidthPx, yPos, gridPaint);
                                }
                            }
                        }

                        canvas.Restore();

                        // Метки для склейки (угловые метки)
                        using (var markPaint = new SKPaint { Color = SKColors.Black, StrokeWidth = 2, IsStroke = true })
                        {
                            // Верхний левый угол
                            canvas.DrawLine(10, 10, 30, 10, markPaint);
                            canvas.DrawLine(10, 10, 10, 30, markPaint);
                            // Верхний правый угол
                            canvas.DrawLine(a4WidthPx - 10, 10, a4WidthPx - 30, 10, markPaint);
                            canvas.DrawLine(a4WidthPx - 10, 10, a4WidthPx - 10, 30, markPaint);
                            // Нижний левый угол
                            canvas.DrawLine(10, a4HeightPx - 10, 30, a4HeightPx - 10, markPaint);
                            canvas.DrawLine(10, a4HeightPx - 10, 10, a4HeightPx - 30, markPaint);
                            // Нижний правый угол
                            canvas.DrawLine(a4WidthPx - 10, a4HeightPx - 10, a4WidthPx - 30, a4HeightPx - 10, markPaint);
                            canvas.DrawLine(a4WidthPx - 10, a4HeightPx - 10, a4WidthPx - 10, a4HeightPx - 30, markPaint);
                        }

                        // Номер страницы (для склейки)
                        using (var pagePaint = new SKPaint { Color = SKColors.Black, TextSize = 14, IsAntialias = true, Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold) })
                        {
                            string pageLabel = $"{pageY + 1} / {pagesY}";
                            canvas.DrawText(pageLabel, a4WidthPx - 60, a4HeightPx - 20, pagePaint);

                            // Координаты страницы
                            string coordLabel = $"({pageX + 1};{pageY + 1})";
                            canvas.DrawText(coordLabel, a4WidthPx - 60, a4HeightPx - 40, pagePaint);
                        }

                        // Сохраняем страницу
                        var fileName = $"{safeName}_A4_стр{pageY + 1}_{pageX + 1}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                        var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

                        using (var image = SKImage.FromBitmap(bitmap))
                        using (var data = image.Encode(SKEncodedImageFormat.Png, 90))
                        {
                            using (var stream = File.Create(filePath))
                            {
                                data.SaveTo(stream);
                            }
                        }

                        result.Add(filePath);
                    }
                }
            }

            // 2. Сохраняем отдельную страницу с легендой (цветов)
            if (hasLegendPage)
            {
                using (var bitmap = new SKBitmap(a4WidthPx, a4HeightPx))
                using (var canvas = new SKCanvas(bitmap))
                {
                    canvas.Clear(SKColors.White);

                    // Рамка
                    using (var borderPaint = new SKPaint { Color = SKColors.Black, StrokeWidth = 2, IsStroke = true })
                    {
                        canvas.DrawRect(SKRect.Create(10, 10, a4WidthPx - 20, a4HeightPx - 20), borderPaint);
                    }

                    // Заголовок
                    using (var titlePaint = new SKPaint
                    {
                        Color = SKColors.Black,
                        TextSize = 24,
                        IsAntialias = true,
                        Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
                    })
                    {
                        canvas.DrawText($"Схема вышивки: {pattern.Name}", 50, 60, titlePaint);
                    }

                    using (var subPaint = new SKPaint
                    {
                        Color = SKColors.Gray,
                        TextSize = 14,
                        IsAntialias = true
                    })
                    {
                        canvas.DrawText($"Размер: {pattern.Width} x {pattern.Height} крестиков", 50, 95, subPaint);
                        canvas.DrawText($"Всего цветов: {colors.Count}", 50, 120, subPaint);
                        canvas.DrawText($"Дата создания: {pattern.CreatedAt:dd.MM.yyyy}", 50, 145, subPaint);
                        canvas.DrawText($"Размер клетки: {cellSize}px", 50, 170, subPaint);
                    }

                    // Таблица цветов
                    float startY = 210;
                    float rowHeight = 45;
                    float colSymbol = 60;
                    float colColor = 70;
                    float colHex = 100;
                    float colDMC = 100;
                    float colCount = 120;

                    // Заголовки таблицы
                    using (var headerPaint = new SKPaint
                    {
                        Color = SKColors.White,
                        TextSize = 12,
                        IsAntialias = true,
                        Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
                    })
                    {
                        using (var headerBg = new SKPaint { Color = SKColors.DarkGray })
                        {
                            canvas.DrawRect(SKRect.Create(40, startY - 5, a4WidthPx - 80, 25), headerBg);
                        }
                        canvas.DrawText("Символ", 50, startY + 8, headerPaint);
                        canvas.DrawText("Образец", 50 + colSymbol, startY + 8, headerPaint);
                        canvas.DrawText("HEX", 50 + colSymbol + colColor, startY + 8, headerPaint);
                        canvas.DrawText("DMC", 50 + colSymbol + colColor + colHex, startY + 8, headerPaint);
                        canvas.DrawText("Количество", 50 + colSymbol + colColor + colHex + colDMC, startY + 8, headerPaint);
                    }

                    float currentY = startY + 30;
                    for (int i = 0; i < colors.Count; i++)
                    {
                        var color = colors[i];
                        var skColor = new SKColor((byte)color.Red, (byte)color.Green, (byte)color.Blue);

                        // Символ
                        using (var symbolPaint = new SKPaint
                        {
                            Color = GetContrastColorForPng(skColor),
                            TextSize = 16,
                            TextAlign = SKTextAlign.Center,
                            IsAntialias = true,
                            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
                        })
                        {
                            canvas.DrawText(color.Symbol ?? "●", 50 + 20, currentY + 15, symbolPaint);
                        }

                        // Образец цвета
                        using (var colorPaint = new SKPaint { Color = skColor })
                        {
                            canvas.DrawRect(SKRect.Create(50 + colSymbol, currentY, 25, 25), colorPaint);
                        }
                        using (var borderPaint = new SKPaint { Color = SKColors.Black, StrokeWidth = 1, IsStroke = true })
                        {
                            canvas.DrawRect(SKRect.Create(50 + colSymbol, currentY, 25, 25), borderPaint);
                        }

                        // Информация
                        using (var textPaint = new SKPaint { Color = SKColors.Black, TextSize = 11, IsAntialias = true })
                        {
                            canvas.DrawText(color.ColorHex, 50 + colSymbol + colColor, currentY + 17, textPaint);
                            canvas.DrawText(color.DMCNumber ?? "—", 50 + colSymbol + colColor + colHex, currentY + 17, textPaint);
                            canvas.DrawText($"{color.PixelCount} шт.", 50 + colSymbol + colColor + colHex + colDMC, currentY + 17, textPaint);
                        }

                        currentY += rowHeight;

                        // Если не помещается на страницу
                        if (currentY > a4HeightPx - 50 && i < colors.Count - 1)
                        {
                            using (var notePaint = new SKPaint { Color = SKColors.Red, TextSize = 10, IsAntialias = true })
                            {
                                canvas.DrawText("Продолжение на следующей странице...", 50, currentY + 20, notePaint);
                            }
                            break;
                        }
                    }

                    // Сохраняем легенду
                    var legendFileName = $"{safeName}_легенда_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                    var legendFilePath = Path.Combine(FileSystem.CacheDirectory, legendFileName);

                    using (var image = SKImage.FromBitmap(bitmap))
                    using (var data = image.Encode(SKEncodedImageFormat.Png, 90))
                    {
                        using (var stream = File.Create(legendFilePath))
                        {
                            data.SaveTo(stream);
                        }
                    }

                    result.Add(legendFilePath);
                }
            }

            return result;
        }

        private List<EmbroideryColor> GetUniqueColorsOnPage(int[,] colorMatrix, List<EmbroideryColor> colors, int startX, int endX, int startY, int endY)
        {
            var uniqueIndices = new HashSet<int>();

            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    uniqueIndices.Add(colorMatrix[x, y]);
                }
            }

            return uniqueIndices.Select(i => colors[i]).ToList();
        }

        /// <summary>
        /// Сохраняет контурную схему на страницы A4 (легенда только на первой странице, одинаковый масштаб)
        /// </summary>
        public List<string> SavePatternAsOutlineA4Pages(
            EmbroideryPattern pattern,
            List<EmbroideryColor> colors,
            int[,] colorMatrix,
            int cellSize = 25,
            bool showSymbols = true,
            bool showGrid = true,
            int stitchesPerPage = 50)
        {
            var result = new List<string>();

            float a4Width = 210f;
            float a4Height = 297f;
            float a4Ratio = a4Width / a4Height;

            int legendWidth = 220;
            int pageWidthStitches = stitchesPerPage;
            int pageHeightStitches = (int)(stitchesPerPage / a4Ratio);
            int firstPageWidthStitches = (int)((a4Width - legendWidth) / a4Width * stitchesPerPage);
            firstPageWidthStitches = Math.Max(firstPageWidthStitches, 30);

            int pagesX = (int)Math.Ceiling((double)pattern.Width / pageWidthStitches);
            int pagesY = (int)Math.Ceiling((double)pattern.Height / pageHeightStitches);
            int totalPages = pagesX * pagesY;
            int currentPage = 0;

            for (int pageY = 0; pageY < pagesY; pageY++)
            {
                for (int pageX = 0; pageX < pagesX; pageX++)
                {
                    currentPage++;

                    int startX = pageX * pageWidthStitches;
                    int startY = pageY * pageHeightStitches;
                    int currentPageWidthStitches = (currentPage == 1) ? firstPageWidthStitches : pageWidthStitches;

                    int endX = Math.Min(startX + currentPageWidthStitches, pattern.Width);
                    int endY = Math.Min(startY + pageHeightStitches, pattern.Height);

                    if (endX <= startX || endY <= startY) continue;

                    int pageWidthPx = (endX - startX) * cellSize;
                    int pageHeightPx = (endY - startY) * cellSize;

                    bool hasLegend = (currentPage == 1);
                    int totalWidth = pageWidthPx + (hasLegend ? legendWidth + 30 : 30);
                    int totalHeight = Math.Max(pageHeightPx + 60, 800);

                    using (var bitmap = new SKBitmap(totalWidth, totalHeight))
                    using (var canvas = new SKCanvas(bitmap))
                    {
                        canvas.Clear(SKColors.White);

                        using (var borderPaint = new SKPaint { Color = SKColors.Black, StrokeWidth = 2, IsStroke = true })
                        {
                            canvas.DrawRect(SKRect.Create(2, 2, totalWidth - 4, totalHeight - 4), borderPaint);
                        }

                        canvas.Save();
                        canvas.Translate(20, 30);

                        // Рисуем символы
                        if (showSymbols && cellSize >= 15)
                        {
                            for (int y = startY; y < endY; y++)
                            {
                                for (int x = startX; x < endX; x++)
                                {
                                    int colorIndex = colorMatrix[x, y];
                                    var symbol = colors[colorIndex].Symbol;

                                    if (!string.IsNullOrEmpty(symbol))
                                    {
                                        using (var textPaint = new SKPaint
                                        {
                                            Color = SKColors.Black,
                                            TextSize = cellSize * 0.45f,
                                            TextAlign = SKTextAlign.Center,
                                            IsAntialias = true,
                                            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
                                        })
                                        {
                                            float centerX = (x - startX) * cellSize + cellSize / 2;
                                            float centerY = (y - startY) * cellSize + cellSize / 2 + textPaint.TextSize * 0.35f;
                                            canvas.DrawText(symbol, centerX, centerY, textPaint);
                                        }
                                    }
                                }
                            }
                        }

                        // Рисуем сетку
                        if (showGrid && cellSize >= 8)
                        {
                            using (var gridPaint = new SKPaint
                            {
                                Color = SKColors.Black,
                                StrokeWidth = Math.Max(0.5f, cellSize / 30f),
                                IsStroke = true,
                                IsAntialias = false
                            })
                            {
                                int cols = endX - startX;
                                int rows = endY - startY;

                                for (int x = 0; x <= cols; x++)
                                {
                                    float xPos = x * cellSize;
                                    canvas.DrawLine(xPos, 0, xPos, pageHeightPx, gridPaint);
                                }

                                for (int y = 0; y <= rows; y++)
                                {
                                    float yPos = y * cellSize;
                                    canvas.DrawLine(0, yPos, pageWidthPx, yPos, gridPaint);
                                }
                            }
                        }

                        canvas.Restore();
                        //ProcessImage
                        // Легенда только на первой странице (цветная для контраста)
                        if (hasLegend)
                        {
                            float legendX = pageWidthPx + 40;
                            float legendY = 30;

                            using (var titlePaint = new SKPaint
                            {
                                Color = SKColors.Black,
                                TextSize = 16,
                                IsAntialias = true,
                                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
                            })
                            {
                                canvas.DrawText("Список цветов", legendX, legendY, titlePaint);
                            }

                            using (var subPaint = new SKPaint
                            {
                                Color = SKColors.Gray,
                                TextSize = 11,
                                IsAntialias = true
                            })
                            {
                                canvas.DrawText($"Всего цветов: {colors.Count}", legendX, legendY + 22, subPaint);
                            }

                            float yOffset = legendY + 55;
                            int maxColorsOnPage = (int)((totalHeight - yOffset) / 44);
                            int colorsToShow = Math.Min(colors.Count, maxColorsOnPage);

                            for (int i = 0; i < colorsToShow; i++)
                            {
                                var color = colors[i];
                                var skColor = new SKColor((byte)color.Red, (byte)color.Green, (byte)color.Blue);

                                using (var colorPaint = new SKPaint { Color = skColor })
                                {
                                    canvas.DrawRect(SKRect.Create(legendX, yOffset, 22, 22), colorPaint);
                                }

                                using (var borderPaint = new SKPaint { Color = SKColors.Black, StrokeWidth = 1, IsStroke = true })
                                {
                                    canvas.DrawRect(SKRect.Create(legendX, yOffset, 22, 22), borderPaint);
                                }

                                using (var symbolPaint = new SKPaint
                                {
                                    Color = GetContrastColorForPng(skColor),
                                    TextSize = 12,
                                    TextAlign = SKTextAlign.Center,
                                    IsAntialias = true,
                                    Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
                                })
                                {
                                    canvas.DrawText(color.Symbol ?? "●", legendX + 11, yOffset + 16, symbolPaint);
                                }

                                using (var textPaint = new SKPaint { Color = SKColors.Black, TextSize = 10, IsAntialias = true })
                                {
                                    canvas.DrawText($"{color.ColorHex}", legendX + 32, yOffset + 9, textPaint);
                                    canvas.DrawText($"DMC: {color.DMCNumber ?? "—"}", legendX + 32, yOffset + 20, textPaint);
                                    canvas.DrawText($"{color.PixelCount} шт.", legendX + 32, yOffset + 31, textPaint);
                                }

                                yOffset += 44;
                            }
                        }

                        using (var footerPaint = new SKPaint { Color = SKColors.Gray, TextSize = 9, IsAntialias = true })
                        {
                            string pageInfo = $"Страница {currentPage} из {totalPages} | {startX + 1}-{endX} / {startY + 1}-{endY} крестиков";
                            canvas.DrawText(pageInfo, 20, totalHeight - 18, footerPaint);

                            string patternInfo = $"{pattern.Name} | {pattern.Width}x{pattern.Height} крестиков";
                            canvas.DrawText(patternInfo, 20, totalHeight - 8, footerPaint);

                            string scaleInfo = $"Масштаб: 1 клетка = {cellSize}px";
                            canvas.DrawText(scaleInfo, totalWidth - 150, totalHeight - 8, footerPaint);
                        }

                        var safeName = string.Join("_", pattern.Name.Split(Path.GetInvalidFileNameChars()));
                        var fileName = hasLegend
                            ? $"{safeName}_контурная_A4_стр{currentPage}_с_легендой_{DateTime.Now:yyyyMMdd_HHmmss}.png"
                            : $"{safeName}_контурная_A4_стр{currentPage}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                        var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

                        using (var image = SKImage.FromBitmap(bitmap))
                        using (var data = image.Encode(SKEncodedImageFormat.Png, 90))
                        {
                            using (var stream = File.Create(filePath))
                            {
                                data.SaveTo(stream);
                            }
                        }

                        result.Add(filePath);
                    }
                }
            }

            return result;
        }


        // Вместо старого FindClosestDMC
        private string FindDMCAndGamma(SKColor color)
        {
            var match = FlossLibrary.FindClosest(color.Red, color.Green, color.Blue);
            if (match != null)
            {
                // Можно сохранить оба кода
                return $"{match.CodeDmc} / {match.CodeGamma}";
            }
            return "— / —";
        }

        // Или отдельно для каждого поля в EmbroideryColor
        private (string dmc, string gamma) FindFlossCodes(SKColor color)
        {
            var match = FlossLibrary.FindClosest(color.Red, color.Green, color.Blue);
            return (match?.CodeDmc ?? "—", match?.CodeGamma ?? "—");
        }






        private SKColor GetContrastColorForPng(SKColor color)
        {
            // Для PNG используем чёрный или белый текст
            double luminance = (0.299 * color.Red + 0.587 * color.Green + 0.114 * color.Blue);
            return luminance > 128 ? SKColors.Black : SKColors.White;
        }
    }
}
