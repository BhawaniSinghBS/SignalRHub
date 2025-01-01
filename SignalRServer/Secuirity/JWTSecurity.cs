using SignalRServer.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SignalRServer.Secuirity
{
    public static class JWTSecurity
    {
        #region securityForTheHub
        public static string GetJwtTokenFromContext(HttpContext httpContext)
        {
            var token = "";
            // Extract the JWT token from the request context
            if (httpContext?.Request?.Headers?.ContainsKey("Authorization") ?? false)
            {
                token = httpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ")?.Last() ?? "";
            }
            if (string.IsNullOrEmpty(token))
            {
                if (!string.IsNullOrEmpty(httpContext?.Request?.Query["access-token"]))
                {
                    string? tokenWithBearer = httpContext?.Request?.Query["access-token"].FirstOrDefault();
                    if (tokenWithBearer?.StartsWith("Bearer ") ?? false)
                    {
                        token = tokenWithBearer.Substring("Bearer ".Length);
                    }
                }
            }
            return token;
        }
        public static string GetAppName(HttpContext httpContext)
        {
            var appName = "";
            // Extract the JWT token from the request context
            if (httpContext?.Request?.Headers?.ContainsKey("AppName") ?? false)
            {
                appName = httpContext.Request.Headers["AppName"].FirstOrDefault()?.Split(" ")?.Last();
            }
            if (string.IsNullOrEmpty(appName))
            {
                if (!string.IsNullOrEmpty(httpContext?.Request?.Query["an"]))
                {
                    appName = httpContext?.Request?.Query["an"];
                }
            }
            return appName;
        }
        // isValidToken,isThisToken
        public static bool IsValidAppRequesting(string appName, List<string> allowedApps)
        {
            if (string.IsNullOrWhiteSpace(appName))
            {
                return false;
            }

            if (allowedApps?.Count() > 0)
            {
                var isValidRequetingApp = allowedApps.Contains(appName.ToLower());
                return isValidRequetingApp;
            }
            return false;
        }
        public static bool IsValidJwtToken(string token, string appName = "")
        {
            if (!string.IsNullOrWhiteSpace(appName) && appName == ServerSettings.ThisAppName && token == ServerSettings.ThisAppToken)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }
            //Validate the JWT token using appropriate libraries
            // You can check the token's issuer, signature, expiration, etc.
            // Example: Check if the token is not null or empty
            var claimsPrincipal = GetClaimsPrincipalFromJwt(token);

            if (claimsPrincipal != null)
            {
                //Perform additional authorization or authentication logic using the claimsPrincipal
                // Access the validated user's claims using claimsPrincipal.Claims

                // Example: Get the user's name claim
                //var userName = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                //var userId = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                //Example: Check if the user has a specific claim

                // var isAdmin = claimsPrincipal.Claims.Any(c => c.Type == "Role" && c.Value == "Admin");
                // Create a custom ClaimsPrincipal with the user's claims
                var customClaimsPrincipal = new ClaimsPrincipal(claimsPrincipal.Identity);

                ///Add the claims to the custom ClaimsPrincipal
                foreach (var claim in claimsPrincipal.Claims)
                {
                    customClaimsPrincipal.AddIdentity(new ClaimsIdentity(new[] { claim }));
                }

                //Create a custom HubCallerContext with the custom ClaimsPrincipal
                // var customContext = new HubCallerContext(Context.ConnectionId, Context.Hub, customClaimsPrincipal);
                return true;
            }
            else
            {
                return false;
            }
            ///return user.HasClaim(c => c.Type == "YourClaimType" && c.Value == "YourClaimValue");
        }
        private static ClaimsPrincipal GetClaimsPrincipalFromJwt(string token)
        {
            //Parse and validate the JWT token

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(ServerSettings.JWTTokenSecretKey); // Replace with your secret key
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidateAudience = false,
                ValidIssuer = ServerSettings.JWTTokenIssuer, // Replace with your issuer
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            ClaimsPrincipal claimsPrincipal = null;

            try
            {
                claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            }
            catch (Exception ex)
            {
                //Handle token validation errors
                // Example: Log the error or throw an exception
                Console.WriteLine(ex.Message);
            }
            return claimsPrincipal;
        }
        #endregion
    }
}
