using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        public static IConfiguration Configuration;
        public static FaceServiceClient faceServiceClient;
        public static Guid FaceListId;
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get([FromQuery(Name = "image")] string imageByte)
        {
            return new string[] { "value1", "value2" };
        }

        // POST api/values
        [HttpPost]
        public void Post([FromQuery(Name = "image")] string imageBase64)
        {
            byte[] image = System.IO.File.ReadAllBytes("C:\\Users\\micael.machado\\Downloads\\paola2.jpg");

            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            FaceListId = Guid.Empty;

            faceServiceClient = new FaceServiceClient(Configuration["FaceAPIKey"], "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/");

            while (true)
            {
                try
                {
                    var containsAnyFaceOnList = UpsertFaceListAndCheckIfContainsFaceAsync().Result;
                    //Detecta a quantidade de faces na imagem
                    var face = DetectFaceAsync(image).Result;
                    if (face != null)
                    {
                        Guid? persistedId = null;
                        if (containsAnyFaceOnList)
                            persistedId = FindSimilarAsync(face.FaceId, FaceListId).Result;

                        //Se nao achou nada semelhante na lista, adiciona a face atual
                        if (persistedId == null)
                        {
                            persistedId = AddFaceAsync(FaceListId, image).Result;
                            Console.WriteLine($"New User with FaceId {persistedId}");
                        }
                        else
                        {
                            //TO-DO
                            //Implementar a busca dos ingredientes na nossa base a partir do persistedid
                            Console.WriteLine($"Face Exists with Face {persistedId}");
                        }
                    }
                    else
                    {
                        //TO-DO
                        //Retornar um erro falando que nenhuma face foi identificada na imagem
                        Console.WriteLine("Not a face!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Probably Rate Limit for API was reached, please try again later");
                }
            }
        }

        private static async Task<bool> UpsertFaceListAndCheckIfContainsFaceAsync()
        {
            var faceListId = FaceListId.ToString();
            var faceLists = await faceServiceClient.ListFaceListsAsync();
            var faceList = faceLists.FirstOrDefault(_ => _.FaceListId == FaceListId.ToString());

            if (faceList == null)
            {
                //caso nao encontre a lista de faces, cria
                await faceServiceClient.CreateFaceListAsync(faceListId, "GeekBurgerFaces", null);
                return false;
            }

            //busca as faces na lista
            var faceListJustCreated = await faceServiceClient.GetFaceListAsync(faceListId);

            return faceListJustCreated.PersistedFaces.Any();
        }
        //Busca a semelhança na face atual com as faces da lista e retorna o 'ID da semelhança' > persistedFaceId
        private static async Task<Guid?> FindSimilarAsync(Guid faceId, Guid faceListId)
        {
            var similarFaces = await faceServiceClient.FindSimilarAsync(faceId, faceListId.ToString());

            var similarFace = similarFaces.FirstOrDefault(_ => _.Confidence > 0.5);

            return similarFace?.PersistedFaceId;
        }

        private static async Task<Face> DetectFaceAsync(byte[] image)
        {
            try
            {
                using (Stream imageFileStream = new MemoryStream(image))
                {
                    var faces = await faceServiceClient.DetectAsync(imageFileStream);
                    return faces.FirstOrDefault();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static async Task<Guid?> AddFaceAsync(Guid faceListId, byte[] image)
        {
            try
            {
                AddPersistedFaceResult faceResult;
                using (Stream imageFileStream = new MemoryStream(image))
                {
                    faceResult = await faceServiceClient.AddFaceToFaceListAsync(faceListId.ToString(), imageFileStream);
                    return faceResult.PersistedFaceId;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Face not included in Face List! \n Erro: " + ex);
                return null;
            }
        }
    }
}