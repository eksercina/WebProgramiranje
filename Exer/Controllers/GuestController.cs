using Exer.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Exer.Controllers
{
    public class GuestController : BaseController
    {
        public ActionResult Reservations(string status)
        {
            if (!RequireRole(UserRole.Guest)) return RedirectToAction("Login", "Account");
            var data = Repository.Load();
            var reservations = data.Reservations.Where(r => !r.Deleted && r.GuestUsername == CurrentUser.Username);
            if (!String.IsNullOrWhiteSpace(status))
            {
                ReservationStatus parsed;
                if (Enum.TryParse(status, true, out parsed)) reservations = reservations.Where(r => r.Status == parsed);
            }
            ViewBag.Accommodations = data.Accommodations;
            ViewBag.Status = status;
            return View(reservations.ToList());
        }

        public ActionResult CreateReservation(int id)
        {
            if (!RequireRole(UserRole.Guest)) return RedirectToAction("Login", "Account");
            var accommodation = Repository.Load().Accommodations.FirstOrDefault(a => a.Id == id && !a.Deleted && a.Available);
            if (accommodation == null) return HttpNotFound();
            return View(new ReservationFormViewModel { Accommodation = accommodation, AccommodationId = id, CheckIn = DateTime.Today.AddDays(1), CheckOut = DateTime.Today.AddDays(2), GuestCount = 1 });
        }

        [HttpPost]
        public ActionResult CreateReservation(ReservationFormViewModel form)
        {
            if (!RequireRole(UserRole.Guest)) return RedirectToAction("Login", "Account");
            var data = Repository.Load();
            var accommodation = data.Accommodations.FirstOrDefault(a => a.Id == form.AccommodationId && !a.Deleted && a.Available);
            if (accommodation == null) return HttpNotFound();
            if (form.CheckOut <= form.CheckIn) ModelState.AddModelError("CheckOut", "Datum odjave mora biti posle datuma prijave.");
            if (form.GuestCount < 1 || form.GuestCount > accommodation.MaxGuests) ModelState.AddModelError("GuestCount", "Broj gostiju nije dozvoljen za ovaj objekat.");
            if (Repository.HasOverlap(data, accommodation.Id, form.CheckIn, form.CheckOut, null)) ModelState.AddModelError("", "Termin se preklapa sa odobrenom rezervacijom.");
            if (!ModelState.IsValid)
            {
                form.Accommodation = accommodation;
                return View(form);
            }
            var nights = (form.CheckOut.Date - form.CheckIn.Date).Days;
            data.Reservations.Add(new Reservation { Id = data.Reservations.Any() ? data.Reservations.Max(r => r.Id) + 1 : 1, AccommodationId = accommodation.Id, GuestUsername = CurrentUser.Username, CheckIn = form.CheckIn.Date, CheckOut = form.CheckOut.Date, GuestCount = form.GuestCount, TotalPrice = nights * accommodation.PricePerNight, Status = ReservationStatus.Created });
            Repository.Save(data);
            return RedirectToAction("Reservations");
        }

        [HttpPost]
        public ActionResult Cancel(int id)
        {
            if (!RequireRole(UserRole.Guest)) return RedirectToAction("Login", "Account");
            var data = Repository.Load();
            var reservation = data.Reservations.FirstOrDefault(r => r.Id == id && r.GuestUsername == CurrentUser.Username);
            if (reservation != null && Repository.CanCancel(reservation))
            {
                reservation.Status = ReservationStatus.Cancelled;
                Repository.Save(data);
            }
            return RedirectToAction("Reservations");
        }

        public ActionResult Reviews()
        {
            if (!RequireRole(UserRole.Guest)) return RedirectToAction("Login", "Account");
            var data = Repository.Load();
            ViewBag.Accommodations = data.Accommodations;
            ViewBag.Finished = data.Reservations.Where(r => r.GuestUsername == CurrentUser.Username && r.Status == ReservationStatus.Finished).ToList();
            return View(data.Reviews.Where(r => !r.Deleted && r.GuestUsername == CurrentUser.Username).ToList());
        }

        [HttpPost]
        public ActionResult SaveReview(Review review)
        {
            if (!RequireRole(UserRole.Guest)) return RedirectToAction("Login", "Account");
            var data = Repository.Load();
            var canReview = data.Reservations.Any(r => r.GuestUsername == CurrentUser.Username && r.AccommodationId == review.AccommodationId && r.Status == ReservationStatus.Finished);
            if (!canReview) return RedirectToAction("Reviews");
            if (review.Id == 0)
            {
                review.Id = data.Reviews.Any() ? data.Reviews.Max(r => r.Id) + 1 : 1;
                review.GuestUsername = CurrentUser.Username;
                review.Status = ReviewStatus.Created;
                data.Reviews.Add(review);
            }
            else
            {
                var existing = data.Reviews.FirstOrDefault(r => r.Id == review.Id && r.GuestUsername == CurrentUser.Username);
                if (existing != null)
                {
                    existing.Title = review.Title;
                    existing.Content = review.Content;
                    existing.Rating = review.Rating;
                    existing.Status = ReviewStatus.Created;
                }
            }
            Repository.Save(data);
            return RedirectToAction("Reviews");
        }

        [HttpPost]
        public ActionResult DeleteReview(int id)
        {
            if (!RequireRole(UserRole.Guest)) return RedirectToAction("Login", "Account");
            var data = Repository.Load();
            var review = data.Reviews.FirstOrDefault(r => r.Id == id && r.GuestUsername == CurrentUser.Username);
            if (review != null) review.Deleted = true;
            Repository.Save(data);
            return RedirectToAction("Reviews");
        }
    }
}