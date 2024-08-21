using Microsoft.AspNetCore.Identity;

namespace BeanScene.Data
{
    //this is APPLICATIONUSER, which means anyone who logs in or registers, ends up here. This is dbo.AspNetUsers
    //this is NOT USER, which is just dbo.User -- the User table includes guests and members, this one 'controls' members, staff and managers

    public class ApplicationUser: IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}