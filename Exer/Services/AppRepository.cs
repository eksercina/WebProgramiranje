using Exer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace Exer.Services
{
    public class AppRepository
    {
        private readonly string dataFile;
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer();

        public AppRepository()
        {
            dataFile = HttpContext.Current.Server.MapPath("~/App_Data/data.json");
            EnsureDataFile();
        }

        public AppData Load()
        {
            EnsureDataFile();
            return serializer.Deserialize<AppData>(File.ReadAllText(dataFile)) ?? SeedData();
        }

        public void Save(AppData data)
        {
            File.WriteAllText(dataFile, serializer.Serialize(data));
        }

        public User CurrentUser(HttpSessionStateBase session)
        {
            var username = session["username"] as string;
            if (String.IsNullOrWhiteSpace(username)) return null;
            return Load().Users.FirstOrDefault(u => u.Username == username && !u.Deleted);
        }

        public string SaveUpload(HttpPostedFileBase file, string fallback)
        {
            if (file == null || file.ContentLength == 0) return fallback;
            var folder = HttpContext.Current.Server.MapPath("~/Content/Uploads");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            var ext = Path.GetExtension(file.FileName);
            if (String.IsNullOrWhiteSpace(ext)) ext = ".jpg";
            var name = Guid.NewGuid().ToString("N") + ext;
            file.SaveAs(Path.Combine(folder, name));
            return "/Content/Uploads/" + name;
        }

        public IEnumerable<Accommodation> QueryAccommodations(HomeViewModel query, bool onlyAvailable)
        {
            var items = Load().Accommodations.Where(a => !a.Deleted);
            if (onlyAvailable) items = items.Where(a => a.Available);
            if (!String.IsNullOrWhiteSpace(query.Name)) items = items.Where(a => Contains(a.Name, query.Name));
            if (!String.IsNullOrWhiteSpace(query.City)) items = items.Where(a => Contains(a.City, query.City));
            if (!String.IsNullOrWhiteSpace(query.Type)) items = items.Where(a => Contains(a.Type, query.Type));
            if (query.MinPrice.HasValue) items = items.Where(a => a.PricePerNight >= query.MinPrice.Value);
            if (query.MaxPrice.HasValue) items = items.Where(a => a.PricePerNight <= query.MaxPrice.Value);
            return SortAccommodations(items, query.Sort).ToList();
        }

        public IEnumerable<Accommodation> SortAccommodations(IEnumerable<Accommodation> items, string sort)
        {
            switch ((sort ?? "").ToLowerInvariant())
            {
                case "name_desc": return items.OrderByDescending(a => a.Name);
                case "price": return items.OrderBy(a => a.PricePerNight);
                case "price_desc": return items.OrderByDescending(a => a.PricePerNight);
                case "date": return items.OrderBy(a => a.PostedAt);
                case "date_desc": return items.OrderByDescending(a => a.PostedAt);
                default: return items.OrderBy(a => a.Name);
            }
        }

        public bool HasOverlap(AppData data, int accommodationId, DateTime checkIn, DateTime checkOut, int? exceptId)
        {
            return data.Reservations.Any(r => !r.Deleted && r.AccommodationId == accommodationId && r.Status == ReservationStatus.Approved && (!exceptId.HasValue || r.Id != exceptId.Value) && checkIn < r.CheckOut && checkOut > r.CheckIn);
        }

        public bool CanCancel(Reservation reservation)
        {
            return (reservation.Status == ReservationStatus.Created || reservation.Status == ReservationStatus.Approved) && DateTime.Now.AddHours(24) <= reservation.CheckIn;
        }

        public void CancelActiveReservationsForGuest(AppData data, string username)
        {
            foreach (var r in data.Reservations.Where(x => x.GuestUsername == username && (x.Status == ReservationStatus.Created || x.Status == ReservationStatus.Approved))) r.Status = ReservationStatus.Cancelled;
        }

        public double AverageRating(AppData data, int accommodationId)
        {
            var ratings = data.Reviews.Where(r => !r.Deleted && r.AccommodationId == accommodationId && r.Status == ReviewStatus.Approved).Select(r => r.Rating).ToList();
            return ratings.Any() ? ratings.Average() : 0;
        }

        private bool Contains(string source, string value)
        {
            return (source ?? "").IndexOf(value ?? "", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void EnsureDataFile()
        {
            var dir = Path.GetDirectoryName(dataFile);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            if (!File.Exists(dataFile)) Save(SeedData());
        }

        private AppData SeedData()
        {
            return new AppData
            {
                Users = new List<User>
                {
                    new User { Username = "admin", Password = "admin", FirstName = "Mihajlo", LastName = "Miletic", Email = "admin@example.com", BirthDate = new DateTime(1990, 1, 1), Gender = Gender.Female, Role = UserRole.Administrator },
                    new User { Username = "host1", Password = "host1", FirstName = "Pedja", LastName = "Balan", Email = "host1@example.com", BirthDate = new DateTime(1988, 5, 12), Gender = Gender.Male, Role = UserRole.Host },
                    new User { Username = "guest1", Password = "guest1", FirstName = "Marko", LastName = "Milovanovic", Email = "guest1@example.com", BirthDate = new DateTime(1998, 3, 4), Gender = Gender.Female, Role = UserRole.Guest }
                },
                Accommodations = new List<Accommodation>
                {
                    new Accommodation { Id = 1, Name = "Hotel Moskva", Type = "Hotel", Description = "Mirna lokacija blizu centra.", Address = "Terazije 20", City = "Beograd", PricePerNight = 75, MaxGuests = 3, ImagePath = "/Content/Images/HotelMoskva.jpg", PostedAt = DateTime.Today.AddDays(-14), Available = true, HostUsername = "host1" },
                    new Accommodation { Id = 2, Name = "Hotel Srbija", Type = "Hotel", Description = "Hotel u centru Vrsac, pogodan za poslovna i turisticka putovanja.", Address = "Svetosavski trg 12", City = "Vrsac", PricePerNight = 50, MaxGuests = 4, ImagePath = "/Content/Images/HotelSrbija.jpg", PostedAt = DateTime.Today.AddDays(-7), Available = true, HostUsername = "host1" },
                    new Accommodation { Id = 3, Name = "Villa Breg", Type = "Hotel", Description = "Hotel sa pogledom na Vrsacke vinograde i bazenom.", Address = "Goranska bb", City = "Vrsac", PricePerNight = 24, MaxGuests = 2, ImagePath = "/Content/Images/VillaBreg.jpg", PostedAt = DateTime.Today.AddDays(-3), Available = false, HostUsername = "host1" }
                },
                Reservations = new List<Reservation>
                {
                    new Reservation { Id = 1, AccommodationId = 1, GuestUsername = "guest1", CheckIn = DateTime.Today.AddDays(-6), CheckOut = DateTime.Today.AddDays(-3), GuestCount = 2, TotalPrice = 225, Status = ReservationStatus.Finished },
                    new Reservation { Id = 2, AccommodationId = 2, GuestUsername = "guest1", CheckIn = DateTime.Today.AddDays(10), CheckOut = DateTime.Today.AddDays(12), GuestCount = 2, TotalPrice = 100, Status = ReservationStatus.Created }
                },
                Reviews = new List<Review>
                {
                    new Review { Id = 1, AccommodationId = 1, GuestUsername = "guest1", Title = "Odlican boravak", Content = "Objekat je uredan, lokacija je prakticna.", Rating = 5, Status = ReviewStatus.Approved }
                }
            };
        }
    }
}

