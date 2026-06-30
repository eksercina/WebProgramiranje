using Exer.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Exer.Controllers
{
    public class AccountController : BaseController
    {
        public ActionResult Login() { return View(); }

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            var user = Repository.Load().Users.FirstOrDefault(u => !u.Deleted && u.Username == username && u.Password == password);
            if (user == null)
            {
                ViewBag.Error = "Neispravno korisnicko ime ili lozinka.";
                return View();
            }
            Session["username"] = user.Username;
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Register()
        {
            return View(new User { BirthDate = new DateTime(2000, 1, 1), Role = UserRole.Guest });
        }

        [HttpPost]
        public ActionResult Register(User user)
        {
            var data = Repository.Load();
            if (data.Users.Any(u => u.Username == user.Username)) ModelState.AddModelError("Username", "Korisnicko ime vec postoji.");
            if (!ModelState.IsValid) return View(user);
            user.Role = UserRole.Guest;
            user.Deleted = false;
            data.Users.Add(user);
            Repository.Save(data);
            Session["username"] = user.Username;
            return RedirectToAction("Profile");
        }

        public ActionResult Profile()
        {
            if (!RequireLogin()) return RedirectToAction("Login");
            var data = Repository.Load();
            var user = CurrentUser;
            return View(new ProfileViewModel
            {
                User = user,
                Reservations = data.Reservations.Where(r => !r.Deleted && r.GuestUsername == user.Username).ToList(),
                Accommodations = data.Accommodations.Where(a => !a.Deleted && a.HostUsername == user.Username).ToList()
            });
        }

        [HttpPost]
        public ActionResult Profile(User model)
        {
            if (!RequireLogin()) return RedirectToAction("Login");
            var data = Repository.Load();
            var user = data.Users.FirstOrDefault(u => u.Username == CurrentUser.Username);
            if (user == null) return RedirectToAction("Logout");
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.BirthDate = model.BirthDate;
            user.Gender = model.Gender;
            if (!String.IsNullOrWhiteSpace(model.Password)) user.Password = model.Password;
            Repository.Save(data);
            TempData["Message"] = "Profil je sacuvan.";
            return RedirectToAction("Profile");
        }
    }
}