﻿using System;
using System.Collections.Generic;
using System.Text;

namespace GeekBurguer.Users.Contract
{
    public class UserretrievedMessage
    {
        public UserState State { get; set; }
        public UserToPost User { get; set; }
    }
}
