using SignalRServer.Secuirity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace SignalRServer.Secuirity
{
    //public class SignalRHubAuthorize  : AuthorizeAttribute
    //[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class SignalRHubAuthorize : AuthorizeAttribute
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SignalRHubAuthorize(IHttpContextAccessor httpContextAccessor = null)
        {
            this._httpContextAccessor = httpContextAccessor;
            string tokenFromHttpContext = JWTSequirity.GetJwtTokenFromContext(_httpContextAccessor.HttpContext);
            bool isValidToken = JWTSequirity.IsValidJwtToken(tokenFromHttpContext ?? "");
            return /*isValidToken*/;
        }
        //protected override bool AuthorizeCore(HttpContextBase httpContext)
        //{
        //    string tokenFromHttpContext = JWTSequirity.GetJwtTokenFromContext(httpContext);
        //    bool isValidToken = JWTSequirity.IsValidJwtToken(tokenFromHttpContext ?? "");

        //    return isValidToken;
        //}

        //protected override bool UserAuthorized(System.Security.Principal.IPrincipal user)
        //{
        //    HubCallerContext hubContext = Context.Request.GetHttpContext();
        //    HttpContext httpContext = hubContext?.Request.HttpContext;

        //    // Access HttpContext properties and methods as needed
        //    string tokenFromHttpContext = JWTSequirity.GetJwtTokenFromContext(httpContext);
        //    bool isValidToken = JWTSequirity.IsValidJwtToken(tokenFromHttpContext ?? "");

        //    return isValidToken;
        //}
    }
}

//using SignalRServer.Sequirity;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.SignalR;
//using Microsoft.AspNetCore.SignalR.Protocol;
//using System.Threading.Tasks;

//public class SignalRHubAuthorizeAttribute : AuthorizeAttribute, IAuthorizationHandler
//{
//    public Task HandleAsync(AuthorizationHandlerContext context)
//    {
//        if (context.Resource is HubInvocationContext hubContext)
//        {
//            string tokenFromHttpContext = JWTSequirity.GetJwtTokenFromContext(hubContext.HubCallerContext.HttpContext);
//            bool isValidToken = JWTSequirity.IsValidJwtToken(tokenFromHttpContext ?? "");

//            if (isValidToken)
//            {
//                context.Succeed(requirement);
//            }
//            else
//            {
//                context.Fail();
//            }
//        }

//        return Task.CompletedTask;
//    }
//}

//using System;
//using System.Threading.Tasks;
//using SignalRServer.Sequirity;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.SignalR;

namespace SignalRServer.Secuirity
{
    //public class SignalRHubAuthorizeAttribute : AuthorizeAttribute
    //{
    //    public SignalRHubAuthorizeAttribute()
    //    {
    //        AuthenticationSchemes = "Bearer"; // Specify the authentication scheme if needed
    //    }
    //}

    //public class SignalRHubAuthorizeHandler : AuthorizationHandler<SignalRHubAuthorizeRequirement>
    //{
    //    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SignalRHubAuthorizeRequirement requirement)
    //    {
    //        if (context.Resource is HubInvocationContext hubContext)
    //        {
    //            string tokenFromHttpContext = JWTSequirity.GetJwtTokenFromContext(hubContext.Context.GetHttpContext());
    //            bool isValidToken = JWTSequirity.IsValidJwtToken(tokenFromHttpContext ?? "");

    //            if (isValidToken)
    //            {
    //                context.Succeed(requirement);
    //            }
    //            else
    //            {
    //                context.Fail();
    //            }
    //        }

    //        return Task.CompletedTask;
    //    }
    //}

    //public class SignalRHubAuthorizeRequirement : IAuthorizationRequirement
    //{
    //    // No additional logic needed for the requirement
    //}
}