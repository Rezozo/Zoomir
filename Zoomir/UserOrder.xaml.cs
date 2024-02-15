using Npgsql;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Zoomir.file;
using Zoomir.models;

namespace Zoomir
{
    /// <summary>
    /// Логика взаимодействия для UserOrder.xaml
    /// </summary>
    public partial class UserOrder : Window
    {
        private TicketCreater ticketCreater;
        private NpgsqlConnection connection = new NpgsqlConnection("Server=localhost;Port=5432;Database=Zoomir;User Id=postgres;Password=0987");
        public ObservableCollection<ProductCountViewModel> Products { get; set; } = new ObservableCollection<ProductCountViewModel>();
        public ObservableCollection<string> ListItems { get; set; } = new ObservableCollection<string>();
        public string SelectedListItem { get; set; } = "";
        public decimal TotalCost { get; set; } = 0m;
        public decimal TotalDiscount { get; set; } = 0m;

        public string OrderStatus { get; set; } = "Новый";
        public int OrderId { get; set; } = 0;
        public string role;
        public string UserRole
        {
            get { return role; }
            set { role = value; }
        }

        public UserOrder()
        {
            InitializeComponent();
            Loaded += UserOrder_Loaded;
            ticketCreater = new TicketCreater();
        }

        private void UserOrder_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProducts();
            LoadPickUp();
            LoadOrderInfo();
            DataContext = this;
            if (OrderStatus != "Новый")
            {
                changeStatusOrder.IsEnabled = false;
                pickUpPointBox.IsEnabled = false;
            }
        }

        private void LoadProducts()
        {
            connection.Open();
            var command = new NpgsqlCommand("SELECT p.id, p.image, p.title, p.description, p.manufacturer, p.cost, p.discount, dp.count FROM deal_product dp " +
                $"INNER JOIN product p ON p.id = dp.product_id WHERE deal_id = {OrderId}", connection);
            var reader = command.ExecuteReader();
            Products.Clear();
            while (reader.Read())
            {
                ProductCountViewModel productViewModel = new ProductCountViewModel();
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
                productViewModel.Count = (int)reader["count"];
                Products.Add(productViewModel);
            }
            reader.Close();
            connection.Close();
        }

        private void LoadPickUp()
        {
            connection.Open();
            var command = new NpgsqlCommand("SELECT title FROM pick_up_point", connection);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                ListItems.Add((string)reader["title"]);
            }
            reader.Close();
            connection.Close();
        }

        private void LoadOrderInfo()
        {
            connection.Open();
            var command = new NpgsqlCommand("SELECT SUM(cost * count) AS total_cost, SUM(CASE WHEN discount > 0 THEN (cost * discount / 100) * count ELSE 0::money END) AS total_discount " +
                $"FROM deal_product dp INNER JOIN product p ON dp.product_id = p.id WHERE deal_id = {OrderId} ", connection);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (reader["total_cost"] != DBNull.Value)
                {
                    var totalCost = (decimal)reader["total_cost"];
                    TotalDiscount = (decimal)reader["total_discount"];
                    TotalCost = totalCost - TotalDiscount;
                } else
                {
                    TotalCost = 0;
                    TotalDiscount = 0;
                }
            }
            totalCostTxt.Text = TotalCost.ToString();
            totalDiscountTxt.Text = TotalDiscount.ToString();
            reader.Close();
            connection.Close();
        }

        public void UpdateCost(int productId, int count)
        {
            connection.Open();
            var command = new NpgsqlCommand($"UPDATE deal_product SET count = {count} WHERE deal_id = {OrderId} AND product_id = {productId}", connection);
            command.ExecuteNonQuery();
            connection.Close();

            LoadOrderInfo();
            LoadProducts();
        }

        public void DeleteProduct(string productId)
        {
            connection.Open();
            var command = new NpgsqlCommand($"DELETE FROM deal_product WHERE deal_id = {OrderId} AND product_id = {productId}", connection);
            command.ExecuteNonQuery();
            connection.Close();

            LoadOrderInfo();
            LoadProducts();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GoToProductList();
        }

        private void GoToProductList()
        {
            ProductListWindow productListWindow = new ProductListWindow();
            productListWindow.OrderId = OrderId;
            productListWindow.OrderStatus = OrderStatus;
            productListWindow.UserRole = role;
            productListWindow.Closed += (s, args) => Close();
            productListWindow.Show();
            Hide();
        }

        private string AllNumbers = "0987654321";
        private void changeStatusOrder_Click(object sender, RoutedEventArgs e)
        {
            Random rand = new Random();
            string code = "";
            for (int i = 0; i < 3; i++)
            {
                code += AllNumbers[rand.Next(AllNumbers.Length)];
            }

            connection.Open();
            var command = new NpgsqlCommand($"SELECT CASE WHEN Min(p.warehouse_count) < 3 THEN true ELSE false END FROM deal_product dp INNER JOIN product p ON dp.product_id = p.id WHERE deal_id = {OrderId}", connection);
            bool isShortShipment = (bool)command.ExecuteScalar();
            connection.Close();

            string shipmentPeriod = "";
            if (isShortShipment)
            {
                shipmentPeriod = "3 дня";
            } else
            {
                shipmentPeriod = "6 дней";
            }

            connection.Open();
            var updateOrder = new NpgsqlCommand($"UPDATE deal SET pick_up_point_id = (SELECT id from pick_up_point where title = '${pickUpPointBox.Text}'), " +
                $"code = {code}, status_id = 2 WHERE id = {OrderId}", connection);
            updateOrder.ExecuteNonQuery();
            connection.Close();

            ticketCreater.CreateTicket(new FullOrderInfo(DateTime.Now, OrderId, Products.ToList(), TotalCost, TotalDiscount, code, pickUpPointBox.Text, shipmentPeriod));

            OrderStatus = "Сформирован";

            GoToProductList();
        }
    }
}
