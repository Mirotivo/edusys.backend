namespace Backend.Interfaces.Events
{
    public interface IEventHandler<T> where T : class
    {
        Task HandleAsync(T eventData);
    }
}
