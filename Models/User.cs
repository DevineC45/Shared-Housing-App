using System.ComponentModel.DataAnnotations;

namespace SharedHousingApp.Models
{
    public class User
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public required string Email { get; set; }

        public required string Password { get; set; }

        public string Role { get; set; } = "Tenant"; // or "Landlord"

        public List<Expense> SharedExpenses { get; set; } = new();
    }
}