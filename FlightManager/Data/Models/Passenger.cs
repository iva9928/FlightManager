using System.ComponentModel.DataAnnotations;

namespace FlightManager.Data.Models
{
    public class Passenger
    {
        public int Id { get; set; }

        public int ReservationId { get; set; }

        public Reservation Reservation { get; set; }

        [Required]
        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string EGN { get; set; }

        public string Phone { get; set; }

        public string Nationality { get; set; }

        public string TicketType { get; set; }
    }
}