namespace Stock.API.Models;

public class OrderInbox
{
    public int Id { get; set; }
    public bool Processed { get; set; }
    public string Payload { get; set; }
}