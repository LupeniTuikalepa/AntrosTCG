using Unity.Services.Multiplayer;

namespace ATCG.Multiplayer
{
    public static class MultiplayerUtilities
    {
        public const string TRUE_VALUE = "TRUE";
        public const string FALSE_VALUE = "FALSE";

        public static PlayerProperty ToBoolProperty(this bool value, VisibilityPropertyOptions visibility = VisibilityPropertyOptions.Member) =>new PlayerProperty(value ? TRUE_VALUE : FALSE_VALUE, visibility);

        public static void SetBool(this IPlayer player, string key, bool value) => player.SetProperty(key, value.ToBoolProperty());
        public static bool GetBool(this IPlayer player, string key) => player.Properties.TryGetValue(key, out var property) && property.GetBool();
        public static bool GetBool(this IReadOnlyPlayer player, string key) => player.Properties.TryGetValue(key, out var property) && property.GetBool();

        public static bool GetBool(this PlayerProperty property) => property.Value == TRUE_VALUE;
    }
}