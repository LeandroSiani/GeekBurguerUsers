using Microsoft.Extensions.Configuration;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GeekBurguer.Users.Services
{
    public class FacialService : IFacialService
    {
        public IConfiguration Configuration { get; set; }
        public static FaceServiceClient faceServiceClient;
        public static Guid FaceListId;

        public FacialService(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public Guid? GetFaceId(byte[] face)
        {
            byte[] image = System.IO.File.ReadAllBytes("D:\\nicolas.jpg");

            FaceListId = Guid.Empty;

            faceServiceClient = new FaceServiceClient(Configuration["FaceAPIKey"], "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/");

            var faceDetected = DetectFaceAsync(image).Result; // face da imagem que foi uploadeada

            if (faceDetected == null)
            {
                // retorna msg de que não encontrou uma face
                return null;
            }

            while (true)
            {

                var containsAnyFaceOnList = UpsertFaceListAndCheckIfContainsFaceAsync().Result;
                //Detecta a quantidade de faces na imagem

                Guid? persistedId = null;
                if (containsAnyFaceOnList)
                    persistedId = FindSimilarAsync(faceDetected.FaceId, FaceListId).Result;

                //Se nao achou nada semelhante na lista, adiciona a face atual
                if (persistedId == null)
                {
                    persistedId = AddFaceAsync(FaceListId, face).Result;
                    //Console.WriteLine($"New User with FaceId {persistedId}");
                    return persistedId;
                }
                else
                {
                    return persistedId;
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
