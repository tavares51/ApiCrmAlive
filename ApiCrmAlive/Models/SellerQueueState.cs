using System.ComponentModel.DataAnnotations;

namespace ApiCrmAlive.Models;

public class SellerQueueState
{
    [Key]
    public int Id { get; set; }

    public Guid? LastSellerId { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

