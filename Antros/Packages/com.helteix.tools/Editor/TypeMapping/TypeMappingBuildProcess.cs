using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Helteix.Tools.Editor.TypeMapping
{
    public class TypeMappingBuildProcess : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            TypeMappingEditor.ScanProject();
        }
    }
}