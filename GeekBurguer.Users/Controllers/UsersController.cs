using GeekBurguer.Users.Contract;
using GeekBurguer.Users.Models;
using GeekBurguer.Users.Repository;
using GeekBurguer.Users.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeekBurguer.Users.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IUsersRepository _usersRepository;
        private IFacialService _facialService;

        public UsersController(IUsersRepository usersRepository, IFacialService facialService)
        {
            _usersRepository = usersRepository;
            _facialService = facialService;
        }

        [HttpGet]
        public ActionResult<User> GetUserById(Guid id)
        {
            return Ok(new User() { Id = new Guid(), Restricoes = "teste"});
        }

        [HttpPost]
        public ActionResult Post([FromBody]UserToPost face)
        {
            byte[] image =  face.Face;

            // verifica na api facila se tem a face eviada
            var id = _facialService.GetFaceId(image);

            if (id == null)
            {
                return Ok(new { msg = "Não existe face nessa imagem" });
            }

            // com o retorno da api facial busca as restrições do suuario se retornou id

            // se tem restricoes
            if (true)
            {
                return Ok(new UserToGet() { Id = id, Restricoes = new List<string>() });
            }
            else
            {
                return Ok();
            }
        }
    }

}
