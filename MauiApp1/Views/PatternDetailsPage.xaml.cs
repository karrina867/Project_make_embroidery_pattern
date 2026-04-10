using MauiApp1.Services;
using MauiApp1.Models;
using System.Collections.ObjectModel;

namespace MauiApp1.Views
{
    public partial class PatternDetailsPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private readonly EmbroideryPattern _pattern;
        private List<EmbroideryColor> _colors;

        public PatternDetailsPage(EmbroideryPattern pattern)
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _pattern = pattern;

            LoadPatternDetails();
        }

        private async void LoadPatternDetails()
        {
            try
            {
                PatternNameLabel.Text = _pattern.Name;
                PatternSizeLabel.Text = $"Размер: {_pattern.Width} x {_pattern.Height} крестиков";
                PatternColorsLabel.Text = $"Количество цветов: {_pattern.ColorCount}";

                _colors = await _databaseService.GetPatternColors(_pattern.Id);
                ColorsCollection.ItemsSource = _colors;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить детали схемы: {ex.Message}", "OK");
            }
        }

        private async void OnExportClicked(object sender, EventArgs e)
        {
            try
            {
                var exportText = $"Схема вышивки: {_pattern.Name}\n";
                exportText += $"Размер: {_pattern.Width} x {_pattern.Height}\n";
                exportText += $"Дата создания: {_pattern.CreatedAt:dd.MM.yyyy}\n\n";
                exportText += "Цвета и символы:\n";
                exportText += "━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n";

                foreach (var color in _colors)
                {
                    exportText += $"{color.Symbol} - {color.ColorHex} (DMC: {color.DMCNumber}) - {color.PixelCount} крестиков\n";
                }

                var fileName = $"{_pattern.Name}_схема.txt";
                var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
                File.WriteAllText(filePath, exportText);

                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Экспорт схемы",
                    File = new ShareFile(filePath)
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось экспортировать: {ex.Message}", "OK");
            }
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            var confirm = await DisplayAlert("Удаление",
                $"Вы уверены, что хотите удалить схему '{_pattern.Name}'?",
                "Да", "Нет");

            if (confirm)
            {
                var deleted = await _databaseService.DeletePattern(_pattern);
                if (deleted)
                {
                    await DisplayAlert("Успех", "Схема удалена", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось удалить схему", "OK");
                }
            }
        }
    }
}


//using MauiApp1.Services;
//using MauiApp1.Models;

//namespace MauiApp1.Views
//{
//    public partial class PatternDetailsPage : ContentPage
//    {
//        private readonly DatabaseService _databaseService;
//        private readonly EmbroideryPattern _pattern;
//        private List<EmbroideryColor> _colors;

//        public PatternDetailsPage(EmbroideryPattern pattern)
//        {
//            InitializeComponent();
//            _databaseService = new DatabaseService();
//            _pattern = pattern;

//            LoadPatternDetails();
//        }

//        private async void LoadPatternDetails()
//        {
//            PatternNameLabel.Text = _pattern.Name;
//            PatternSizeLabel.Text = $"Размер: {_pattern.Width} x {_pattern.Height} крестиков";
//            PatternColorsLabel.Text = $"Количество цветов: {_pattern.ColorCount}";

//            _colors = await _databaseService.GetPatternColors(_pattern.Id);
//            ColorsCollection.ItemsSource = _colors;
//        }

//        private async void OnExportClicked(object sender, EventArgs e)
//        {
//            // Экспорт в текстовый файл
//            var exportText = $"Схема вышивки: {_pattern.Name}\n";
//            exportText += $"Размер: {_pattern.Width} x {_pattern.Height}\n";
//            exportText += $"Дата создания: {_pattern.CreatedAt:dd.MM.yyyy}\n\n";
//            exportText += "Цвета и символы:\n";
//            exportText += "━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n";

//            foreach (var color in _colors)
//            {
//                exportText += $"{color.Symbol} - {color.ColorHex} (DMC: {color.DMCNumber}) - {color.PixelCount} крестиков\n";
//            }

//            var fileName = $"{_pattern.Name}_схема.txt";
//            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
//            File.WriteAllText(filePath, exportText);

//            await Share.Default.RequestAsync(new ShareFileRequest
//            {
//                Title = "Экспорт схемы",
//                File = new ShareFile(filePath)
//            });
//        }

//        private async void OnDeleteClicked(object sender, EventArgs e)
//        {
//            var confirm = await DisplayAlert("Удаление",
//                $"Вы уверены, что хотите удалить схему '{_pattern.Name}'?",
//                "Да", "Нет");

//            if (confirm)
//            {
//                var deleted = await _databaseService.DeletePattern(_pattern);
//                if (deleted)
//                {
//                    await DisplayAlert("Успех", "Схема удалена", "OK");
//                    await Navigation.PopAsync();
//                }
//                else
//                {
//                    await DisplayAlert("Ошибка", "Не удалось удалить схему", "OK");
//                }
//            }
//        }
//    }
//}