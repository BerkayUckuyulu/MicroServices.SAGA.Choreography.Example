namespace Shared
{
    public static class QueueNames
    {
        public static Uri GetQueueUri(string queueName) => new($"queue:{queueName}");

        public const string Stock_OrderCreatedQueue = "stock_orderCreatedQueue";
        public const string Order_OutOfStockEventQueue = "order_outOfStockEventQueue";
        public const string Payment_InStockEventQueue = "payment_inStockEventQueue";
        public const string Order_PaymentCompletedEventQueue = "order_paymentCompletedEventQueue";
        public const string Order_PaymentFailedEventQueue = "order_paymentFailedEventQueue";
        public const string Stock_PaymentFailedEventQueue = "stock_paymentFailedEventQueue";

    }
}

