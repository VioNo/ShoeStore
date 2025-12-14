using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Data.Entity;

namespace ShoeStore
{
    public partial class OrderEditWindow : Window, INotifyPropertyChanged
    {
        private Order _order;
        private bool _isNewOrder;
        private Addresses _newAddress;

        // Списки для ComboBox
        public List<Tovar> Products { get; set; }
        public List<Addresses> Addresses { get; set; }
        public List<User> Users { get; set; }
        public List<Statuses> Statuses { get; set; }

        // Свойства для привязки
        public string WindowTitle => _isNewOrder ? "Добавление заказа" : "Редактирование заказа";

        public bool IsNewOrder => _isNewOrder;

        public Visibility DeleteButtonVisibility => _isNewOrder ? Visibility.Collapsed : Visibility.Visible;

        public Visibility NewAddressVisibility => _isNewOrder ? Visibility.Visible : Visibility.Collapsed;

        public int? OrderNumber
        {
            get => _order.Order_number;
            set { _order.Order_number = value; OnPropertyChanged(); }
        }

        public string Article
        {
            get => _order.Article;
            set { _order.Article = value; OnPropertyChanged(); }
        }

        public string SelectedProductArticle
        {
            get => _order.Article;
            set
            {
                _order.Article = value;
                OnPropertyChanged();
                // Автоматически устанавливаем связанный товар
                if (!string.IsNullOrEmpty(value))
                {
                    var product = Products.FirstOrDefault(p => p.Article == value);
                    if (product != null)
                    {
                        _order.Tovar = product;
                    }
                }
            }
        }

        public int? Quantity
        {
            get => _order.Quantity;
            set { _order.Quantity = value; OnPropertyChanged(); }
        }

        public DateTime? OrderDate
        {
            get => _order.Order_date;
            set { _order.Order_date = value; OnPropertyChanged(); }
        }

        public DateTime? DeliveryDate
        {
            get => _order.Delivery_date;
            set { _order.Delivery_date = value; OnPropertyChanged(); }
        }

        public int? SelectedAddressId
        {
            get => _order.Id_address;
            set { _order.Id_address = value; OnPropertyChanged(); }
        }

        public int? SelectedUserId
        {
            get => _order.Id_user;
            set { _order.Id_user = value; OnPropertyChanged(); }
        }

        public int? Code
        {
            get => _order.Code;
            set { _order.Code = value; OnPropertyChanged(); }
        }

        public int? SelectedStatusId
        {
            get => _order.Id_status;
            set { _order.Id_status = value; OnPropertyChanged(); }
        }

        public string NewAddress
        {
            get => _newAddress?.Address ?? "";
            set
            {
                if (_newAddress == null)
                    _newAddress = new Addresses();
                _newAddress.Address = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public OrderEditWindow(Order order)
        {
            InitializeComponent();
            DataContext = this;

            _isNewOrder = order == null;
            _order = order ?? new Order();
            _newAddress = new Addresses();

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var context = ShoeStoreBD.GetContext();

                // Загружаем данные для ComboBox
                Products = context.Tovar
                    .Include(p => p.Types)
                    .ToList();

                Addresses = context.Addresses.ToList();
                Users = context.User.ToList();
                Statuses = context.Statuses.ToList();

                OnPropertyChanged(nameof(Products));
                OnPropertyChanged(nameof(Addresses));
                OnPropertyChanged(nameof(Users));
                OnPropertyChanged(nameof(Statuses));

                // Если заказ новый, устанавливаем значения по умолчанию
                if (_isNewOrder)
                {
                    OrderDate = DateTime.Now;
                    DeliveryDate = DateTime.Now.AddDays(7);

                    if (Statuses.Any())
                        SelectedStatusId = Statuses.First(s => s.Status == "Новый").Id_status;
                    if (Users.Any())
                        SelectedUserId = Users.First().Id_user;
                    if (Products.Any())
                        SelectedProductArticle = Products.First().Article;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Article))
            {
                MessageBox.Show("Введите артикул заказа!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!OrderDate.HasValue)
            {
                MessageBox.Show("Выберите дату заказа!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var context = ShoeStoreBD.GetContext();

                // Если введен новый адрес, сохраняем его
                if (_isNewOrder && !string.IsNullOrWhiteSpace(NewAddress))
                {
                    context.Addresses.Add(_newAddress);
                    context.SaveChanges();
                    SelectedAddressId = _newAddress.Id_address;
                }

                if (_isNewOrder)
                {
                    // Добавляем новый заказ
                    context.Order.Add(_order);
                }
                else
                {
                    // Обновляем существующий заказ
                    context.Entry(_order).State = System.Data.Entity.EntityState.Modified;
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
            var result = MessageBox.Show("Вы уверены, что хотите удалить этот заказ?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var context = ShoeStoreBD.GetContext();
                    context.Order.Attach(_order);
                    context.Order.Remove(_order);
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