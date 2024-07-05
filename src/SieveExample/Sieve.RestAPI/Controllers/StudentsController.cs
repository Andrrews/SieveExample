using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sieve.Domain.Services.Interfaces;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Sieve.RestAPI.Controllers
{ 
    [ApiController]
    [Route("[controller]")]
    public class StudentsController : ControllerBase
    {
        private IStudentService _studentService;

        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }
        // GET: StudentsController
        [HttpGet(Name = "GetByFilter")]
        [SwaggerOperation(OperationId = "GetByFilter")]
        public async Task<ActionResult> GetByFilter([FromQuery] SieveModel paginationParams)
        {
            try
            {
                var result = await _studentService.SearchAsync(paginationParams, resultEntity => (resultEntity));
                if (!result.IsSuccess)
                {
                    return Problem(detail: result.ErrorMessage, statusCode: (int)result.StatusCode);
                }

                return Ok(result.Entity);
            }
            catch (WebException e)
            {
                var webex = HandleWebException(e);
                return Problem(detail: e.Message, statusCode: (int)webex);
            }
            catch (Exception e)
            {
                return Problem(detail: e.Message, statusCode:(int)HttpStatusCode.InternalServerError);
            }
           
        }

        private HttpStatusCode HandleWebException(WebException ex)
        {
          
            switch (ex.Status)
            {
                case WebExceptionStatus.NameResolutionFailure:
                    return HttpStatusCode.BadGateway;

                case WebExceptionStatus.ConnectFailure:
                    return HttpStatusCode.ServiceUnavailable;

                case WebExceptionStatus.Timeout:
                    return HttpStatusCode.RequestTimeout;

                case WebExceptionStatus.ProtocolError:
                    if (ex.Response is HttpWebResponse response)
                    {
                        return response.StatusCode;
                    }

                    return HttpStatusCode.BadGateway;

                default:
                    return HttpStatusCode.InternalServerError;
            }
        }
 
    }
}
