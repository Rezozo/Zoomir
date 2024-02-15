using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zoomir.models
{
    public class FullOrderInfo
    {
        public DateTime Date { get; set; }
        public int Id { get; set; }
        public List<ProductCountViewModel> ProductCount { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalDiscount { get; set; }
        public string PickUpPoint { get; set; }
        public string Code { get; set; }
        public string DeliveryPeriod { get; set; }
        public FullOrderInfo() { }

        public FullOrderInfo(DateTime date, int id, List<ProductCountViewModel> productCount, decimal totalPrice, decimal totalDiscount, string code, string pickUpPoint, string deliveryPeriod)
        {
            Date = date;
            Id = id;
            ProductCount = productCount;
            TotalPrice = totalPrice;
            TotalDiscount = totalDiscount;
            Code = code;
            PickUpPoint = pickUpPoint;
            DeliveryPeriod = deliveryPeriod;
        }
    }
}
