namespace FlightManager.Models
{
    public class FlightListViewModel
    {
        public int Id { get; set; }

        public string FromLocation { get; set; }

        public string ToLocation { get; set; }

        public DateTime DepartureTime { get; set; }

        public TimeSpan Duration { get; set; }

        public int EconomySeats { get; set; }

        public int BusinessSeats { get; set; }
    }
}