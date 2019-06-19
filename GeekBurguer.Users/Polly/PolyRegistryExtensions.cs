using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace GeekBurguer.Users.Polly
{
    public static class PolyRegistryExtensions
    {
        public static IPolicyRegistry<string> AddBasicRetryPolicy(this IPolicyRegistry<string> policyRegistry)
        {
            var retryPolicy = Policy
                .Handle<Exception>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, retryCount => TimeSpan.FromSeconds(Math.Pow(2, retryCount)), (result, timeSpan, retryCount, context) =>
                {
                    if (!context.TryGetLogger(out var logger)) return;

                    if (result.Exception != null)
                    {
                        logger.LogError(result.Exception, "Ocorreu um erro na tentativa {RetryAttempt} para a politica {PolicyKey}", retryCount, context.PolicyKey);
                    }
                    else
                    {
                        logger.LogError("Um status de insucessoe {StatusCode} foi recebido na tentativa {RetryAttempt} para a politica {PolicyKey}",
                            (int)result.Result.StatusCode, retryCount, context.PolicyKey);
                    }

                    context.TryGetValue("url", out var url);
                })
                .WithPolicyKey(PolicyNames.BasicRetry);

            policyRegistry.Add(PolicyNames.BasicRetry, retryPolicy);

            return policyRegistry;
        }
    }
}
