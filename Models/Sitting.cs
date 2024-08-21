using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BeanScene.Models
{
    public enum sittingtype
    {
        Breakfast,
        Lunch,
        Dinner
    }

    public enum sittingstatus //this has no use; the sitting will automatically be closed if there are enough reservations....
    {
        Open,
        Closed
    }

    public class Sitting
    {
        [Key]
        [DisplayName("Sitting")]
        public int SittingId { get; set; }

        [DisplayName("Type")]
        public sittingtype SittingType {  get; set; }

        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh\\:mm}", ApplyFormatInEditMode = true)]
        [DisplayName("Start Time")]
        public DateTime StartTime { get; set; }

        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh\\:mm}", ApplyFormatInEditMode = true)]
        [DisplayName("End Time")]
        public DateTime EndTime { get; set; }

        [DisplayName("Status")]
        public sittingstatus SittingStatus { get; set; }

        //foreign key relationships:
        [DisplayName("Reservations")]
        public List<Reservation> FKReservation { get; set; } = new List<Reservation>();
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartTime >= EndTime)
            {
                yield return new ValidationResult("End time must be after start time.", new[] { "EndTime" });
            }
        }
    }
}
