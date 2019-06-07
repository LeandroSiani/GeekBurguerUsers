using GeekBurguer.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeekBurguer.Users.Repository
{
    public interface IUserRetrievedEventRepository
    {
        UserRetrievedEvent Get(Guid eventId);
        bool Add(UserRetrievedEvent productChangedEvent);
        bool Update(UserRetrievedEvent productChangedEvent);
        void Save();
    }
}
