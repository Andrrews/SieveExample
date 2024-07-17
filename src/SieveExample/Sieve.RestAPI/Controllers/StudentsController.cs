using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sieve.Domain.Services.Interfaces;
using Sieve.Models;
using Sieve.Persistence.UnitOfWork;
using Sieve.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Sieve.RestAPI.Controllers
{ 
    [ApiController]
    [Route("[controller]")]
    public class StudentsController : ControllerBase
    {
        private IStudentService _studentService;
        private readonly ISieveProcessor _processor;
        private readonly IUnitOfWork _unitOfWork;

        public StudentsController(IStudentService studentService, ISieveProcessor sieveProcessor, IUnitOfWork unitOfWork)
        {
            _studentService = studentService;
            _processor = sieveProcessor;
            _unitOfWork = unitOfWork;
        }
     
        [HttpGet("domain-filter",Name = "DomainGetByFilter")]
        [SwaggerOperation(OperationId = "DomainGetByFilter")]
        public async Task<ActionResult> DomainGetByFilter([FromQuery] SieveModel paginationParams)
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


        //MINIMALNE CO POTRZEBA DO URUCHOMIENIA SIEVE
        [HttpGet("entity-filter",Name = "EntityGetByFilter")]
        [SwaggerOperation(OperationId = "EntityGetByFilter")]
        public async Task<ActionResult> EntityGetByFilter([FromQuery] SieveModel sieveModel)
        {
            var result = _unitOfWork.StudentRepository.Entities.AsNoTracking();
            result = _processor.Apply(sieveModel, result);
            return Ok(result.ToList());
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
