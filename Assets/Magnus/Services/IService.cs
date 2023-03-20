namespace Rhinox.Magnus
{
    public interface IService
    {
        bool IsActive { get; }

        void DumpInformation(IInformationDump data);
    }
}