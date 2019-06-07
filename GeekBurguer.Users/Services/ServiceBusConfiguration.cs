using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeekBurguer.Users.Services
{
    namespace GeekBurger.Products.Service
    {
        public class ServiceBusConfiguration
        {
            public string ConnectionString { get; set; }
            public string ResourceGroup { get; set; }
            public string NamespaceName { get; set; }
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
            public string SubscriptionId { get; set; }
            public string TenantId { get; set; }
        }
    }
}
