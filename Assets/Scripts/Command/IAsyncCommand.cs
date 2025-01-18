public interface IAsyncCommand : ICommand
{
    bool IsCompleted { get; }
}
