using System.ComponentModel.DataAnnotations;
using SharedHousingApp.Models;

namespace SharedHousingApp.Models
{
    public class Chore
    {
        public int Id { get; set; }

        public required string Title { get; set; }

        public int AssignedToUserId { get; set; }

        public bool IsComplete { get; set; } = false;

        public bool RepeatWeekly { get; set; } = false;

        public User? AssignedToUser { get; set; }
    }
}