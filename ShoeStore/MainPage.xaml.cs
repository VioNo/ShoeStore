using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ShoeStore
{
    /// <summary>
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new ShoeStoreBD())
                {
                    var user = db.User.AsNoTracking()
                        .FirstOrDefault(u => u.Login == login && u.Password == password);

                    if (user == null)
                    {
                        MessageBox.Show("Неверный логин или пароль", "Ошибка авторизации",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Получаем полное имя пользователя
                    string userFullName = user.Fio;

                    switch (user.Id_role)
                    {
                        case 1: // Администратор
                            NavigationService.Navigate(new AdminPage());
                            break;
                        case 2: // Менеджер
                            NavigationService.Navigate(new ManagPage());
                            break;
                        case 3: // Авторизованный клиент
                            // Передаем данные пользователя на страницу AvtoClient
                            NavigationService.Navigate(new AvtoClient(user));
                            break;
                        default:
                            MessageBox.Show("Неизвестная роль пользователя", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к базе данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GuestButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new GuestPage());
        }
    }
}