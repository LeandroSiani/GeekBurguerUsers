using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeekBurguer.Users.Models
{
    public class User
    {
        public Guid? Id { get; set; }
        public Byte[] Face { get; set; }
        public string Restricoes { get; set; }
    }

}
