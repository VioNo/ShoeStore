using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.IO;

namespace ShoeStore
{
    public partial class ProductEditWindow : Window, INotifyPropertyChanged
    {
        private Tovar _product;
        private bool _isNewProduct;
        private BitmapImage _photoPreview;

        // Списки для ComboBox
        public List<Types> Types { get; set; }
        public List<Categories> Categories { get; set; }
        public List<Manufactureres> Manufacturers { get; set; }
        public List<Supplieres> Suppliers { get; set; }

        // Свойства для привязки
        public string WindowTitle => _isNewProduct ? "Добавление товара" : "Редактирование товара";

        public bool IsNewProduct => _isNewProduct;

        public Visibility DeleteButtonVisibility => _isNewProduct ? Visibility.Collapsed : Visibility.Visible;

        public Visibility PhotoPreviewVisibility => string.IsNullOrWhiteSpace(Photo) ? Visibility.Collapsed : Visibility.Visible;

        public BitmapImage PhotoPreview
        {
            get => _photoPreview;
            set
            {
                _photoPreview = value;
                OnPropertyChanged();
            }
        }

        public string Article
        {
            get => _product.Article;
            set { _product.Article = value; OnPropertyChanged(); }
        }

        public int? SelectedTypeId
        {
            get => _product.Type;
            set { _product.Type = value; OnPropertyChanged(); }
        }

        public int? SelectedCategoryId
        {
            get => _product.Id_category;
            set { _product.Id_category = value; OnPropertyChanged(); }
        }

        public string ProductName
        {
            get => _product.Types?.Type ?? "";
            set
            {
                if (_product.Types != null)
                    _product.Types.Type = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get => _product.Description;
            set { _product.Description = value; OnPropertyChanged(); }
        }

        public int? SelectedManufacturerId
        {
            get => _product.Id_manufacturer;
            set { _product.Id_manufacturer = value; OnPropertyChanged(); }
        }

        public int? SelectedSupplierId
        {
            get => _product.Id_supplier;
            set { _product.Id_supplier = value; OnPropertyChanged(); }
        }

        public int? Price
        {
            get => _product.Price;
            set { _product.Price = value; OnPropertyChanged(); }
        }

        public double? Discount
        {
            get => _product.Discount;
            set { _product.Discount = value; OnPropertyChanged(); }
        }

        public double? Quantity
        {
            get => _product.Quantity;
            set { _product.Quantity = value; OnPropertyChanged(); }
        }

        public string Measurement
        {
            get => _product.Measurement;
            set { _product.Measurement = value; OnPropertyChanged(); }
        }

        public string Photo
        {
            get => _product.Photo;
            set
            {
                _product.Photo = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PhotoPreviewVisibility));
                LoadPhotoPreview();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ProductEditWindow(Tovar product)
        {
            InitializeComponent();
            DataContext = this;

            _isNewProduct = product == null;
            _product = product ?? new Tovar();

            LoadData();
            LoadPhotoPreview();
        }

        private void LoadData()
        {
            try
            {
                var context = ShoeStoreBD.GetContext();

                Types = context.Types.ToList();
                Categories = context.Categories.ToList();
                Manufacturers = context.Manufactureres.ToList();
                Suppliers = context.Supplieres.ToList();

                OnPropertyChanged(nameof(Types));
                OnPropertyChanged(nameof(Categories));
                OnPropertyChanged(nameof(Manufacturers));
                OnPropertyChanged(nameof(Suppliers));

                // Если товар новый, устанавливаем значения по умолчанию
                if (_isNewProduct)
                {
                    if (Types.Any())
                        SelectedTypeId = Types.First().Id_type;
                    if (Categories.Any())
                        SelectedCategoryId = Categories.First().Id_category;
                    if (Manufacturers.Any())
                        SelectedManufacturerId = Manufacturers.First().Id_manufacturer;
                    if (Suppliers.Any())
                        SelectedSupplierId = Suppliers.First().Id_supplier;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadPhotoPreview()
        {
            if (string.IsNullOrWhiteSpace(Photo))
            {
                PhotoPreview = null;
                return;
            }

            try
            {
                string projectRoot = GetProjectRootDirectory();
                string resourcesPath = Path.Combine(projectRoot, "Resources", Photo);

                if (File.Exists(resourcesPath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(resourcesPath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    PhotoPreview = bitmap;
                }
                else
                {
                    PhotoPreview = null;
                }
            }
            catch
            {
                PhotoPreview = null;
            }
        }

        private string GetProjectRootDirectory()
        {
            string currentDir = Directory.GetCurrentDirectory();
            string projectRoot = Path.GetFullPath(Path.Combine(currentDir, "..", ".."));
            return projectRoot;
        }

        private void SelectPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Изображения (*.jpg;*.jpeg;*.png;*.gif;*.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp|Все файлы (*.*)|*.*",
                Title = "Выберите изображение товара",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFile = openFileDialog.FileName;
                CopyPhotoToResources(selectedFile);
            }
        }

        private void CopyPhotoToResources(string sourceFilePath)
        {
            try
            {
                // Получаем имя файла
                string fileName = Path.GetFileName(sourceFilePath);

                // Генерируем уникальное имя файла на основе артикула
                string uniqueFileName = GenerateUniqueFileName(fileName);

                // Получаем путь к папке Resources
                string projectRoot = GetProjectRootDirectory();
                string resourcesPath = Path.Combine(projectRoot, "Resources");

                // Создаем папку Resources, если она не существует
                if (!Directory.Exists(resourcesPath))
                {
                    Directory.CreateDirectory(resourcesPath);
                }

                string destinationPath = Path.Combine(resourcesPath, uniqueFileName);

                // Копируем файл
                File.Copy(sourceFilePath, destinationPath, true);

                // Обновляем свойство Photo (только имя файла)
                Photo = uniqueFileName;

                MessageBox.Show($"Изображение успешно сохранено как: {uniqueFileName}", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка копирования изображения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateUniqueFileName(string originalFileName)
        {
            // Если есть артикул, используем его как основу для имени файла
            string baseName = string.IsNullOrWhiteSpace(Article) ? "product" : Article;

            // Получаем расширение файла
            string extension = Path.GetExtension(originalFileName);

            // Убираем недопустимые символы из имени файла
            string safeBaseName = new string(baseName.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());

            // Генерируем имя файла
            string fileName = $"{safeBaseName}{extension}";

            // Проверяем, существует ли файл с таким именем
            string projectRoot = GetProjectRootDirectory();
            string resourcesPath = Path.Combine(projectRoot, "Resources");
            string fullPath = Path.Combine(resourcesPath, fileName);

            // Если файл уже существует, добавляем цифру в конец
            if (File.Exists(fullPath))
            {
                int counter = 1;
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);

                while (File.Exists(fullPath))
                {
                    fileName = $"{fileNameWithoutExt}_{counter}{extension}";
                    fullPath = Path.Combine(resourcesPath, fileName);
                    counter++;
                }
            }

            return fileName;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Article))
            {
                MessageBox.Show("Введите артикул товара!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var context = ShoeStoreBD.GetContext();

                if (_isNewProduct)
                {
                    // Добавляем новый товар
                    context.Tovar.Add(_product);
                }
                else
                {
                    // Обновляем существующий товар
                    context.Entry(_product).State = System.Data.Entity.EntityState.Modified;
                }

                context.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите удалить этот товар?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Удаляем фото из папки Resources, если оно существует
                    if (!string.IsNullOrWhiteSpace(Photo))
                    {
                        string projectRoot = GetProjectRootDirectory();
                        string photoPath = Path.Combine(projectRoot, "Resources", Photo);

                        if (File.Exists(photoPath))
                        {
                            File.Delete(photoPath);
                        }
                    }

                    var context = ShoeStoreBD.GetContext();
                    context.Tovar.Attach(_product);
                    context.Tovar.Remove(_product);
                    context.SaveChanges();

                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}