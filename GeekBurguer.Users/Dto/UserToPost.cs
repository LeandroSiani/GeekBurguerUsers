using GeekBurguer.Users.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeekBurguer.Users.Dto
{
    
    public class UserToPost
    {
        [JsonConverter(typeof(Base64FileJsonConverter))]
        public string Face { get; set; }
    }
}
