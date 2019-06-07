using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace GeekBurguer.Users.Contract
{
    public class UserToPost
    {
        public Byte[] Face { get; set; }

        public Guid RequesterId { get; set; }
    }
}
