using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ShoeStore
{
    public partial class GuestPage : Page
    {
        private List<Tovar> _allTovars = new List<Tovar>();

        public GuestPage()
        {
            InitializeComponent();
            LoadTovar();
        }

        private void LoadTovar()
        {
            try
            {
                var context = ShoeStoreBD.GetContext();
                _allTovars = context.Tovar
                    .Include(t => t.Types)
                    .Include(t => t.Categories)
                    .Include(t => t.Manufactureres)
                    .Include(t => t.Supplieres)
                    .ToList();

                ItemsControlTovar.ItemsSource = _allTovars;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MainPage());
        }
    }

    // КОНВЕРТЕРЫ (ДОЛЖНЫ БЫТЬ PUBLIC!)
    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string imageName = value as string;

            // Если имя файла пустое, используем заглушку
            if (string.IsNullOrWhiteSpace(imageName))
            {
                return LoadDefaultImage();
            }

            try
            {
                // Путь к папке Resources в проекте
                string projectRoot = GetProjectRootDirectory();
                string resourcesPath = Path.Combine(projectRoot, "Resources", imageName);

                if (File.Exists(resourcesPath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(resourcesPath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
                else
                {
                    return LoadDefaultImage();
                }
            }
            catch
            {
                return LoadDefaultImage();
            }
        }

        // Метод для получения пути к папке проекта
        private string GetProjectRootDirectory()
        {
            string currentDir = Directory.GetCurrentDirectory();
            string projectRoot = Path.GetFullPath(Path.Combine(currentDir, "..", ".."));
            return projectRoot;
        }

        // Метод для загрузки изображения-заглушки
        private BitmapImage LoadDefaultImage()
        {
            try
            {
                string projectRoot = GetProjectRootDirectory();
                string defaultImagePath = Path.Combine(projectRoot, "Resources", "picture.png");

                if (File.Exists(defaultImagePath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(defaultImagePath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
                else
                {
                    return CreateSimpleDefaultImage();
                }
            }
            catch
            {
                return CreateSimpleDefaultImage();
            }
        }

        // Метод для создания простой заглушки программно
        private BitmapImage CreateSimpleDefaultImage()
        {
            try
            {
                int width = 100;
                int height = 100;

                DrawingVisual drawingVisual = new DrawingVisual();
                using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                {
                    drawingContext.DrawRectangle(Brushes.LightGray, null, new Rect(0, 0, width, height));
                    drawingContext.DrawRectangle(null, new Pen(Brushes.DarkGray, 1), new Rect(0, 0, width, height));

                    FormattedText text = new FormattedText(
                        "No Image",
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Arial"),
                        12,
                        Brushes.Black);

                    drawingContext.DrawText(text, new Point(20, 40));
                }

                RenderTargetBitmap renderTarget = new RenderTargetBitmap(
                    width, height, 96, 96, PixelFormats.Pbgra32);
                renderTarget.Render(drawingVisual);

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderTarget));

                using (MemoryStream stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    stream.Position = 0;

                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = stream;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();

                    return bitmapImage;
                }
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DiscountColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal discountDecimal && discountDecimal > 0)
            {
                return discountDecimal > 15 ?
                    new SolidColorBrush(Colors.Green) :
                    new SolidColorBrush(Colors.Red);
            }
            else if (value is double discountDouble && discountDouble > 0)
            {
                return discountDouble > 15 ?
                    new SolidColorBrush(Colors.Green) :
                    new SolidColorBrush(Colors.Red);
            }
            else if (value is int discountInt && discountInt > 0)
            {
                return discountInt > 15 ?
                    new SolidColorBrush(Colors.Green) :
                    new SolidColorBrush(Colors.Red);
            }
            else if (value is float discountFloat && discountFloat > 0)
            {
                return discountFloat > 15 ?
                    new SolidColorBrush(Colors.Green) :
                    new SolidColorBrush(Colors.Red);
            }

            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DiscountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal discountDecimal && discountDecimal > 0)
            {
                return Visibility.Visible;
            }
            else if (value is double discountDouble && discountDouble > 0)
            {
                return Visibility.Visible;
            }
            else if (value is int discountInt && discountInt > 0)
            {
                return Visibility.Visible;
            }
            else if (value is float discountFloat && discountFloat > 0)
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}