using System.Collections.Generic;
using Helteix.Tools.Settings;
using UnityEngine;

namespace ATCG.Cards
{
    /// <summary>
    /// Debug-only starting-deck config (singleton). Stores the GUIDs of the cards that make up the
    /// debug launch deck (used by BattleLauncher), edited from the Cards Manager window. This keeps the
    /// selection off the card assets themselves — it's temporary until a real deckbuilder exists.
    /// </summary>
    [AutoGenerateGameSettings, GameSettingsPath("Antros/Debug/Starting Deck")]
    public class DebugStartingDeck : GameSettings<DebugStartingDeck>
    {
        [SerializeField]
        private List<string> activeCardGuids = new();

        public int ActiveCount => activeCardGuids.Count;

        public bool IsActive(GameCardData card)
            => card != null && activeCardGuids.Contains(card.ID.ToString());

        /// <summary>Active cards; if nothing is selected, everything is returned (debug safety).</summary>
        public IEnumerable<GameCardData> Filter(IEnumerable<GameCardData> all)
        {
            if (activeCardGuids.Count == 0)
                return all;

            List<GameCardData> result = new();
            foreach (GameCardData card in all)
                if (IsActive(card))
                    result.Add(card);

            return result;
        }

#if UNITY_EDITOR
        public void EditorSetActive(GameCardData card, bool active)
        {
            if (card == null)
                return;

            string id = card.ID.ToString();
            if (active)
            {
                if (activeCardGuids.Contains(id))
                    return;
                activeCardGuids.Add(id);
            }
            else if (!activeCardGuids.Remove(id))
            {
                return;
            }

            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
