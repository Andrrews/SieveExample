
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sieve.Models;
using Sieve.Persistence.UnitOfWork;
using Sieve.Services;
using SieveExample.Domain.Helpers.PagedList;
using System.Diagnostics;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Sieve.Domain.Extensions;
using Sieve.Domain.Services.Interfaces;
using Sieve.Domain.Services.Models;

namespace Sieve.Domain.Services
{
    /// <summary>
    /// This is a base service class that provides basic CRUD operations for a given entity type T. 
    /// It includes methods for adding, updating, deleting, and retrieving entities, with optional tracking.
    /// It also provides methods for searching and paging through entities.
    /// </summary>
    /// <returns>
    /// The BaseService class does not return a value but provides a set of methods to manipulate and retrieve data.
    /// </returns>
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

        /// <summary>
        /// Enables tracking for the service. Once tracking is enabled, changes to the entities will be tracked and can be persisted to the database.
        /// </summary>
        /// <returns>
        /// Returns the current instance of the service with tracking enabled.
        /// </returns>
        public IBaseService<T> WithTracking()
        {
            _tracking = true;
            return this;
        }

        /// <summary>
        /// This method is used to disable entity tracking for the current service instance.
        /// </summary>
        /// <returns>
        /// Returns the current service instance with tracking disabled.
        /// </returns>
        public IBaseService<T> WithoutTracking()
        {
            _tracking = false;
            return this;
        }

        /// <summary>
        /// Asynchronously retrieves an entity of type T by its ID from the database. 
        /// If the entity is not found, it returns an error message and HTTP status code for not found.
        /// </summary>
        /// <param name="id">The ID of the entity to retrieve.</param>
        /// <returns>
        /// A ServiceResult containing the retrieved entity of type T if found, 
        /// otherwise an error message and HTTP status code for not found.
        /// </returns>
        public async Task<ServiceResult<T>> GetByIdAsync(params object?[]? id)
        {
            try
            {
                var result = _tracking
                    ? await _unitOfWork.Repository<T>().GetByIdAsync(id)
                    : await _unitOfWork.Repository<T>().WithoutTracking().GetByIdAsync(id);

                if (result == null)
                {
                    return ServiceResult<T>.Error("Object not found in database.", HttpStatusCode.NotFound);
                }

                return ServiceResult<T>.Success(result);
            }
            catch (Exception ex)
            {
                return LogError(ex.Message);
            }
        }

        /// <summary>
        /// Asynchronously searches for entities of type TOut based on the provided pagination parameters and formats the result using the provided callback function.
        /// </summary>
        /// <param name="paginationParams">The pagination parameters to apply to the search.</param>
        /// <param name="formatterCallback">The function to use for formatting the result.</param>
        /// <returns>
        /// A ServiceResult containing a paged list of entities of type TOut if the search is successful, or an error message if an exception is thrown.
        /// </returns>
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
        /// <summary>
        /// Asynchronously retrieves a paged list of data based on the provided SieveModel. The SieveModel contains sorting and filtering rules.
        /// </summary>
        /// <param name="sieveModel">The SieveModel containing the rules for sorting and filtering the data.</param>
        /// <returns>
        /// A tuple containing a read-only list of the paged data and the total count of the data.
        /// </returns>
        public async Task<(IReadOnlyList<T> data, int totalCount)> GetPagedDataAsync(SieveModel sieveModel)
        {
            var query = _unitOfWork.Repository<T>().Entities.AsNoTracking();
            var pagedData = _sieveService.Apply(sieveModel, query);
            var totalCount = await query.CountAsync();
            var data = await pagedData.ToListAsync();
            return (data, totalCount);
        }
        /// <summary>
        /// Asynchronously adds a new entity of type T to the repository.
        /// </summary>
        /// <param name="entity">The entity of type T to be added.</param>
        /// <returns>
        /// Returns a ServiceResult of type T. If the operation is successful, it returns the added entity. If an exception occurs, it logs the error and returns a failure result.
        /// </returns>
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

        /// <summary>
        /// Asynchronously adds and saves an entity of type T to the repository.
        /// </summary>
        /// <param name="entity">The entity of type T to be added to the repository.</param>
        /// <returns>
        /// Returns a ServiceResult of type T. If the operation is successful, it returns the added entity and a HttpStatusCode of Created. If an exception occurs, it logs the error and returns the error message.
        /// </returns>
        public async Task<ServiceResult<T>> AddAndSaveAsync(T entity)
        {
            try
            {
                var result = await _unitOfWork.Repository<T>().AddAsync(entity);
                await SaveChangesAsync(CancellationToken.None);

                return ServiceResult<T>.Success(result, HttpStatusCode.Created);
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
