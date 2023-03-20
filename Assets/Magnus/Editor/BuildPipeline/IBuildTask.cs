using System.Collections;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Rhinox.Magnus.Editor
{
    public interface IBuildTask
    {
        bool Run();
        IEnumerator RunPostBuild(BuildTarget target, string pathToBuiltProject);
        void RunPreBuild(BuildReport report);
    }
}