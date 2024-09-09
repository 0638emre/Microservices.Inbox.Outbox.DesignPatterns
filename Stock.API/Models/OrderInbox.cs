using System.ComponentModel.DataAnnotations;

namespace Stock.API.Models;

public class OrderInbox
{
    [Key]
    public Guid IdempotentToken { get; set; } //idempotent sorunsalı için. 
    public bool Processed { get; set; }
    public string Payload { get; set; }
}