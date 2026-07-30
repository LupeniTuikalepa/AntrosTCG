using ATCG.Capacities.Data.Status;

namespace ATCG.Editor.Tools.DatabaseBrowser
{
    /// <summary>Browses every StatusData asset under Resources/Database/Status.</summary>
    public sealed class StatusBrowserTool : DatabaseBrowserTool<StatusData>
    {
        protected override string FolderPath => "Assets/Resources/Database/Status";
        public override string DisplayName => "Status";
        public override string Icon => "✦";
        public override int Order => 60;
    }
}
