using System.ComponentModel.DataAnnotations;

namespace Demo.Entities
{
    public class LoginEntity
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Domain { get; set; } = "example.org";
        [Required]
        [DataType(DataType.Password)]
        public string Credentials { get; set; }
        public bool RememberMe { get; set; }
    }
}