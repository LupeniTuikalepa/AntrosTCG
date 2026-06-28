using System;
using System.Reflection;
using ATCG.Battle.Entities.Components;
using UnityEngine;

namespace ATCG.Battle.Entities.EditorTools
{
    /// <summary>
    /// Builds a human-friendly label and a player pastille color for an entity, shared
    /// by the entity list and the grid so naming/coloring stays consistent.
    ///
    ///   - Name: the card Title (BattleCardComponent.battleCard.Title) when present,
    ///           otherwise "Entity {id}".
    ///   - Color: the owning player's color (BelongsToPlayerComponent.playerNumber),
    ///           or white when the entity has no player.
    ///
    /// All component access is by cached id + reflection on the boxed value, editor-side.
    /// </summary>
    public static class EntityLabel
    {
        public readonly struct Info
        {
            public readonly string Name;
            public readonly Color Pastille;
            public readonly bool Active;

            public Info(string name, Color pastille, bool active)
            {
                Name = name;
                Pastille = pastille;
                Active = active;
            }
        }

        private static int battleCardId = -1;
        private static int belongsToPlayerId = -1;
        private static int battleCellId = -1;
        private static int gridMemberId = -1;
        private static FieldInfo battleCardField;
        private static PropertyInfo titleProp;
        private static FieldInfo playerNumberField;
        private static FieldInfo gridCoordinatesField;

        public static Info Build(World world, int entityId)
        {
            string name = $"Entity {entityId}";
            Color color = PlayerColorResolver.NoPlayer;
            bool active = false;

            Entity entity = new(entityId);
            try { active = world.IsActive(entity); } catch { /* teardown race */ }

            if (!world.IsAlive(entity))
                return new Info(name, color, active);

            EntityMeta meta;
            try { meta = world.GetMeta(entity); }
            catch { return new Info(name, color, active); }

            // Naming priority: card title > cell coordinate > default "Entity N".
            EnsureIds();

            if (battleCardId >= 0 && meta.HasComponent(battleCardId))
            {
                string title = TryGetCardTitle(world, entityId);
                if (!string.IsNullOrEmpty(title))
                    name = title;
            }
            else if (battleCellId >= 0 && meta.HasComponent(battleCellId))
            {
                // A cell entity: name it by its grid coordinate.
                if (TryGetCellCoordinate(world, entityId, out string coord))
                    name = $"Cell {coord}";
            }

            // Player pastille.
            if (belongsToPlayerId >= 0 && meta.HasComponent(belongsToPlayerId))
            {
                if (TryGetPlayerNumber(world, entityId, out int playerNumber))
                    color = PlayerColorResolver.ForPlayerNumber(playerNumber);
            }

            return new Info(name, color, active);
        }

        private static void EnsureIds()
        {
            if (battleCardId < 0)
                battleCardId = FindId(typeof(BattleCardComponent));
            if (belongsToPlayerId < 0)
                belongsToPlayerId = FindId(typeof(BelongsToPlayerComponent));
            if (battleCellId < 0)
                battleCellId = FindId(typeof(BattleCellComponent));
            if (gridMemberId < 0)
                gridMemberId = FindId(typeof(GridMemberComponent));
        }

        private static bool TryGetCellCoordinate(World world, int entityId, out string coord)
        {
            coord = null;
            if (gridMemberId < 0)
                return false;

            object boxed = GetBoxed(world, gridMemberId, entityId);
            if (boxed == null)
                return false;

            gridCoordinatesField ??= typeof(GridMemberComponent).GetField(
                "coordinates", BindingFlags.Public | BindingFlags.Instance);
            if (gridCoordinatesField == null)
                return false;

            object hex;
            try { hex = gridCoordinatesField.GetValue(boxed); }
            catch { return false; }
            if (hex == null)
                return false;

            coord = hex.ToString(); // HexCoordinates.ToString() => "(X : Y)"
            return !string.IsNullOrEmpty(coord);
        }

        private static string TryGetCardTitle(World world, int entityId)
        {
            object boxed = GetBoxed(world, battleCardId, entityId);
            if (boxed == null)
                return null;

            battleCardField ??= typeof(BattleCardComponent).GetField(
                "battleCard", BindingFlags.Public | BindingFlags.Instance);
            if (battleCardField == null)
                return null;

            object card;
            try { card = battleCardField.GetValue(boxed); }
            catch { return null; }
            if (card == null)
                return null;

            titleProp ??= card.GetType().GetProperty("Title", BindingFlags.Public | BindingFlags.Instance);
            if (titleProp == null)
                return null;

            try { return titleProp.GetValue(card) as string; }
            catch { return null; }
        }

        private static bool TryGetPlayerNumber(World world, int entityId, out int playerNumber)
        {
            playerNumber = 0;
            object boxed = GetBoxed(world, belongsToPlayerId, entityId);
            if (boxed == null)
                return false;

            playerNumberField ??= typeof(BelongsToPlayerComponent).GetField(
                "playerNumber", BindingFlags.Public | BindingFlags.Instance);
            if (playerNumberField == null)
                return false;

            try
            {
                playerNumber = Convert.ToInt32(playerNumberField.GetValue(boxed));
                return true;
            }
            catch { return false; }
        }

        private static object GetBoxed(World world, int componentId, int entityId)
        {
            IComponentStore store;
            try { store = world.GetStore(componentId); }
            catch { return null; }
            if (store == null)
                return null;
            try { return store.GetBoxed(entityId); }
            catch { return null; }
        }

        private static int FindId(Type componentType)
        {
            for (int id = 0; id < ComponentRegistry.MaxComponents; id++)
            {
                if (ComponentRegistry.GetTypeForComponentID(id) == componentType)
                    return id;
            }
            return -1;
        }
    }
}