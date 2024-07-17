﻿namespace Sieve.RestAPI.Sieve.Models
{
    public class PagedList<T> where T : class
    {
        public int PageCount { get; set; }
        public int TotalItemCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < PageCount;
        public bool IsFirstPage => PageNumber == 1;
        public bool IsLastPage => PageNumber == PageCount;
        public int FirstItemOnPage => (PageNumber - 1) * PageSize + 1;
        public int LastItemOnPage => FirstItemOnPage + PageData.Count - 1;
        public List<T> PageData { get; set; } = new List<T>();
    }
}