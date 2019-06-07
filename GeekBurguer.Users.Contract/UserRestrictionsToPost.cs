using System;
using System.Collections.Generic;
using System.Text;

namespace GeekBurguer.Users.Contract
{
    public class UserRestrictionsToPost
    {
        public Guid UserId { get; set; }
        public List<String> Restricoes { get; set; }
        public Guid RequesterId { get; set; }
        public string Others { get; set; }
    }
}
