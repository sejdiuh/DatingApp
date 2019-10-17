using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helper;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;
        public DatingRepository(DataContext context)
        {
            _context = context;

        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Like> GetLike(int userId, int recepientId)
        {
           return await _context.Likes.FirstOrDefaultAsync(x => x.LikerId == userId &&  x.LikeeId == recepientId);
        }

        public async Task<Photo> GetMainPhotoUserPhoto(int userId)
        {
            return await _context.Photos.Where(x => x.UserId == userId).FirstOrDefaultAsync(x => x.IsMain);
        }

        public async Task<Photo> GetPhoto(int id)
        {
           return await _context.Photos.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<User> GetUser(int id)
        {
           return await _context.Users.Include(x => x.Photos).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users = _context.Users.Include(x => x.Photos).OrderByDescending(x => x.LastActive).AsQueryable();

            users = users.Where(x => x.Id != userParams.UserId);

            users = users.Where(x => x.Gender == userParams.Gender);

            if(userParams.Likers)
            {    
                var userLiker  = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(x => userLiker.Contains(x.Id));
            }
            if(userParams.Likess)
            {
                var userLikees  = await GetUserLikes(userParams.UserId, userParams.Likess);
                users = users.Where(x => userLikees.Contains(x.Id));
            }

            if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDoB = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var maxDoB = DateTime.Today.AddYears(-userParams.MinAge);

                users = users.Where(x => x.DateOfBirth >= minDoB && x.DateOfBirth <= maxDoB);
            }

            if(!string.IsNullOrEmpty(userParams.OrderBy)) {

                switch (userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(x => x.Created);
                        break;
                    default: 
                        users = users.OrderByDescending(x => x.Created);
                        break;
                }
            }

            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }   

        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            var user = await _context.Users.Include(x => x.Likers).Include(x => x.Likees).FirstOrDefaultAsync(x => x.Id == id);

            if(likers)
            {
                return user.Likers.Where(x => x.LikeeId == id).Select(x => x.LikerId);
            }
            else
            {
                return user.Likees.Where(x => x.LikeeId == id).Select(x => x.LikeeId);
            }
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}