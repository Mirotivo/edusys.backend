using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Wallet : IUpdatable
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    [ForeignKey(nameof(Wallet.UserId))]
    public User? User { get; set; }
    public decimal Balance { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Wallet()
    {

    }
    public override string ToString()
    {
        return $"Wallet: {Id}, UserId: {UserId}, Balance: {Balance:C}, UpdatedAt: {UpdatedAt}";
    }
}
