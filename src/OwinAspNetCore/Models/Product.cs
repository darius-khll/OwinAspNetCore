using System.ComponentModel.DataAnnotations;

namespace OwinAspNetCore.Models
{
    public class Product
    {
        [Key]
        public virtual int Id { get; set; }

        public virtual string Name { get; set; }

        public virtual decimal Price { get; set; }
    }
}
