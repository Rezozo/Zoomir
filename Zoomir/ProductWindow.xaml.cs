using Microsoft.Win32;
using Npgsql;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Zoomir.models;

namespace Zoomir
{
    /// <summary>
    /// Логика взаимодействия для ProductWindow.xaml
    /// </summary>
    public partial class ProductWindow : Window
    {
        private NpgsqlConnection connection = new NpgsqlConnection("Server=localhost;Port=5432;Database=Zoomir;User Id=postgres;Password=0987");
        private string role;
        public ProductWindow()
        {
            InitializeComponent();
        }

        public string UserRole
        {
            get { return role;}
            set { role = value; }
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(titleTxt.Text) || string.IsNullOrEmpty(descriptionTxt.Text) || string.IsNullOrEmpty(manufacturerTxt.Text) || string.IsNullOrEmpty(costTxt.Text)) 
            {
                MessageBox.Show("Заполните все обязательные поля!", "Предупреждение");
                return;
            }
            BitmapSource image = productImage.Source as BitmapSource;
            byte[] imageData = null;
            if (image != null)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();

                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(memoryStream);

                    imageData = memoryStream.ToArray();
                }
            } else
            {
                imageData = File.ReadAllBytes("D:\\images\\pic.png");
            }

            string title = titleTxt.Text;
            string description = descriptionTxt.Text;
            string manufacturer = manufacturerTxt.Text;
            decimal cost;
            int discount = 0;

            try
            {
                cost = decimal.Parse(costTxt.Text.Replace('.', ','));
            } catch
            {
                MessageBox.Show("Укажите верный формат стоимости, число с запятой, например, 399,00", "Ошибка");
                return;
            }

            if (discountTxt.Text != "")
            {
                try
                {
                    discount = int.Parse(discountTxt.Text);
                    if (discount > 100 || discount < 0)
                    {
                        MessageBox.Show("Укажите верный формат скидки: от 1 до 100", "Ошибка");
                        return;
                    }
                }
                catch
                {
                    MessageBox.Show("Укажите верный формат скидки: от 1 до 100", "Ошибка");
                    return;
                }
            }

            connection.Open();
            if (DataContext == null)
            {
                var command = new NpgsqlCommand($"INSERT INTO product VALUES (DEFAULT, @image, '{title}', '{description}', " +
                    $"'{manufacturer}', '{cost}', '{discount}', 0)", connection);
                command.Parameters.AddWithValue("image", imageData);
                command.ExecuteNonQuery();
            } else
            {
                int id = (DataContext as ProductViewModel).Id;
                var command = new NpgsqlCommand($"UPDATE product SET image =@image, title='{title}', description='{description}', " +
                    $"manufacturer='{manufacturer}', cost='{cost}', discount='{discount}' WHERE id={id}", connection);
                command.Parameters.AddWithValue("image", imageData);
                command.ExecuteNonQuery();
            }
            connection.Close();

            MessageBox.Show("Данные успешно обновлены!");

            goToProductList();
        }

        private void backBtn_Click(object sender, RoutedEventArgs e)
        {
            goToProductList();
        }

        private void goToProductList()
        {
            ProductListWindow product = new ProductListWindow();
            product.UserRole = role;
            Hide();
            product.Closed += (s, args) => Close();
            product.Show();
        }

        private void productImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png, *.gif) | *.jpg; *.jpeg; *.png; *.gif";

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedImagePath = openFileDialog.FileName;

                byte[] imageData = File.ReadAllBytes(selectedImagePath);

                (DataContext as ProductViewModel).Image = imageData;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(imageData);
                bitmapImage.EndInit();

                productImage.Source = bitmapImage;
            }
        }
    }
}
