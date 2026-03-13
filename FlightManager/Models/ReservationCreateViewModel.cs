using System.ComponentModel.DataAnnotations;

namespace FlightManager.Models
{
    public class ReservationCreateViewModel
    {
        public int FlightId { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public List<PassengerViewModel> Passengers { get; set; }
            = new List<PassengerViewModel>();
    }
}