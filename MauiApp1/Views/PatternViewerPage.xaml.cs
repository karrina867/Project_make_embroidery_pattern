using MauiApp1.Services;
using MauiApp1.Models;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace MauiApp1.Views
{
    public partial class PatternViewerPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private readonly EmbroideryPattern _pattern;
        private readonly User _currentUser;
        private List<EmbroideryColor> _colors;

        // Параметры отображения
        private float _cellSize = 30; // Размер ячейки в пикселях
        private float _minCellSize = 10;
        private float _maxCellSize = 80;
        private float _offsetX = 0;
        private float _offsetY = 0;

        // Для отслеживания касаний
        private float _touchStartX, _touchStartY;
        private bool _isPanning = false;

        // Кэш схемы для быстрой отрисовки
        private SKBitmap _patternBitmap;

        public PatternViewerPage(EmbroideryPattern pattern, User user)
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _pattern = pattern;
            _currentUser = user;

            LoadPatternData();
        }

        private async void LoadPatternData()
        {
            try
            {
                // Проверяем права доступа
                if (_pattern.UserId != _currentUser.Id)
                {
                    await DisplayAlert("Ошибка", "У вас нет доступа к этой схеме", "OK");
                    await Navigation.PopAsync();
                    return;
                }

                // Загружаем цвета
                _colors = await _databaseService.GetPatternColors(_pattern.Id, _currentUser.Id);

                // Обновляем информацию
                PatternNameLabel.Text = _pattern.Name;
                PatternInfoLabel.Text = $"{_pattern.Width} × {_pattern.Height} крестиков | {_pattern.ColorCount} цветов";

                // Генерируем схему в памяти
                await GeneratePatternBitmap();

                // Обновляем канву
                PatternCanvas.InvalidateSurface();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить схему: {ex.Message}", "OK");
            }
        }

        private async Task GeneratePatternBitmap()
        {
            await Task.Run(() =>
            {
                // Загружаем матрицу цветов из файла
                if (!string.IsNullOrEmpty(_pattern.PatternDataFile))
                {
                    try
                    {
                        var processor = new ImageProcessor();
                        var (colorMatrix, width, height) = processor.LoadColorMatrix(_pattern.PatternDataFile);

                        // Создаем карту цветов по индексам
                        var colorMap = new Dictionary<int, SKColor>();
                        for (int i = 0; i < _colors.Count; i++)
                        {
                            colorMap[i] = new SKColor(
                                (byte)_colors[i].Red,
                                (byte)_colors[i].Green,
                                (byte)_colors[i].Blue);
                        }

                        // Создаем битмап из матрицы
                        _patternBitmap = new SKBitmap(width, height);

                        for (int x = 0; x < width; x++)
                        {
                            for (int y = 0; y < height; y++)
                            {
                                int colorIndex = colorMatrix[x, y];
                                if (colorMap.ContainsKey(colorIndex))
                                {
                                    _patternBitmap.SetPixel(x, y, colorMap[colorIndex]);
                                }
                            }
                        }

                        System.Diagnostics.Debug.WriteLine($"Схема загружена: {width}x{height}, {_colors.Count} цветов");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка загрузки матрицы: {ex.Message}");
                        CreateBitmapFromColorsOnly();
                    }
                }
                else
                {
                    CreateBitmapFromColorsOnly();
                }
            });
        }

        private void CreateBitmapFromColorsOnly()
        {
            // Для старых схем без матрицы - равномерное распределение цветов
            _patternBitmap = new SKBitmap(_pattern.Width, _pattern.Height);

            for (int x = 0; x < _pattern.Width; x++)
            {
                for (int y = 0; y < _pattern.Height; y++)
                {
                    // Используем позицию для определения цвета (волновой паттерн)
                    int colorIndex = (x + y) % _colors.Count;
                    var color = _colors[colorIndex];
                    _patternBitmap.SetPixel(x, y, new SKColor((byte)color.Red, (byte)color.Green, (byte)color.Blue));
                }
            }
        }

        private void OnCanvasPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var info = e.Info;

            // Очищаем фон
            canvas.Clear(SKColors.White);

            if (_patternBitmap == null) return;

            // Вычисляем размеры схемы в пикселях
            float totalWidth = _pattern.Width * _cellSize;
            float totalHeight = _pattern.Height * _cellSize;

            // Применяем смещение (панорамирование)
            canvas.Translate(_offsetX, _offsetY);

            // Рисуем сетку и залитые ячейки
            for (int x = 0; x < _pattern.Width; x++)
            {
                for (int y = 0; y < _pattern.Height; y++)
                {
                    float left = x * _cellSize;
                    float top = y * _cellSize;
                    SKRect rect = SKRect.Create(left, top, _cellSize, _cellSize);

                    // Получаем цвет пикселя
                    var pixelColor = _patternBitmap.GetPixel(x, y);

                    // Рисуем заливку
                    using (var paint = new SKPaint { Color = pixelColor })
                    {
                        canvas.DrawRect(rect, paint);
                    }

                    // Находим символ для этого цвета
                    var embroideryColor = _colors.FirstOrDefault(c =>
                        c.Red == pixelColor.Red &&
                        c.Green == pixelColor.Green &&
                        c.Blue == pixelColor.Blue);

                    if (embroideryColor != null && !string.IsNullOrEmpty(embroideryColor.Symbol))
                    {
                        // Рисуем символ
                        using (var paint = new SKPaint
                        {
                            Color = GetContrastColor(pixelColor),
                            TextSize = _cellSize * 0.6f,
                            TextAlign = SKTextAlign.Center,
                            IsAntialias = true
                        })
                        {
                            float centerX = left + _cellSize / 2;
                            float centerY = top + _cellSize / 2 - paint.TextSize * 0.35f;
                            canvas.DrawText(embroideryColor.Symbol, centerX, centerY, paint);
                        }
                    }

                    // Рисуем границу ячейки
                    using (var paint = new SKPaint { Color = SKColors.Black, StrokeWidth = 1, IsStroke = true })
                    {
                        canvas.DrawRect(rect, paint);
                    }
                }
            }

            // Рисуем толстые линии каждые 10 клеток для удобства
            using (var thickPaint = new SKPaint { Color = SKColors.DarkBlue, StrokeWidth = 2 })
            {
                for (int x = 10; x < _pattern.Width; x += 10)
                {
                    float xPos = x * _cellSize;
                    canvas.DrawLine(xPos, 0, xPos, totalHeight, thickPaint);
                }

                for (int y = 10; y < _pattern.Height; y += 10)
                {
                    float yPos = y * _cellSize;
                    canvas.DrawLine(0, yPos, totalWidth, yPos, thickPaint);
                }
            }
        }

        private SKColor GetContrastColor(SKColor color)
        {
            double luminance = (0.299 * color.Red + 0.587 * color.Green + 0.114 * color.Blue);
            return luminance > 128 ? SKColors.Black : SKColors.White;
        }

        #region Обработка касаний (масштабирование и панорамирование)

        private void OnCanvasTouch(object sender, SKTouchEventArgs e)
        {
            if (e.ActionType == SKTouchAction.Pressed)
            {
                _touchStartX = e.Location.X - _offsetX;
                _touchStartY = e.Location.Y - _offsetY;
                _isPanning = true;
                e.Handled = true;
            }
            else if (e.ActionType == SKTouchAction.Moved && _isPanning)
            {
                _offsetX = e.Location.X - _touchStartX;
                _offsetY = e.Location.Y - _touchStartY;

                // Ограничиваем панорамирование
                float maxOffsetX = _pattern.Width * _cellSize;
                float maxOffsetY = _pattern.Height * _cellSize;
                _offsetX = Math.Clamp(_offsetX, -maxOffsetX + 100, maxOffsetX - 100);
                _offsetY = Math.Clamp(_offsetY, -maxOffsetY + 100, maxOffsetY - 100);

                PatternCanvas.InvalidateSurface();
                e.Handled = true;
            }
            else if (e.ActionType == SKTouchAction.Released)
            {
                _isPanning = false;
                e.Handled = true;
            }
        }

        private void OnZoomInClicked(object sender, EventArgs e)
        {
            _cellSize = Math.Min(_cellSize * 1.2f, _maxCellSize);
            UpdateZoomLabel();
            PatternCanvas.InvalidateSurface();
        }

        private void OnZoomOutClicked(object sender, EventArgs e)
        {
            _cellSize = Math.Max(_cellSize / 1.2f, _minCellSize);
            UpdateZoomLabel();
            PatternCanvas.InvalidateSurface();
        }

        private void OnResetZoomClicked(object sender, EventArgs e)
        {
            _cellSize = 30;
            _offsetX = 0;
            _offsetY = 0;
            UpdateZoomLabel();
            PatternCanvas.InvalidateSurface();
        }

        private void UpdateZoomLabel()
        {
            int percent = (int)(_cellSize / 30 * 100);
            ZoomLabel.Text = $"{percent}%";
        }

        #endregion

        private async void OnShowLegendClicked(object sender, EventArgs e)
        {
            // Показываем легенду цветов
            var legend = "━━━━━━ ЦВЕТА И СИМВОЛЫ ━━━━━━\n\n";
            foreach (var color in _colors)
            {
                legend += $"{color.Symbol} - {color.ColorHex}";
                if (!string.IsNullOrEmpty(color.DMCNumber))
                    legend += $" (DMC: {color.DMCNumber})";
                legend += $" - {color.PixelCount} крестиков\n";
            }

            await DisplayAlert("Легенда схемы", legend, "OK");
        }
    }
}