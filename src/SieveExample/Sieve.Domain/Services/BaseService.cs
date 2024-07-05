using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sieve.Models;
using Sieve.Persistence.UnitOfWork;
using Sieve.Services;
using SieveExample.Domain.Helpers.PagedList;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sieve.Domain.Extensions;
using Sieve.Domain.Services.Interfaces;
using Sieve.Domain.Services.Models;

namespace Sieve.Domain.Services
{
     public class BaseService<T> : IBaseService<T> where T : class
    {
        private readonly ILogger<T> _logger;

        protected readonly IUnitOfWork _unitOfWork;
        protected readonly ISieveProcessor? _sieveService;
        protected readonly IOptions<SieveOptions>? _sieveOptions;
         
        private bool _tracking = true;
        

        public BaseService(ILogger<T> logger,
                           ISieveProcessor? sieveService,
                           IOptions<SieveOptions>? sieveOptions,
                           IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _sieveService = sieveService;
            _sieveOptions = sieveOptions;
        }

        public IBaseService<T> WithTracking()
        {
            _tracking = true;
            return this;
        }

        public IBaseService<T> WithoutTracking()
        {
            _tracking = false;
            return this;
        }

        public async Task<ServiceResult<T>> GetByIdAsync(params object?[]? id)
        {
            try
            {
                var result = _tracking
                    ? await _unitOfWork.Repository<T>().GetByIdAsync(id)
                    : await _unitOfWork.Repository<T>().WithoutTracking().GetByIdAsync(id);

                if (result == null)
                {
                    return ServiceResult<T>.Error( "Object not found in database.",HttpStatusCode.NotFound);
                }

                return ServiceResult<T>.Success(result);
            }
            catch (Exception ex)
            {
                return LogError(ex.Message);
            }
        }

        public async Task<ServiceResult<IPagedList<TOut?>>> SearchAsync<TOut>(SieveModel paginationParams, Func<T, TOut> formatterCallback)
        {
            try
            {
                var query = _unitOfWork.Repository<T>().Entities.AsNoTracking();

                var result = await query.ToPagedListAsync(_sieveService,
                                                          _sieveOptions,
                                                          paginationParams,
                                                          formatterCallback);

                return ServiceResult<IPagedList<TOut?>>.Success(result);
            }
            catch (Exception ex)
            {
                var error = LogError(ex.Message);

                return ServiceResult<IPagedList<TOut?>>.Error(ex.Message);
            }
        }
        public async Task<(IReadOnlyList<T> data, int totalCount)> GetPagedDataAsync(SieveModel sieveModel)
        {
            var query = _unitOfWork.Repository<T>().Entities.AsNoTracking();
            var pagedData = _sieveService.Apply(sieveModel, query);
            var totalCount = await query.CountAsync();
            var data = await pagedData.ToListAsync();
            return (data, totalCount);
        }
        public async Task<ServiceResult<T>> AddAsync(T entity)
        {
            try
            {
                var result = await _unitOfWork.Repository<T>().AddAsync(entity);

                return ServiceResult<T>.Success(result);
            }
            catch (Exception ex)
            {
                return LogError(ex.Message);
            }
        }

        public async Task<ServiceResult<T>> AddAndSaveAsync(T entity)
        {
            try
            {
                var result = await _unitOfWork.Repository<T>().AddAsync(entity);
                await SaveChangesAsync(CancellationToken.None);

                return ServiceResult<T>.Success(result,HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                return LogError(ex.Message);
            }
        }

        public async Task<T> UpdateAsync(T entity, params object?[]? id)
        {
            return await _unitOfWork.Repository<T>().UpdateAsync(entity, id);
        }

        public async Task<ServiceResult<T>> UpdateAndSaveAsync(T entity, params object?[]? id)
        {
            if (id == null)
            {

                return ServiceResult<T>.Error("Object ID cannot be null or empty.", HttpStatusCode.BadRequest);
            }

            try
            {
                if (!await _unitOfWork.Repository<T>().Exists(id))
                {
                    return ServiceResult<T>.Error("Object " + typeof(T) + " with ID " + id[0] + " does not exists in database.", HttpStatusCode.NotFound);
      
                }

                var result = await _unitOfWork.Repository<T>().UpdateAsync(entity, id);
                await SaveChangesAsync(CancellationToken.None);

                return ServiceResult<T>.Success(result);
            }
            catch (Exception ex)
            {
                return LogError(ex.Message);
            }
        }

        public async Task DeleteAsync(params object?[]? id)
        {
            var entity = await _unitOfWork.Repository<T>().GetByIdAsync(id);

            await _unitOfWork.Repository<T>().DeleteAsync(entity);

            return;
        }

        public async Task<ServiceResult<T>> DeleteAndSaveAsync(params object?[]? id)
        {
            if (id == null)
            {
                return ServiceResult<T>.Error("Object ID cannot be null or empty.", HttpStatusCode.BadRequest);
            }

            try
            {
                if (!await _unitOfWork.Repository<T>().Exists(id))
                {
                    return ServiceResult<T>.Error("Object " + typeof(T) + " with ID " + id[0] + " does not exists in database.", HttpStatusCode.NotFound);
                }

                await DeleteAsync(id);
                await SaveChangesAsync(CancellationToken.None);

                return ServiceResult<T>.Success(default(T));
            }
            catch (Exception ex)
            {
                return LogError(ex.Message);
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        protected ServiceResult<T> LogError(string errorMessage)
        {
            var traceId = Activity.Current?.TraceId;
            var spanId = Activity.Current?.SpanId;

            _logger.LogInformation("Exception: " + errorMessage + " TraceID: " + traceId + "-" + spanId);

            return ServiceResult<T>.Error("Internal server error", HttpStatusCode.InternalServerError);
        }
    }
}
