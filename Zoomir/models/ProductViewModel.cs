using System.Drawing;

namespace Zoomir.models
{
    public  class ProductViewModel
    {
        public int Id { get; set; }
        public byte[] Image { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Manufacturer { get; set; }
        public decimal Cost { get; set; }
        public int Discount { get; set; }

        public ProductViewModel() { }

        public ProductViewModel(int id, byte[] image, string title, string description, string manufacturer, decimal cost, int discount)
        {
            Id = id;
            Image = image;
            Title = title;
            Description = description;
            Manufacturer = manufacturer;
            Cost = cost;
            Discount = discount;
        }
    }
}
