////using System.Collections.Concurrent;
////using System.Diagnostics;
////using Btms.Backend.Data;
////using Btms.Consumers.Extensions;
////using Btms.Metrics;
////using SlimMessageBus;
////using SlimMessageBus.Host.Interceptor;

////namespace Btms.Consumers.Interceptors;

////public class DbContextSaveInterceptor<TMessage>(IMongoDbContext dbContext) : IConsumerInterceptor<TMessage> where TMessage : notnull
////{
////    public async Task<object> OnHandle(TMessage message, Func<Task<object>> next, IConsumerContext context)
////    {
////        var result = await next();
////        ////await dbContext.SaveChangesAsync(context.CancellationToken);
////        return result;
////    }
////}