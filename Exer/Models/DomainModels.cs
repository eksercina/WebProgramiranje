using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Exer.Models
{
    public enum UserRole { Guest, Host, Administrator }
    public enum Gender { Male, Female, Other }
    public enum ReservationStatus { Created, Approved, Cancelled, Finished }
    public enum ReviewStatus { Created, Approved, Rejected }

    public class User
    {
        [Required] public string Username { get; set; }
        [Required] public string Password { get; set; }
        [Required] public string FirstName { get; set; }
        [Required] public string LastName { get; set; }
        [Required] public string Email { get; set; }
        public DateTime BirthDate { get; set; }
        public Gender Gender { get; set; }
        public UserRole Role { get; set; }
        public bool Deleted { get; set; }
    }

    public class Accommodation
    {
        public int Id { get; set; }
        [Required] public string Name { get; set; }
        [Required] public string Type { get; set; }
        public string Description { get; set; }
        [Required] public string Address { get; set; }
        [Required] public string City { get; set; }
        public decimal PricePerNight { get; set; }
        public int MaxGuests { get; set; }
        public string ImagePath { get; set; }
        public DateTime PostedAt { get; set; }
        public bool Available { get; set; }
        public string HostUsername { get; set; }
        public bool Deleted { get; set; }
    }

    public class Reservation
    {
        public int Id { get; set; }
        public int AccommodationId { get; set; }
        public string GuestUsername { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int GuestCount { get; set; }
        public decimal TotalPrice { get; set; }
        public ReservationStatus Status { get; set; }
        public bool Deleted { get; set; }
    }

    public class Review
    {
        public int Id { get; set; }
        public int AccommodationId { get; set; }
        public string GuestUsername { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
        public string ImagePath { get; set; }
        public ReviewStatus Status { get; set; }
        public bool Deleted { get; set; }
    }

    public class AppData
    {
        public List<User> Users { get; set; }
        public List<Accommodation> Accommodations { get; set; }
        public List<Reservation> Reservations { get; set; }
        public List<Review> Reviews { get; set; }
    }
}