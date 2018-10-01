namespace Game.API.Common.Models
{
    using Newtonsoft.Json;
    using System.Net;

    public class APIResponse
    {
        public APIResponse(HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            StatusCode = statusCode;
        }

        public virtual HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public virtual bool Success { get; set; } = true;

        public virtual string Exception { get; set; } = null;
        public virtual string ExceptionType { get; set; } = null;
        public virtual string ExceptionStack { get; set; } = null;

        public virtual double Elapsed { get; set; } = 0;

        [JsonIgnore]
        public static APIResponse Successful
        {
            get
            {
                return new APIResponse();
            }
        }
    }

    public class APIResponse<T> : APIResponse
    {
        public APIResponse(HttpStatusCode statusCode = HttpStatusCode.OK)
            :base (statusCode)
        {

        }

        public virtual T Response { get; set; } = default(T);
    }
}
