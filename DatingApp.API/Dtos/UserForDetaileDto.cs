using System;
using System.Collections.Generic;
using DatingApp.API.Models;

namespace DatingApp.API.Dtos
{
    public class UserForDetaileDto
    {
        
        public int Id { get; set; }
        public string  Username { get; set; } 
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string Gender { get; set; }
        public int Age  { get; set; }   
        public string KnownAs  { get; set; }  
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }
        public string Instruction { get; set; }
        public string  LokingFor { get; set; }
        public string  Interests { get; set; }
        public string  City { get; set; }
        public string  Country { get; set; }
        public string PhotoUrl { get; set; } 
        public ICollection<PhotoDetailedDto> Photos { get; set; }
    }
}