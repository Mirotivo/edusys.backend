using System.ComponentModel.DataAnnotations;

namespace Backend.Database.Models
{
    public class Country
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(255)]
        public string Name { get; set; }
        [MaxLength(10)]
        public string Code { get; set; }

        public Country()
        {
            Name = string.Empty;
            Code = string.Empty;
        }
    }
}

