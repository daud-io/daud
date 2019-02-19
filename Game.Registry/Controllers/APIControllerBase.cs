namespace Game.Registry.Controllers
{
    using Game.API.Common.Models;
    using Game.API.Common.Security;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using System;
    using System.Net;

    [
        Authorize,
        Route("api/v1/[controller]")
    ]
    public abstract class APIControllerBase : Controller
    {
        private DateTime RequestStarted; // tracking action response time
        protected bool SuppressWrapper = false;
        protected HttpStatusCode ExceptionlessStatusCode = HttpStatusCode.OK;

        protected readonly ISecurityContext SecurityContext;

        public APIControllerBase(ISecurityContext securityContext)
        {
            this.SecurityContext = securityContext;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // for timing the request
            this.RequestStarted = DateTime.Now;
            base.OnActionExecuting(context);
        }

        // we're going to change the response object type and wrap it with an object
        // that normalizes successful responses and exceptions.
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            // calculate the amount of time the action took to execute
            // this excludes any middleware overhead
            var elapsed = DateTime.Now.Subtract(this.RequestStarted).TotalMilliseconds;

            if (SuppressWrapper)
            {
                base.OnActionExecuted(context);
                return;
            }

            // check if the action threw an exception
            if (context.Exception != null)
            {
                Console.Error.WriteLine($"Exception in API Controller: {context.Exception}");

                // this one was not thrown intentionally by our code
                var exception = context.Exception;

                context.Result = new ObjectResult(new APIResponse<object>
                {
                    Exception = exception.Message,
                    ExceptionType = exception.GetType().Name,
                    ExceptionStack = exception.StackTrace,
                    Success = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    Elapsed = elapsed
                });

                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                context.Exception = null;
            }
            else
            {
                // there was no exception
                // we will still wrap the result with a consistent decorator

                var objectResponse = context.Result is ObjectResult
                    ? (context.Result as ObjectResult).Value
                    : null;

                // wrap the successful response
                context.Result = new ObjectResult(new APIResponse<object>
                {
                    Exception = null,
                    Success = true,
                    StatusCode = ExceptionlessStatusCode,
                    Elapsed = elapsed,
                    Response = objectResponse
                });

                context.HttpContext.Response.StatusCode = (int)ExceptionlessStatusCode;
            }

            base.OnActionExecuted(context);
        }
    }
}
