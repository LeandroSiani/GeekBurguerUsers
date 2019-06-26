using AutoMapper;
using GeekBurguer.Users.Contract;
using GeekBurguer.Users.Models;
using GeekBurguer.Users.Polly;
using GeekBurguer.Users.Repository;
using GeekBurguer.Users.Services;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Registry;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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

		[HttpGet]
		public ActionResult<User> GetUserById(Guid id)
		{
			return Ok(new User() { Id = new Guid(), Restricoes = "teste" });
		}

		[HttpPost]
		public IActionResult Post([FromBody]UserToPost userPost)
		//public async Task<IActionResult> Post([FromBody]UserToPost userPost)
		{
			AddUserAsync(userPost);
			return Ok("Processando");
			/*			// tenta chamar a api de reconhecimento com o polly
			var response = PostToAPI(userPost).Result;

			if (response.IsSuccessStatusCode)
			{
				AddUser(JsonConvert.DeserializeObject<UserToPost>(await response.Content.ReadAsStringAsync()));
				return Ok("Processando");
			}
			else
			{
				return BadRequest("Ocorreu um erro com a sua requisição");
			}
			*/
		}

        private Policy CheckApiFaces()
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
            var policy = CheckApiFaces();
            Guid? id = policy.ExecuteAsync(async () => _facialService.GetFaceId(userToPost.Face)).Result;            

            //Guid? id = _facialService.GetFaceId(face);

            if (id != null)
			{
				var user = _usersRepository.GetUserById(id);
				if (user == null)
				{
					user = new User() { Id = id, Face = face, Restricoes = null };
					
                    await policy.ExecuteAsync(async () => {
                        _usersRepository.Add(user);
                        _usersRepository.Save();                        
                    });
                    await policy.ExecuteAsync(async () => _logService.SendMessagesAsync($"{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day} {DateTime.Now.Hour} {DateTime.Now.Minute} USER - Esta imagem não contem uma face"));
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

		[HttpPost("foodRestrictions")]
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

		private async Task<HttpResponseMessage> PostRestricoes(UserRestrictionsToPost foodRestrictions)
		{
			var client = new HttpClient();
			var byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(foodRestrictions));

			var content = new ByteArrayContent(byteData);

			content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			var retryPolicy = _policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>(PolicyNames.BasicRetry)
							  ?? Policy.NoOpAsync<HttpResponseMessage>();

			//var retries = 0;
			//// ReSharper disable once AccessToDisposedClosure
			//var response = await retryPolicy.ExecuteAsync((ctx) =>
			//{
			//	client.DefaultRequestHeaders.Remove("retries");
			//	client.DefaultRequestHeaders.Add("retries", new[] { retries++.ToString() });

			//	var baseUrl = _baseUri;
			//	if (string.IsNullOrWhiteSpace(baseUrl))
			//	{
			//		var uri = Request.GetUri();
			//		baseUrl = uri.Scheme + Uri.SchemeDelimiter + uri.Host + ":" + uri.Port;
			//	}

			//	//var isValid = Uri.IsWellFormedUriString(apiUrl, UriKind.Absolute);
			//	//return client.PostAsync(isValid ? $"{baseUrl}{apiUrl}" : $"{baseUrl}/api/Face", content);
			//	return client.PostAsync($"{baseUrl}/api/foodRestrictions", content);
			//}, context);

			var context = new Context($"foodRestrictions-{Guid.NewGuid()}", new Dictionary<string, object>
				{
					{ PolicyContextItems.Logger, _logger }, { "url", Request.GetUri() }
				});

			var retries = 0;
			var response = await retryPolicy.ExecuteAsync((ctx) => 
			{
				client.DefaultRequestHeaders.Remove("retries");
				client.DefaultRequestHeaders.Add("retries", new[] { retries++.ToString() });

				var baseUrl = _baseUri;
				if (string.IsNullOrWhiteSpace(baseUrl))
				{
					var uri = Request.GetUri();
					baseUrl = uri.Scheme + Uri.SchemeDelimiter + uri.Host + ":" + uri.Port;
				}

				return client.PostAsync($"{baseUrl}/users/foodRestrictions", content);
			}, context);
			

			content.Dispose();

			return response;
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

		//private async Task<HttpResponseMessage> PostToAPI(UserToPost userPost)
		//{
		//	// cria o byte data pra enviar o objeto
		//	var client = new HttpClient();
		//	var byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(userPost));

		//	var content = new ByteArrayContent(byteData);

		//	content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

		//	var retryPolicy = _policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>(PolicyNames.BasicRetry)
		//					 ?? Policy.NoOpAsync<HttpResponseMessage>();

		//	var context = new Context($"GetSomeData-{Guid.NewGuid()}", new Dictionary<string, object>
		//		{
		//			{ PolicyContextItems.Logger, _logger }, { "url", _baseUri }
		//		});

		//	var retries = 0;
		//	var response = await retryPolicy.ExecuteAsync((ctx) =>
		//	{
		//		client.DefaultRequestHeaders.Remove("retries");
		//		client.DefaultRequestHeaders.Add("retries", new[] { retries++.ToString() });

		//		var baseUrl = _baseUri;
		//		if (string.IsNullOrWhiteSpace(baseUrl))
		//		{
		//			var uri = Request.GetUri();
		//			baseUrl = uri.Scheme + Uri.SchemeDelimiter + uri.Host + ":" + uri.Port;
		//		}

		//		var isValid = Uri.IsWellFormedUriString(baseUrl, UriKind.Absolute);

		//		return client.PostAsync(isValid ? $"{baseUrl}{AddUserEndpoint}" : $"{baseUrl}/api/user", content);
		//	}, context);

		//	return response;
		//}
	}
}
