using GeekBurguer.Users.Models;
using GeekBurguer.Users.Repository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeekBurguer.Users.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IUsersRepository _usersRepository;

        public UsersController(IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        [HttpGet]
        public ActionResult<User> GetUserById(Guid id)
        {
            return Ok(new User() { Id = new Guid(), Restricoes = "teste"});
        }

        [HttpPost]
        public ActionResult CarregarFace(byte[] face)
        {
            return Ok();
        }
    }
}
