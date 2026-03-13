namespace FlightManager.Models
{
    public class FlightSearchViewModel
    {
        public string FromLocation { get; set; }

        public string ToLocation { get; set; }

        public DateTime? Date { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}