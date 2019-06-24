using AutoMapper;
using GeekBurguer.Users.Contract;
using GeekBurguer.Users.Models;
using GeekBurguer.Users.Polly;
using GeekBurguer.Users.Repository;
using GeekBurguer.Users.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Registry;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using GeekBurguer.Users.Extensions;
using Microsoft.AspNetCore.Http;

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
        private readonly IReadOnlyPolicyRegistry<string> _policyRegistry;
		public string _baseUri;
		public IConfiguration _configuration;
		private readonly ILogger _logger;
		public string AddUserEndpoint;

		public UsersController(IUsersRepository usersRepository, IFacialService facialService, IMapper mapper,
            IUserRetrievedService userRetrievedService, ILogService logService, IReadOnlyPolicyRegistry<string> policyRegistry, IConfiguration configuration, ILogger logger)
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

        [HttpGet]
        public ActionResult<User> GetUserById(Guid id)
        {
            return Ok(new User() { Id = new Guid(), Restricoes = "teste" });
        }

        [HttpPost]
        public ActionResult Post([FromBody]UserToPost userPost)
        {
            AddUser(userPost);
            return Ok("Processando");
        }

        [HttpPost("foodRestrictions")]
        public ActionResult Post([FromBody]UserRestrictionsToPost foodRestrictions)
        {
            GravarRestricoes(foodRestrictions);
            return Ok("Processando");

        }

        private async void GravarRestricoes(UserRestrictionsToPost foodRestrictions)
        {
            var user = _usersRepository.GetUserById(foodRestrictions.UserId);

            var restricoes = "";
            foreach (var item in foodRestrictions.Restrictions)
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
                _logService.SendMessagesAsync($"{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day} {DateTime.Now.Hour} {DateTime.Now.Minute} RESTRIÇÕES - usuário '{user.Id}' gerado restrições com sucesso!");
            }
            else
            {
                _logService.SendMessagesAsync($"{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day} {DateTime.Now.Hour} {DateTime.Now.Minute} RESTRIÇÕES - usuário '{user.Id}' não localizado");
            }
        }

        private async void AddUser(UserToPost userPost)
        {
			// cria o byte data pra enviar o objeto
			var client = new HttpClient();
			var byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(userPost));

			var content = new ByteArrayContent(byteData);

			content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			var retryPolicy = _policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>(PolicyNames.BasicRetry)
							 ?? Policy.NoOpAsync<HttpResponseMessage>();

			var context = new Context($"GetSomeData-{Guid.NewGuid()}", new Dictionary<string, object>
				{
					{ PolicyContextItems.Logger, _logger }, { "url", _baseUri }
				});

			var retries = 0;
			var response = await retryPolicy.ExecuteAsync((ctx) =>
			{
				client.DefaultRequestHeaders.Remove("retries");
				client.DefaultRequestHeaders.Add("retries", new[] { retries++.ToString() });

				var baseUrl = _baseUri;
				if (string.IsNullOrWhiteSpace(baseUrl))
				{
					var uri = GetUri();
					baseUrl = uri.Scheme + Uri.SchemeDelimiter + uri.Host + ":" + uri.Port;
				}

				var isValid = Uri.IsWellFormedUriString(baseUrl, UriKind.Absolute);

				return client.PostAsync(isValid ? $"{baseUrl}{AddUserEndpoint}" : $"{baseUrl}/api/user", content);
			}, context);

			var face = userPost.Face;

           Guid? id = _facialService.GetFaceId(face);

            if (id != null)
            {
                var user = _usersRepository.GetUserById(id);
                if (user == null)
                {
                    user = new User() { Id = id, Face = face, Restricoes = null };
                    _usersRepository.Add(user);
                    _usersRepository.Save();
                    _logService.SendMessagesAsync($"{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day} {DateTime.Now.Hour} {DateTime.Now.Minute} USER - usuário '{user.Id}' criado com sucesso!");
                    //return Created("users/" + user.Id, user);
                }
                else
                {
                    _usersRepository.SendMessage(true);
                }
            }
            else
            {
                //"Esta imagem não contem uma face"
                _logService.SendMessagesAsync($"{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day} {DateTime.Now.Hour} {DateTime.Now.Minute} USER - Esta imagem não contem uma face");
            }
        }

		private static Uri GetUri()
		{
			var request = Request;
			var builder = new UriBuilder();
			builder.Scheme = request.Scheme;
			builder.Host = request.Host.Value;
			builder.Path = request.Path;
			builder.Query = request.QueryString.ToUriComponent();
			return builder.Uri;
		}
	}

}
