using System;
using System.Collections.Generic;
using System.Text;

namespace GeekBurguer.Users.Contract
{
    public class UserRetrievedMessage
    {

        public Guid? UserId { get; set; }
        public bool AreRestrictionsSet { get; set; }
    }
}
