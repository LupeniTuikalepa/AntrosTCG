using UnityEditor;

namespace Helteix.Tools.Editor.Settings
{
    public class GameSettingsAssetProcessor : AssetModificationProcessor
    {
        private static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
        {
            if (GameSettingsAssetDatabase.OnWillDeleteSettings(assetPath))
            {
                return AssetDeleteResult.DidNotDelete;
            }

            return AssetDeleteResult.DidNotDelete;
        }
    }
}