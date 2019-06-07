using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeekBurguer.Users.Models;
using GeekBurguer.Users.Services;
using Microsoft.EntityFrameworkCore;

namespace GeekBurguer.Users.Repository
{
    public class UsersRepository : IUsersRepository
    {

        private UsersDbContext _dbContext;
        public IUserRetrievedService _userRetrievedService;

        public UsersRepository(UsersDbContext dbContext, IUserRetrievedService userRetrievedService)
        {
            _dbContext = dbContext;
            _userRetrievedService = userRetrievedService;
        }

        public bool Add(User user)
        {
            _dbContext.Add(user);
            return true;
        }

        public User GetUserById(Guid? userId)
        {
            return _dbContext.Users?.FirstOrDefault(u => u.Id == userId );
        }

        public void Save()
        {
            _userRetrievedService.AddToMessageList(_dbContext.ChangeTracker.Entries<User>());

            _dbContext.SaveChanges();

            _productChangedService.SendMessagesAsync();
        }

        public bool UpdateRestricoes(User user)
        {
            var userDb = GetUserById(user.Id);
            if (userDb != null)
            {
                userDb.Restricoes = user.Restricoes;
                userDb.Others = user.Others;
                _dbContext.Entry(userDb).State = EntityState.Modified;
                return true;
            }
            
            return false;
        }
    }
}
