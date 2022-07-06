using Microsoft.EntityFrameworkCore;

namespace Collections.Utils
{
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;

        public bool HasNextPage => PageIndex < TotalPages;

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int page, int perPage)
        {
            try
            {
                var count = await source.CountAsync();
                var items = await source.Skip((page - 1) * perPage).Take(perPage).ToListAsync();
                return new PaginatedList<T>(items, count, page, perPage);
            }
            catch
            {
                return new PaginatedList<T>(new List<T>(), 0, page, perPage);
            }
        }
    }
}
