namespace SieveExample.Domain.Helpers.PagedList
{
    public interface IPagedList<out TT, TOut> : IPagedList<TOut>
    {
        IPagedList<TOut> GetMetaData();
    }

    public interface IPagedList<TOut>
    {
        int PageCount { get; }

        int TotalItemCount { get; }

        int PageNumber { get; }

        int PageSize { get; }

        bool HasPreviousPage { get; }

        bool HasNextPage { get; }

        bool IsFirstPage { get; }

        bool IsLastPage { get; }

        int FirstItemOnPage { get; }

        int LastItemOnPage { get; }

        public IEnumerable<TOut> PageData { get; }
    }
}
