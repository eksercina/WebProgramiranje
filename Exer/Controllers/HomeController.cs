using Exer.Models;
using System.Linq;
using System.Web.Mvc;

namespace Exer.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index(HomeViewModel query)
        {
            query.Accommodations = Repository.QueryAccommodations(query, true);
            ViewBag.User = CurrentUser;
            return View(query);
        }

        public ActionResult Details(int id)
        {
            var data = Repository.Load();
            var accommodation = data.Accommodations.FirstOrDefault(a => a.Id == id && !a.Deleted);
            if (accommodation == null) return HttpNotFound();
            return View(new AccommodationDetailsViewModel
            {
                Accommodation = accommodation,
                Reviews = data.Reviews.Where(r => !r.Deleted && r.AccommodationId == id && r.Status == ReviewStatus.Approved).ToList(),
                AverageRating = Repository.AverageRating(data, id)
            });
        }
    }
}