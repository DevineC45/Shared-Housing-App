using System.ComponentModel.DataAnnotations;
using SharedHousingApp.Models;

namespace SharedHousingApp.Models;

public class Expense
{
    public int Id { get; set; }

    public required string Title { get; set; }   // e.g., “Electricity Bill”, “Groceries”

    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Date is required.")]
    [DataType(DataType.Date)]
    public DateTime? Date { get; set; }   // nullable so Required can fire

    public int PaidByUserId { get; set; }
    public User? PaidByUser { get; set; }

    public bool IsSettled { get; set; } = false;  // Optional

    public List<User> SharedWithUsers { get; set; } = new();
}
