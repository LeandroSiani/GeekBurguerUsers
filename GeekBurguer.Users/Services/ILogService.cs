using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeekBurguer.Users.Services
{
    public interface ILogService
    {
        void SendMessagesAsync(string message);
    }
}
