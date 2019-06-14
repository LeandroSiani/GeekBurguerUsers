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
        private ILogService _logService;

        public UsersController(IUsersRepository usersRepository, IFacialService facialService, IMapper mapper,
            IUserRetrievedService userRetrievedService, ILogService logService)
        {
            _usersRepository = usersRepository;
            _facialService = facialService;
            _mapper = mapper;
            _userRetrievedService = userRetrievedService;
            _logService = logService;
        }

        [HttpGet]
        public ActionResult<User> GetUserById(Guid id)
        {
            return Ok(new User() { Id = new Guid(), Restricoes = "teste" });
        }

        [HttpPost]
        public ActionResult Post([FromBody]UserToPost userPost)
        {
            AddUserAsync(userPost);
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

        private async void AddUserAsync(UserToPost userPost)
        {
            var face = Encoding.ASCII.GetBytes(userPost.Face);
            Guid? id = _facialService.GetFaceId(face);
            if (id == null)
            {
                //"Esta imagem não contem uma face"
               // _logService.SendMessagesAsync($"{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day} {DateTime.Now.Hour} {DateTime.Now.Minute} USER - Esta imagem não contem uma face");
            }

            var user = _usersRepository.GetUserById(id);
            if (user == null)
            {

                user = new User() { Id = id, Face = face, Restricoes = null };
                _usersRepository.Add(user);
                _usersRepository.Save();
                //_logService.SendMessagesAsync($"{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day} {DateTime.Now.Hour} {DateTime.Now.Minute} User was created/update");
                //return Created("users/" + user.Id, user);
            }
        }
    }

}
