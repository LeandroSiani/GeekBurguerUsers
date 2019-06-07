using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GeekBurguer.Users.Contract;
using GeekBurguer.Users.Repository;
using GeekBurguer.Users.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;

namespace GeekBurguer.Users.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestrictionsController : Controller
    {
        //public IFacialService _facialService;
        public IUsersRepository _usersRepository;

        public RestrictionsController(/*IFacialService facialService,*/ IUsersRepository usersRepository)
        {
            //_facialService = facialService;
            _usersRepository = usersRepository;
        }

        // GET api/values
        //[HttpGet]
        //public ActionResult<IEnumerable<string>> Get([FromQuery(Name = "image")] string imageByte)
        //{
        //    return new string[] { "value1", "value2" };
        //}

        // POST api/values

        [HttpPost]
        public ActionResult Post([FromBody]UserRestrictionsToPost foodRestrictions)
        {

            // verifica na api facila se tem a face eviada
            var user = _usersRepository.GetUserById(foodRestrictions.UserId);
            if (user != null)
            {
                _usersRepository.Save();
                return Ok(user);
            }
            return BadRequest();
        }

    }
}