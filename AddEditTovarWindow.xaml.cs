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
using System.IO;
using Microsoft.Win32;

namespace ShoeStore_Oreshkov
{
    /// <summary>
    /// Логика взаимодействия для AddEditTovarWindow.xaml
    /// </summary>
    public partial class AddEditTovarWindow : Window
    {
        private Product _Product;
        private string _photoPath;
        public AddEditTovarWindow(Product product)
        {
            InitializeComponent();

            var nullImage = new BitmapImage(new Uri("pack://application:,,,/Pictures/picture.png"));
            var db = ShoeStore_OreshkovEntities.GetContext();

            CategoryCombobox.ItemsSource = db.Category.ToList();
            ProviderCombobox.ItemsSource = db.Provider.ToList();
            SupplierCombobox.ItemsSource = db.Supplier.ToList();

            if (product == null)
            {
                _Product = new Product();
                _photoPath = null;
                TovarImage.Source = nullImage;
                return;
            }
            _Product = product;

            _photoPath = product.Photo;

            TovarNameTextbox.Text = product.Product_Name.TovarName.ToString();

            CategoryCombobox.SelectedIndex = product.Category.ID_Category - 1;

            DescriptionTextbox.Text = product.Description.ToString();

            ProviderCombobox.SelectedIndex = product.Provider.ID_ProviderName - 1;

            SupplierCombobox.SelectedIndex = product.Supplier.ID_Supplier - 1;

            PriceTextbox.Text = product.price.ToString();

            MeasureTextbox.Text = product.MeasureUnit.ToString();

            InStockNumberTextbox.Text = product.InStock.ToString();

            SaleTextbox.Text = product.Sale.ToString();
        }

        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string targetDirectory = @"C:\Users\oresh\Downloads\testdisk\testdisk-7.1\ShoeStore_Oreshkov\Pictures\";

                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png";
                if (dialog.ShowDialog() == true)
                {
                    string sourceFilePath = dialog.FileName;
                    _photoPath = dialog.SafeFileName;
                    SelectedImageName.Text = _photoPath;

                    string newPath = System.IO.Path.Combine(targetDirectory, _photoPath);

                    if (!string.Equals(System.IO.Path.GetFullPath(sourceFilePath),
                                       System.IO.Path.GetFullPath(newPath),
                                       StringComparison.OrdinalIgnoreCase))
                    {
                        File.Copy(sourceFilePath, newPath, true);
                    }

                    _photoPath = System.IO.Path.Combine("Pictures/", dialog.SafeFileName);
                    _Product.Photo = _photoPath;
                    SelectedImageName.Text = _photoPath;

                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(newPath, UriKind.Absolute);
                    bitmap.EndInit();
                    TovarImage.Source = bitmap;

                    ShoeStore_OreshkovEntities.GetContext().SaveChanges();

                    MessageBox.Show("Изображение успешно выбрано и сохранено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выборе изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var db = ShoeStore_OreshkovEntities.GetContext();

                if (string.IsNullOrWhiteSpace(TovarNameTextbox.Text) ||
                    CategoryCombobox.SelectedItem == null ||
                    ProviderCombobox.SelectedItem == null ||
                    SupplierCombobox.SelectedItem == null ||
                    string.IsNullOrWhiteSpace(PriceTextbox.Text) ||
                    string.IsNullOrWhiteSpace(MeasureTextbox.Text) ||
                    string.IsNullOrWhiteSpace(InStockNumberTextbox.Text) ||
                    string.IsNullOrWhiteSpace(SaleTextbox.Text) ||
                    string.IsNullOrWhiteSpace(DescriptionTextbox.Text))
                {
                    MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                //Цена
                string priceText = PriceTextbox.Text.Trim().Replace(',', '.');
                if (!decimal.TryParse(priceText, System.Globalization.NumberStyles.AllowDecimalPoint,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal price))
                {
                    MessageBox.Show("Цена должна быть числом (например, 1999.90).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (price < 0)
                {
                    MessageBox.Show("Цена не может быть отрицательной.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (price != Math.Round(price, 2))
                {
                    MessageBox.Show("В цене допускается не более двух знаков после запятой.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                //Количество на складе
                if (!int.TryParse(InStockNumberTextbox.Text.Trim(), out int inStock))
                {
                    MessageBox.Show("Количество на складе должно быть целым числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (inStock < 0)
                {
                    MessageBox.Show("Количество на складе не может быть отрицательным.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                //Название товара
                var productName = db.Product_Name.FirstOrDefault(pn => pn.TovarName == TovarNameTextbox.Text);
                if (productName == null)
                {
                    productName = new Product_Name { TovarName = TovarNameTextbox.Text };
                    db.Product_Name.Add(productName);
                    db.SaveChanges();
                }
                _Product.ID_ProductName = productName.ID_TovarName;

                //Категория
                var selectedCategory = CategoryCombobox.SelectedItem as Category;
                _Product.ID_Category = selectedCategory?.ID_Category;

                //Описание
                _Product.Description = DescriptionTextbox.Text;

                //Поставщик
                var selectedSupplier = SupplierCombobox.SelectedItem as Supplier;
                _Product.ID_Supplier = selectedSupplier?.ID_Supplier;

                //Производитель
                var selectedProvider = ProviderCombobox.SelectedItem as Provider;
                _Product.ID_Provider = selectedProvider?.ID_ProviderName;

                //Цена
                _Product.price = (int)price;

                //Единица измерения
                _Product.MeasureUnit = MeasureTextbox.Text;

                //В наличии
                _Product.InStock = inStock;

                //Скидка
                if (int.TryParse(SaleTextbox.Text, out int sale))
                {
                    _Product.Sale = sale;
                }
                else
                {
                    _Product.Sale = null;
                }

                //Фото
                _Product.Photo = _photoPath;

                if (_Product.ID == 0)
                    db.Product.Add(_Product);

                db.SaveChanges();

                MessageBox.Show("Товар успешно сохранён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
