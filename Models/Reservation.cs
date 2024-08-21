using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BeanScene.Models

{
    public enum resstatus //this is a drop-down list later
    {
        Pending,
        Confirmed,
        Cancelled,
        Seated,
        Completed
    }

    public enum source //this is a drop-down list later
    {
        Online,
        Mobile,
        InPerson,
        Email,
        Phone
    }

    public class Reservation
    {
        [Key]
        [DisplayName("Reservation ID")]
        public int ResId { get; set; }

        [Required]
        [ForeignKey("UserId")]
        [DisplayName("User")]
        public int UserId { get; set; }

        [Required]
        [ForeignKey("SittingId")]
        [DisplayName("Sitting")]
        public int SittingId { get; set; }

        [Required]
        [ForeignKey("TableId")]
        public int TableId { get; set; }

        [DisplayName("Number Of Guests")] //there are only 4 guests per table because an 'area' limit is 40, and there are usually just ten tables -- flawed logic if you add more tables, though
        [Range(1, 4, ErrorMessage = "Cannot exceed 4. Please open more reservations for other guests.")]
        public int NumOfGuests { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh\\:mm}", ApplyFormatInEditMode = true)]
        [DisplayName("Start Time")]
        public DateTime ResStartTime { get; set; }

        [DisplayName("Duration (hours)")]
        public int Duration { get; set; }

        public source Source { get; set; }

        [DisplayName("Special Requests")]
        public string? SpecialReqs { get; set; }

        [DisplayName("Booking Status")]
        public resstatus ResStatus { get; set; }

        //foreign key relationships:
        public User? FKUser { get; set; }

        [DisplayName("Sitting")]
        public Sitting? FKSitting { get; set; }

        public Table? FKTable { get; set; }

    }
}