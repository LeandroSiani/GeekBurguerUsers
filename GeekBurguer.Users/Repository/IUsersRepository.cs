using GeekBurguer.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeekBurguer.Users.Repository
{
    public interface IUsersRepository
    {
        User GetUserById(Guid? productId);
        bool Add(User product);
        bool UpdateRestricoes(User product);
        void Save();        
        void SendMessage(bool exists);
        
    }
}
