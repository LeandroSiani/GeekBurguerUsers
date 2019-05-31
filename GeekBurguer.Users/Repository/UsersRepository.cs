using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeekBurguer.Users.Models;

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

        public User GetUserById(Guid userId)
        {
            return _dbContext.Users?.FirstOrDefault(u => u.Id == userId );
        }

        public void Save()
        {
            _dbContext.SaveChanges();
        }

        public bool Update(User user)
        {
            throw new NotImplementedException();
        }
    }
}
