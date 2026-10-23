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

namespace ShoeStore_Oreshkov
{
    /// <summary>
    /// Логика взаимодействия для MainMenuWindow.xaml
    /// </summary>
    public partial class MainMenuWindow : Window
    {
        private bool SortAsc;
        private Product product;
        private Product_Name ProductName;
        private int _role;
        private User _user;
        public MainMenuWindow(int role, User user)
        {
            InitializeComponent();

            var db = ShoeStore_OreshkovEntities.GetContext();

            TovarList.ItemsSource = db.Product.ToList();

            var suppliers = db.Supplier.ToList();

            suppliers.Insert(0, new Supplier { ID_Supplier = 0, SupplierName = "Все поставщики" });

            FilterComboBox.ItemsSource = suppliers;
            FilterComboBox.SelectedIndex = 0;

            _user = user;

            switch (role)
            {
                case 1:
                    Search.Visibility = Visibility.Visible;
                    GroupBoxSort.Visibility = Visibility.Visible;
                    PostSort.Visibility = Visibility.Visible;
                    ResetButton.Visibility = Visibility.Visible;
                    AddTovarButton.Visibility = Visibility.Visible;
                    DeleteTovarButton.Visibility = Visibility.Visible;
                    OrdersButton.Visibility = Visibility.Visible;
                    FIO.Text = user.FIO;
                    _role = role;
                    break;
                case 2:
                    Search.Visibility = Visibility.Visible;
                    GroupBoxSort.Visibility = Visibility.Visible;
                    PostSort.Visibility = Visibility.Visible;
                    ResetButton.Visibility = Visibility.Visible;
                    OrdersButton.Visibility = Visibility.Visible;
                    FIO.Text = user.FIO;
                    _role = role;
                    break;
                case 3:
                    //Search.Visibility = Visibility.Visible;
                    FIO.Text = user.FIO;
                    _role = role;
                    break;
                default:
                    FIO.Text = "Гость";
                    break;
            }

            Filter();
        }

        private void Filter()
        {
            var db = ShoeStore_OreshkovEntities.GetContext();

            var searchList = db.Product.ToList();

            if (FilterComboBox.SelectedItem is Supplier selectedSupplier)
            {
                if (selectedSupplier.ID_Supplier != 0)
                {
                    searchList = searchList.Where(p => p.ID_Supplier == selectedSupplier.ID_Supplier).ToList();
                }
                else
                {
                    searchList = db.Product.ToList();
                }
            }
            if (SearchTextBox.Text.Length > 0)
            {
                string search = SearchTextBox.Text.ToLower();
                searchList = searchList
                    .Where(x =>
                        (x.Product_Name != null && x.Product_Name.TovarName.ToLower().Contains(search)) ||
                        (x.Supplier != null && x.Supplier.SupplierName.ToLower().Contains(search)) ||
                        (x.Description != null && x.Description.ToLower().Contains(search))
                    )
                    .ToList();
            }
            if (SortAsc == true)
            {
                searchList = searchList.OrderBy(p => p.InStock).ToList();
            }
            else if (SortAsc == false)
            {
                searchList = searchList.OrderByDescending(p => p.InStock).ToList();
            }


            TovarList.ItemsSource = searchList;
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var AuthorizationWindow = new AuthorizationWindow();
            AuthorizationWindow.Show();
            this.Close();
        }

        private void OrdersButton_Click(object sender, RoutedEventArgs e)
        {
            var ordersWindow = new OrdersWindow(_role, _user);
            ordersWindow.Show();
            this.Close();
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Filter();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            FilterComboBox.SelectedItem = 0;
            SearchTextBox.Text = string.Empty;
            SortAsc = false;
            TovarList.ItemsSource = ShoeStore_OreshkovEntities.GetContext().Product.ToList();
        }

        private void SearchTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            Filter();
        }

        private void SortAscButton_Click(object sender, RoutedEventArgs e)
        {
            SortAsc = true;
            Filter();
        }

        private void SortDownButton_Click(object sender, RoutedEventArgs e)
        {
            SortAsc = false;
            Filter();
        }

        private void TovarList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_role == 1)
            {
                var prodName = ShoeStore_OreshkovEntities.GetContext().Product_Name;
                product = TovarList.SelectedItem as Product;


                AddEditTovarWindow EditTovarWindow = new AddEditTovarWindow(product);
                EditTovarWindow.ShowDialog();

                Filter();
            }
            else
            {
                MessageBox.Show("У вас нет прав.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AddTovarButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditTovarWindow(null);
            window.ShowDialog();
            Filter();
        }

        private void DeleteTovarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(TovarList.SelectedItem is Product selected))
            {
                MessageBox.Show("Выберите товар в списке.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var db = ShoeStore_OreshkovEntities.GetContext();

            bool usedInOrders = db.Order_Details.Any(o => o.ID_Article == selected.ID);
            if (usedInOrders)
            {
                MessageBox.Show("Нельзя удалить товар: он используется в заказах.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Удалить выбранный товар?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                db.Product.Remove(selected);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось удалить товар.\n" + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Filter();
        }
    }
}
