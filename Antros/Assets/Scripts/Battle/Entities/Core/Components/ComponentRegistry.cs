namespace ATCG.Battle.Entities.Core.Components
{
    public static class ComponentRegistry
    {
        private static int index;

        public static int Next() => index++;
    }
}