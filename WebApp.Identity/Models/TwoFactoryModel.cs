using System.ComponentModel.DataAnnotations;

namespace WebApp.Identity.Models
{
    public class TwoFactoryModel
    {
        [Required]
        public string Token { get; set; }
    }
}
