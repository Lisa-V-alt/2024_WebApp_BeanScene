using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BeanScene.Models
{
    public enum TableArea //dropdown list later
    {
        Main,
        Outside,
        Balcony
    }

    public class Table
    {
        public Table()
        {
            // Set the default TableCapacity to 4 (an area limit is 40 and there are usually just 10 tables, flawed logic but i'm bad at maths)
            TableCapacity = 4;
        }

        [Key]
        public int TableId { get; set; }

        [DisplayName("Table Number")]
        [Range(1, int.MaxValue, ErrorMessage = "Table number cannot be negative or 0.")]
        public int TableNo { get; set; }

        [DisplayName("Table Area")]
        public TableArea Area { get; set; }

        [DisplayName("Table Capacity")]
        public int TableCapacity { get; } // Make it read-only

        [DisplayName("Table Name")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [ScaffoldColumn(false)]
        public string TableName { get; set; }

        //foreign key relationships:
        [DisplayName("Reservations")]
        public List<Reservation> FKReservation { get; set; } = new List<Reservation>();

        // Method to generate TableName
        public void UpdateTableName()
        {
            TableName = $"{Area.ToString().Substring(0, 1)}{TableNo}";
        }
    }
}