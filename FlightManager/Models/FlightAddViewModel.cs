using System.ComponentModel.DataAnnotations;

namespace FlightManager.Models
{
    public class FlightAddViewModel
    {
        [Required]
        [Display(Name = "От")]
        public string FromLocation { get; set; }

        [Required]
        [Display(Name = "До")]
        public string ToLocation { get; set; }

        [Required]
        [Display(Name = "Излитане")]
        public DateTime DepartureTime { get; set; }

        [Required]
        [Display(Name = "Кацане")]
        public DateTime ArrivalTime { get; set; }

        [Required]
        public string AircraftType { get; set; }

        [Required]
        public string AircraftNumber { get; set; }

        [Required]
        public string PilotName { get; set; }

        [Required]
        public int EconomySeats { get; set; }

        [Required]
        public int BusinessSeats { get; set; }
    }
}