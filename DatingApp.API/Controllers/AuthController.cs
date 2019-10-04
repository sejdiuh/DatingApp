using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _con;
        private readonly IMapper _mapper;

        public AuthController(IAuthRepository repo, IConfiguration con, IMapper mapper)
        {
            _con = con;
            _mapper = mapper;
            _repo = repo;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto model)
        {
            model.Username = model.Username.ToLower();

            if (await _repo.UserExists(model.Username))
                return BadRequest("Username alredy exists!");

            var user = new User { Username = model.Username };

            var result = await _repo.Register(user, model.Password);

            return StatusCode(201);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto model)
        {
            var userFromRepo = await _repo.Login(model.Username.ToLower(), model.Password);

            if (userFromRepo == null)
                return Unauthorized(); 

            var claims = new[]{
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_con.GetSection("AppSetings:Token").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor{
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            var user = _mapper.Map<UserForListDto>(userFromRepo);

            return Ok(new {
                token = tokenHandler.WriteToken(token),
                user = user
            });
        }
    }
}