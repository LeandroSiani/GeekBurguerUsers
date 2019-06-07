using GeekBurguer.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeekBurguer.Users.Services
{
    public interface IFacialService : IHostedService
    {

        Guid GetFaceId(byte[] face);
    }
}
