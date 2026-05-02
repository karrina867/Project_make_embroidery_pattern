using MauiApp1.Services;
using MauiApp1.Models;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace MauiApp1.Views
{
    public partial class PatternProgressPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private readonly EmbroideryPattern _pattern;
        private readonly User _currentUser;
        private List<EmbroideryColor> _colors;
        private HashSet<(int x, int y)> _completedStitches;

        // Параметры отображения
        private float _cellSize = 30;
        private float _minCellSize = 15;
        private float _maxCellSize = 80;
        private float _offsetX = 0;
        private float _offsetY = 0;

        private float _touchStartX, _touchStartY;
        private bool _isPanning = false;

        private SKBitmap _patternBitmap;
        private int _totalStitches;

        public PatternProgressPage(EmbroideryPattern pattern, User user)
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _pattern = pattern;
            _currentUser = user;
            _completedStitches = new HashSet<(int x, int y)>();

            PatternCanvas.SizeChanged += OnCanvasSizeChanged;
        }

        private void OnCanvasSizeChanged(object sender, EventArgs e)
        {
            if (PatternCanvas.Width > 0 && PatternCanvas.Height > 0 && _patternBitmap != null)
            {
                FitToScreen();
            }
        }

        private void FitToScreen()
        {
            if (_patternBitmap == null) return;

            float canvasWidth = (float)PatternCanvas.Width;
            float canvasHeight = (float)PatternCanvas.Height;

            float cellSizeByWidth = canvasWidth / _pattern.Width;
            float cellSizeByHeight = canvasHeight / _pattern.Height;

            _cellSize = Math.Min(cellSizeByWidth, cellSizeByHeight);
            _cellSize = Math.Clamp(_cellSize, _minCellSize, _maxCellSize);

            float totalWidth = _pattern.Width * _cellSize;
            float totalHeight = _pattern.Height * _cellSize;

            _offsetX = (canvasWidth - totalWidth) / 2;
            _offsetY = (canvasHeight - totalHeight) / 2;

            UpdateZoomLabel();
            PatternCanvas.InvalidateSurface();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                // Индикатор загрузки
                await DisplayAlert("Загрузка", "Загрузка схемы...", "OK");

                PatternNameLabel.Text = _pattern.Name;

                // Загружаем цвета
                _colors = await _databaseService.GetPatternColors(_pattern.Id, _currentUser.Id);

                // Загружаем прогресс
                _completedStitches = await _databaseService.GetProgress(_pattern.Id, _currentUser.Id);

                _totalStitches = _pattern.Width * _pattern.Height;
                UpdateProgressDisplay();

                // Загружаем схему (как в PatternViewerPage)
                await LoadPatternBitmap();

                FitToScreen();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить: {ex.Message}", "OK");
            }
        }

        private void UpdateProgressDisplay()
        {
            int completed = _completedStitches.Count;
            int percent = _totalStitches > 0 ? completed * 100 / _totalStitches : 0;
            ProgressBar.Progress = percent / 100f;
            ProgressLabel.Text = $"Прогресс: {completed} из {_totalStitches} крестиков ({percent}%)";
        }

        private async Task LoadPatternBitmap()
        {
            await Task.Run(() =>
            {
                if (!string.IsNullOrEmpty(_pattern.PatternDataFile))
                {
                    try
                    {
                        var processor = new ImageProcessor();
                        var (colorMatrix, width, height) = processor.LoadColorMatrix(_pattern.PatternDataFile);

                        var colorMap = new Dictionary<int, SKColor>();
                        for (int i = 0; i < _colors.Count; i++)
                        {
                            colorMap[i] = new SKColor(
                                (byte)_colors[i].Red,
                                (byte)_colors[i].Green,
                                (byte)_colors[i].Blue);
                        }

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
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}");
                    }
                }
            });
        }

        private void OnCanvasPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.White);

            if (_patternBitmap == null)
            {
                using (var paint = new SKPaint { Color = SKColors.Black, TextSize = 20, TextAlign = SKTextAlign.Center })
                {
                    canvas.DrawText("Загрузка...", e.Info.Width / 2, e.Info.Height / 2, paint);
                }
                return;
            }

            canvas.Translate(_offsetX, _offsetY);

            for (int y = 0; y < _pattern.Height; y++)
            {
                for (int x = 0; x < _pattern.Width; x++)
                {
                    float left = x * _cellSize;
                    float top = y * _cellSize;
                    var rect = SKRect.Create(left, top, _cellSize, _cellSize);

                    var pixelColor = _patternBitmap.GetPixel(x, y);

                    // Если крестик выполнен - закрашиваем зелёным
                    if (_completedStitches.Contains((x, y)))
                    {
                        using (var paint = new SKPaint { Color = new SKColor(100, 200, 100, 200) })
                        {
                            canvas.DrawRect(rect, paint);
                        }
                    }
                    else
                    {
                        using (var paint = new SKPaint { Color = pixelColor })
                        {
                            canvas.DrawRect(rect, paint);
                        }
                    }

                    // Рисуем символ
                    if (_cellSize >= 20)
                    {
                        var embroideryColor = _colors.FirstOrDefault(c =>
                            c.Red == pixelColor.Red &&
                            c.Green == pixelColor.Green &&
                            c.Blue == pixelColor.Blue);

                        if (embroideryColor != null && !string.IsNullOrEmpty(embroideryColor.Symbol))
                        {
                            using (var textPaint = new SKPaint
                            {
                                Color = GetContrastColor(pixelColor),
                                TextSize = _cellSize * 0.45f,
                                TextAlign = SKTextAlign.Center,
                                IsAntialias = true,
                                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
                            })
                            {
                                float centerX = left + _cellSize / 2;
                                float centerY = top + _cellSize / 2 + textPaint.TextSize * 0.35f;
                                canvas.DrawText(embroideryColor.Symbol, centerX, centerY, textPaint);
                            }
                        }
                    }

                    // Граница клетки
                    if (_cellSize <= 40)
                    {
                        using (var paint = new SKPaint { Color = SKColors.LightGray, StrokeWidth = 0.5f, IsStroke = true })
                        {
                            canvas.DrawRect(rect, paint);
                        }
                    }
                }
            }
        }

        private void OnCanvasTouch(object sender, SKTouchEventArgs e)
        {
            if (e.ActionType == SKTouchAction.Pressed)
            {
                // Отмечаем крестик
                float canvasX = e.Location.X - _offsetX;
                float canvasY = e.Location.Y - _offsetY;

                int x = (int)(canvasX / _cellSize);
                int y = (int)(canvasY / _cellSize);

                if (x >= 0 && x < _pattern.Width && y >= 0 && y < _pattern.Height)
                {
                    ToggleStitch(x, y);
                }
                e.Handled = true;
            }
            else if (e.ActionType == SKTouchAction.Moved)
            {
                // Панорамирование (если нужно)
                if (!_isPanning)
                {
                    _touchStartX = e.Location.X - _offsetX;
                    _touchStartY = e.Location.Y - _offsetY;
                    _isPanning = true;
                }
                _offsetX = e.Location.X - _touchStartX;
                _offsetY = e.Location.Y - _touchStartY;
                ClampOffset();
                PatternCanvas.InvalidateSurface();
                e.Handled = true;
            }
            else if (e.ActionType == SKTouchAction.Released)
            {
                _isPanning = false;
                e.Handled = true;
            }
        }

        private async void ToggleStitch(int x, int y)
        {
            if (_completedStitches.Contains((x, y)))
            {
                _completedStitches.Remove((x, y));
            }
            else
            {
                _completedStitches.Add((x, y));
            }

            // Сохраняем прогресс в БД
            await _databaseService.SaveProgress(_pattern.Id, _currentUser.Id, _completedStitches);
            await _databaseService.UpdatePercentComplete(_pattern.Id, _currentUser.Id, _totalStitches, _completedStitches.Count);

            UpdateProgressDisplay();
            PatternCanvas.InvalidateSurface();
        }

        private void ClampOffset()
        {
            float totalWidth = _pattern.Width * _cellSize;
            float totalHeight = _pattern.Height * _cellSize;
            float canvasWidth = (float)PatternCanvas.Width;
            float canvasHeight = (float)PatternCanvas.Height;

            float maxOffsetX = Math.Max(0, totalWidth - canvasWidth);
            float maxOffsetY = Math.Max(0, totalHeight - canvasHeight);

            if (totalWidth <= canvasWidth)
            {
                _offsetX = (canvasWidth - totalWidth) / 2;
            }
            else
            {
                _offsetX = Math.Clamp(_offsetX, -maxOffsetX, 0);
            }

            if (totalHeight <= canvasHeight)
            {
                _offsetY = (canvasHeight - totalHeight) / 2;
            }
            else
            {
                _offsetY = Math.Clamp(_offsetY, -maxOffsetY, 0);
            }
        }

        private void OnZoomInClicked(object sender, EventArgs e)
        {
            float oldCellSize = _cellSize;
            _cellSize = Math.Min(_cellSize * 1.2f, _maxCellSize);
            AdjustOffsetAfterZoom(oldCellSize);
            UpdateZoomLabel();
            PatternCanvas.InvalidateSurface();
        }

        private void OnZoomOutClicked(object sender, EventArgs e)
        {
            float oldCellSize = _cellSize;
            _cellSize = Math.Max(_cellSize / 1.2f, _minCellSize);
            AdjustOffsetAfterZoom(oldCellSize);
            UpdateZoomLabel();
            PatternCanvas.InvalidateSurface();
        }

        private void AdjustOffsetAfterZoom(float oldCellSize)
        {
            if (PatternCanvas.Width > 0 && PatternCanvas.Height > 0)
            {
                float centerX = (float)PatternCanvas.Width / 2 - _offsetX;
                float centerY = (float)PatternCanvas.Height / 2 - _offsetY;
                float ratio = _cellSize / oldCellSize;
                _offsetX = (float)PatternCanvas.Width / 2 - centerX * ratio;
                _offsetY = (float)PatternCanvas.Height / 2 - centerY * ratio;
            }
            ClampOffset();
        }

        private void OnResetZoomClicked(object sender, EventArgs e)
        {
            FitToScreen();
        }

        private void UpdateZoomLabel()
        {
            int percent = (int)((_cellSize / 30) * 100);
            ZoomLabel.Text = $"{percent}%";
        }

        private async void OnStatsClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Статистика",
                $"{_pattern.Name}\n\n" +
                $"Всего крестиков: {_totalStitches}\n" +
                $"Выполнено: {_completedStitches.Count}\n" +
                $"Осталось: {_totalStitches - _completedStitches.Count}\n" +
                $"Прогресс: {(_totalStitches > 0 ? _completedStitches.Count * 100 / _totalStitches : 0)}%",
                "OK");
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private SKColor GetContrastColor(SKColor color)
        {
            double luminance = (0.299 * color.Red + 0.587 * color.Green + 0.114 * color.Blue);
            return luminance > 128 ? SKColors.Black : SKColors.White;
        }
    }
}