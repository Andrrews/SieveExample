using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sieve.Domain.Services.Models
{
    public class ServiceResult<T> where T : class
    {
        public bool IsSuccess { get; set; }
        public T? Entity { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string ErrorMessage { get; set; }

        public static ServiceResult<T> Success(T entity, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new ServiceResult<T>
            {
                IsSuccess = true,
                Entity = entity,
                StatusCode = statusCode,
                ErrorMessage = string.Empty
            };
        }

        public static ServiceResult<T> Error(string errorMessage, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Entity = default(T),
                StatusCode = statusCode,
                ErrorMessage = errorMessage
            };
        }
    }
}
