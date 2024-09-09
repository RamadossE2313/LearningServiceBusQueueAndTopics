namespace LearningServiceBusQueueAndTopicsSender.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = null!;
        public double Amount { get; set; }

        public static List<Order> Orders { get; } = new List<Order>()
        {
            new Order { Id = 1, ProductName = "Laptop", Amount = 999.99 },
            new Order { Id = 2, ProductName = "Smartphone", Amount = 499.99 },
            new Order { Id = 3, ProductName = "Headphones", Amount = 89.99 },
            new Order { Id = 4, ProductName = "Keyboard", Amount = 49.99 },
            new Order { Id = 5, ProductName = "Mouse", Amount = 29.99 },
            new Order { Id = 6, ProductName = "Monitor", Amount = 199.99 },
            new Order { Id = 7, ProductName = "Webcam", Amount = 59.99 },
            new Order { Id = 8, ProductName = "Printer", Amount = 129.99 },
            new Order { Id = 9, ProductName = "External Hard Drive", Amount = 149.99 },
            new Order { Id = 10, ProductName = "USB Flash Drive", Amount = 19.99 },
            new Order { Id = 11, ProductName = "Bluetooth Speaker", Amount = 79.99 },
            new Order { Id = 12, ProductName = "Smartwatch", Amount = 199.99 },
            new Order { Id = 13, ProductName = "Tablet", Amount = 349.99 },
            new Order { Id = 14, ProductName = "Router", Amount = 89.99 },
            new Order { Id = 15, ProductName = "External SSD", Amount = 229.99 },
            new Order { Id = 16, ProductName = "Desk Lamp", Amount = 39.99 },
            new Order { Id = 17, ProductName = "Office Chair", Amount = 159.99 },
            new Order { Id = 18, ProductName = "Graphics Card", Amount = 349.99 },
            new Order { Id = 19, ProductName = "USB Hub", Amount = 24.99 },
            new Order { Id = 20, ProductName = "Charging Cable", Amount = 9.99 }
            };

        public static Order DefaultOrder { get;  } = new Order { Id = 1, ProductName = "Laptop", Amount = 999.99 };
    }
}
