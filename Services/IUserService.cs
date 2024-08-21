using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using BeanScene.Models;

namespace BeanScene.Services
{
    public interface IUserService
    {
        public class Status
        {
            public int StatusCode { get; set; }
            public string Message { get; set; }
        }
        Task<Status> ChangePasswordAsync(ChangePassword model, string username);
        Task<Status> LoginAsync(Login model);
        Task<Status> LogoutAsync();
        Task<Status> RegisterAsync(Register model);
    }
}
