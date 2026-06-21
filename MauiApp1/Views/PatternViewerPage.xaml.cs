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

        private Dictionary<long, SKPoint> _activeTouches = new Dictionary<long, SKPoint>();
        private float _oldDistance = 0;

        private long _panningTouchId = -1;
        private float _touchStartX, _touchStartY;

        private SKBitmap _patternBitmap;
        private int _canvasWidth = 0, _canvasHeight = 0;
        private bool _isLoaded = false;

        private Dictionary<uint, string> _symbolCache;

        private Dictionary<uint, SKColor> _contrastCache;

        private readonly SKPaint _fillPaint = new SKPaint { IsAntialias = false };
        private readonly SKPaint _textPaint = new SKPaint { TextAlign = SKTextAlign.Center, IsAntialias = true };
        private readonly SKPaint _gridPaint = new SKPaint { Color = new SKColor(180, 180, 180), StrokeWidth = 0.5f, IsStroke = true, IsAntialias = false };
        private readonly SKPaint _thickPaint = new SKPaint { Color = SKColors.White, StrokeWidth = 3f, IsStroke = true, IsAntialias = false };

        private readonly bool _openedFromCreate;

        public PatternViewerPage(EmbroideryPattern pattern, User user, bool openedFromCreate = false)
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _pattern = pattern;
            _currentUser = user;
            _openedFromCreate = openedFromCreate;

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
                        CreateBitmapFromColorsOnly();
                    }
                }
                else
                {
                    CreateBitmapFromColorsOnly();
                }

                BuildCaches();
            });
        }

        private void CreateBitmapFromColorsOnly()
        {
            var bitmap = new SKBitmap(_pattern.Width, _pattern.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
            var pixels = new SKColor[_pattern.Width * _pattern.Height];
            for (int y = 0; y < _pattern.Height; y++)
            {
                for (int x = 0; x < _pattern.Width; x++)
                {
                    int colorIndex = (x + y) % _colors.Count;
                    var color = _colors[colorIndex];
                    pixels[y * _pattern.Width + x] = new SKColor((byte)color.Red, (byte)color.Green, (byte)color.Blue);
                }
            }
            bitmap.Pixels = pixels;
            _patternBitmap = bitmap;
        }

        private void BuildCaches()
        {
            _symbolCache = new Dictionary<uint, string>();
            _contrastCache = new Dictionary<uint, SKColor>();

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

        private void OnCanvasPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(new SKColor(26, 26, 26));

            if (_patternBitmap == null || !_isLoaded)
            {
                _fillPaint.Color = SKColors.White;
                _fillPaint.TextSize = 20;
                _fillPaint.TextAlign = SKTextAlign.Center;
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

            int startX = Math.Max(0, (int)(-_offsetX / _cellSize));
            int startY = Math.Max(0, (int)(-_offsetY / _cellSize));
            int endX = Math.Min(_pattern.Width, (int)((-_offsetX + _canvasWidth) / _cellSize) + 1);
            int endY = Math.Min(_pattern.Height, (int)((-_offsetY + _canvasHeight) / _cellSize) + 1);

            bool drawSymbols = _cellSize >= 18 && _symbolCache != null;
            float textSize = _cellSize * 0.4f;

            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    float left = x * _cellSize;
                    float top = y * _cellSize;
                    var rect = SKRect.Create(left, top, _cellSize, _cellSize);

                    var pixelColor = _patternBitmap.GetPixel(x, y);

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

            if (_cellSize >= 10)  
            {
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
            }

            if (_cellSize >= 15) 
            {
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

        private async void OnBackClicked(object sender, EventArgs e)
        {
            if (_openedFromCreate)
            {
                var navStack = Navigation.NavigationStack.ToList();
                var myPatternsPage = navStack.OfType<MyPatternsPage>().FirstOrDefault();

                if (myPatternsPage != null)
                {
                    for (int i = navStack.Count - 2; i >= 0; i--)
                    {
                        if (navStack[i] is MyPatternsPage) break;
                        Navigation.RemovePage(navStack[i]);
                    }
                    await Navigation.PopAsync();
                }
                else
                {
                    var patternsPage = new MyPatternsPage(_currentUser);
                    Navigation.InsertPageBefore(patternsPage, this);
                    await Navigation.PopAsync();
                }
            }
            else
            {
                await Navigation.PopAsync();
            }
        }

        protected override bool OnBackButtonPressed()
        {
            if (_openedFromCreate)
            {
                OnBackClicked(this, EventArgs.Empty);
                return true;
            }

            return base.OnBackButtonPressed();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            PatternCanvas.SizeChanged -= OnCanvasSizeChanged;
            _fillPaint.Dispose();
            _textPaint.Dispose();
            _gridPaint.Dispose();
            _thickPaint.Dispose();
        }


#if WINDOWS
private void WinUI_PointerWheelChanged(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
{
    // 1. Получаем позицию курсора мыши относительно холста SkiaSharp
    var currentPoint = e.GetCurrentPoint(sender as Microsoft.UI.Xaml.UIElement);
    if (currentPoint == null) return;

    SKPoint mousePoint = new SKPoint((float)currentPoint.Position.X, (float)currentPoint.Position.Y);

    // 2. Получаем дельту прокрутки колёсика
    int delta = currentPoint.Properties.MouseWheelDelta;
    if (delta == 0) return;

    // 3. Считаем масштаб (крутим вперед — увеличиваем, назад — уменьшаем)
    float scaleFactor = delta > 0 ? 1.1f : 0.9f;

    float newCellSize = _cellSize * scaleFactor;
    newCellSize = Math.Clamp(newCellSize, _minCellSize, _maxCellSize);

    if (newCellSize != _cellSize)
    {
        // Масштабируем холст относительно точки курсора
        _offsetX = mousePoint.X - (mousePoint.X - _offsetX) * (newCellSize / _cellSize);
        _offsetY = mousePoint.Y - (mousePoint.Y - _offsetY) * (newCellSize / _cellSize);
        _cellSize = newCellSize;

        // Ограничиваем сдвиг, чтобы схема не улетала
        ClampOffset();

        // Перерисовываем холст
        PatternCanvas.InvalidateSurface();
    }

    // Говорим системе, что событие обработано
    e.Handled = true;
}
#endif


    }
}