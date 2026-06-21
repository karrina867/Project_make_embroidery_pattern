using MauiApp1.Services;
using MauiApp1.Models;
using SkiaSharp;

namespace MauiApp1.Views
{
    public partial class CreatePatternPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private readonly User _currentUser;
        private string _selectedImagePath;
        private int _originalImageWidth = 0;
        private int _originalImageHeight = 0;
        private bool _isUpdating = false;

        public CreatePatternPage(User user)
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _currentUser = user;

            this.Loaded += (s, e) =>
            {
                PatternNameEntry.Unfocus();
                WidthEntry.Unfocus();
                HeightEntry.Unfocus();
            };
        }

        private async void OnSelectImageClicked(object sender, EventArgs e)
        {
            try
            {
                PatternNameEntry.Unfocus();
                WidthEntry.Unfocus();
                HeightEntry.Unfocus();
                var status = await Permissions.RequestAsync<Permissions.StorageRead>();
                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlert("Ошибка", "Нет доступа к галерее", "OK");
                    return;
                }

                var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Выберите изображение из галереи"
                });

                if (result != null)
                {
                    _selectedImagePath = result.FullPath;
                    SelectedImage.Source = ImageSource.FromFile(_selectedImagePath);

                    SelectedImage.IsVisible = true;
                    SelectImageLabel.IsVisible = false;

                    PatternNameEntry.Unfocus();
                    WidthEntry.Unfocus();
                    HeightEntry.Unfocus();

                    var processor = new MauiApp1.Services.ImageProcessor();
                    using (var correctBitmap = processor.FixOrientation(_selectedImagePath))
                    {
                        if (correctBitmap != null)
                        {
                            _originalImageWidth = correctBitmap.Width;
                            _originalImageHeight = correctBitmap.Height;

                            System.Diagnostics.Debug.WriteLine($"Выбрано фото: реальный размер после разворота = {_originalImageWidth}x{_originalImageHeight}");
                        }
                        else
                        {
                            using (var stream = File.OpenRead(_selectedImagePath))
                            using (var bitmap = SKBitmap.Decode(stream))
                            {
                                if (bitmap != null)
                                {
                                    _originalImageWidth = bitmap.Width;
                                    _originalImageHeight = bitmap.Height;
                                }
                            }
                        }
                    }

                    SaveButton.IsEnabled = true;
                    MessageLabel.Text = "";
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить изображение: {ex.Message}", "OK");
            }
        }

        private void OnWidthChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdating) return;

            if (KeepAspectCheckBox.IsChecked && _originalImageWidth > 0 && _originalImageHeight > 0)
            {
                if (int.TryParse(WidthEntry.Text, out int newWidth) && newWidth > 0)
                {
                    _isUpdating = true;

                    int newHeight = (int)(newWidth * (double)_originalImageHeight / _originalImageWidth);
                    HeightEntry.Text = newHeight.ToString();

                    _isUpdating = false;
                }
            }

        }

        private void OnHeightChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdating) return;

            if (KeepAspectCheckBox.IsChecked && _originalImageWidth > 0 && _originalImageHeight > 0)
            {
                if (int.TryParse(HeightEntry.Text, out int newHeight) && newHeight > 0)
                {
                    _isUpdating = true;

                    int newWidth = (int)(newHeight * (double)_originalImageWidth / _originalImageHeight);
                    WidthEntry.Text = newWidth.ToString();

                    _isUpdating = false;
                }
            }

        }

        private const int MAX_PATTERN_SIZE = 500;
        private const int MAX_PATTERNS_PER_USER = 100;

      
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }


        private async void OnSavePatternClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PatternNameEntry.Text))
            {
                MessageLabel.Text = "Введите название схемы";
                return;
            }

            if (PatternNameEntry.Text.Length < 2)
            {
                MessageLabel.Text = "Название схемы должно содержать минимум 2 символа";
                return;
            }

            if (!int.TryParse(WidthEntry.Text, out int width) || width <= 0 || width > MAX_PATTERN_SIZE)
            {
                MessageLabel.Text = $"Введите корректную ширину (1-{MAX_PATTERN_SIZE})";
                return;
            }

            if (!int.TryParse(HeightEntry.Text, out int height) || height <= 0 || height > MAX_PATTERN_SIZE)
            {
                MessageLabel.Text = $"Введите корректную высоту (1-{MAX_PATTERN_SIZE})";
                return;
            }

            var userPatterns = await _databaseService.GetUserPatterns(_currentUser.Id);
            if (userPatterns.Count >= MAX_PATTERNS_PER_USER)
            {
                await DisplayAlert("Ограничение", $"Вы можете создать не более {MAX_PATTERNS_PER_USER} схем", "OK");
                return;
            }

            if (string.IsNullOrEmpty(_selectedImagePath))
            {
                MessageLabel.Text = "Выберите изображение";
                return;
            }

            try
            {
                SaveButton.IsEnabled = false;
                SaveButton.Text = "Обработка изображения...";

                var processor = new ImageProcessor();
                var (colors, colorMatrix) = await processor.ProcessImageWithMatrix(_selectedImagePath, width, height);

                SaveButton.Text = "Сохранение схемы...";

                var matrixFileName = processor.SaveColorMatrix(colorMatrix, PatternNameEntry.Text);

                byte[] imageBytes = File.ReadAllBytes(_selectedImagePath);

                EmbroideryPattern pattern = null; 

                using (var stream = new MemoryStream(imageBytes))
                {
                    pattern = new EmbroideryPattern
                    {
                        Name = PatternNameEntry.Text,
                        Width = width,
                        Height = height,
                        ColorCount = colors.Count,
                        CellSize = 30,
                        PatternDataFile = matrixFileName
                    };

                    var (success, message, patternId) = await _databaseService.SavePattern(pattern, stream, _currentUser.Id);

                    if (success)
                    {
                        await _databaseService.SavePatternColors(patternId, colors);
                        await _databaseService.UpdatePatternColorCount(patternId, colors.Count);

                        pattern = await _databaseService.GetPatternById(patternId, _currentUser.Id);

                        if (pattern != null)
                        {
                            await Navigation.PushAsync(new PatternViewerPage(pattern, _currentUser, openedFromCreate: true));

                            var createPageInStack = Navigation.NavigationStack.FirstOrDefault(p => p is CreatePatternPage);
                            if (createPageInStack != null)
                            {
                                Navigation.RemovePage(createPageInStack);
                            }
                        }
                        else
                        {
                            await Navigation.PopAsync();
                        }
                    }
                    else
                    {
                        MessageLabel.Text = message;
                        SaveButton.IsEnabled = true;
                        SaveButton.Text = "Сохранить схему";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageLabel.Text = $"Ошибка: {ex.Message}";
                SaveButton.IsEnabled = true;
                SaveButton.Text = "Сохранить схему";
            }
        } 
    }
}