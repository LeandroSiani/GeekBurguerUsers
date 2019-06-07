using AutoMapper;
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
        public IMapper _mapper;

        public UsersController(IUsersRepository usersRepository, IFacialService facialService, IMapper mapper)
        {
            _usersRepository = usersRepository;
            _facialService = facialService;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<User> GetUserById(Guid id)
        {
            return Ok(new User() { Id = new Guid(), Restricoes = "teste" });
        }

        [HttpPost]
        public ActionResult Post([FromBody]UserToPost face)
        {
            byte[] image = face.Face;

            // verifica na api facila se tem a face eviada
            Guid? id;
            id = _facialService.GetFaceId(image);

            if (id == null)
            {
                return BadRequest("Esta imagem não contem uma face");
            }
            var user = _usersRepository.GetUserById(id);
            if (user == null)
            {
                user = new User() { Id = id, Face = image, Restricoes = null };
                _usersRepository.Add(user);
                _usersRepository.Save();
                return Created("users/" + user.Id, user);
            }
            return Ok(user);
        }

        [HttpPost("foodRestrictions")]
        public ActionResult Post([FromBody]UserRestrictionsToPost foodRestrictions)
        {
            var user = _usersRepository.GetUserById(foodRestrictions.UserId);

            var restricoes = "";
            foreach (var item in foodRestrictions.Restricoes)
            {
                restricoes += String.Concat(item, ",");
            }
            restricoes += ".";
            restricoes = restricoes.Replace(",.", "");

            user.Others = foodRestrictions.Others;
            user.Restricoes = restricoes;
            if (_usersRepository.UpdateRestricoes(user))
            {
                _usersRepository.Save();
                return Ok(user);
            }
            return NotFound();
        }
    }

}
