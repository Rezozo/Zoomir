using Npgsql;
using System.Windows;
using System.Windows.Controls;
using Zoomir.models;

namespace Zoomir
{
    /// <summary>
    /// Логика взаимодействия для ProductOrderControl.xaml
    /// </summary>
    public partial class ProductOrderControl : UserControl
    {
        private NpgsqlConnection connection = new NpgsqlConnection("Server=localhost;Port=5432;Database=Zoomir;User Id=postgres;Password=0987");
        public static readonly DependencyProperty ProductProperty =
            DependencyProperty.Register("Product", typeof(ProductViewModel), typeof(ProductOrderControl), new PropertyMetadata(null));

        public ProductViewModel Product
        {
            get { return (ProductViewModel)GetValue(ProductProperty); }
            set { SetValue(ProductProperty, value); }
        }

        public ProductOrderControl()
        {
            InitializeComponent();
            Loaded += ProductOrderControl_Loaded;
        }

        private void ProductOrderControl_Loaded(object sender, RoutedEventArgs e)
        {
            UserOrder userOrder = Window.GetWindow(this) as UserOrder;

            if (userOrder.OrderStatus != "Новый")
            {
                plusBtn.Visibility = Visibility.Hidden;
                minusBtn.Visibility = Visibility.Hidden;
            }
        }

        private void plusBtn_Click(object sender, RoutedEventArgs e)
        {
            var newCount = (int.Parse(countTxt.Text) + 1).ToString();
            UserOrder userOrder = Window.GetWindow(this) as UserOrder;
            countTxt.Text = newCount;
            userOrder.UpdateCost(int.Parse(idTxt.Text), int.Parse(newCount));
        }

        private void minusBtn_Click(object sender, RoutedEventArgs e)
        {
            var newCount = (int.Parse(countTxt.Text) - 1).ToString();
            UserOrder userOrder = Window.GetWindow(this) as UserOrder;
            if (newCount == "0")
            {
                MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить этот продукт?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    userOrder.DeleteProduct(idTxt.Text);
                }
                return;
            }

            countTxt.Text = newCount;
            userOrder.UpdateCost(int.Parse(idTxt.Text), int.Parse(newCount));
        }
    }
}
