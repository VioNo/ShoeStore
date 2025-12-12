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
        private string _discountFilter = "Все товары";
        private string _currentSort = "";

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

                // Применяем фильтрацию и сортировку
                ApplyFiltersAndSort();

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

            // Фильтр по скидке
            cmbDiscountFilter.SelectedIndex = 0;
        }

        private void ApplyFiltersAndSort()
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

            // Фильтр по скидке
            if (_discountFilter == "Со скидкой")
            {
                filtered = filtered.Where(t => (t.Discount ?? 0) > 0);
            }
            else if (_discountFilter == "Без скидки")
            {
                filtered = filtered.Where(t => (t.Discount ?? 0) == 0);
            }

            // Применяем сортировку
            switch (_currentSort)
            {
                case "PriceAsc":
                    filtered = filtered.OrderBy(t => t.Price);
                    break;
                case "PriceDesc":
                    filtered = filtered.OrderByDescending(t => t.Price);
                    break;
                case "QuantityAsc":
                    filtered = filtered.OrderBy(t => t.Quantity);
                    break;
                case "QuantityDesc":
                    filtered = filtered.OrderByDescending(t => t.Quantity);
                    break;
                case "DiscountAsc":
                    filtered = filtered.OrderBy(t => t.Discount ?? 0);
                    break;
                case "DiscountDesc":
                    filtered = filtered.OrderByDescending(t => t.Discount ?? 0);
                    break;
                default:
                    // Сортировка по умолчанию - по артикулу
                    filtered = filtered.OrderBy(t => t.Article);
                    break;
            }

            _filteredTovars = filtered.ToList();
            ItemsControlTovar.ItemsSource = _filteredTovars;
        }

        private void UpdateStatistics()
        {
            if (_filteredTovars == null) return;

            txtTotalItems.Text = $"Всего товаров: {_filteredTovars.Count}";

            int withDiscount = _filteredTovars.Count(t => (t.Discount ?? 0) > 0);
            txtWithDiscount.Text = $"Со скидкой: {withDiscount}";

            int lowStock = _filteredTovars.Count(t => (t.Quantity ?? 0) < 10);
            txtLowStock.Text = $"Мало на складе: {lowStock}";
        }

        // Обработчики событий фильтрации

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _currentSearch = txtSearch.Text;
            ApplyFiltersAndSort();
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
            ApplyFiltersAndSort();
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
            ApplyFiltersAndSort();
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
            ApplyFiltersAndSort();
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
            ApplyFiltersAndSort();
            UpdateStatistics();
        }

        private void CmbDiscountFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbDiscountFilter.SelectedItem is ComboBoxItem selectedItem)
            {
                _discountFilter = selectedItem.Content.ToString();
            }
            ApplyFiltersAndSort();
            UpdateStatistics();
        }

        // Обработчики сортировки

        private void RbSort_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb == null) return;

            _currentSort = rb.Name switch
            {
                "rbPriceAsc" => "PriceAsc",
                "rbPriceDesc" => "PriceDesc",
                "rbQuantityAsc" => "QuantityAsc",
                "rbQuantityDesc" => "QuantityDesc",
                "rbDiscountAsc" => "DiscountAsc",
                "rbDiscountDesc" => "DiscountDesc",
                _ => ""
            };

            ApplyFiltersAndSort();
        }

        // Обработчики кнопок

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
            MessageBox.Show("Данные успешно обновлены!", "Обновление",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnResetFilters_Click(object sender, RoutedEventArgs e)
        {
            // Сброс фильтров
            txtSearch.Text = "";
            cmbTypeFilter.SelectedIndex = 0;
            cmbCategoryFilter.SelectedIndex = 0;
            cmbManufacturerFilter.SelectedIndex = 0;
            cmbSupplierFilter.SelectedIndex = 0;
            cmbDiscountFilter.SelectedIndex = 0;

            // Сброс сортировки
            _currentSort = "";

            // Сброс радио-кнопок
            var sortPanel = rbPriceAsc.Parent as StackPanel;
            if (sortPanel != null)
            {
                foreach (var child in sortPanel.Children)
                {
                    if (child is RadioButton radioButton)
                    {
                        radioButton.IsChecked = false;
                    }
                }
            }

            ApplyFiltersAndSort();
            UpdateStatistics();
        }

        private void BtnAddNew_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функционал добавления товара будет реализован позже", "В разработке",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn?.Tag is string article)
            {
                var tovar = _allTovars.FirstOrDefault(t => t.Article == article);
                if (tovar != null)
                {
                    MessageBox.Show($"Редактирование товара: {tovar.Types?.Type}\nАртикул: {article}",
                        "Редактирование", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void BtnChangePhoto_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn?.Tag is string article)
            {
                MessageBox.Show($"Изменение фото для товара Артикул: {article}",
                    "Изменение фото", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn?.Tag is string article)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этот товар?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var context = ShoeStoreBD.GetContext();
                        var tovar = context.Tovar.FirstOrDefault(t => t.Article == article);
                        if (tovar != null)
                        {
                            context.Tovar.Remove(tovar);
                            context.SaveChanges();
                            LoadData(); // Перезагружаем данные
                            MessageBox.Show($"Товар {article} успешно удален!",
                                "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
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

    public class ManagerQuantityColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal quantityDecimal)
            {
                return GetColorForQuantity((double)quantityDecimal);
            }
            else if (value is double quantityDouble)
            {
                return GetColorForQuantity(quantityDouble);
            }
            else if (value is int quantityInt)
            {
                return GetColorForQuantity(quantityInt);
            }
            else if (value is float quantityFloat)
            {
                return GetColorForQuantity(quantityFloat);
            }

            return new SolidColorBrush(Colors.Black);
        }

        private SolidColorBrush GetColorForQuantity(double quantity)
        {
            if (quantity <= 0)
                return new SolidColorBrush(Colors.Red);
            else if (quantity < 10)
                return new SolidColorBrush(Colors.Orange);
            else if (quantity < 50)
                return new SolidColorBrush(Colors.Gold);
            else
                return new SolidColorBrush(Colors.Green);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DiscountPriceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int price && parameter is double discount)
            {
                double discountedPrice = price * (1 - discount / 100);
                return discountedPrice.ToString("N0");
            }
            return value?.ToString() ?? "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}