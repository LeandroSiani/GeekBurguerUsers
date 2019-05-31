using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GeekBurguer.Users.Contract;
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
        public IFacialService _facialService;

        public RestrictionsController(IFacialService facialService)
        {
            _facialService = facialService;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get([FromQuery(Name = "image")] string imageByte)
        {
            return new string[] { "value1", "value2" };
        }

        // POST api/values
        [HttpPost]
        public IActionResult Post([FromQuery(Name = "image")] byte[] face)
        {
            // verifica na api facila se tem a face eviada
            var id = _facialService.GetFaceId(face);

            if (id == null)
            {
                return Ok(new { msg = "Não existe face nessa imagem" });
            }

            // com o retorno da api facial busca as restrições do suuario se retornou id
            return Ok();
        }
    }
}