using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GeekBurguer.Users.Models
{
    public class UserRetrievedEvent
    {
        [Key]
        public Guid EventId { get; set; }

        //public UserState State { get; set; }

        //[ForeignKey("ProductId")]
        public User User { get; set; }

        public bool MessageSent { get; set; }
    }
}
