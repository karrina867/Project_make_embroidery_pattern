using MauiApp1.Services;
using MauiApp1.Models;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using System.Collections.Generic;

namespace MauiApp1.Views
{
    public partial class PatternViewerPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private readonly EmbroideryPattern _pattern;
        private readonly User _currentUser;
        private List<EmbroideryColor> _colors;
        private bool _isDmcMode = true;

        private float _cellSize = 30;
        private float _minCellSize = 2;//10
        private float _maxCellSize = 150;
        private float _offsetX = 0;
        private float _offsetY = 0;

        private bool _isFirstLayout = true;

        // Для отслеживания активных касаний
        private Dictionary<long, SKPoint> _activeTouches = new Dictionary<long, SKPoint>();
        private float _oldDistance = 0;

        // Для панорамирования одним пальцем
        private long _panningTouchId = -1;
        private float _touchStartX, _touchStartY;

        private SKBitmap _patternBitmap;
        private int _canvasWidth = 0, _canvasHeight = 0;
        private bool _isLoaded = false;

        public PatternViewerPage(EmbroideryPattern pattern, User user)
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _pattern = pattern;
            _currentUser = user;

            // Подписываемся на событие изменения размера
            PatternCanvas.SizeChanged += OnCanvasSizeChanged;
        }

        //2.5
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadPatternData();
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

        private async Task LoadPatternData()
        {
            try
            {
                if (_pattern.UserId != _currentUser.Id)
                {
                    await DisplayAlert("Ошибка", "У вас нет доступа к этой схеме", "OK");
                    await Navigation.PopAsync();
                    return;
                }

                _colors = await _databaseService.GetPatternColors(_pattern.Id, _currentUser.Id);
                PatternNameLabel.Text = _pattern.Name;

                await LoadPatternBitmap();

                _isLoaded = true;

                // Если канва уже имеет размеры, центрируем сразу
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

            // Берем размер клетки так, чтобы вся схема влезла, и делаем её чуть меньше (на 90%), чтобы были отступы
            float newCellSize = Math.Min(cellSizeByWidth, cellSizeByHeight) * 0.9f;
            _cellSize = Math.Clamp(newCellSize, _minCellSize, _maxCellSize);

            // Центрируем схему
            _offsetX = (_canvasWidth - _pattern.Width * _cellSize) / 2f;
            _offsetY = (_canvasHeight - _pattern.Height * _cellSize) / 2f;

            PatternCanvas?.InvalidateSurface();
        }

        //private void FitToScreen()
        //{
        //    if (_patternBitmap == null) return;

        //    // Если размеры всё ещё не определены, пробуем получить через Dispatch
        //    if (_canvasWidth <= 0 || _canvasHeight <= 0)
        //    {
        //        _canvasWidth = (int)PatternCanvas.Width;
        //        _canvasHeight = (int)PatternCanvas.Height;
        //    }

        //    if (_canvasWidth <= 0 || _canvasHeight <= 0)
        //    {
        //        // Откладываем центрирование на следующий цикл
        //        Dispatcher.Dispatch(() => FitToScreen());
        //        return;
        //    }

        //    float cellSizeByWidth = (float)_canvasWidth / _pattern.Width;
        //    float cellSizeByHeight = (float)_canvasHeight / _pattern.Height;

        //    float newCellSize = Math.Min(cellSizeByWidth, cellSizeByHeight);
        //    _cellSize = Math.Clamp(newCellSize, _minCellSize, _maxCellSize);

        //    // Центрируем схему
        //    _offsetX = (_canvasWidth - _pattern.Width * _cellSize) / 2;
        //    _offsetY = (_canvasHeight - _pattern.Height * _cellSize) / 2;

        //    System.Diagnostics.Debug.WriteLine($"FitToScreen: canvas={_canvasWidth}x{_canvasHeight}, pattern={_pattern.Width}x{_pattern.Height}, cellSize={_cellSize}, offset=({_offsetX},{_offsetY})");

        //    PatternCanvas?.InvalidateSurface();
        //}

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
            _patternBitmap = new SKBitmap(_pattern.Width, _pattern.Height);
            for (int x = 0; x < _pattern.Width; x++)
            {
                for (int y = 0; y < _pattern.Height; y++)
                {
                    int colorIndex = (x + y) % _colors.Count;
                    var color = _colors[colorIndex];
                    _patternBitmap.SetPixel(x, y, new SKColor((byte)color.Red, (byte)color.Green, (byte)color.Blue));
                }
            }
        }

        private void UpdateColorsList()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ColorsStackLayout.Children.Clear();

                foreach (var color in _colors)
                {
                    var contrastColor = GetContrastColor(color);

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

        private Color GetContrastColor(EmbroideryColor color)
        {
            double luminance = (0.299 * color.Red + 0.587 * color.Green + 0.114 * color.Blue);
            return luminance > 128 ? Colors.Black : Colors.White;
        }

        private SKColor GetContrastSKColor(SKColor color)
        {
            double luminance = (0.299 * color.Red + 0.587 * color.Green + 0.114 * color.Blue);
            return luminance > 128 ? SKColors.Black : SKColors.White;
        }

        //FitTo

        //private void OnCanvasPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        //{
        //    var canvas = e.Surface.Canvas;
        //    canvas.Clear(new SKColor(26, 26, 26));

        //    if (_patternBitmap == null || !_isLoaded)
        //    {
        //        using (var paint = new SKPaint { Color = SKColors.White, TextSize = 20, TextAlign = SKTextAlign.Center })
        //        {
        //            canvas.DrawText("Загрузка...", e.Info.Width / 2, e.Info.Height / 2, paint);
        //        }
        //        return;
        //    }

        //    canvas.Translate(_offsetX, _offsetY);

        //    // Рисуем клетки
        //    for (int y = 0; y < _pattern.Height; y++)
        //    {
        //        for (int x = 0; x < _pattern.Width; x++)
        //        {
        //            float left = x * _cellSize;
        //            float top = y * _cellSize;
        //            var rect = SKRect.Create(left, top, _cellSize, _cellSize);

        //            var pixelColor = _patternBitmap.GetPixel(x, y);

        //            using (var paint = new SKPaint { Color = pixelColor })
        //            {
        //                canvas.DrawRect(rect, paint);
        //            }

        //            // Рисуем символ
        //            if (_cellSize >= 18)
        //            {
        //                var embroideryColor = _colors.FirstOrDefault(c =>
        //                    c.Red == pixelColor.Red &&
        //                    c.Green == pixelColor.Green &&
        //                    c.Blue == pixelColor.Blue);

        //                if (embroideryColor != null && !string.IsNullOrEmpty(embroideryColor.Symbol))
        //                {
        //                    using (var textPaint = new SKPaint
        //                    {
        //                        Color = GetContrastSKColor(pixelColor),
        //                        TextSize = _cellSize * 0.4f,
        //                        TextAlign = SKTextAlign.Center,
        //                        IsAntialias = true
        //                    })
        //                    {
        //                        float centerX = left + _cellSize / 2;
        //                        float centerY = top + _cellSize / 2 + textPaint.TextSize * 0.35f;
        //                        canvas.DrawText(embroideryColor.Symbol, centerX, centerY, textPaint);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    // Рисуем чёрные линии между всеми клетками
        //    using (var blackPaint = new SKPaint { Color = SKColors.Black, StrokeWidth = 0.5f, IsStroke = true })
        //    {
        //        for (int x = 0; x <= _pattern.Width; x++)
        //        {
        //            float xPos = x * _cellSize;
        //            canvas.DrawLine(xPos, 0, xPos, _pattern.Height * _cellSize, blackPaint);
        //        }
        //        for (int y = 0; y <= _pattern.Height; y++)
        //        {
        //            float yPos = y * _cellSize;
        //            canvas.DrawLine(0, yPos, _pattern.Width * _cellSize, yPos, blackPaint);
        //        }
        //    }

        //    // Рисуем жирные чёрные линии каждые 10 клеток
        //    using (var thickPaint = new SKPaint { Color = SKColors.Black, StrokeWidth = 3f, IsStroke = true })
        //    {
        //        for (int x = 10; x < _pattern.Width; x += 10)
        //        {
        //            float xPos = x * _cellSize;
        //            canvas.DrawLine(xPos, 0, xPos, _pattern.Height * _cellSize, thickPaint);
        //        }
        //        for (int y = 10; y < _pattern.Height; y += 10)
        //        {
        //            float yPos = y * _cellSize;
        //            canvas.DrawLine(0, yPos, _pattern.Width * _cellSize, yPos, thickPaint);
        //        }
        //    }
        //}

        private void OnCanvasPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(new SKColor(26, 26, 26)); // Тёмный фон

            if (_patternBitmap == null || !_isLoaded)
            {
                using (var paint = new SKPaint { Color = SKColors.White, TextSize = 20, TextAlign = SKTextAlign.Center })
                {
                    canvas.DrawText("Загрузка...", e.Info.Width / 2, e.Info.Height / 2, paint);
                }
                return;
            }

            if (_isFirstLayout && e.Info.Width > 0 && e.Info.Height > 0)
            {
                _canvasWidth = e.Info.Width;
                _canvasHeight = e.Info.Height;

                // Сбрасываем флаг ДО вызова, чтобы избежать зацикливания
                _isFirstLayout = false;

                // Вызываем принудительный расчет масштаба
                FitToScreen();
                return; // Прерываем текущий кадр, FitToScreen сам вызовет InvalidateSurface() уже с новыми координатами!
            }

            canvas.Translate(_offsetX, _offsetY);

            // Рисуем клетки
            for (int y = 0; y < _pattern.Height; y++)
            {
                for (int x = 0; x < _pattern.Width; x++)
                {
                    float left = x * _cellSize;
                    float top = y * _cellSize;
                    var rect = SKRect.Create(left, top, _cellSize, _cellSize);

                    var pixelColor = _patternBitmap.GetPixel(x, y);

                    using (var paint = new SKPaint { Color = pixelColor })
                    {
                        canvas.DrawRect(rect, paint);
                    }

                    // Рисуем символ
                    if (_cellSize >= 18)
                    {
                        var embroideryColor = _colors.FirstOrDefault(c =>
                            c.Red == pixelColor.Red &&
                            c.Green == pixelColor.Green &&
                            c.Blue == pixelColor.Blue);

                        if (embroideryColor != null && !string.IsNullOrEmpty(embroideryColor.Symbol))
                        {
                            using (var textPaint = new SKPaint
                            {
                                Color = GetContrastSKColor(pixelColor),
                                TextSize = _cellSize * 0.4f,
                                TextAlign = SKTextAlign.Center,
                                IsAntialias = true
                            })
                            {
                                float centerX = left + _cellSize / 2;
                                float centerY = top + _cellSize / 2 + textPaint.TextSize * 0.35f;
                                canvas.DrawText(embroideryColor.Symbol, centerX, centerY, textPaint);
                            }
                        }
                    }
                }
            }

            // Рисуем светлые линии между всеми клетками (видны на любом фоне)
            using (var lightPaint = new SKPaint { Color = new SKColor(180, 180, 180), StrokeWidth = 0.5f, IsStroke = true })
            {
                for (int x = 0; x <= _pattern.Width; x++)
                {
                    float xPos = x * _cellSize;
                    canvas.DrawLine(xPos, 0, xPos, _pattern.Height * _cellSize, lightPaint);
                }
                for (int y = 0; y <= _pattern.Height; y++)
                {
                    float yPos = y * _cellSize;
                    canvas.DrawLine(0, yPos, _pattern.Width * _cellSize, yPos, lightPaint);
                }
            }

            // Рисуем жирные белые линии каждые 10 клеток
            using (var thickPaint = new SKPaint { Color = SKColors.White, StrokeWidth = 3f, IsStroke = true })
            {
                for (int x = 10; x < _pattern.Width; x += 10)
                {
                    float xPos = x * _cellSize;
                    canvas.DrawLine(xPos, 0, xPos, _pattern.Height * _cellSize, thickPaint);
                }
                for (int y = 10; y < _pattern.Height; y += 10)
                {
                    float yPos = y * _cellSize;
                    canvas.DrawLine(0, yPos, _pattern.Width * _cellSize, yPos, thickPaint);
                }
            }
        }

        private void OnCanvasTouch(object sender, SKTouchEventArgs e)
        {
            long id = e.Id;

            if (e.ActionType == SKTouchAction.Pressed)
            {
                _activeTouches[id] = e.Location;

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
                            SKPoint center = new SKPoint((points[0].X + points[1].X) / 2, (points[0].Y + points[1].Y) / 2);
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

        private float CalculateDistance(SKPoint p1, SKPoint p2)
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        //private void ClampOffset()
        //{
        //    float totalWidth = _pattern.Width * _cellSize;
        //    float totalHeight = _pattern.Height * _cellSize;
        //    float maxOffsetX = Math.Max(0, totalWidth - _canvasWidth);
        //    float maxOffsetY = Math.Max(0, totalHeight - _canvasHeight);

        //    if (totalWidth <= _canvasWidth)
        //    {
        //        _offsetX = (_canvasWidth - totalWidth) / 2;
        //    }
        //    else
        //    {
        //        _offsetX = Math.Clamp(_offsetX, -maxOffsetX, 0);
        //    }

        //    if (totalHeight <= _canvasHeight)
        //    {
        //        _offsetY = (_canvasHeight - totalHeight) / 2;
        //    }
        //    else
        //    {
        //        _offsetY = Math.Clamp(_offsetY, -maxOffsetY, 0);
        //    }
        //}

        private void ClampOffset()
        {
            float totalWidth = _pattern.Width * _cellSize;
            float totalHeight = _pattern.Height * _cellSize;

            // --- КОРРЕКТИРОВКА ПО ШИРИНЕ (X) ---
            if (totalWidth <= _canvasWidth)
            {
                // Если схема меньше экрана, она ВСЕГДА строго по центру
                _offsetX = (_canvasWidth - totalWidth) / 2f;
            }
            else
            {
                // Если схема больше экрана, не даем ей улетать за границы видимости
                float minX = _canvasWidth - totalWidth;
                float maxX = 0;
                _offsetX = Math.Clamp(_offsetX, minX, maxX);
            }

            // --- КОРРЕКТИРОВКА ПО ВЫСОТЕ (Y) ---
            if (totalHeight <= _canvasHeight)
            {
                // Если схема меньше экрана, она ВСЕГДА строго по центру
                _offsetY = (_canvasHeight - totalHeight) / 2f;
            }
            else
            {
                // Если схема больше экрана, не даем ей улетать за границы видимости
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

        //private async void OnBackClicked(object sender, EventArgs e)
        //{
        //    await Navigation.PopToRootAsync();
        //}

        private async void OnBackClicked(object sender, EventArgs e)
        {
            try
            {
                // 1. Создаем страницу со списком схем и передаем туда текущего пользователя
                // ВНИМАНИЕ: Замени 'PatternsListPage' на точное название твоего класса страницы со списком схем!
                var patternsListPage = new MyPatternsPage(_currentUser);

                // 2. Делаем её новой корневой страницей приложения, чтобы очистить историю переходов
                if (Application.Current?.MainPage is NavigationPage navPage)
                {
                    // Переключаем главную страницу на список схем внутри контейнера навигации
                    Application.Current.MainPage = new NavigationPage(patternsListPage);
                }
                else
                {
                    // Если NavigationPage не используется глобально
                    Application.Current.MainPage = patternsListPage;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка навигации: {ex.Message}");
                // Запасной вариант, если что-то пошло не так — просто выходим назад
                await Navigation.PopAsync();
            }
        }


        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            PatternCanvas.SizeChanged -= OnCanvasSizeChanged;
        }
    }
}