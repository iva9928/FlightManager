namespace FlightManager.Data.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        public int FlightId { get; set; }

        public Flight Flight { get; set; }

        public string Email { get; set; }

        public bool Confirmed { get; set; }

        public ICollection<Passenger> Passengers { get; set; }
            = new List<Passenger>();
    }
}