using System.Collections.Generic;

namespace Rhinox.Magnus.CommandSystem
{
    public class ArrayInformationDump : IInformationDump
    {
        private List<string> _contents;
        public IReadOnlyCollection<string> Contents => _contents;

        public ArrayInformationDump()
        {
            _contents = new List<string>();
        }

        public void WriteLine(string data)
        {
            _contents.Add(data);
        }
    }
}