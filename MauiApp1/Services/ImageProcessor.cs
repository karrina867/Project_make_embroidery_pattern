using MauiApp1.Data;
using MauiApp1.Models;
using SkiaSharp;

namespace MauiApp1.Services
{
    public class ImageProcessor
    {
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
                using (var originalBitmap = FixOrientation(imagePath))
                {
                    if (originalBitmap == null)
                        throw new Exception("Не удалось загрузить изображение");

                    var resizedBitmap = ResizeBitmap(originalBitmap, targetWidth, targetHeight);

                    var colors = ExtractColors(resizedBitmap);

                    var reducedColors = ReduceColors(colors, maxColors: 50);

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

                    var resizedBitmap = ResizeBitmap(originalBitmap, targetWidth, targetHeight);

                    var colorDict = ExtractColorsWithPositions(resizedBitmap);

                    var reducedColors = ReduceColors(colorDict.Values.ToList(), maxColors: 50);

                    var embroideryColors = AssignSymbols(reducedColors);

                    var colorMap = new Dictionary<SKColor, int>();
                    for (int i = 0; i < embroideryColors.Count; i++)
                    {
                        var color = new SKColor(
                            (byte)embroideryColors[i].Red,
                            (byte)embroideryColors[i].Green,
                            (byte)embroideryColors[i].Blue);
                        colorMap[color] = i;
                    }

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
                                int closestIndex = FindClosestColorIndex(simplifiedColor, embroideryColors);
                                colorMatrix[x, y] = closestIndex;
                            }
                        }
                    }

                    return (embroideryColors, colorMatrix);
                }
            });
        }
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

        public string SaveColorMatrix(int[,] matrix, string patternName)
        {
            var safeName = string.Join("_", patternName.Split(Path.GetInvalidFileNameChars()));
            var fileName = $"matrix_{safeName}_{Guid.NewGuid()}.dat";
            var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            using (var writer = new BinaryWriter(File.OpenWrite(filePath)))
            {
                writer.Write(matrix.GetLength(0)); 
                writer.Write(matrix.GetLength(1)); 

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

            using (var codec = SKCodec.Create(filePath))
            {
                if (codec == null) return SKBitmap.Decode(filePath);

                var bitmap = SKBitmap.Decode(filePath);
                if (bitmap == null) return null;

                var orientation = codec.EncodedOrigin;

                if (orientation == SKEncodedOrigin.TopLeft)
                {
                    return bitmap;
                }

                int degrees = 0;
                bool flip = false;

                switch (orientation)
                {
                    case SKEncodedOrigin.TopRight: 
                        flip = true;
                        break;
                    case SKEncodedOrigin.BottomRight: 
                        degrees = 180;
                        break;
                    case SKEncodedOrigin.BottomLeft: 
                        degrees = 180;
                        flip = true;
                        break;
                    case SKEncodedOrigin.LeftTop: 
                        degrees = 90;
                        flip = true;
                        break;
                    case SKEncodedOrigin.RightTop:
                        degrees = 90;
                        break;
                    case SKEncodedOrigin.RightBottom: 
                        degrees = 270;
                        flip = true;
                        break;
                    case SKEncodedOrigin.LeftBottom: 
                        degrees = 270;
                        break;
                }

                if (degrees == 0 && !flip) return bitmap;

                bool isSwapped = degrees == 90 || degrees == 270;
                int newWidth = isSwapped ? bitmap.Height : bitmap.Width;
                int newHeight = isSwapped ? bitmap.Width : bitmap.Height;

                var rotatedBitmap = new SKBitmap(newWidth, newHeight);

                using (var canvas = new SKCanvas(rotatedBitmap))
                {
                    canvas.Clear(SKColors.Transparent);

                    canvas.Translate(newWidth / 2f, newHeight / 2f);
                    canvas.RotateDegrees(degrees);

                    if (flip)
                    {
                        canvas.Scale(-1, 1);
                    }

                    canvas.DrawBitmap(bitmap, -bitmap.Width / 2f, -bitmap.Height / 2f);
                }

                bitmap.Dispose(); 
                return rotatedBitmap;
            }
        }

        private List<QuantizedColor> ExtractColors(SKBitmap bitmap)
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
                    colorDict[simplifiedColor].Positions.Add(new SKPointI(x, y));
                }
            }

            return colorDict.Values.OrderByDescending(c => c.Count).ToList();
        }

        private SKColor SimplifyColor(SKColor color, int step = 32)
        {

            byte r = (byte)((color.Red / step) * step);
            byte g = (byte)((color.Green / step) * step);
            byte b = (byte)((color.Blue / step) * step);
            return new SKColor(r, g, b);
        }

        private List<QuantizedColor> ReduceColors(List<QuantizedColor> colors, int maxColors)
        {
            if (colors.Count <= maxColors)
                return colors;

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

        public async Task<string> SavePatternMatrix(int[,] colorIndices, List<EmbroideryColor> colors)
        {
            return await Task.Run(() =>
            {
                var fileName = $"pattern_{Guid.NewGuid()}.pmat";
                var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

                using (var writer = new StreamWriter(filePath))
                {
                    writer.WriteLine($"{colorIndices.GetLength(0)},{colorIndices.GetLength(1)}");

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

            var skColors = new SKColor[colors.Count];
            for (int i = 0; i < colors.Count; i++)
            {
                skColors[i] = new SKColor((byte)colors[i].Red, (byte)colors[i].Green, (byte)colors[i].Blue);
            }

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

                using (var canvas = new SKCanvas(bitmap))
                {

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

                if (showSymbols && cellSize >= 20)
                {
                    using (var canvas = new SKCanvas(bitmap))
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

                        foreach (var paint in textPaints)
                        {
                            paint?.Dispose();
                        }
                    }
                }

                if (showGrid && cellSize >= 8)
                {
                    using (var canvas = new SKCanvas(bitmap))
                    using (var gridPaint = new SKPaint
                    {
                        Color = SKColors.Black,
                        StrokeWidth = Math.Max(0.5f, cellSize / 30f),
                        IsStroke = true,
                        IsAntialias = false 
                    })
                    {
                        for (int x = 0; x <= pattern.Width; x++)
                        {
                            float xPos = x * cellSize;
                            canvas.DrawLine(xPos, 0, xPos, height, gridPaint);
                        }

                        for (int y = 0; y <= pattern.Height; y++)
                        {
                            float yPos = y * cellSize;
                            canvas.DrawLine(0, yPos, width, yPos, gridPaint);
                        }
                    }
                }

                var safeName = string.Join("_", pattern.Name.Split(Path.GetInvalidFileNameChars()));
                var fileName = $"{safeName}_схема_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

                using (var image = SKImage.FromBitmap(bitmap))
                using (var data = image.Encode(SKEncodedImageFormat.Png, 85))
                {
                    using (var stream = File.Create(filePath))
                    {
                        data.SaveTo(stream);
                    }
                }

                return filePath;
            }
        }

        public List<string> SavePatternAsPages(
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

            System.Diagnostics.Debug.WriteLine($"Разделение схемы {pattern.Width}x{pattern.Height} на {pagesX}x{pagesY} = {totalPages} страниц");

            var skColors = new SKColor[colors.Count];
            for (int i = 0; i < colors.Count; i++)
            {
                skColors[i] = new SKColor((byte)colors[i].Red, (byte)colors[i].Green, (byte)colors[i].Blue);
            }

            int totalCells = pattern.Width * pattern.Height;
            var colorIndices = new int[totalCells];
            for (int y = 0; y < pattern.Height; y++)
            {
                for (int x = 0; x < pattern.Width; x++)
                {
                    colorIndices[y * pattern.Width + x] = colorMatrix[x, y];
                }
            }

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

                        canvas.Clear(SKColors.White);

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
                canvas.Clear(SKColors.White);

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

                        for (int x = 0; x <= pattern.Width; x++)
                        {
                            float xPos = x * cellSize;
                            canvas.DrawLine(xPos, 0, xPos, height, gridPaint);
                        }

                        for (int y = 0; y <= pattern.Height; y++)
                        {
                            float yPos = y * cellSize;
                            canvas.DrawLine(0, yPos, width, yPos, gridPaint);
                        }
                    }

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
                        canvas.Clear(SKColors.White);

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

            int a4WidthPx = 794;
            int a4HeightPx = 1123;

            int margin = 40;
            int availableWidth = a4WidthPx - margin * 2;
            int availableHeight = a4HeightPx - margin * 2;

            int pagesX = (int)Math.Ceiling((double)pattern.Width / stitchesPerPage);
            int pagesY = (int)Math.Ceiling((double)pattern.Height / stitchesPerPage);

            int pageWidthStitches = stitchesPerPage;
            int pageHeightStitches = stitchesPerPage;

            int totalPages = pagesX * pagesY;
            int currentPage = 0;

            bool hasLegendPage = true;

            var safeName = string.Join("_", pattern.Name.Split(Path.GetInvalidFileNameChars()));

            var skColors = new SKColor[colors.Count];
            for (int i = 0; i < colors.Count; i++)
            {
                skColors[i] = new SKColor((byte)colors[i].Red, (byte)colors[i].Green, (byte)colors[i].Blue);
            }

            int totalCells = pattern.Width * pattern.Height;
            var colorIndices = new int[totalCells];
            for (int y = 0; y < pattern.Height; y++)
            {
                for (int x = 0; x < pattern.Width; x++)
                {
                    colorIndices[y * pattern.Width + x] = colorMatrix[x, y];
                }
            }

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

                        using (var cropPaint = new SKPaint { Color = SKColors.LightGray, StrokeWidth = 1, IsStroke = true })
                        {
                            canvas.DrawRect(SKRect.Create(2, 2, a4WidthPx - 4, a4HeightPx - 4), cropPaint);
                        }

                        float offsetX = margin + (availableWidth - actualPageWidthPx) / 2;
                        float offsetY = margin + (availableHeight - actualPageHeightPx) / 2;

                        canvas.Save();
                        canvas.Translate(offsetX, offsetY);

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

                        using (var markPaint = new SKPaint { Color = SKColors.Black, StrokeWidth = 2, IsStroke = true })
                        {
                            canvas.DrawLine(10, 10, 30, 10, markPaint);
                            canvas.DrawLine(10, 10, 10, 30, markPaint);

                            canvas.DrawLine(a4WidthPx - 10, 10, a4WidthPx - 30, 10, markPaint);
                            canvas.DrawLine(a4WidthPx - 10, 10, a4WidthPx - 10, 30, markPaint);

                            canvas.DrawLine(10, a4HeightPx - 10, 30, a4HeightPx - 10, markPaint);
                            canvas.DrawLine(10, a4HeightPx - 10, 10, a4HeightPx - 30, markPaint);

                            canvas.DrawLine(a4WidthPx - 10, a4HeightPx - 10, a4WidthPx - 30, a4HeightPx - 10, markPaint);
                            canvas.DrawLine(a4WidthPx - 10, a4HeightPx - 10, a4WidthPx - 10, a4HeightPx - 30, markPaint);
                        }

                        using (var pagePaint = new SKPaint { Color = SKColors.Black, TextSize = 14, IsAntialias = true, Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold) })
                        {
                            string pageLabel = $"{pageY + 1} / {pagesY}";
                            canvas.DrawText(pageLabel, a4WidthPx - 60, a4HeightPx - 20, pagePaint);

                            string coordLabel = $"({pageX + 1};{pageY + 1})";
                            canvas.DrawText(coordLabel, a4WidthPx - 60, a4HeightPx - 40, pagePaint);
                        }

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

            if (hasLegendPage)
            {
                using (var bitmap = new SKBitmap(a4WidthPx, a4HeightPx))
                using (var canvas = new SKCanvas(bitmap))
                {
                    canvas.Clear(SKColors.White);

                    using (var borderPaint = new SKPaint { Color = SKColors.Black, StrokeWidth = 2, IsStroke = true })
                    {
                        canvas.DrawRect(SKRect.Create(10, 10, a4WidthPx - 20, a4HeightPx - 20), borderPaint);
                    }

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

                    float startY = 210;
                    float rowHeight = 45;
                    float colSymbol = 60;
                    float colColor = 70;
                    float colHex = 100;
                    float colDMC = 100;
                    float colCount = 120;

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

                        using (var colorPaint = new SKPaint { Color = skColor })
                        {
                            canvas.DrawRect(SKRect.Create(50 + colSymbol, currentY, 25, 25), colorPaint);
                        }
                        using (var borderPaint = new SKPaint { Color = SKColors.Black, StrokeWidth = 1, IsStroke = true })
                        {
                            canvas.DrawRect(SKRect.Create(50 + colSymbol, currentY, 25, 25), borderPaint);
                        }

                        using (var textPaint = new SKPaint { Color = SKColors.Black, TextSize = 11, IsAntialias = true })
                        {
                            canvas.DrawText(color.ColorHex, 50 + colSymbol + colColor, currentY + 17, textPaint);
                            canvas.DrawText(color.DMCNumber ?? "—", 50 + colSymbol + colColor + colHex, currentY + 17, textPaint);
                            canvas.DrawText($"{color.PixelCount} шт.", 50 + colSymbol + colColor + colHex + colDMC, currentY + 17, textPaint);
                        }

                        currentY += rowHeight;

                        if (currentY > a4HeightPx - 50 && i < colors.Count - 1)
                        {
                            using (var notePaint = new SKPaint { Color = SKColors.Red, TextSize = 10, IsAntialias = true })
                            {
                                canvas.DrawText("Продолжение на следующей странице...", 50, currentY + 20, notePaint);
                            }
                            break;
                        }
                    }

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

        //private List<EmbroideryColor> GetUniqueColorsOnPage(int[,] colorMatrix, List<EmbroideryColor> colors, int startX, int endX, int startY, int endY)
        //{
        //    var uniqueIndices = new HashSet<int>();

        //    for (int y = startY; y < endY; y++)
        //    {
        //        for (int x = startX; x < endX; x++)
        //        {
        //            uniqueIndices.Add(colorMatrix[x, y]);
        //        }
        //    }

        //    return uniqueIndices.Select(i => colors[i]).ToList();
        //}

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

        //private string FindDMCAndGamma(SKColor color)
        //{
        //    var match = FlossLibrary.FindClosest(color.Red, color.Green, color.Blue);
        //    if (match != null)
        //    {
        //        // Можно сохранить оба кода
        //        return $"{match.CodeDmc} / {match.CodeGamma}";
        //    }
        //    return "— / —";
        //}

        private (string dmc, string gamma) FindFlossCodes(SKColor color)
        {
            var match = FlossLibrary.FindClosest(color.Red, color.Green, color.Blue);
            return (match?.CodeDmc ?? "—", match?.CodeGamma ?? "—");
        }

        private SKColor GetContrastColorForPng(SKColor color)
        {
            double luminance = (0.299 * color.Red + 0.587 * color.Green + 0.114 * color.Blue);
            return luminance > 128 ? SKColors.Black : SKColors.White;
        }

        public List<string> ExportToA4Auto(
            EmbroideryPattern pattern,
            List<EmbroideryColor> colors,
            int[,] colorMatrix,
            bool isOutline)
        {
            const int cellSize = 20;//РАЗМЕР КЛЕТКИ
            const int a4W = 794;
            const int a4H = 1123;
            const int margin = 30;
            const int legendW = 200;
            const int legendRowH = 36;

            int availW = a4W - margin * 2;
            int availH = a4H - margin * 2;

            int maxCols = availW / cellSize;
            int maxRows = availH / cellSize;

            int availWwithLegend = availW - legendW - 10;
            int maxColsWithLegend = availWwithLegend / cellSize;

            int legendHeaderH = 60;
            int legendTotalH = legendHeaderH + colors.Count * legendRowH;

            bool fitsOnOnePage =
                pattern.Width <= maxColsWithLegend &&
                pattern.Height <= maxRows &&
                legendTotalH <= availH;

            if (fitsOnOnePage)
            {
                var page = RenderA4Page(
                    pattern, colors, colorMatrix, isOutline,
                    cellSize, a4W, a4H, margin,
                    srcX: 0, srcY: 0,
                    srcW: pattern.Width, srcH: pattern.Height,
                    pageNum: 1, totalPages: 1,
                    includeLegend: true, legendW: legendW, legendRowH: legendRowH);

                var safeName = string.Join("_", pattern.Name.Split(Path.GetInvalidFileNameChars()));
                var path = Path.Combine(FileSystem.CacheDirectory,
                    $"{safeName}_{DateTime.Now:yyyyMMdd_HHmmss}_1.png");
                SaveBitmap(page, path);
                page.Dispose();
                return new List<string> { path };
            }

            var result = new List<string>();
            var safeName2 = string.Join("_", pattern.Name.Split(Path.GetInvalidFileNameChars()));

            int pagesX = (int)Math.Ceiling((double)pattern.Width / maxCols);
            int pagesY = (int)Math.Ceiling((double)pattern.Height / maxRows);
            int totalPages = pagesX * pagesY + 1; 
            int pageNum = 0;

            for (int py = 0; py < pagesY; py++)
            {
                for (int px = 0; px < pagesX; px++)
                {
                    pageNum++;
                    int sx = px * maxCols;
                    int sy = py * maxRows;
                    int sw = Math.Min(maxCols, pattern.Width - sx);
                    int sh = Math.Min(maxRows, pattern.Height - sy);

                    var bmp = RenderA4Page(
                        pattern, colors, colorMatrix, isOutline,
                        cellSize, a4W, a4H, margin,
                        sx, sy, sw, sh,
                        pageNum, totalPages,
                        includeLegend: false, legendW: legendW, legendRowH: legendRowH);

                    var path = Path.Combine(FileSystem.CacheDirectory,
                        $"{safeName2}_{DateTime.Now:yyyyMMdd_HHmmss}_{pageNum}.png");
                    SaveBitmap(bmp, path);
                    bmp.Dispose();
                    result.Add(path);
                }
            }

            pageNum++;
            var legendPage = RenderLegendPage(pattern, colors, a4W, a4H, margin, cellSize, pageNum, totalPages);
            var legendPath = Path.Combine(FileSystem.CacheDirectory,
                $"{safeName2}_{DateTime.Now:yyyyMMdd_HHmmss}_{pageNum}_легенда.png");
            SaveBitmap(legendPage, legendPath);
            legendPage.Dispose();
            result.Add(legendPath);

            return result;
        }

        private SKBitmap RenderA4Page(
      EmbroideryPattern pattern,
      List<EmbroideryColor> colors,
      int[,] colorMatrix,
      bool isOutline,
      int cellSize, int a4W, int a4H, int margin,
      int srcX, int srcY, int srcW, int srcH,
      int pageNum, int totalPages,
      bool includeLegend, int legendW, int legendRowH)
        {
            var skColors = new SKColor[colors.Count];
            for (int i = 0; i < colors.Count; i++)
                skColors[i] = new SKColor((byte)colors[i].Red, (byte)colors[i].Green, (byte)colors[i].Blue);

            var bitmap = new SKBitmap(a4W, a4H);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.White);

            using (var borderPaint = new SKPaint { Color = new SKColor(200, 200, 200), StrokeWidth = 1, IsStroke = true })
                canvas.DrawRect(SKRect.Create(5, 5, a4W - 10, a4H - 10), borderPaint);

            int schemaW = srcW * cellSize;
            int schemaH = srcH * cellSize;

            int totalContentW = schemaW + (includeLegend ? legendW + 10 : 0);
            int offsetX = margin + Math.Max(0, ((a4W - margin * 2 - (includeLegend ? legendW + 10 : 0)) - schemaW) / 2);
            int offsetY = margin + Math.Max(0, (a4H - margin * 2 - 40 - schemaH) / 2);

            canvas.Save();
            canvas.Translate(offsetX, offsetY);

            for (int y = 0; y < srcH; y++)
            {
                for (int x = 0; x < srcW; x++)
                {
                    int ci = colorMatrix[srcX + x, srcY + y];
                    float left = x * cellSize;
                    float top = y * cellSize;
                    var rect = SKRect.Create(left, top, cellSize, cellSize);

                    if (!isOutline)
                    {
                        using var fillPaint = new SKPaint { Color = skColors[ci], IsAntialias = false };
                        canvas.DrawRect(rect, fillPaint);
                    }

                    var symbol = colors[ci].Symbol;
                    if (!string.IsNullOrEmpty(symbol))
                    {
                        var textColor = isOutline ? SKColors.Black : GetContrastColorForPng(skColors[ci]);
                        using var textPaint = new SKPaint
                        {
                            Color = textColor,
                            TextSize = cellSize * 0.45f,
                            TextAlign = SKTextAlign.Center,
                            IsAntialias = true,
                            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold,
                                SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
                        };
                        canvas.DrawText(symbol, left + cellSize / 2f,
                            top + cellSize / 2f + textPaint.TextSize * 0.35f, textPaint);
                    }
                }
            }

            using (var gridPaint = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = 1f,
                IsStroke = true,
                IsAntialias = false
            })
            {
                for (int x = 0; x <= srcW; x++)
                    canvas.DrawLine(x * cellSize, 0, x * cellSize, schemaH, gridPaint);
                for (int y = 0; y <= srcH; y++)
                    canvas.DrawLine(0, y * cellSize, schemaW, y * cellSize, gridPaint);
            }

            using (var thickPaint = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = 2.5f,
                IsStroke = true,
                IsAntialias = false
            })
            {
                for (int x = 10; x < srcW; x += 10)
                    canvas.DrawLine(x * cellSize, 0, x * cellSize, schemaH, thickPaint);
                for (int y = 10; y < srcH; y += 10)
                    canvas.DrawLine(0, y * cellSize, schemaW, y * cellSize, thickPaint);
            }

            canvas.Restore();

            if (includeLegend)
            {
                int lx = offsetX + schemaW + 10;
                int ly = margin;

                using (var titlePaint = new SKPaint
                {
                    Color = SKColors.Black,
                    TextSize = 14,
                    IsAntialias = true,
                    Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold,
                        SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
                })
                    canvas.DrawText("Нитки:", lx, ly + 14, titlePaint);

                ly += 30;
                foreach (var color in colors)
                {
                    var skc = new SKColor((byte)color.Red, (byte)color.Green, (byte)color.Blue);

                    using (var cp = new SKPaint { Color = skc })
                        canvas.DrawRect(SKRect.Create(lx, ly, 22, 22), cp);
                    using (var bp = new SKPaint { Color = SKColors.Black, StrokeWidth = 1, IsStroke = true })
                        canvas.DrawRect(SKRect.Create(lx, ly, 22, 22), bp);
                    using (var sp = new SKPaint
                    {
                        Color = GetContrastColorForPng(skc),
                        TextSize = 11,
                        TextAlign = SKTextAlign.Center,
                        IsAntialias = true
                    })
                        canvas.DrawText(color.Symbol ?? "●", lx + 11, ly + 16, sp);

                    using (var tp = new SKPaint { Color = SKColors.Black, TextSize = 10, IsAntialias = true })
                    {
                        canvas.DrawText($"DMC {color.DMCNumber ?? "—"}", lx + 28, ly + 9, tp);
                        canvas.DrawText($"{color.PixelCount} кр.", lx + 28, ly + 20, tp);
                    }

                    ly += legendRowH;
                }
            }

            using (var footerPaint = new SKPaint { Color = SKColors.Gray, TextSize = 10, IsAntialias = true })
            {
                string info = totalPages > 1
                    ? $"{pattern.Name}  |  {pattern.Width}×{pattern.Height}  |  стр. {pageNum} из {totalPages}"
                    : $"{pattern.Name}  |  {pattern.Width}×{pattern.Height} крестиков";
                canvas.DrawText(info, margin, a4H - 12, footerPaint);
                canvas.DrawText($"клетка = {cellSize}px", a4W - 120, a4H - 12, footerPaint);
            }

            return bitmap;
        }
        private SKBitmap RenderLegendPage(
            EmbroideryPattern pattern,
            List<EmbroideryColor> colors,
            int a4W, int a4H, int margin,
            int cellSize, int pageNum, int totalPages)
        {
            var bitmap = new SKBitmap(a4W, a4H);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.White);

            using (var borderPaint = new SKPaint { Color = new SKColor(200, 200, 200), StrokeWidth = 1, IsStroke = true })
                canvas.DrawRect(SKRect.Create(5, 5, a4W - 10, a4H - 10), borderPaint);

            int x = margin, y = margin;

            using (var titlePaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 20,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold,
                    SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
            })
                canvas.DrawText($"Список ниток: {pattern.Name}", x, y + 20, titlePaint);

            y += 50;
            using (var subPaint = new SKPaint { Color = SKColors.Gray, TextSize = 13, IsAntialias = true })
            {
                canvas.DrawText($"Размер: {pattern.Width} × {pattern.Height} крестиков  |  Цветов: {colors.Count}", x, y, subPaint);
            }
            y += 30;

            int rowH = 38;
            int colW = (a4W - margin * 2) / 2;
            int col = 0;
            int baseX = margin;

            foreach (var color in colors)
            {
                int cx = baseX + col * colW;
                var skc = new SKColor((byte)color.Red, (byte)color.Green, (byte)color.Blue);

                using (var cp = new SKPaint { Color = skc })
                    canvas.DrawRect(SKRect.Create(cx, y, 26, 26), cp);
                using (var bp = new SKPaint { Color = SKColors.Black, StrokeWidth = 1, IsStroke = true })
                    canvas.DrawRect(SKRect.Create(cx, y, 26, 26), bp);
                using (var sp = new SKPaint
                {
                    Color = GetContrastColorForPng(skc),
                    TextSize = 12,
                    TextAlign = SKTextAlign.Center,
                    IsAntialias = true,
                    Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold,
                        SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
                })
                    canvas.DrawText(color.Symbol ?? "●", cx + 13, y + 18, sp);

                using (var tp = new SKPaint { Color = SKColors.Black, TextSize = 11, IsAntialias = true })
                {
                    canvas.DrawText($"DMC {color.DMCNumber ?? "—"}  Gamma {color.GammaNumber ?? "—"}", cx + 34, y + 10, tp);
                    canvas.DrawText($"{color.ColorHex}  |  {color.PixelCount} крестиков", cx + 34, y + 22, tp);
                }

                col++;
                if (col >= 2) { col = 0; y += rowH; }

                if (y > a4H - margin - 40) break;
            }

            using (var footerPaint = new SKPaint { Color = SKColors.Gray, TextSize = 10, IsAntialias = true })
                canvas.DrawText($"{pattern.Name}  |  стр. {pageNum} из {totalPages}  (легенда)", margin, a4H - 12, footerPaint);

            return bitmap;
        }

        private void SaveBitmap(SKBitmap bitmap, string filePath)
        {
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 90);
            using var stream = File.Create(filePath);
            data.SaveTo(stream);
        }
    }
}
