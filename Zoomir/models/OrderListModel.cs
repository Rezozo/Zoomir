using System;

namespace Zoomir.models
{
    public class OrderListModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalDiscount { get; set; }
        public string Status { get; set; }

        public OrderListModel() { } 
        public OrderListModel(int id, DateTime date, decimal totalPrice, decimal totalDiscount, string status)
        {
            Id = id;
            Date = date;
            TotalPrice = totalPrice;
            TotalDiscount = totalDiscount;
            Status = status;
        }
    }
}
