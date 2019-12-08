using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JetKarmaBot.Services.Handling
{
    public interface IRequestHandler
    {
        Task Handle(RequestContext ctx, Func<RequestContext, Task> next);
    }
    public class RequestChain : IRequestHandler
    {
        List<IRequestHandler> handlerStack = new List<IRequestHandler>();
        public async Task Handle(RequestContext ctx, Func<RequestContext, Task> next = null)
        {
            int i = 0;
            Func<RequestContext, Task> chainNext = null;
            chainNext = (newCtx) =>
            {
                if (i == handlerStack.Count) return Task.CompletedTask;
                IRequestHandler handler = handlerStack[i++];
                return handler.Handle(newCtx, chainNext);
            };
            await chainNext(ctx);
            if (next != null)
                await next(ctx);
        }
        public void Add(IRequestHandler handler)
        {
            handlerStack.Add(handler);
        }
    }
}