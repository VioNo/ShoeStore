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
    public partial class AvtoClient : Page
    {
        private List<Tovar> _allTovars = new List<Tovar>();
        private User _currentUser;

        public AvtoClient(User user)
        {
            InitializeComponent();
            _currentUser = user;
            LoadUserInfo();
            LoadTovar();
        }

        private void LoadUserInfo()
        {
            if (_currentUser != null)
            {
                UserInfoTextBlock.Text = _currentUser.Fio;
            }
            else
            {
                UserInfoTextBlock.Text = "Гость";
            }
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

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение выхода",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                NavigationService.Navigate(new MainPage());
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MainPage());
        }
    }

    // ============================================
    // УНИКАЛЬНЫЕ КОНВЕРТЕРЫ ДЛЯ AVTO CLIENT
    // ============================================

    public class AutoClientImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string imageName = value as string;

            if (string.IsNullOrWhiteSpace(imageName))
            {
                return LoadDefaultImage();
            }

            try
            {
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

        private string GetProjectRootDirectory()
        {
            string currentDir = Directory.GetCurrentDirectory();
            string projectRoot = Path.GetFullPath(Path.Combine(currentDir, "..", ".."));
            return projectRoot;
        }

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
                        CultureInfo.CurrentCulture,
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

    public class AutoClientDiscountColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal discount = 0;

            if (value is decimal discountDecimal)
                discount = discountDecimal;
            else if (value is double discountDouble)
                discount = (decimal)discountDouble;
            else if (value is int discountInt)
                discount = discountInt;
            else if (value is float discountFloat)
                discount = (decimal)discountFloat;

            if (discount > 0)
            {
                return discount > 15 ?
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

    public class AutoClientDiscountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Visibility.Collapsed;

            decimal discount = 0;

            if (value is decimal discountDecimal)
                discount = discountDecimal;
            else if (value is double discountDouble)
                discount = (decimal)discountDouble;
            else if (value is int discountInt)
                discount = discountInt;
            else if (value is float discountFloat)
                discount = (decimal)discountFloat;

            return discount > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для подсветки фона строки
    /// </summary>
    public class AutoClientRowBackgroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return Brushes.White;

            decimal discount = 0;
            int quantity = 0;

            // Извлекаем скидку
            if (values[0] != null)
            {
                if (values[0] is decimal discountDecimal)
                    discount = discountDecimal;
                else if (values[0] is double discountDouble)
                    discount = (decimal)discountDouble;
                else if (values[0] is int discountInt)
                    discount = discountInt;
                else if (values[0] is float discountFloat)
                    discount = (decimal)discountFloat;
            }

            // Извлекаем количество
            if (values[1] != null)
            {
                if (values[1] is int quantityInt)
                    quantity = quantityInt;
                else if (values[1] is decimal quantityDecimal)
                    quantity = (int)quantityDecimal;
                else if (values[1] is double quantityDouble)
                    quantity = (int)quantityDouble;
            }

            // Если товара нет на складе - голубой фон (#ADD8E6)
            if (quantity <= 0)
            {
                return new SolidColorBrush(Color.FromArgb(255, 173, 216, 230));
            }

            // Если скидка превышает 15% - зеленый фон (#2E8B57)
            if (discount > 15)
            {
                return new SolidColorBrush(Color.FromArgb(255, 46, 139, 87));
            }

            // Во всех остальных случаях - белый фон
            return Brushes.White;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для отображения итоговой цены
    /// </summary>
    public class AutoClientPriceDisplayConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return string.Empty;

            decimal price = 0;
            decimal discount = 0;

            // Извлекаем цену
            if (values[0] != null)
            {
                if (values[0] is decimal priceDecimal)
                    price = priceDecimal;
                else if (values[0] is double priceDouble)
                    price = (decimal)priceDouble;
                else if (values[0] is int priceInt)
                    price = priceInt;
                else if (values[0] is float priceFloat)
                    price = (decimal)priceFloat;
            }

            // Извлекаем скидку
            if (values[1] != null)
            {
                if (values[1] is decimal discountDecimal)
                    discount = discountDecimal;
                else if (values[1] is double discountDouble)
                    discount = (decimal)discountDouble;
                else if (values[1] is int discountInt)
                    discount = discountInt;
                else if (values[1] is float discountFloat)
                    discount = (decimal)discountFloat;
            }

            // Если есть скидка, рассчитываем итоговую цену
            if (discount > 0)
            {
                decimal finalPrice = price * (100 - discount) / 100;
                return $"{finalPrice:F2} ₽";
            }

            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



}