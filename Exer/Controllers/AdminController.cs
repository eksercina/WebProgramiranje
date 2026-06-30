using Exer.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Exer.Controllers
{
    public class AdminController : BaseController
    {
        public ActionResult Users(AdminUsersViewModel query)
        {
            if (!RequireRole(UserRole.Administrator)) return RedirectToAction("Login", "Account");
            var users = Repository.Load().Users.Where(u => !u.Deleted && u.Role != UserRole.Administrator);
            if (!String.IsNullOrWhiteSpace(query.FirstName)) users = users.Where(u => (u.FirstName ?? "").IndexOf(query.FirstName, StringComparison.OrdinalIgnoreCase) >= 0);
            if (!String.IsNullOrWhiteSpace(query.LastName)) users = users.Where(u => (u.LastName ?? "").IndexOf(query.LastName, StringComparison.OrdinalIgnoreCase) >= 0);
            if (query.BirthFrom.HasValue) users = users.Where(u => u.BirthDate >= query.BirthFrom.Value);
            if (query.BirthTo.HasValue) users = users.Where(u => u.BirthDate <= query.BirthTo.Value);
            if (query.Role.HasValue) users = users.Where(u => u.Role == query.Role.Value);
            switch ((query.Sort ?? "").ToLowerInvariant())
            {
                case "first_desc": users = users.OrderByDescending(u => u.FirstName); break;
                case "birth": users = users.OrderBy(u => u.BirthDate); break;
                case "birth_desc": users = users.OrderByDescending(u => u.BirthDate); break;
                case "role": users = users.OrderBy(u => u.Role); break;
                case "role_desc": users = users.OrderByDescending(u => u.Role); break;
                default: users = users.OrderBy(u => u.FirstName); break;
            }
            query.Users = users.ToList();
            return View(query);
        }

        public ActionResult EditUser(string id)
        {
            if (!RequireRole(UserRole.Administrator)) return RedirectToAction("Login", "Account");
            var user = Repository.Load().Users.FirstOrDefault(u => u.Username == id && u.Role != UserRole.Administrator && !u.Deleted);
            return View(user ?? new User { BirthDate = new DateTime(2000, 1, 1), Role = UserRole.Host });
        }

        [HttpPost]
        public ActionResult EditUser(User model)
        {
            if (!RequireRole(UserRole.Administrator)) return RedirectToAction("Login", "Account");
            var data = Repository.Load();
            var user = data.Users.FirstOrDefault(u => u.Username == model.Username);
            if (user == null)
            {
                model.Role = model.Role == UserRole.Administrator ? UserRole.Host : model.Role;
                data.Users.Add(model);
            }
            else if (user.Role != UserRole.Administrator)
            {
                user.Password = model.Password;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.BirthDate = model.BirthDate;
                user.Gender = model.Gender;
                user.Role = model.Role == UserRole.Administrator ? user.Role : model.Role;
            }
            Repository.Save(data);
            return RedirectToAction("Users");
        }

        [HttpPost]
        public ActionResult DeleteUser(string id)
        {
            if (!RequireRole(UserRole.Administrator)) return RedirectToAction("Login", "Account");
            var data = Repository.Load();
            var user = data.Users.FirstOrDefault(u => u.Username == id && u.Role != UserRole.Administrator);
            if (user != null)
            {
                user.Deleted = true;
                if (user.Role == UserRole.Guest) Repository.CancelActiveReservationsForGuest(data, user.Username);
            }
            Repository.Save(data);
            return RedirectToAction("Users");
        }

        public ActionResult Accommodations(bool? available, string sort)
        {
            if (!RequireRole(UserRole.Administrator)) return RedirectToAction("Login", "Account");
            var items = Repository.Load().Accommodations.Where(a => !a.Deleted);
            if (available.HasValue) items = items.Where(a => a.Available == available.Value);
            ViewBag.Available = available;
            ViewBag.Sort = sort;
            return View(Repository.SortAccommodations(items, sort).ToList());
        }

        [HttpPost]
        public ActionResult ToggleAccommodation(int id, bool available)
        {
            if (!RequireRole(UserRole.Administrator)) return RedirectToAction("Login", "Account");
            var data = Repository.Load();
            var item = data.Accommodations.FirstOrDefault(a => a.Id == id && !a.Deleted);
            if (item != null) item.Available = available;
            Repository.Save(data);
            return RedirectToAction("Accommodations");
        }

        [HttpPost]
        public ActionResult DeleteAccommodation(int id)
        {
            if (!RequireRole(UserRole.Administrator)) return RedirectToAction("Login", "Account");
            var data = Repository.Load();
            var active = data.Reservations.Any(r => r.AccommodationId == id && (r.Status == ReservationStatus.Created || r.Status == ReservationStatus.Approved));
            var item = data.Accommodations.FirstOrDefault(a => a.Id == id && !a.Deleted);
            if (item != null && !active) item.Deleted = true;
            Repository.Save(data);
            return RedirectToAction("Accommodations");
        }

        public ActionResult Reservations()
        {
            if (!RequireRole(UserRole.Administrator)) return RedirectToAction("Login", "Account");
            var data = Repository.Load();
            ViewBag.Accommodations = data.Accommodations;
            return View(data.Reservations.Where(r => !r.Deleted).OrderByDescending(r => r.CheckIn).ToList());
        }

        [HttpPost]
        public ActionResult ChangeReservationStatus(int id, ReservationStatus status)
        {
            if (!RequireRole(UserRole.Administrator)) return RedirectToAction("Login", "Account");
            var data = Repository.Load();
            var reservation = data.Reservations.FirstOrDefault(r => r.Id == id && !r.Deleted);
            if (reservation != null)
            {
                if (status == ReservationStatus.Cancelled && !Repository.CanCancel(reservation)) return RedirectToAction("Reservations");
                if (reservation.Status == ReservationStatus.Created || status == ReservationStatus.Finished) reservation.Status = status;
            }
            Repository.Save(data);
            return RedirectToAction("Reservations");
        }

        public ActionResult Reviews()
        {
            if (!RequireRole(UserRole.Administrator)) return RedirectToAction("Login", "Account");
            var data = Repository.Load();
            ViewBag.Accommodations = data.Accommodations;
            return View(data.Reviews.Where(r => !r.Deleted).ToList());
        }

        [HttpPost]
        public ActionResult ChangeReviewStatus(int id, ReviewStatus status)
        {
            if (!RequireRole(UserRole.Administrator)) return RedirectToAction("Login", "Account");
            var data = Repository.Load();
            var review = data.Reviews.FirstOrDefault(r => r.Id == id && !r.Deleted);
            if (review != null && review.Status == ReviewStatus.Created) review.Status = status;
            Repository.Save(data);
            return RedirectToAction("Reviews");
        }
    }
}