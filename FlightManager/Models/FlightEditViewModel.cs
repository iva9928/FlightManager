using System.ComponentModel.DataAnnotations;

namespace FlightManager.Models
{
    public class FlightEditViewModel
    {
        public int Id { get; set; }

        [Required]
        public string FromLocation { get; set; }

        [Required]
        public string ToLocation { get; set; }

        [Required]
        public DateTime DepartureTime { get; set; }

        [Required]
        public DateTime ArrivalTime { get; set; }

        public string AircraftType { get; set; }

        public string AircraftNumber { get; set; }

        public string PilotName { get; set; }

        public int EconomySeats { get; set; }

        public int BusinessSeats { get; set; }
    }
}