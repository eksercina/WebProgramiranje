using Exer.Models;
using Exer.Services;
using System.Web.Mvc;

namespace Exer.Controllers
{
    public class BaseController : Controller
    {
        protected readonly AppRepository Repository = new AppRepository();
        protected User CurrentUser { get { return Repository.CurrentUser(Session); } }

        protected bool RequireLogin()
        {
            if (CurrentUser != null) return true;
            TempData["Message"] = "Morate biti prijavljeni.";
            return false;
        }

        protected bool RequireRole(UserRole role)
        {
            var user = CurrentUser;
            if (user != null && user.Role == role) return true;
            TempData["Message"] = "Nemate pravo pristupa trazenoj stranici.";
            return false;
        }
    }
}