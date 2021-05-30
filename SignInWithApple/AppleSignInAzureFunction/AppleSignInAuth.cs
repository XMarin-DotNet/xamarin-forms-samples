using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using Xamarin.AppleSignIn;

namespace AppleSignInAzureFunction
{
    public static class AppleSignInAuth
    {
        [FunctionName("applesignin_auth")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            // Pass along the state and nonce generated by the client app
            // TODO: IMPORTANT You should really cache this state in a lookup table
            // that way when the callback request comes in, we can ensure it was from a request
            // which originated from us.
            var state = req.Query["state"];

            // Pass along the nonce, it will be returned in the id_token we send back to the app
            var nonce = req.Query["nonce"];

            if (string.IsNullOrEmpty(state) || string.IsNullOrEmpty(nonce))
                return new BadRequestResult();
             
            // Create a new oauth instance
            var apple = new AppleSignInClient(Config.ServerId, Config.KeyId, Config.TeamId, new Uri(Config.RedirectUri), Config.P8FileContents, state, nonce);

            // Generate the auth url to redirect to
            var url = apple.GenerateAuthorizationUrl();
            
            // Redirect the browser to the auth url
            return new RedirectResult(url.OriginalString, false);
        }
    }
}
