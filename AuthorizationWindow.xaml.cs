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

namespace ShoeStore_Oreshkov
{
    /// <summary>
    /// Логика взаимодействия для AuthorizationWindow.xaml
    /// </summary>
    public partial class AuthorizationWindow : Window
    {
        private int role = 0;
        private User user;

        public AuthorizationWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordTextBox.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста, введите логин и пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var db = ShoeStore_OreshkovEntities.GetContext();
            var user = db.User.FirstOrDefault(u => u.Login == login && u.Password == password);
            role = (int)user.ID_Role;

            if (user != null)
            {
                var mainMenuWindow = new MainMenuWindow(role, user);
                mainMenuWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GuestLoginButton_Click(object sender, RoutedEventArgs e)
        {
            var mainMenuWindow = new MainMenuWindow(role, user);
            mainMenuWindow.Show();
            this.Close();
        }
    }
}
