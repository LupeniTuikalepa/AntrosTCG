using ATCG.Databases;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Elements
{
    [CreateAssetMenu(fileName = "ElementData", menuName = "ATCG/ElementData")]
    public class ElementData : GameDatabaseObject
    {
        [field: SerializeField]
        public Element Element { get; private set; }

        [field: SerializeField]
        public string Name { get; private set; }
        [field: SerializeField, TextArea(4, 15)]
        public string Description { get; private set; }

        [field: SerializeField, PreviewField]
        public Sprite Icon { get; private set; }

        [field: SerializeField, ColorUsage(false)]
        public Color Color { get; private set; } = Color.white;
    }


}