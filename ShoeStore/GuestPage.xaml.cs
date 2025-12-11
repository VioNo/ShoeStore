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

    // Конвертеры прямо в этом файле
    //public class ImagePathConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        string imageName = value as string;
    //        if (string.IsNullOrWhiteSpace(imageName))
    //            return null;

    //        try
    //        {
    //            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
    //            string imagePath = Path.Combine(baseDir, "Images", imageName);

    //            if (File.Exists(imagePath))
    //            {
    //                BitmapImage bitmap = new BitmapImage();
    //                bitmap.BeginInit();
    //                bitmap.UriSource = new Uri(imagePath);
    //                bitmap.CacheOption = BitmapCacheOption.OnLoad;
    //                bitmap.EndInit();
    //                bitmap.Freeze();
    //                return bitmap;
    //            }
    //            return null;
    //        }
    //        catch
    //        {
    //            return null;
    //        }
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
    
    //public class DiscountColorConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if (value is double discount && discount > 0)
    //        {
    //            return discount > 15 ?
    //                new SolidColorBrush(Colors.Green) :
    //                new SolidColorBrush(Colors.Red);
    //        }
    //        return new SolidColorBrush(Colors.Transparent);
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    //public class DiscountToVisibilityConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if (value is double discount && discount > 0)
    //        {
    //            return Visibility.Visible;
    //        }
    //        return Visibility.Collapsed;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}