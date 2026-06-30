using Exer.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Exer.Controllers
{
    public class HostController : BaseController
    {
        public ActionResult Accommodations(bool? available, string sort)
        {
            if (!RequireRole(UserRole.Host)) return RedirectToAction("Login", "Account");
            var data = Repository.Load();
            var items = data.Accommodations.Where(a => !a.Deleted && a.HostUsername == CurrentUser.Username);
            if (available.HasValue) items = items.Where(a => a.Available == available.Value);
            ViewBag.Available = available;
            ViewBag.Sort = sort;
            return View(Repository.SortAccommodations(items, sort).ToList());
        }

        public ActionResult EditAccommodation(int? id)
        {
            if (!RequireRole(UserRole.Host)) return RedirectToAction("Login", "Account");
            if (!id.HasValue) return View(new Accommodation { Available = true, PostedAt = DateTime.Today });
            var item = Repository.Load().Accommodations.FirstOrDefault(a => a.Id == id.Value && a.HostUsername == CurrentUser.Username && !a.Deleted);
            if (item == null || !item.Available) return RedirectToAction("Accommodations");
            return View(item);
        }

        [HttpPost]
        public ActionResult EditAccommodation(Accommodation model, HttpPostedFileBase image)
        {
            if (!RequireRole(UserRole.Host)) return RedirectToAction("Login", "Account");
            var data = Repository.Load();
            Accommodation item;
            if (model.Id == 0)
            {
                item = model;
                item.Id = data.Accommodations.Any() ? data.Accommodations.Max(a => a.Id) + 1 : 1;
                item.HostUsername = CurrentUser.Username;
                item.PostedAt = DateTime.Today;
                data.Accommodations.Add(item);
            }
            else
            {
                item = data.Accommodations.FirstOrDefault(a => a.Id == model.Id && a.HostUsername == CurrentUser.Username && a.Available && !a.Deleted);
                if (item == null) return RedirectToAction("Accommodations");
                item.Name = model.Name;
                item.Type = model.Type;
                item.Description = model.Description;
                item.Address = model.Address;
                item.City = model.City;
                item.PricePerNight = model.PricePerNight;
                item.MaxGuests = model.MaxGuests;
                item.Available = model.Available;
            }
            item.ImagePath = Repository.SaveUpload(image, item.ImagePath ?? "/Content/Images/apartment.svg");
            Repository.Save(data);
            return RedirectToAction("Accommodations");
        }

        [HttpPost]
        public ActionResult DeleteAccommodation(int id)
        {
            if (!RequireRole(UserRole.Host)) return RedirectToAction("Login", "Account");
            var data = Repository.Load();
            var item = data.Accommodations.FirstOrDefault(a => a.Id == id && a.HostUsername == CurrentUser.Username && a.Available && !a.Deleted);
            if (item != null) item.Deleted = true;
            Repository.Save(data);
            return RedirectToAction("Accommodations");
        }
    }
}