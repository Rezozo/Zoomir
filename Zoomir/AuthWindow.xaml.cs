using Npgsql;
using System.Windows;
namespace Zoomir
{
    public partial class AuthWindow : Window
    {
        private NpgsqlConnection connection = new NpgsqlConnection("Server=localhost;Port=5432;Database=Zoomir;User Id=postgres;Password=0987");
        public AuthWindow()
        {
            InitializeComponent();
        }

        private void guestBtn_Click(object sender, RoutedEventArgs e)
        {
            ProductListWindow productListWindow = new ProductListWindow();
            productListWindow.UserRole = "";
            productListWindow.Closed += (s, args) => Close();
            productListWindow.Show();
            Hide();
        }

        private void authBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(loginTxt.Text) && !string.IsNullOrWhiteSpace(passwordBox.Password)) 
            {
                connection.Open();
                var command = new NpgsqlCommand($"SELECT role_id FROM users WHERE login = '{loginTxt.Text}' AND password = '{passwordBox.Password}'", connection);
                var role = command.ExecuteScalar();
                connection.Close();
                if (role != null)
                {
                    ProductListWindow productListWindow = new ProductListWindow();
                    if (role.ToString() == "1")
                    {
                        productListWindow.UserRole = "Клиент";
                    } else if (role.ToString() == "2")
                    {
                        productListWindow.UserRole = "Администратор";
                    } else
                    {
                        productListWindow.UserRole = "Менеджер";
                    }

                    productListWindow.Closed += (s, args) => Close();
                    productListWindow.Show();
                    Hide();
                } else
                {
                    MessageBox.Show("Неправильный логин или пароль", "Предупреждение");
                }
            } else
            {
                MessageBox.Show("Введите логин и пароль", "Предупреждение");
            }
        }
    }
}
