using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ScanDrive.Domain.DTOs.Common;

namespace ScanDrive.Infrastructure.Extensions;

public static class QueryableExtensions
{
    public static async Task<PagedList<T>> ToPagedListAsync<T>(
        this IQueryable<T> query,
        PaginationParams paginationParams)
    {
        var count = await query.CountAsync();
        
        if (!string.IsNullOrWhiteSpace(paginationParams.OrderBy))
        {
            var property = typeof(T).GetProperty(paginationParams.OrderBy);
            if (property != null)
            {
                var parameter = Expression.Parameter(typeof(T), "x");
                var propertyAccess = Expression.Property(parameter, property);
                var orderByExp = Expression.Lambda(propertyAccess, parameter);

                var orderByMethod = paginationParams.IsDescending
                    ? nameof(Queryable.OrderByDescending)
                    : nameof(Queryable.OrderBy);

                var resultType = property.PropertyType;
                var orderByGeneric = typeof(Queryable)
                    .GetMethods()
                    .First(x => x.Name == orderByMethod && x.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(T), resultType);

                query = (IQueryable<T>)orderByGeneric.Invoke(null, new object[] { query, orderByExp })!;
            }
        }

        var items = await query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        return new PagedList<T>(items, count, paginationParams);
    }

    public static IQueryable<T> ApplyFilter<T>(
        this IQueryable<T> query,
        BaseFilter filter,
        Expression<Func<T, bool>>? additionalFilter = null)
        where T : class
    {
        if (filter.IsActive.HasValue)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, "IsActive");
            var value = Expression.Constant(filter.IsActive.Value);
            var condition = Expression.Equal(property, value);
            var lambda = Expression.Lambda<Func<T, bool>>(condition, parameter);
            query = query.Where(lambda);
        }

        if (filter.ShopId.HasValue)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, "ShopId");
            var value = Expression.Constant(filter.ShopId.Value);
            var condition = Expression.Equal(property, value);
            var lambda = Expression.Lambda<Func<T, bool>>(condition, parameter);
            query = query.Where(lambda);
        }

        if (filter.StartDate.HasValue)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, "CreatedAt");
            var value = Expression.Constant(filter.StartDate.Value);
            var condition = Expression.GreaterThanOrEqual(property, value);
            var lambda = Expression.Lambda<Func<T, bool>>(condition, parameter);
            query = query.Where(lambda);
        }

        if (filter.EndDate.HasValue)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, "CreatedAt");
            var value = Expression.Constant(filter.EndDate.Value);
            var condition = Expression.LessThanOrEqual(property, value);
            var lambda = Expression.Lambda<Func<T, bool>>(condition, parameter);
            query = query.Where(lambda);
        }

        if (additionalFilter != null)
        {
            query = query.Where(additionalFilter);
        }

        return query;
    }
} 