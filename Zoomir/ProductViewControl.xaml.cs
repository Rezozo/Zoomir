using Npgsql;
using System;
using System.Windows;
using System.Windows.Controls;
using Zoomir.models;

namespace Zoomir
{
    public partial class ProductViewControl : UserControl
    {
        private NpgsqlConnection connection = new NpgsqlConnection("Server=localhost;Port=5432;Database=Zoomir;User Id=postgres;Password=0987");
        public static readonly DependencyProperty ProductProperty =
            DependencyProperty.Register("Product", typeof(ProductViewModel), typeof(ProductViewControl), new PropertyMetadata(null));

        public ProductViewControl()
        {
            InitializeComponent();
            Loaded += ProductViewControl_Loaded;
        }

        public ProductViewModel Product
        {
            get { return (ProductViewModel)GetValue(ProductProperty); }
            set { SetValue(ProductProperty, value); }
        }

        private void ProductViewControl_Loaded(object sender, RoutedEventArgs e)
        {
            ContextMenu contextMenu = new ContextMenu();
            ProductListWindow productListWindow = Window.GetWindow(this) as ProductListWindow;

            if (productListWindow.role != "Администратор")
            {
                MenuItem addToOrderMenuItem = new MenuItem();
                addToOrderMenuItem.Header = "Добавить к заказу";
                addToOrderMenuItem.Click += AddToOrderMenuItem_Click;
                contextMenu.Items.Add(addToOrderMenuItem);
            }

            if (productListWindow.role == "Администратор")
            {
                MenuItem addToOrderMenuItem = new MenuItem();
                addToOrderMenuItem.Header = "Редактировать";
                addToOrderMenuItem.Click += UpdateMenuItem_Click;
                contextMenu.Items.Add(addToOrderMenuItem);

                MenuItem deleteMenuItem = new MenuItem();
                deleteMenuItem.Header = "Удалить";
                deleteMenuItem.Click += DeleteMenuItem_Click;
                contextMenu.Items.Add(deleteMenuItem);
            }

            this.ContextMenu = contextMenu;
        }

        private void AddToOrderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem contextMenu = (sender as MenuItem);

            ProductViewModel product = (contextMenu.DataContext as ProductViewModel);

            if (product != null)
            {
                ProductListWindow productListWindow = Window.GetWindow(this) as ProductListWindow;

                if (productListWindow != null)
                {
                    if (productListWindow.OrderStatus != "Новый" && productListWindow.OrderStatus != "Не создан")
                    {
                        MessageBox.Show("Невозможно обновить сформированный заказ");
                        return;
                    }

                    productListWindow.showOrder.Visibility = Visibility.Visible;

                    if (productListWindow.OrderId == 0)
                    {
                        connection.Open();
                        var insertDealCommand = new NpgsqlCommand($"INSERT INTO deal VALUES (DEFAULT, @date, null, null, 1) RETURNING id", connection);
                        insertDealCommand.Parameters.AddWithValue("date", DateTime.Now);
                        int id = (int)insertDealCommand.ExecuteScalar();
                        productListWindow.OrderId = id;
                        productListWindow.OrderStatus = "Новый";
                        connection.Close();
                    }

                    connection.Open();
                    var getCountCommand = new NpgsqlCommand($"select count from deal_product where deal_id = {productListWindow.OrderId} AND product_id = {product.Id}", connection);
                    object count = getCountCommand.ExecuteScalar();
                    connection.Close();

                    if (count != null)
                    {
                        connection.Open();
                        var updateDealProductCommand = new NpgsqlCommand($"UPDATE deal_product SET count = @count WHERE deal_id = {productListWindow.OrderId} AND product_id = {product.Id}", connection);
                        updateDealProductCommand.Parameters.AddWithValue("count", (int)count + 1);
                        updateDealProductCommand.ExecuteNonQuery();
                        connection.Close();
                    } else
                    {
                        connection.Open();
                        var insertDealProductCommand = new NpgsqlCommand($"INSERT INTO deal_product VALUES (DEFAULT, {productListWindow.OrderId}, {product.Id}, 1)", connection);
                        insertDealProductCommand.ExecuteNonQuery();
                        connection.Close();
                    }
                }
            }
        }

        private void UpdateMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menu = (sender as MenuItem);
            ProductViewModel product = (menu.DataContext as ProductViewModel);

            ProductListWindow productListWindow = Window.GetWindow(this) as ProductListWindow;
            productListWindow.Hide();

            ProductWindow productWindow = new ProductWindow();
            productWindow.DataContext = product;
            productWindow.UserRole = productListWindow.UserRole;
            productWindow.Closed += (s, args) => productListWindow.Close();
            productWindow.Show();
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ProductViewModel product = menuItem.DataContext as ProductViewModel;

            if (product != null)
            {
                MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить этот продукт?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    ProductListWindow productListWindow = Window.GetWindow(this) as ProductListWindow;

                    if (productListWindow != null)
                    {
                        productListWindow.Products.Remove(product);

                        connection.Open();
                        var command = new NpgsqlCommand($"DELETE FROM product WHERE id = {product.Id}", connection);
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                }
            }
        }
    }
}
