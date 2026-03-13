namespace FlightManager.Models
{
    public class FlightDetailsViewModel
    {
        public int Id { get; set; }

        public string FromLocation { get; set; }

        public string ToLocation { get; set; }

        public DateTime DepartureTime { get; set; }

        public DateTime ArrivalTime { get; set; }

        public string AircraftType { get; set; }

        public string AircraftNumber { get; set; }

        public string PilotName { get; set; }

        public int EconomySeats { get; set; }

        public int BusinessSeats { get; set; }

        public IEnumerable<PassengerViewModel> Passengers { get; set; }
    }
}