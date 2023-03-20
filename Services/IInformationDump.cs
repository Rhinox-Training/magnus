using System.Collections.Generic;

namespace Rhinox.Magnus
{
    public interface IInformationDump
    {
        IReadOnlyCollection<string> Contents { get; }

        void WriteLine(string data);
    }
}