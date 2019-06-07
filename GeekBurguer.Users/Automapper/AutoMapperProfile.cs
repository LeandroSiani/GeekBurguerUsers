using AutoMapper;
using GeekBurguer.Users.Contract;
using GeekBurguer.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeekBurguer.Users.Automapper
{
    public class AutomapperProfile : Profile
    {
        public AutomapperProfile()
        {
            CreateMap<User, UserToGet>();
            //CreateMap<Item, ItemToGet>();
        }
    }
}
