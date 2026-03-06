namespace WebApplicationASP.Models
{
    public class Activity
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Category { get; set; }

        public string Status { get; set; }

        public string ImageUrl { get; set; }

        public DateTime ExpireDate { get; set; }
    }
}