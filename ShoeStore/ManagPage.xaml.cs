using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ShoeStore
{
    public partial class ManagPage : Page
    {
        private List<Tovar> _allTovars = new List<Tovar>();
        private List<Tovar> _filteredTovars = new List<Tovar>();
        private List<Types> _types = new List<Types>();
        private List<Categories> _categories = new List<Categories>();
        private List<Manufactureres> _manufacturers = new List<Manufactureres>();
        private List<Supplieres> _suppliers = new List<Supplieres>();
        private User _currentUser;

        // Текущие фильтры
        private string _currentSearch = "";
        private int? _selectedTypeId = null;
        private int? _selectedCategoryId = null;
        private int? _selectedManufacturerId = null;
        private int? _selectedSupplierId = null;

        public ManagPage(User user)
        {
            InitializeComponent();
            _currentUser = user;
            LoadUserInfo();
            LoadData();
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

        private void LoadData()
        {
            try
            {
                var context = ShoeStoreBD.GetContext();

                // Загружаем все данные
                _allTovars = context.Tovar
                    .Include(t => t.Types)
                    .Include(t => t.Categories)
                    .Include(t => t.Manufactureres)
                    .Include(t => t.Supplieres)
                    .ToList();

                // Загружаем данные для фильтров
                _types = context.Types.ToList();
                _categories = context.Categories.ToList();
                _manufacturers = context.Manufactureres.ToList();
                _suppliers = context.Supplieres.ToList();

                // Заполняем фильтры
                FillFilters();

                // Применяем фильтрацию
                ApplyFilters();

                // Обновляем статистику
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillFilters()
        {
            // Типы
            cmbTypeFilter.Items.Clear();
            cmbTypeFilter.Items.Add(new ComboBoxItem { Content = "Все типы", Tag = "All" });
            foreach (var type in _types)
            {
                cmbTypeFilter.Items.Add(type);
            }
            cmbTypeFilter.SelectedIndex = 0;

            // Категории
            cmbCategoryFilter.Items.Clear();
            cmbCategoryFilter.Items.Add(new ComboBoxItem { Content = "Все категории", Tag = "All" });
            foreach (var category in _categories)
            {
                cmbCategoryFilter.Items.Add(category);
            }
            cmbCategoryFilter.SelectedIndex = 0;

            // Производители
            cmbManufacturerFilter.Items.Clear();
            cmbManufacturerFilter.Items.Add(new ComboBoxItem { Content = "Все производители", Tag = "All" });
            foreach (var manufacturer in _manufacturers)
            {
                cmbManufacturerFilter.Items.Add(manufacturer);
            }
            cmbManufacturerFilter.SelectedIndex = 0;

            // Поставщики
            cmbSupplierFilter.Items.Clear();
            cmbSupplierFilter.Items.Add(new ComboBoxItem { Content = "Все поставщики", Tag = "All" });
            foreach (var supplier in _suppliers)
            {
                cmbSupplierFilter.Items.Add(supplier);
            }
            cmbSupplierFilter.SelectedIndex = 0;
        }

        private void ApplyFilters()
        {
            if (_allTovars == null) return;

            // Применяем фильтры
            var filtered = _allTovars.AsEnumerable();

            // Поиск по текстовым полям
            if (!string.IsNullOrWhiteSpace(_currentSearch))
            {
                string searchLower = _currentSearch.ToLower();
                filtered = filtered.Where(t =>
                    (t.Types?.Type?.ToLower().Contains(searchLower) ?? false) ||
                    (t.Description?.ToLower().Contains(searchLower) ?? false) ||
                    (t.Article?.ToLower().Contains(searchLower) ?? false) ||
                    (t.Categories?.Category?.ToLower().Contains(searchLower) ?? false) ||
                    (t.Manufactureres?.Manufacturer?.ToLower().Contains(searchLower) ?? false) ||
                    (t.Supplieres?.Supplier?.ToLower().Contains(searchLower) ?? false));
            }

            // Фильтр по типу
            if (_selectedTypeId.HasValue)
            {
                filtered = filtered.Where(t => t.Type == _selectedTypeId.Value);
            }

            // Фильтр по категории
            if (_selectedCategoryId.HasValue)
            {
                filtered = filtered.Where(t => t.Id_category == _selectedCategoryId.Value);
            }

            // Фильтр по производителю
            if (_selectedManufacturerId.HasValue)
            {
                filtered = filtered.Where(t => t.Id_manufacturer == _selectedManufacturerId.Value);
            }

            // Фильтр по поставщику
            if (_selectedSupplierId.HasValue)
            {
                filtered = filtered.Where(t => t.Id_supplier == _selectedSupplierId.Value);
            }

            _filteredTovars = filtered.ToList();
            ItemsControlTovar.ItemsSource = _filteredTovars;
        }

        private void UpdateStatistics()
        {
            if (_filteredTovars == null) return;

            int totalItems = _filteredTovars.Count;
            int withDiscount = _filteredTovars.Count(t => (t.Discount ?? 0) > 0);
            int lowStock = _filteredTovars.Count(t => (t.Quantity ?? 0) < 10);

            txtStatistics.Text = $"Показано товаров: {totalItems} | Со скидкой: {withDiscount} | Мало на складе: {lowStock}";
        }

        // Обработчики событий фильтрации

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _currentSearch = txtSearch.Text;
            ApplyFilters();
            UpdateStatistics();
        }

        private void CmbTypeFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTypeFilter.SelectedItem is Types selectedType)
            {
                _selectedTypeId = selectedType.Id_type;
            }
            else
            {
                _selectedTypeId = null;
            }
            ApplyFilters();
            UpdateStatistics();
        }

        private void CmbCategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCategoryFilter.SelectedItem is Categories selectedCategory)
            {
                _selectedCategoryId = selectedCategory.Id_category;
            }
            else
            {
                _selectedCategoryId = null;
            }
            ApplyFilters();
            UpdateStatistics();
        }

        private void CmbManufacturerFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbManufacturerFilter.SelectedItem is Manufactureres selectedManufacturer)
            {
                _selectedManufacturerId = selectedManufacturer.Id_manufacturer;
            }
            else
            {
                _selectedManufacturerId = null;
            }
            ApplyFilters();
            UpdateStatistics();
        }

        private void CmbSupplierFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSupplierFilter.SelectedItem is Supplieres selectedSupplier)
            {
                _selectedSupplierId = selectedSupplier.Id_supplier;
            }
            else
            {
                _selectedSupplierId = null;
            }
            ApplyFilters();
            UpdateStatistics();
        }

        // Обработчики кнопок

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
            MessageBox.Show("Данные успешно обновлены!", "Обновление",
                MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void ViewOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ManagOrdersPage(_currentUser));
        }
    }

    // Конвертеры для ManagPage

    public class ManagerImagePathConverter : IValueConverter
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
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
                        Brushes.Black,
                        VisualTreeHelper.GetDpi(drawingVisual).PixelsPerDip);

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
    }

    public class ManagerDiscountColorConverter : IValueConverter
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

    public class ManagerDiscountToVisibilityConverter : IValueConverter
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