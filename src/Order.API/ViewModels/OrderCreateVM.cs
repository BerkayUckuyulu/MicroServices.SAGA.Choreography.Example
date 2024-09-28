using System;
namespace Order.API.ViewModels
{
    public class OrderCreateVM
    {
        public int BuyerId { get; set; }
        public List<OrderItemVM> OrderItems { get; set; }
    }
}