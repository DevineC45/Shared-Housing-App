using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedHousingApp.Models
{
    public class Announcement
    {
        public int Id { get; set; }

        public required string Title { get; set; }

        public required string Content { get; set; }

        public DateTime PostedAt { get; set; } = DateTime.Now;

        //Can add PostedByUserId if I decide to allow multiple households/landlords in future
    }
}

