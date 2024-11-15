using NLog;

namespace JetKarmaBot.Services.Handling;

public interface IRequestHandler
{
    Task Handle(RequestContext ctx, Func<RequestContext, Task> next);
}
public class RequestChain : IRequestHandler
{
    [Inject] private Logger log;
    List<IRequestHandler> handlerStack = new List<IRequestHandler>();
    public async Task Handle(RequestContext ctx, Func<RequestContext, Task> next = null)
    {
        int i = 0;
        Func<RequestContext, Task> chainNext = null;
        chainNext = (newCtx) =>
        {
            if (i == handlerStack.Count)
            {
                log.ConditionalTrace("(next) End of request chain");
                return Task.CompletedTask;
            }
            IRequestHandler handler = handlerStack[i++];
            log.ConditionalTrace($"(next) Executing handler {handler.GetType().Name}");
            return handler.Handle(newCtx, chainNext);
        };
        await chainNext(ctx);
        if (next != null)
            await next(ctx);
    }
    public void Add(IRequestHandler handler)
    {
        log.ConditionalTrace($"Adding {handler.GetType().Name} to reqchain");
        handlerStack.Add(handler);
    }
}