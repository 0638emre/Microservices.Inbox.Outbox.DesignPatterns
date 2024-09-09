using System.ComponentModel.DataAnnotations;

namespace Order.API.Models;

public class OrderOutbox
{
    [Key]
    public Guid IdempotentToken { get; set; } //idempotent sorunsalı için. 
    public DateTime OccuredOn { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public string Type { get; set; } //event
    public string Payload { get; set; }
}