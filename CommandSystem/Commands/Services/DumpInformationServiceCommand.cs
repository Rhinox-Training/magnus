using System.Linq;

namespace Rhinox.Magnus.CommandSystem
{
    public class DumpInformationServiceCommand : BaseServiceConsoleCommand
    {
        public override string CommandName => "dump-service";
        
        protected override string[] ExecuteFor(IService service)
        {
            var arr = new ArrayInformationDump();
            service.DumpInformation(arr);
            return arr.Contents.ToArray();
        }
    }
}