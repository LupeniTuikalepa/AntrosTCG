using ATCG.Databases;

namespace ATCG.Elements
{
    public static class ElementExtensions
    {
        public static bool TryGetData(this Element element, out ElementData data)
        {
            foreach (var elementData in GameDatabase.Global.GetAll<ElementData>())
            {
                if (elementData.Element == element)
                {
                    data = elementData;
                    return true;
                }
            }

            data = null;
            return false;
        }
    }
}