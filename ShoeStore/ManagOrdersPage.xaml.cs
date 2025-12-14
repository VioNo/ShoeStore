using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace ShoeStore
{
    public partial class ManagOrdersPage : Page
    {
        private List<Order> _orders = new List<Order>();
        private User _currentUser;

        public ManagOrdersPage(User user)
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

                // Загружаем все заказы с связанными данными
                _orders = context.Order
                    .Include(o => o.Tovar)
                    .Include(o => o.Tovar.Types)
                    .Include(o => o.Addresses)
                    .Include(o => o.Statuses)
                    .Include(o => o.User)
                    .OrderByDescending(o => o.Order_date) // Простая сортировка по дате заказа
                    .ToList();

                // Отображаем заказы
                ItemsControlOrders.ItemsSource = _orders;

                // Обновляем статистику
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStatistics()
        {
            if (_orders == null) return;

            int totalOrders = _orders.Count;
            int newOrders = _orders.Count(o => o.Statuses?.Status == "Новый");
            int deliveredOrders = _orders.Count(o => o.Statuses?.Status == "Доставлен");

            txtStatistics.Text = $"Всего заказов: {totalOrders} | Новых: {newOrders} | Доставлено: {deliveredOrders}";
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
            NavigationService.GoBack();
        }
    }

    // Конвертер для цвета статуса
    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string status)
            {
                switch (status)
                {
                    case "Новый":
                        return new SolidColorBrush(Colors.Blue);
                    case "В обработке":
                        return new SolidColorBrush(Colors.Orange);
                    case "Отправлен":
                        return new SolidColorBrush(Colors.Purple);
                    case "Доставлен":
                        return new SolidColorBrush(Colors.Green);
                    case "Отменен":
                        return new SolidColorBrush(Colors.Red);
                    default:
                        return new SolidColorBrush(Colors.Gray);
                }
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}