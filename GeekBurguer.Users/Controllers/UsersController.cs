using AutoMapper;
using GeekBurguer.Users.Contract;
using GeekBurguer.Users.Models;
using GeekBurguer.Users.Polly;
using GeekBurguer.Users.Repository;
using GeekBurguer.Users.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using System;
using System.Net;
using System.Threading.Tasks;
//using GeekBurguer.Users.Extensions;

namespace GeekBurguer.Users.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IUsersRepository _usersRepository;
        private IFacialService _facialService;
        public IMapper _mapper;
        private readonly IUserRetrievedService _userRetrievedService;
        private ILogService _logService;
        private readonly IReadOnlyPolicyRegistry<string> _policyRegistry;
        public string _baseUri;
        public IConfiguration _configuration;
        private readonly ILogger _logger;
        public string AddUserEndpoint;

        public UsersController(IUsersRepository usersRepository, IFacialService facialService, IMapper mapper,
            IUserRetrievedService userRetrievedService, ILogService logService, IReadOnlyPolicyRegistry<string> policyRegistry, IConfiguration configuration, ILogger<UsersController> logger)
        {
            _usersRepository = usersRepository;
            _facialService = facialService;
            _mapper = mapper;
            _userRetrievedService = userRetrievedService;
            _logService = logService;
            _policyRegistry = policyRegistry;
            _configuration = configuration;
            _baseUri = _configuration.GetSection("BaseUri").Value;
            _logger = logger;
            AddUserEndpoint = _configuration.GetSection("AddUserEndpoint").Value;
        }

        /// <summary>
        /// Busca a face enviada na API Facial da Azure e insere o usuário no banco 
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/users
        ///
        /// </remarks>
        /// <returns></returns>
        /// <response code="200">Retorna sucesso "Processando"</response>
        /// <response code="400">Regras de negócios inválidas ou solicitação mal formatada</response>   
        /// <response code="500">Erro do Servidor Interno</response>   
        /// <response code="401">Não autorizado</response>
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [HttpPost]
        public IActionResult Post([FromBody]UserToPost userPost)
        {
            AddUserAsync(userPost);
            return Ok("Processando");
        }

        private Policy GetPolicy()
        {
            return Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, retryCount => TimeSpan.FromSeconds(Math.Pow(2, retryCount)), (result, timeSpan, retryCount, context) =>
                {
                    _logger.LogError(result, "Ocorreu um erro na tentativa {RetryAttempt} para a politica {PolicyKey}", retryCount, context.PolicyKey);
                }).WithPolicyKey(PolicyNames.BasicRetry);
        }

        private async Task AddUserAsync(UserToPost userToPost)
        {
            var face = userToPost.Face;
            var policy = GetPolicy();
            Guid? id = policy.ExecuteAsync(async () => _facialService.GetFaceId(userToPost.Face)).Result;

            //Guid? id = _facialService.GetFaceId(face);

            if (id != null)
            {
                var user = _usersRepository.GetUserById(id);
                if (user == null)
                {
                    user = new User() { Id = id, Face = face, Restricoes = null };

                    await policy.ExecuteAsync(async () =>
                    {
                        _usersRepository.Add(user);
                        _usersRepository.Save();
                    });
                    _logService.SendMessagesAsync($"{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day} {DateTime.Now.Hour} {DateTime.Now.Minute} USER - usuário '{user.Id}' criado com sucesso!");
                    //return Created("users/" + user.Id, user);
                }
                else
                {
                    await policy.ExecuteAsync(async () => _usersRepository.SendMessage(true));
                }
            }
            else
            {
                //"Esta imagem não contem uma face"
                await policy.ExecuteAsync(async () => _logService.SendMessagesAsync($"{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day} {DateTime.Now.Hour} {DateTime.Now.Minute} USER - Esta imagem não contem uma face"));
            }
        }

        /// <summary>
        /// Insere restrições alimentares do usuário
        /// </summary> 
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/users/foodRestrictions
        ///
        /// </remarks>
        /// <returns></returns>
        /// <response code="200">Retorna sucesso e insere as restrições alimentares do usuário</response>
        /// <response code="400">Regras de negócios inválidas ou solicitação mal formatada</response>   
        /// <response code="500">Erro do Servidor Interno</response>   
        /// <response code="401">Não autorizado</response>
        [HttpPost]
        [Route("foodRestrictions")]
        public async Task<IActionResult> Post([FromBody]UserRestrictionsToPost foodRestrictions)
        {
            if (ModelState.IsValid)
            {
                GravarRestricoes(foodRestrictions);
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
        private async void GravarRestricoes(UserRestrictionsToPost foodRestrictions)
        {
            var policy = GetPolicy();
            var user = _usersRepository.GetUserById(foodRestrictions.UserId);

            if (user == null)
            {
                await policy.ExecuteAsync(async () => _logService.SendMessagesAsync($"{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day} {DateTime.Now.Hour} {DateTime.Now.Minute} - usuário não existe"));
                return;
            }

            var restricoes = "";
            foreach (var item in foodRestrictions.Restrictions)
                restricoes += String.Concat(item, ",");

            restricoes += ".";
            restricoes = restricoes.Replace(",.", "");

            user.Others = foodRestrictions.Others;
            user.Restricoes = restricoes;

            if (_usersRepository.UpdateRestricoes(user))
            {
                await policy.ExecuteAsync(async () =>
                {
                    _usersRepository.Save();
                    _logService.SendMessagesAsync($"{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day} {DateTime.Now.Hour} {DateTime.Now.Minute} RESTRIÇÕES - usuário '{user.Id}' gerado restrições com sucesso!");
                });
            }
            else
            {
                await policy.ExecuteAsync(async () =>
                {
                    _logService.SendMessagesAsync($"{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day} {DateTime.Now.Hour} {DateTime.Now.Minute} RESTRIÇÕES - usuário '{user.Id}' não localizado");
                });
            }
        }
    }
}
