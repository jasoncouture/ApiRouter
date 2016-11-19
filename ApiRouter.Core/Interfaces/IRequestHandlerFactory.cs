namespace ApiRouter.Core.Interfaces
{
    public interface IRequestHandlerFactory
    {
        IRequestHandler GetHandler();
        void Release(IRequestHandler requestHandler);
    }
}