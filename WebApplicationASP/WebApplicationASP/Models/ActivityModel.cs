using System.ComponentModel.DataAnnotations;

namespace WebApplicationASP.Models
{
    public class Activity
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime ExpireDate { get; set; }
        public int Number { get; set; }
        public List<string> Category { get; set; } = [];
        public string Status { get; set; }
        public string ImageUrl { get; set; }
        public List<string> Member { get; set; }
        public string Host { get; set; }
    }
}