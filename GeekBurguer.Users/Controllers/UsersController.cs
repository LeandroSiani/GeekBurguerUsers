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
        private IUserRetrievedService _userRetrievedService;

        public UsersController(IUsersRepository usersRepository, IFacialService facialService, IMapper mapper, IUserRetrievedService userRetrievedService)
        {
            _usersRepository = usersRepository;
            _facialService = facialService;
            _mapper = mapper;
            _userRetrievedService = userRetrievedService;
        }

        [HttpGet]
        public ActionResult<User> GetUserById(Guid id)
        {
            return Ok(new User() { Id = new Guid(), Restricoes = "teste" });
        }

        [HttpPost]
        public ActionResult Post([FromBody]UserToPost userPost)
        {
            _userRetrievedService.AddUser(userPost, _facialService, _usersRepository);
            return Ok("Processando");
        }

        [HttpPost("/foodRestrictions")]
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
