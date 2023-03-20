namespace Rhinox.Magnus.CommandSystem
{
    public interface IConsoleCommand
    {
        string CommandName { get; }

        string[] Execute(string[] args);
    }
}