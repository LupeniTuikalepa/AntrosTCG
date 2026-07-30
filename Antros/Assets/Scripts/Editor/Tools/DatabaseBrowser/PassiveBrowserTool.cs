using ATCG.Passives.Datas;

namespace ATCG.Editor.Tools.DatabaseBrowser
{
    /// <summary>Browses every PassiveData asset under Resources/Database/Passives.</summary>
    public sealed class PassiveBrowserTool : DatabaseBrowserTool<PassiveData>
    {
        protected override string FolderPath => "Assets/Resources/Database/Passives";
        public override string DisplayName => "Passives";
        public override string Icon => "◆";
        public override int Order => 61;
    }
}
