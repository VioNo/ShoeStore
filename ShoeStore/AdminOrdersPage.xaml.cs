using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace ShoeStore
{
    public partial class AdminOrdersPage : Page
    {
        private List<Order> _orders = new List<Order>();
        private User _currentUser;

        public AdminOrdersPage(User user)
        {
            InitializeComponent();
            _currentUser = user;
            LoadUserInfo();
            LoadData();

            // Добавляем обработчик двойного клика для редактирования заказа
            ListBoxOrders.AddHandler(MouseUpEvent, new MouseButtonEventHandler(OrderItem_MouseUp), true);
            this.KeyDown += AdminOrdersPage_KeyDown;
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
                    .OrderByDescending(o => o.Order_date)
                    .ToList();

                // Отображаем заказы
                ListBoxOrders.ItemsSource = _orders;

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

        // Контекстное меню - Редактировать
        private void EditOrderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is Order selectedOrder)
            {
                EditOrder(selectedOrder);
            }
        }

        // Контекстное меню - Удалить
        private void DeleteOrderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is Order selectedOrder)
            {
                DeleteOrder(selectedOrder);
            }
        }

        // Редактирование заказа по двойному клику
        private void OrderItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                // Ищем родительский Border
                var source = e.OriginalSource as DependencyObject;
                while (source != null && !(source is Border))
                {
                    source = VisualTreeHelper.GetParent(source);
                }

                if (source is Border border && border.DataContext is Order selectedOrder)
                {
                    EditOrder(selectedOrder);
                }
            }
        }

        // Метод редактирования заказа
        private void EditOrder(Order order)
        {
            var editWindow = new OrderEditWindow(order);
            editWindow.ShowDialog();
            if (editWindow.DialogResult == true)
            {
                LoadData();
                MessageBox.Show("Заказ успешно обновлен!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Метод удаления заказа
        private void DeleteOrder(Order order)
        {
            var result = MessageBox.Show($"Вы уверены, что хотите удалить заказ '{order.Article}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var context = ShoeStoreBD.GetContext();
                    var orderToDelete = context.Order.FirstOrDefault(o => o.Id_order == order.Id_order);

                    if (orderToDelete != null)
                    {
                        context.Order.Remove(orderToDelete);
                        context.SaveChanges();

                        LoadData();
                        MessageBox.Show("Заказ успешно удален!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Обработчик горячих клавиш
        private void AdminOrdersPage_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.A: // Ctrl+A - добавить заказ
                        AddEditButton_Click(null, null);
                        e.Handled = true;
                        break;

                    case Key.R: // Ctrl+R - обновить
                        LoadData();
                        MessageBox.Show("Данные заказов успешно обновлены!", "Обновление",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        e.Handled = true;
                        break;
                }
            }

            if (e.Key == Key.Delete)
            {
                // Удаление заказа по клавише Delete
                if (ListBoxOrders.SelectedItem is Order selectedOrder)
                {
                    DeleteOrder(selectedOrder);
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
            NavigationService.GoBack();
        }

        private void AddEditButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new OrderEditWindow(null);
            editWindow.ShowDialog();
            if (editWindow.DialogResult == true)
            {
                LoadData();
                MessageBox.Show("Заказ успешно добавлен!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    // Конвертер для цвета статуса (если еще нет в проекте)
    public class AdminStatusColorConverter : IValueConverter
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