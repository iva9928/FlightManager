namespace FlightManager.Models
{
    public class ReservationListViewModel
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public int FlightId { get; set; }

        public bool Confirmed { get; set; }

        public int PassengersCount { get; set; }
    }
}