using AutoMapper;
using GeekBurguer.Users.Contract;
using GeekBurguer.Users.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
            CreateMap<User, UserToPost>();
            CreateMap<EntityEntry<User>, UserRetrievedMessage>().ForMember(dest => dest.User, opt => opt.MapFrom(src => src.Entity));
            CreateMap<EntityEntry<User>, UserRetrievedEvent>().ForMember(dest => dest.User, opt => opt.MapFrom(src => src.Entity));
            //CreateMap<Item, ItemToGet>();
        }
    }
}
