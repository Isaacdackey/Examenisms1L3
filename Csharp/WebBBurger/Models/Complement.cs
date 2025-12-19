using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBBurger.Models
{
    [Table("complement")]
    public class Complement
    {
        [Key]
        [ForeignKey("Product")]
        public int Id { get; set; }

        [StringLength(20)]
        public string? TypeComplement { get; set; }

        [StringLength(20)]
        public string? Volume { get; set; }

        
        public required  Product Product { get; set; }
    }
}