using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Cdms.Backend.Data.Extensions;

public static class QueryableExtensions
{
    public static async Task<List<TSource>> ToListAsync<TSource>(
        this IQueryable<TSource> source,
        CancellationToken cancellationToken = default)
    {
        if (source is IAsyncEnumerable<TSource> asyncEnumerable)
        {
            var list = new List<TSource>();
            await foreach (var element in asyncEnumerable.WithCancellation(cancellationToken))
            {
                list.Add(element);
            }

            return list;
        }

        if (source is IMongoQueryable<TSource> mongoQueryable)
        {
            return await IAsyncCursorSourceExtensions.ToListAsync(mongoQueryable, cancellationToken);
        }


        return source.AsEnumerable().ToList();

    }
}