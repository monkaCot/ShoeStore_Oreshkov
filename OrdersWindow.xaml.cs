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
using System.Windows.Shapes;
using System.Data.Entity;

namespace ShoeStore_Oreshkov
{
    public partial class OrdersWindow : Window
    {
        private int _role;
        private User _user;

        public OrdersWindow(int role, User user)
        {
            InitializeComponent();
            _role = role;
            _user = user;

            if (role == 1)
            {
                AddOrderButton.Visibility = Visibility.Visible;
                DeleteOrderButton.Visibility = Visibility.Visible;
            }

            LoadOrders();
        }

        private void LoadOrders()
        {
            var db = new ShoeStore_OreshkovEntities();
            OrdersList.ItemsSource = db.Order
                .Include(o => o.Order_Status)
                .Include(o => o.Pickup_Points)
                .AsNoTracking()
                .ToList();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainMenu = new MainMenuWindow(_role, _user);
            mainMenu.Show();
            this.Close();
        }

        private void OrderItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_role != 1)
            {
                MessageBox.Show("У вас нет прав для редактирования заказов.","Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!(sender is ListViewItem item)) return;
            if (!(item.DataContext is Order order)) return;

            var window = new AddEditOrderWindow(order.ID_Order_Number, _user);
            if (window.ShowDialog() == true)
                LoadOrders();
        }

        private void AddOrderButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditOrderWindow(0, _user);
            if (window.ShowDialog() == true)
                LoadOrders();
        }

        private void DeleteOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(OrdersList.SelectedItem is Order selected))
            {
                MessageBox.Show("Выберите заказ в списке.","Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Удалить заказ №{selected.ID_Order_Number}?","Подтверждение", MessageBoxButton.YesNo,MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                var db = new ShoeStore_OreshkovEntities();

                var details = db.Order_Details
                    .Where(d => d.ID_Order_Number == selected.ID_Order_Number)
                    .ToList();
                foreach (var detail in details)
                {
                    db.Order_Details.Remove(detail);
                }    

                var orderToDelete = db.Order
                    .FirstOrDefault(o => o.ID_Order_Number == selected.ID_Order_Number);

                if (orderToDelete != null)
                {
                    db.Order.Remove(orderToDelete);
                }

                db.SaveChanges();
                LoadOrders();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось удалить заказ.\n" + ex.Message,"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
