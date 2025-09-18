using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedHousingApp.Models
{
    public class CalendarEvent
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }  = string.Empty;  // Fixes warning

        public string? Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public string? EventType { get; set; } // "Landlord" or "Tenant"

        public string? CreatedByName { get; set; } // User's name (for "Joe", etc.)
    }
}