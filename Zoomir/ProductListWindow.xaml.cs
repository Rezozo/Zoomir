using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Zoomir.models;

namespace Zoomir
{
    /// <summary>
    /// Логика взаимодействия для ProductListWindow.xaml
    /// </summary>
    public partial class ProductListWindow : Window
    {
        private NpgsqlConnection connection = new NpgsqlConnection("Server=localhost;Port=5432;Database=Zoomir;User Id=postgres;Password=0987");
        public ObservableCollection<ProductViewModel> Products { get; set; } = new ObservableCollection<ProductViewModel>();    
        public string role;
        public int OrderId { get; set; } = 0;
        public string OrderStatus { get; set; } = "Не создан";

        public string UserRole
        {
            get { return role; }
            set { role = value; }
        }

        public ProductListWindow()
        {
            InitializeComponent();
            LoadProducts();
            DataContext = this;
            this.Loaded += ProductListWindow_Loaded;
        }

        private void ProductListWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (role == "Администратор" || role == "Менеджер")
            {
                showAllOrders.Visibility = Visibility.Visible;
            }
            if (role == "Администратор")
            {
                createProduct.Visibility = Visibility.Visible;
            }
            if (OrderId != 0 && role != "Администратор")
            {
                showOrder.Visibility = Visibility.Visible;
            }
        }

        private void LoadProducts()
        {
            connection.Open();
            var command = new NpgsqlCommand("SELECT * FROM product", connection);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                ProductViewModel productViewModel = new ProductViewModel();
                productViewModel.Id = (int)reader["id"];
                productViewModel.Image = ((byte[])reader["image"]);
                productViewModel.Title = (string)reader["title"];
                productViewModel.Description = (string)reader["description"];
                productViewModel.Manufacturer = (string)reader["manufacturer"];
                productViewModel.Cost = (decimal)reader["cost"];
                if (reader["discount"] != DBNull.Value)
                {
                    productViewModel.Discount = (int)reader["discount"];
                }
                Products.Add(productViewModel);
            }
            reader.Close();
            connection.Close();
        }

        private void showOrder_Click(object sender, RoutedEventArgs e)
        {
            UserOrder userOrder = new UserOrder();
            userOrder.OrderId = OrderId;
            userOrder.OrderStatus = OrderStatus;
            userOrder.UserRole = role;
            userOrder.Closed += (s, args) => Close();
            userOrder.Show();
            Hide();
        }

        private void showAllOrders_Click(object sender, RoutedEventArgs e)
        {
            OrderListWindow orderListWindow = new OrderListWindow();
            orderListWindow.UserRole = role;
            orderListWindow.Closed += (s, args) => Close();
            orderListWindow.Show();
            Hide();
        }

        private void createProduct_Click(object sender, RoutedEventArgs e)
        {
            ProductWindow productWindow = new ProductWindow();
            productWindow.UserRole = UserRole;
            productWindow.Closed += (s, args) => Close();
            productWindow.Show();
            Hide();
        }
    }
}
