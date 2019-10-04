using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helper;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotoController : ControllerBase
    {
        public IDatingRepository _datingRepository { get; set; }
        public IMapper _mapper { get; set; }
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;

        public PhotoController(IDatingRepository datingRepository, IMapper mapper, IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _cloudinaryConfig = cloudinaryConfig;
            _mapper = mapper;
            _datingRepository = datingRepository;

            Account acc = new Account(
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
           ); 

           _cloudinary = new Cloudinary(acc);
        }

        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photFromRepo = await _datingRepository.GetPhoto(id);
            var photo = _mapper.Map<PhotoForReturnDto>(photFromRepo);

            return Ok(photo);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, [FromForm]PhotoForCreationDto photoForCreationDto)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)){
                return Unauthorized();
            }

            var userFromRepo = await _datingRepository.GetUser(userId);
            var file = photoForCreationDto.File;
            var uploadResult = new ImageUploadResult();

            if(file.Length > 0)
            {
                using(var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(5000).Crop("fill").Gravity("face")
                    };
                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }

            photoForCreationDto.Url = uploadResult.Uri.ToString();
            photoForCreationDto.PublicId = uploadResult.PublicId;
            
            var photo = _mapper.Map<Photo>(photoForCreationDto);
            
            
            if(!userFromRepo.Photos.Any(u => u.IsMain))
            { 
                photo.IsMain = true;
            }
            userFromRepo.Photos.Add(photo);
            if(await _datingRepository.SaveAll())
            {
                var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);
                return CreatedAtRoute("GetPhoto", new { id = photo.Id}, photoToReturn);
            }

            return BadRequest("Could not add the photo");
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id) 
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)){
                return Unauthorized();
            }

            var user = await _datingRepository.GetUser(userId);
            if(!user.Photos.Any(x => x.Id == id))
                return Unauthorized();

            var photoFromRepo = await _datingRepository.GetPhoto(id);
            if(photoFromRepo.IsMain)
                return BadRequest("This is alredy main photo");

            var currentMainPhoto = await _datingRepository.GetMainPhotoUserPhoto(userId);
            currentMainPhoto.IsMain = false;

            photoFromRepo.IsMain = true;

            if(await _datingRepository.SaveAll())
                return NoContent();
                
            return BadRequest("Could not set photo to main");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)){
                return Unauthorized();
            }

            var user = await _datingRepository.GetUser(userId);
            if(!user.Photos.Any(x => x.Id == id))
                return Unauthorized();

            var photoFromRepo = await _datingRepository.GetPhoto(id);
            if(photoFromRepo.IsMain)
                return BadRequest("You cannot delete your main photo");

            var deleteParams = new DeletionParams(photoFromRepo.PublicId);
            var result = _cloudinary.Destroy(deleteParams);

            if(result.Result == "ok")
            {
                _datingRepository.Delete(photoFromRepo);
            }

            if(await _datingRepository.SaveAll())
                return Ok();
            return BadRequest("Failed to delete photo");
        }
    }
}