using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;

namespace ShoeStore
{
    public partial class AvtoClient : Page
    {
        private List<Tovar> _allTovars = new List<Tovar>();
        private User _currentUser;

        // Конструктор с параметром для передачи данных пользователя
        public AvtoClient(User user)
        {
            InitializeComponent();
            _currentUser = user;
            LoadUserInfo();
            LoadTovar();
        }

        // Метод для загрузки информации о пользователе
        private void LoadUserInfo()
        {
            //if (_currentUser != null)
            //{
            //    UserInfoTextBlock.Text = _currentUser.Fio;
            //}
        }

        private void LoadTovar()
        {
            //try
            //{
            //    var context = ShoeStoreBD.GetContext();

            //    // Загружаем данные с включением всех связанных сущностей
            //    _allTovars = context.Tovar
            //        .Include(t => t.Types)
            //        .Include(t => t.Categories)
            //        .Include(t => t.Manufactureres)
            //        .Include(t => t.Supplieres)
            //        .ToList();

            //    ItemsControlTovar.ItemsSource = _allTovars;
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
            //                   "Ошибка",
            //                   MessageBoxButton.OK,
            //                   MessageBoxImage.Error);
            //}
        }

        // Кнопка выхода
        //private void LogoutButton_Click(object sender, RoutedEventArgs e)
        //{
        //    NavigationService.Navigate(new MainPage());
        //}

        //// Кнопка возврата на главную
        //private void BackButton_Click(object sender, RoutedEventArgs e)
        //{
        //    NavigationService.Navigate(new MainPage());
        //}
    }
}