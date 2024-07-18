using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sieve.Domain.Services.Interfaces;
using Sieve.Models;
using Sieve.Persistence.Models;
using Sieve.Persistence.UnitOfWork;
using Sieve.RestAPI.Models;
using Sieve.RestAPI.Sieve.Extensions;
using Sieve.RestAPI.Sieve.Models;
using Sieve.Services;
using Swashbuckle.AspNetCore.Annotations;
using static Sieve.Extensions.MethodInfoExtended;

namespace Sieve.RestAPI.Controllers
{ 
    [ApiController]
    [Route("[controller]")]
    public class StudentsController : ControllerBase
    {
        private IStudentService _studentService;
        private readonly ISieveProcessor _processor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<SieveOptions> _sieveOptions;

        public StudentsController(IStudentService studentService, ISieveProcessor sieveProcessor,IOptions<SieveOptions> sieveOptions, IUnitOfWork unitOfWork)
        {
            _studentService = studentService;
            _processor = sieveProcessor;
            _sieveOptions = sieveOptions;
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
             
            var query = _unitOfWork.StudentRepository.Entities.Take(100).AsNoTracking();
 
            //result = _processor.Apply(sieveModel, result, applyPagination:true);
            var pagedResult = await query.ToPagedListAsync(_processor, _sieveOptions, sieveModel, student => new StudentDTO()
            {
                Id = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                BirthDate = student.BirthDate
            });
             

            return Ok(pagedResult);
        }

        [HttpDelete("entity-delete", Name = "EntityDelete")]
        [SwaggerOperation(OperationId = "EntityDelete")]
        public async Task<ActionResult> EntityDeleteById(int id)
        {
            try
            {
                _unitOfWork.StudentRepository.DeleteById(id);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }
        
        [HttpDelete("domain-delete", Name = "DomainDelete")]
        [SwaggerOperation(OperationId = "DomainDelete")]
        public async Task<ActionResult> DomainDeleteById(int id)
        {
                var result = await _studentService.DeleteAndSaveAsync(id);
                if (!result.IsSuccess)
                {
                    return Problem(detail:result.ErrorMessage, statusCode:(int)result.StatusCode);
                }
                return Ok();
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
