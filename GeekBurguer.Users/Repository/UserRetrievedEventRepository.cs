using GeekBurguer.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeekBurguer.Users.Repository
{
    public class UserRetrievedEventRepository : IUserRetrievedEventRepository
    {
        private readonly UsersDbContext _dbContext;

        public UserRetrievedEventRepository(UsersDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public UserRetrievedEvent Get(Guid eventId)
        {
            throw new NotImplementedException();

            //return _dbContext.UserRetrievedEvents
            //    .FirstOrDefault(product => product.EventId == eventId);
        }

        public bool Add(UserRetrievedEvent productChangedEvent)
        {
            //productChangedEvent.User =
            //    _dbContext.Users
            //    .FirstOrDefault(_ => _.Id == productChangedEvent.User.Id);

            //productChangedEvent.EventId = Guid.NewGuid();

            //_dbContext.UserRetrievedEvents.Add(productChangedEvent);

            return true;
        }


        public bool Update(UserRetrievedEvent productChangedEvent)
        {
            return true;
        }

        public void Save()
        {
            _dbContext.SaveChanges();
        }
    }
}
