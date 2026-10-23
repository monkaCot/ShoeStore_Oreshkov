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
    public partial class AddEditOrderWindow : Window
    {
        private ShoeStore_OreshkovEntities _db;
        private Order _order;
        private User _user;

        public AddEditOrderWindow(int orderId, User user)
        {
            InitializeComponent();

            _db = new ShoeStore_OreshkovEntities();
            _user = user;

            StatusCombobox.ItemsSource = _db.Order_Status.ToList();
            PickupPointCombobox.ItemsSource = _db.Pickup_Points.ToList();

            if (orderId == 0)
            {
                _order = new Order();
                int nextOrderNumber = 1;
                if (_db.Order.Any())
                {
                    nextOrderNumber = _db.Order.Max(o => o.ID_Order_Number) + 1;
                }
                OrderNumberTextbox.Text = nextOrderNumber.ToString();
                OrderDatePicker.SelectedDate = DateTime.Now;
                return;
            }

            _order = _db.Order.FirstOrDefault(o => o.ID_Order_Number == orderId);
            if (_order == null)
            {
                MessageBox.Show("Заказ не найден.", "Ошибка",MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            OrderNumberTextbox.Text = _order.ID_Order_Number.ToString();
            StatusCombobox.SelectedValue = _order.ID_Order_Status;
            PickupPointCombobox.SelectedValue = _order.ID_Pickup_Point;
            OrderDatePicker.SelectedDate = _order.OrderDate;
            DeliveryDatePicker.SelectedDate = _order.DeliveryDate;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (StatusCombobox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите статус заказа.", "Ошибка",MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (PickupPointCombobox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите пункт выдачи.", "Ошибка",MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (OrderDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Укажите дату заказа.", "Ошибка",MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (DeliveryDatePicker.SelectedDate.HasValue &&
                    OrderDatePicker.SelectedDate.HasValue &&
                    DeliveryDatePicker.SelectedDate.Value < OrderDatePicker.SelectedDate.Value)
                {
                    MessageBox.Show("Дата выдачи не может быть раньше даты заказа.","Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _order.ID_Order_Status = (int)StatusCombobox.SelectedValue;
                _order.ID_Pickup_Point = (int)PickupPointCombobox.SelectedValue;
                _order.OrderDate = OrderDatePicker.SelectedDate;
                _order.DeliveryDate = DeliveryDatePicker.SelectedDate;

                if (_order.ID_User == null)
                {
                    _order.ID_User = _user.ID_User;
                }

                if (_order.ID_Order_Number == 0)
                {
                    _db.Order.Add(_order);
                }

                _db.SaveChanges();

                MessageBox.Show("Заказ успешно сохранён.", "Успех",MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}","Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
