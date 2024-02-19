using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Zoomir.models;

namespace Zoomir
{
    /// <summary>
    /// Логика взаимодействия для OrderListWindow.xaml
    /// </summary>
    public partial class OrderListWindow : Window
    {
        private NpgsqlConnection connection = new NpgsqlConnection("Server=localhost;Port=5432;Database=Zoomir;User Id=postgres;Password=0987");
        public List<OrderListModel> Orders { get; set; } = new List<OrderListModel>();
        public string UserRole { get; set; } = "";

        public OrderListWindow()
        {
            InitializeComponent();
            LoadOrders();
            DataContext = this;
        }

        private void LoadOrders()
        {
            connection.Open();
            var command = new NpgsqlCommand("SELECT d.id, d.date, ds.title, SUM(cost * count) AS total_cost, SUM(CASE WHEN discount > 0 THEN (cost * discount / 100) * count ELSE 0::money END) AS total_discount " +
                "FROM deal_product dp INNER JOIN product p ON dp.product_id = p.id " +
                "RIGHT JOIN deal d ON dp.deal_id = d.id " +
                "INNER JOIN deal_status ds ON ds.id = d.status_id GROUP BY d.id, ds.title", connection);
            var reader = command.ExecuteReader();
            while (reader.Read()) 
            {
                OrderListModel order = new OrderListModel();
                order.Id = (int)reader["id"];
                order.Date = (DateTime)reader["date"];
                order.Status = (string)reader["title"];
                if (reader["total_cost"] != DBNull.Value)
                {
                    order.TotalPrice = (decimal)reader["total_cost"];
                    order.TotalDiscount = (decimal)reader["total_discount"];
                } else
                {
                    order.TotalPrice = 0;
                    order.TotalDiscount = 0;
                }
                Orders.Add(order);
            }
            reader.Close();
            connection.Close();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ProductListWindow productListWindow = new ProductListWindow();
            productListWindow.UserRole = UserRole;
            productListWindow.Closed += (s, args) => Close();
            productListWindow.Show();
            Hide();
        }
        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var orderModel = (OrderListModel)(sender as DataGridCell).DataContext;

            if (orderModel.Status.Equals("Сформирован"))
            {
                MessageBox.Show("Нельзя изменять заказ в статусе Сформирован");
                return;
            }

            UserOrder userOrder = new UserOrder();
            userOrder.OrderId = orderModel.Id;
            userOrder.OrderStatus = orderModel.Status;
            userOrder.UserRole = UserRole;
            userOrder.Closed += (s, args) => Close();
            userOrder.Show();
            Hide();
        }
    }
}
