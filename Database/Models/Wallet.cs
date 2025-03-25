using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Wallet : IOwnable, IUpdatable
{
    [Key]
    public int Id { get; set; }
    public string UserId { get; set; }
    [ForeignKey(nameof(Wallet.UserId))]
    public User? User { get; set; }
    public decimal Balance { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Wallet()
    {
        UserId = string.Empty;
    }
    public override string ToString()
    {
        return $"Wallet: {Id}, UserId: {UserId}, Balance: {Balance:C}, UpdatedAt: {UpdatedAt}";
    }
}

