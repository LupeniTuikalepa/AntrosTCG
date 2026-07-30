using ATCG.Capacities;

namespace ATCG.Editor.Tools.DatabaseBrowser
{
    /// <summary>
    /// Browses every DeployableData asset under Resources/Database/Deployables. DeployableData
    /// has no Name/Element, so the browser lists by asset file name and hides the Element filter.
    /// </summary>
    public sealed class DeployableBrowserTool : DatabaseBrowserTool<DeployableData>
    {
        protected override string FolderPath => "Assets/Resources/Database/Deployables";
        public override string DisplayName => "Deployables";
        public override string Icon => "⬠";
        public override int Order => 62;
    }
}
