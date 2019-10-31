using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helper;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/users/{userId}/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IDatingRepository _datingRepository;
        private readonly IMapper _mapper;
        public MessagesController(IDatingRepository datingRepository, IMapper mapper)
        {
            _mapper = mapper;
            _datingRepository = datingRepository;

        }

        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage(int userId, int id)
        {
            if(id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) // check if id its the same id (from token) with logged user
                return Unauthorized();
            
            var messageFromRepo = await _datingRepository.GetMessage(id);

            if(messageFromRepo == null)
                return NotFound();

            return Ok(messageFromRepo);
        }

        [HttpGet]
        public async Task<IActionResult> GetMessagesForUser(int userId, [FromQuery]MessageParams messageParams)
        {
            if(userId  != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) // check if id its the same id (from token) with logged user
                return Unauthorized();
            
            messageParams.UserId = userId;

            var messageFromRepo = await _datingRepository.GetMessagesForUser(messageParams);

            var message = _mapper.Map<IEnumerable<MessageToReturn>>(messageFromRepo);
            Response.AddPagination(messageFromRepo.CurrentPage, messageFromRepo.PageSize, messageFromRepo.TotalCount, messageFromRepo.TotalPages);
            
            return Ok(messageFromRepo);
        }

        [HttpGet("thread/{recipientId}")]
        public async Task<IActionResult> GetMessageTherad(int userId, int recipientId)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) // check if id its the same id (from token) with logged user
                return Unauthorized();
            
            var messageFromRepo = await _datingRepository.GetMessageThread(userId, recipientId);
            var messageThread = _mapper.Map<IEnumerable<MessageToReturn>>(messageFromRepo);

            return Ok(messageThread);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, MessageFroCreationDto messageFroCreationDto)
        {
            var sender = await _datingRepository.GetUser(userId);

            if(sender.Id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) // check if id its the same id (from token) with logged user
                return Unauthorized();

            messageFroCreationDto.SenderId = userId;

            var recipient = await _datingRepository.GetUser(messageFroCreationDto.RecipientId);
            if(recipient == null)
                return BadRequest("Could not find user");

            var message = _mapper.Map<Message>(messageFroCreationDto);

            _datingRepository.Add(message);

            if(await _datingRepository.SaveAll())
            {
                var messageToReturn = _mapper.Map<MessageToReturn>(message);
                return CreatedAtRoute("GetMessage", new { id = message.Id }, messageToReturn);
            }
            throw new Exception("Creating message failed on save");
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteMessage(int id, int userId)
        {
            if(userId!= int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) // check if id its the same id (from token) with logged user
                return Unauthorized();

            var messsageFromRepo = await _datingRepository.GetMessage(id);

            if (messsageFromRepo.SenderId == userId)
                messsageFromRepo.SenderDeleted = true;

            if (messsageFromRepo.RecipientId == userId)
                messsageFromRepo.RecipientDeleted = true;

            if(messsageFromRepo.SenderDeleted && messsageFromRepo.RecipientDeleted)
                _datingRepository.Delete(messsageFromRepo);
            
            if(await _datingRepository.SaveAll())
                return NoContent();
            
            throw new Exception("Error deleting the message");
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkMessageAsRead(int userId, int id)
        {
            if(userId!= int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) // check if id its the same id (from token) with logged user
                return Unauthorized();
            var message = await _datingRepository.GetMessage(id);

            if(message.RecipientId != userId)
                return Unauthorized();

            message.IsRead = true;
            message.DateRead = DateTime.Now;

            await _datingRepository.SaveAll();
            return NoContent();
        }
    }
}