using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeekBurguer.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace GeekBurguer.Users.Repository
{
    public class UsersRepository : IUsersRepository
    {

        private UsersDbContext _dbContext;

        public UsersRepository(UsersDbContext dbContext)
        {
            _dbContext = dbContext;
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
            _dbContext.SaveChanges();
        }

        public bool UpdateRestricoes(User user)
        {
            var userDb = GetUserById(user.Id);
            if (userDb != null)
            {
                userDb.Restricoes = user.Restricoes;
            }
            _dbContext.Entry(userDb).State = EntityState.Modified;            
            return true;
        }
    }
}
