namespace Order.API.ViewModels;

public class OrderItemVM
{
    public decimal Price { get; set; }
    public int Count { get; set; }
    public int ProductId { get; set; }
}