using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BeanScene.Models
{
    //this is just for the manager's use if they want to edit a new member to become staff or a manager as well.
    public class UserRoleViewModel
    {
        [Key]
        public string UserId { get; set; }
        public string Email { get; set; }
        public IList<string> Roles { get; set; }
    }
}
