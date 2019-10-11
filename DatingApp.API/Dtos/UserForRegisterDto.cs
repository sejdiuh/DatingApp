using System;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dtos
{
    public class UserForRegisterDto
    {

        public UserForRegisterDto()
        {
            Created = DateTime.Now;
            LastActive = DateTime.Now;
        }
        [Required]
        public string Username { get; set; }     

        [Required]
        [StringLength(8, MinimumLength = 4, ErrorMessage= "Password must be beetwen 4 and 8 characters")]
        public string Password { get; set; }     
        [Required]
        public string Gender { get; set; }
        public string KnownAS { get; set; }
        public DateTime DateOfBirth { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Country { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }


    }
}