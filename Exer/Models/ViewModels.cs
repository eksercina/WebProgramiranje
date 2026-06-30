using System;
using System.Collections.Generic;

namespace Exer.Models
{
    public class HomeViewModel
    {
        public IEnumerable<Accommodation> Accommodations { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Type { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string Sort { get; set; }
    }

    public class AccommodationDetailsViewModel
    {
        public Accommodation Accommodation { get; set; }
        public IEnumerable<Review> Reviews { get; set; }
        public double AverageRating { get; set; }
    }

    public class ReservationFormViewModel
    {
        public int AccommodationId { get; set; }
        public Accommodation Accommodation { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int GuestCount { get; set; }
    }

    public class ProfileViewModel
    {
        public User User { get; set; }
        public IEnumerable<Reservation> Reservations { get; set; }
        public IEnumerable<Accommodation> Accommodations { get; set; }
    }

    public class AdminUsersViewModel
    {
        public IEnumerable<User> Users { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? BirthFrom { get; set; }
        public DateTime? BirthTo { get; set; }
        public UserRole? Role { get; set; }
        public string Sort { get; set; }
    }
}