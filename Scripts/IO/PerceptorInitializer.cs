using Rhinox.Perceptor;
using Rhinox.Utilities.Attributes;

[InitializationHandler]
public static class PerceptorInitializer
{
    [OrderedRuntimeInitialize(-20000)]
    public static void InitLogger()
    {
        PLog.CreateIfNotExists();
    }
}
