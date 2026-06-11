using MauiApp1.Services;
using MauiApp1.Models;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using System.Collections.Generic;

namespace MauiApp1.Views
{
    public partial class PatternProgressPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private readonly EmbroideryPattern _pattern;
        private readonly User _currentUser;
        private List<EmbroideryColor> _colors;
        private HashSet<(int x, int y)> _completedStitches;
        private bool _isDmcMode = true;

        private float _cellSize = 30;
        private float _minCellSize = 2;
        private float _maxCellSize = 150;
        private float _offsetX = 0;
        private float _offsetY = 0;

        private bool _isFirstLayout = true;

        // Для отслеживания активных касаний (pinch-to-zoom)
        private Dictionary<long, SKPoint> _activeTouches = new Dictionary<long, SKPoint>();
        private float _oldDistance = 0;

        // Для панорамирования одним пальцем
        private long _panningTouchId = -1;
        private float _touchStartX, _touchStartY;

        // Флаг: было ли движение пальца (чтобы не путать пан с тапом)
        private bool _touchMoved = false;
        private SKPoint _touchPressLocation;

        private SKBitmap _patternBitmap;
        private int _canvasWidth = 0, _canvasHeight = 0;
        private bool _isLoaded = false;
        private int _totalStitches;

        // Кэш символов и контрастных цветов для O(1) поиска
        private Dictionary<uint, string> _symbolCache;
        private Dictionary<uint, SKColor> _contrastCache;

        // Переиспользуемые SKPaint объекты
        private readonly SKPaint _fillPaint = new SKPaint { IsAntialias = false };
        private readonly SKPaint _textPaint = new SKPaint { TextAlign = SKTextAlign.Center, IsAntialias = true };
        private readonly SKPaint _crossPaint = new SKPaint { Color = new SKColor(80, 220, 80), IsStroke = true, IsAntialias = true };
        private readonly SKPaint _gridPaint = new SKPaint { Color = new SKColor(180, 180, 180), StrokeWidth = 0.5f, IsStroke = true, IsAntialias = false };
        private readonly SKPaint _thickPaint = new SKPaint { Color = SKColors.White, StrokeWidth = 3f, IsStroke = true, IsAntialias = false };

        public PatternProgressPage(EmbroideryPattern pattern, User user)
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _pattern = pattern;
            _currentUser = user;
            _completedStitches = new HashSet<(int x, int y)>();

            PatternCanvas.SizeChanged += OnCanvasSizeChanged;

#if WINDOWS
            PatternCanvas.HandlerChanged += (s, e) =>
            {
                if (PatternCanvas.Handler?.PlatformView is Microsoft.UI.Xaml.UIElement winView)
                {
                    winView.PointerWheelChanged += WinUI_PointerWheelChanged;
                }
            };
#endif
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadData();
        }

        private void OnCanvasSizeChanged(object sender, EventArgs e)
        {
            if (PatternCanvas.Width > 0 && PatternCanvas.Height > 0)
            {
                _canvasWidth = (int)PatternCanvas.Width;
                _canvasHeight = (int)PatternCanvas.Height;

                if (_isLoaded && _patternBitmap != null)
                {
                    FitToScreen();
                }
            }
        }

        private async Task LoadData()
        {
            try
            {
                PatternNameLabel.Text = _pattern.Name;

                _colors = await _databaseService.GetPatternColors(_pattern.Id, _currentUser.Id);
                _completedStitches = await _databaseService.GetProgress(_pattern.Id, _currentUser.Id);

                _totalStitches = _pattern.Width * _pattern.Height;
                UpdateProgressDisplay();

                await LoadPatternBitmap();

                _isLoaded = true;

                if (PatternCanvas.Width > 0 && PatternCanvas.Height > 0)
                {
                    _canvasWidth = (int)PatternCanvas.Width;
                    _canvasHeight = (int)PatternCanvas.Height;
                    FitToScreen();
                }

                UpdateColorsList();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить схему: {ex.Message}", "OK");
            }
        }

        private void FitToScreen()
        {
            if (_patternBitmap == null) return;
            if (_canvasWidth <= 0 || _canvasHeight <= 0) return;

            float cellSizeByWidth = (float)_canvasWidth / _pattern.Width;
            float cellSizeByHeight = (float)_canvasHeight / _pattern.Height;

            float newCellSize = Math.Min(cellSizeByWidth, cellSizeByHeight) * 0.9f;
            _cellSize = Math.Clamp(newCellSize, _minCellSize, _maxCellSize);

            _offsetX = (_canvasWidth - _pattern.Width * _cellSize) / 2f;
            _offsetY = (_canvasHeight - _pattern.Height * _cellSize) / 2f;

            PatternCanvas?.InvalidateSurface();
        }

        private async void OnClearProgressClicked(object sender, EventArgs e)
        {
            // Подтверждение очистки
            var confirm =  await DisplayAlert("Очистка прогресса", "Вы уверены, что хотите очистить весь прогресс вышивки?", "Да, очистить", "Отмена");

            if (!confirm) return;

            try
            {
                // Очищаем локальный HashSet
                _completedStitches.Clear();

                // Сохраняем пустой прогресс в базу данных
                await _databaseService.SaveProgress(_pattern.Id, _currentUser.Id, _completedStitches);
                await _databaseService.UpdatePercentComplete(_pattern.Id, _currentUser.Id, _totalStitches, 0);

                // Обновляем отображение
                UpdateProgressDisplay();
                PatternCanvas.InvalidateSurface();

               // await DisplayAlert("Успех", "Прогресс вышивки полностью очищен", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось очистить прогресс: {ex.Message}", "OK");
            }
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

                        var colorArray = new SKColor[_colors.Count];
                        for (int i = 0; i < _colors.Count; i++)
                        {
                            colorArray[i] = new SKColor(
                                (byte)_colors[i].Red,
                                (byte)_colors[i].Green,
                                (byte)_colors[i].Blue);
                        }

                        // Используем Pixels[] напрямую — в 10-20x быстрее чем SetPixel
                        var bitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
                        var pixels = new SKColor[width * height];

                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                int colorIndex = colorMatrix[x, y];
                                pixels[y * width + x] = colorIndex < colorArray.Length
                                    ? colorArray[colorIndex]
                                    : SKColors.Magenta;
                            }
                        }

                        bitmap.Pixels = pixels;
                        _patternBitmap = bitmap;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}");
                    }
                }

                BuildCaches();
            });
        }

        // Строим словари для O(1) поиска символов и контрастных цветов
        private void BuildCaches()
        {
            _symbolCache = new Dictionary<uint, string>();
            _contrastCache = new Dictionary<uint, SKColor>();

            if (_colors == null) return;

            foreach (var color in _colors)
            {
                var skColor = new SKColor((byte)color.Red, (byte)color.Green, (byte)color.Blue);
                uint key = (uint)skColor;

                if (!_symbolCache.ContainsKey(key))
                    _symbolCache[key] = color.Symbol ?? "";

                if (!_contrastCache.ContainsKey(key))
                {
                    double luminance = 0.299 * color.Red + 0.587 * color.Green + 0.114 * color.Blue;
                    _contrastCache[key] = luminance > 128 ? SKColors.Black : SKColors.White;
                }
            }
        }

        private void UpdateProgressDisplay()
        {
            int completed = _completedStitches.Count;
            int percent = _totalStitches > 0 ? completed * 100 / _totalStitches : 0;
            ProgressBar.Progress = percent / 100f;
            ProgressLabel.Text = $"Прогресс: {completed} из {_totalStitches} крестиков ({percent}%)";
        }

        private void UpdateColorsList()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ColorsStackLayout.Children.Clear();

                foreach (var color in _colors)
                {
                    var contrastColor = GetContrastColorMAUI(color);

                    var frame = new Frame
                    {
                        WidthRequest = 60,
                        HeightRequest = 65,
                        Padding = 5,
                        CornerRadius = 8,
                        BackgroundColor = Color.FromArgb(color.ColorHex),
                        BorderColor = Colors.Gray,
                        HasShadow = false
                    };

                    var symbolLabel = new Label
                    {
                        Text = color.Symbol,
                        FontSize = 16,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = contrastColor,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.CenterAndExpand
                    };

                    var codeLabel = new Label
                    {
                        Text = _isDmcMode ? (color.DMCNumber ?? "—") : (color.GammaNumber ?? "—"),
                        FontSize = 10,
                        TextColor = contrastColor,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.End
                    };

                    var stack = new VerticalStackLayout();
                    stack.Children.Add(symbolLabel);
                    stack.Children.Add(codeLabel);
                    frame.Content = stack;

                    ColorsStackLayout.Children.Add(frame);
                }
            });
        }

        private Color GetContrastColorMAUI(EmbroideryColor color)
        {
            double luminance = (0.299 * color.Red + 0.587 * color.Green + 0.114 * color.Blue);
            return luminance > 128 ? Colors.Black : Colors.White;
        }

        private void OnCanvasPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(new SKColor(26, 26, 26));

            if (_patternBitmap == null || !_isLoaded)
            {
                _fillPaint.Color = SKColors.White;
                _fillPaint.IsStroke = false;
                canvas.DrawText("Загрузка...", e.Info.Width / 2, e.Info.Height / 2, _fillPaint);
                return;
            }

            if (_isFirstLayout && e.Info.Width > 0 && e.Info.Height > 0)
            {
                _canvasWidth = e.Info.Width;
                _canvasHeight = e.Info.Height;
                _isFirstLayout = false;
                FitToScreen();
                return;
            }

            canvas.Translate(_offsetX, _offsetY);

            // Viewport culling: рисуем только видимые клетки
            int startX = Math.Max(0, (int)(-_offsetX / _cellSize));
            int startY = Math.Max(0, (int)(-_offsetY / _cellSize));
            int endX = Math.Min(_pattern.Width, (int)((-_offsetX + _canvasWidth) / _cellSize) + 1);
            int endY = Math.Min(_pattern.Height, (int)((-_offsetY + _canvasHeight) / _cellSize) + 1);

            bool drawSymbols = _cellSize >= 18 && _symbolCache != null;
            float textSize = _cellSize * 0.4f;
            float crossMargin = _cellSize * 0.15f;
            float crossStrokeWidth = Math.Max(1.5f, _cellSize * 0.12f);

            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    float left = x * _cellSize;
                    float top = y * _cellSize;
                    var rect = SKRect.Create(left, top, _cellSize, _cellSize);

                    var pixelColor = _patternBitmap.GetPixel(x, y);

                    if (_completedStitches.Contains((x, y)))
                    {
                        // Затемнённый цвет для выполненной клетки
                        _fillPaint.Color = new SKColor(
                            (byte)(pixelColor.Red * 0.4f),
                            (byte)(pixelColor.Green * 0.4f),
                            (byte)(pixelColor.Blue * 0.4f));
                        _fillPaint.IsStroke = false;
                        canvas.DrawRect(rect, _fillPaint);

                        // Зелёный крестик
                        if (_cellSize >= 8)
                        {
                            _crossPaint.StrokeWidth = crossStrokeWidth;
                            canvas.DrawLine(left + crossMargin, top + crossMargin,
                                left + _cellSize - crossMargin, top + _cellSize - crossMargin, _crossPaint);
                            canvas.DrawLine(left + _cellSize - crossMargin, top + crossMargin,
                                left + crossMargin, top + _cellSize - crossMargin, _crossPaint);
                        }
                    }
                    else
                    {
                        _fillPaint.Color = pixelColor;
                        _fillPaint.IsStroke = false;
                        canvas.DrawRect(rect, _fillPaint);

                        if (drawSymbols)
                        {
                            uint key = (uint)pixelColor;
                            if (_symbolCache.TryGetValue(key, out string symbol) && !string.IsNullOrEmpty(symbol))
                            {
                                _textPaint.Color = _contrastCache.TryGetValue(key, out var contrast)
                                    ? contrast : SKColors.Black;
                                _textPaint.TextSize = textSize;

                                float centerX = left + _cellSize / 2;
                                float centerY = top + _cellSize / 2 + textSize * 0.35f;
                                canvas.DrawText(symbol, centerX, centerY, _textPaint);
                            }
                        }
                    }
                }
            }

            // Сетка только для видимой области
            for (int x = startX; x <= endX; x++)
            {
                float xPos = x * _cellSize;
                canvas.DrawLine(xPos, startY * _cellSize, xPos, endY * _cellSize, _gridPaint);
            }
            for (int y = startY; y <= endY; y++)
            {
                float yPos = y * _cellSize;
                canvas.DrawLine(startX * _cellSize, yPos, endX * _cellSize, yPos, _gridPaint);
            }

            // Жирные белые линии каждые 10 клеток (только видимые)
            int thick10StartX = (startX / 10) * 10;
            int thick10StartY = (startY / 10) * 10;
            for (int x = thick10StartX; x <= endX; x += 10)
            {
                if (x == 0) continue;
                float xPos = x * _cellSize;
                canvas.DrawLine(xPos, startY * _cellSize, xPos, endY * _cellSize, _thickPaint);
            }
            for (int y = thick10StartY; y <= endY; y += 10)
            {
                if (y == 0) continue;
                float yPos = y * _cellSize;
                canvas.DrawLine(startX * _cellSize, yPos, endX * _cellSize, yPos, _thickPaint);
            }
        }

        private void OnCanvasTouch(object sender, SKTouchEventArgs e)
        {
            long id = e.Id;

            if (e.ActionType == SKTouchAction.Pressed)
            {
                _activeTouches[id] = e.Location;
                _touchMoved = false;
                _touchPressLocation = e.Location;

                if (_activeTouches.Count == 1)
                {
                    _panningTouchId = id;
                    _touchStartX = e.Location.X - _offsetX;
                    _touchStartY = e.Location.Y - _offsetY;
                }
                else if (_activeTouches.Count == 2)
                {
                    _panningTouchId = -1;
                    var points = _activeTouches.Values.ToList();
                    _oldDistance = CalculateDistance(points[0], points[1]);
                }
                e.Handled = true;
            }
            else if (e.ActionType == SKTouchAction.Moved)
            {
                if (_activeTouches.ContainsKey(id))
                    _activeTouches[id] = e.Location;

                // Считаем движение значимым если сдвинулись более чем на 5 пикселей
                float dx = e.Location.X - _touchPressLocation.X;
                float dy = e.Location.Y - _touchPressLocation.Y;
                if (Math.Sqrt(dx * dx + dy * dy) > 5)
                    _touchMoved = true;

                if (_activeTouches.Count == 1 && _panningTouchId != -1 && _activeTouches.ContainsKey(_panningTouchId))
                {
                    var point = _activeTouches[_panningTouchId];
                    _offsetX = point.X - _touchStartX;
                    _offsetY = point.Y - _touchStartY;
                    ClampOffset();
                    PatternCanvas.InvalidateSurface();
                }
                else if (_activeTouches.Count == 2)
                {
                    var points = _activeTouches.Values.ToList();
                    float newDistance = CalculateDistance(points[0], points[1]);
                    if (_oldDistance > 0)
                    {
                        float scale = newDistance / _oldDistance;
                        float newCellSize = _cellSize * scale;
                        newCellSize = Math.Clamp(newCellSize, _minCellSize, _maxCellSize);

                        if (newCellSize != _cellSize)
                        {
                            SKPoint center = new SKPoint(
                                (points[0].X + points[1].X) / 2,
                                (points[0].Y + points[1].Y) / 2);
                            _offsetX = center.X - (center.X - _offsetX) * (newCellSize / _cellSize);
                            _offsetY = center.Y - (center.Y - _offsetY) * (newCellSize / _cellSize);
                            _cellSize = newCellSize;
                            ClampOffset();
                            PatternCanvas.InvalidateSurface();
                        }
                    }
                    _oldDistance = newDistance;
                }
                e.Handled = true;
            }
            else if (e.ActionType == SKTouchAction.Released || e.ActionType == SKTouchAction.Cancelled)
            {
                // Если не было движения — это тап, отмечаем/снимаем крестик
                if (e.ActionType == SKTouchAction.Released && !_touchMoved && _activeTouches.Count == 1)
                {
                    float canvasX = e.Location.X - _offsetX;
                    float canvasY = e.Location.Y - _offsetY;

                    int x = (int)(canvasX / _cellSize);
                    int y = (int)(canvasY / _cellSize);

                    if (x >= 0 && x < _pattern.Width && y >= 0 && y < _pattern.Height)
                    {
                        ToggleStitch(x, y);
                    }
                }

                _activeTouches.Remove(id);
                if (_panningTouchId == id)
                    _panningTouchId = -1;

                if (_activeTouches.Count == 0)
                {
                    _oldDistance = 0;
                }
                else if (_activeTouches.Count == 1)
                {
                    var remaining = _activeTouches.First();
                    _panningTouchId = remaining.Key;
                    _touchStartX = remaining.Value.X - _offsetX;
                    _touchStartY = remaining.Value.Y - _offsetY;
                }
                e.Handled = true;
            }
            else
            {
                e.Handled = true;
            }
        }

        private async void ToggleStitch(int x, int y)
        {
            if (_completedStitches.Contains((x, y)))
                _completedStitches.Remove((x, y));
            else
                _completedStitches.Add((x, y));

            await _databaseService.SaveProgress(_pattern.Id, _currentUser.Id, _completedStitches);
            await _databaseService.UpdatePercentComplete(_pattern.Id, _currentUser.Id, _totalStitches, _completedStitches.Count);

            UpdateProgressDisplay();
            PatternCanvas.InvalidateSurface();
        }

        private float CalculateDistance(SKPoint p1, SKPoint p2)
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        private void ClampOffset()
        {
            float totalWidth = _pattern.Width * _cellSize;
            float totalHeight = _pattern.Height * _cellSize;

            if (totalWidth <= _canvasWidth)
            {
                _offsetX = (_canvasWidth - totalWidth) / 2f;
            }
            else
            {
                float minX = _canvasWidth - totalWidth;
                float maxX = 0;
                _offsetX = Math.Clamp(_offsetX, minX, maxX);
            }

            if (totalHeight <= _canvasHeight)
            {
                _offsetY = (_canvasHeight - totalHeight) / 2f;
            }
            else
            {
                float minY = _canvasHeight - totalHeight;
                float maxY = 0;
                _offsetY = Math.Clamp(_offsetY, minY, maxY);
            }
        }

        private async void OnScrollLeftClicked(object sender, EventArgs e)
        {
            await ColorsScrollView.ScrollToAsync(ColorsScrollView.ScrollX - 150, 0, true);
        }

        private async void OnScrollRightClicked(object sender, EventArgs e)
        {
            await ColorsScrollView.ScrollToAsync(ColorsScrollView.ScrollX + 150, 0, true);
        }

        private void OnDmcClicked(object sender, EventArgs e)
        {
            _isDmcMode = true;
            DmcButton.BackgroundColor = Color.FromArgb("#4CAF50");
            GammaButton.BackgroundColor = Color.FromArgb("#9E9E9E");
            UpdateColorsList();
        }

        private void OnGammaClicked(object sender, EventArgs e)
        {
            _isDmcMode = false;
            DmcButton.BackgroundColor = Color.FromArgb("#9E9E9E");
            GammaButton.BackgroundColor = Color.FromArgb("#4CAF50");
            UpdateColorsList();
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

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            PatternCanvas.SizeChanged -= OnCanvasSizeChanged;
            _fillPaint.Dispose();
            _textPaint.Dispose();
            _crossPaint.Dispose();
            _gridPaint.Dispose();
            _thickPaint.Dispose();
        }

#if WINDOWS
        private void WinUI_PointerWheelChanged(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var currentPoint = e.GetCurrentPoint(sender as Microsoft.UI.Xaml.UIElement);
            if (currentPoint == null) return;

            SKPoint mousePoint = new SKPoint((float)currentPoint.Position.X, (float)currentPoint.Position.Y);

            int delta = currentPoint.Properties.MouseWheelDelta;
            if (delta == 0) return;

            float scaleFactor = delta > 0 ? 1.1f : 0.9f;
            float newCellSize = Math.Clamp(_cellSize * scaleFactor, _minCellSize, _maxCellSize);

            if (newCellSize != _cellSize)
            {
                _offsetX = mousePoint.X - (mousePoint.X - _offsetX) * (newCellSize / _cellSize);
                _offsetY = mousePoint.Y - (mousePoint.Y - _offsetY) * (newCellSize / _cellSize);
                _cellSize = newCellSize;
                ClampOffset();
                PatternCanvas.InvalidateSurface();
            }

            e.Handled = true;
        }
#endif
    }
}
