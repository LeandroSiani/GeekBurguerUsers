using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using Microsoft.Azure.ServiceBus;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using GeekBurguer.Users.Models;
using GeekBurguer.Users.Repository;
using GeekBurguer.Users.Contract;

namespace GeekBurguer.Users.Services
{
    public interface IUserRetrievedService : IHostedService
    {
        void SendMessagesAsync();
        void AddToMessageList(IEnumerable<EntityEntry<User>> changes);
        void AddToMessageListExits(IEnumerable<EntityEntry<User>> changes);
    }
}