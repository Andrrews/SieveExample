namespace SieveExample.Domain.Helpers.PagedList
{
    public abstract class BasePagedList<T, TOut> : PagedListMetaData<TOut>, IPagedList<T, TOut>
    {

        protected internal BasePagedList()
        {
        }

        protected internal BasePagedList(int pageNumber, int pageSize, int totalItemCount)
        {
            if (pageNumber < 1)
                throw new ArgumentOutOfRangeException(nameof(pageNumber), pageNumber, "PageNumber cannot be below 1.");
            if (pageSize < 1)
                throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "PageSize cannot be less than 1.");

            // set source to blank list if superset is null to prevent exceptions
            TotalItemCount = totalItemCount;
            PageSize = pageSize;
            PageNumber = pageNumber;
            PageCount = TotalItemCount > 0
                            ? (int)Math.Ceiling(TotalItemCount / (double)PageSize)
                            : 0;
            HasPreviousPage = PageNumber > 1;
            HasNextPage = PageNumber < PageCount;
            IsFirstPage = PageNumber == 1;
            IsLastPage = PageNumber >= PageCount;
            FirstItemOnPage = (PageNumber - 1) * PageSize + 1;
            var numberOfLastItemOnPage = FirstItemOnPage + PageSize - 1;
            LastItemOnPage = numberOfLastItemOnPage > TotalItemCount
                                 ? TotalItemCount
                                 : numberOfLastItemOnPage;
        }

        IEnumerable<TOut> IPagedList<TOut>.PageData => throw new NotImplementedException();

        IPagedList<TOut> IPagedList<T, TOut>.GetMetaData()
        {
            return new PagedListMetaData<TOut>(this);
        }
    }
}
