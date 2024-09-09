using Shared.Datas;

namespace Shared.Events;

public class OrderCreatedEvent
{
    public Guid IdempotentToken { get; set; }
    public int BuyerId { get; set; }
    public int OrderId { get; set; }
    public decimal TotalPrice { get; set; }
    public List<OrderItem> OrderItems { get; set; }
}