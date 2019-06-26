using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using GeekBurguer.Users.Models;
using GeekBurguer.Users.Polly;
using GeekBurguer.Users.Services;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Registry;

namespace GeekBurguer.Users.Repository
{
    public class UsersRepository : IUsersRepository
    {
        private UsersDbContext _dbContext;
        public IUserRetrievedService _userRetrievedService;
        private readonly IReadOnlyPolicyRegistry<string> _policyRegistry;

        public UsersRepository(UsersDbContext dbContext, IUserRetrievedService userRetrievedService, IReadOnlyPolicyRegistry<string> policyRegistry)
        {
            _dbContext = dbContext;
            _userRetrievedService = userRetrievedService;
            _policyRegistry = policyRegistry;
        }

        public bool Add(User user)
        {
            _dbContext.Add(user);
            return true;
        }

        public User GetUserById(Guid? userId)
        {
            return _dbContext.Users?.FirstOrDefault(u => u.Id == userId);
        }

        public void Save()
        {
            SendMessage(false);

            _dbContext.SaveChanges();
        }

        public void SendMessage(bool exists)
        {

            if (exists)
                _userRetrievedService.AddToMessageListExits(_dbContext.ChangeTracker.Entries<User>());
            else
                _userRetrievedService.AddToMessageList(_dbContext.ChangeTracker.Entries<User>());
            
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
