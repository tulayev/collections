﻿namespace Collections.Utils
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> Paginate<T>(this IQueryable<T> queryable, int page, int perPage = 10)
        {
            return queryable.Skip((page - 1) * perPage).Take(perPage);
        }
    }
}
