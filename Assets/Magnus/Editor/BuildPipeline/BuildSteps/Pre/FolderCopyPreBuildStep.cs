using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.IO;
using Sirenix.OdinInspector;

namespace Rhinox.Magnus.Editor
{
    public class FolderCopyPreBuildStep : PreBuildStep
    {
        [ValueDropdown(nameof(GetDropdown))]
        public string PackageName = null;

        public string SourcePath = "";

        public string TargetPath = "Assets";

        private const string NONE_PACKAGE = "_NONE_";
    
        protected override bool OnExecute()
        {
            if (string.IsNullOrWhiteSpace(PackageName) ||
                string.IsNullOrWhiteSpace(SourcePath) ||
                string.IsNullOrWhiteSpace(TargetPath))
                return false;

            string absSrcPath;
            if (PackageName != NONE_PACKAGE)
            {
                string srcPath = FileHelper.Combine("Packages", PackageName, SourcePath);
                absSrcPath = Path.GetFullPath(srcPath);
            }
            else
            {
                absSrcPath = FileHelper.Combine(FileHelper.GetProjectPath(), SourcePath);
            }

            string absTargetPath = FileHelper.Combine(FileHelper.GetProjectPath(), TargetPath);
            
            FileHelper.CopyDirectory(absSrcPath, absTargetPath);
            return true;
        }

        private ICollection<ValueDropdownItem> GetDropdown()
        {
            var list = Utility.ListPackages()
                .Select(x => new ValueDropdownItem(x.name, x.name))
                .ToList();
            list.Insert(0, new ValueDropdownItem(NONE_PACKAGE, NONE_PACKAGE));
            return list;
        }
    }
}