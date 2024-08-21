using System;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BeanScene.Models
{
    //this is NOT APPLICATIONUSER, anyone who logs in or registers, ends up there. This is NOT dbo.AspNetUsers.
    //this is ONLY linked to 'roles' in a way that when someone registers, they register to BOTH APPLICATIONUSER AND USER.
    //WHY?
    //because: maybe a guest gets added, but they don't want to verify their email. that means they end up here
    //maybe someone used to be a guest, but now they're a member. that means they end up in both.
    //it also means they can have their membership revoked.
    //and managers don't necessarily want to be in the 'guest/member' table.
    //this is USER, which is dbo.User -- the User table includes guests and members only, NOT roles like staff, manager or member linked with asp identity framework
    public enum membership //this is very important in reservations -- an applicationuser becomes a member automatically when they get registered. a guest is a 'ghost', they exist but not under aspnetroles at all. anyone who DIDN'T book online, but still gets added to the system.
    {
        Guest,
        Member
    }

    public class User
    {
        [Key]
        [DisplayName("User")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(15, ErrorMessage = "First name cannot exceed 15 characters")]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(15, ErrorMessage = "Last name cannot exceed 15 characters")]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Invalid phone number format")]
        public string Phone {  get; set; }

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string Email { get; set; }

        [DisplayName("Membership")]
        [Required(ErrorMessage = "Account type is required")]
        public membership Membership { get; set; }

        //foreign key relationships:
        [DisplayName("Reservations")]
        public List<Reservation> FKReservation { get; set; } = new List<Reservation>();
    }
}
