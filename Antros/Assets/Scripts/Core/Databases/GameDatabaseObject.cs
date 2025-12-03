using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Databases
{
    public abstract class GameDatabaseObject : ScriptableObject
    {
        [SerializeField, ReadOnly]
        private string guidText;

        public Guid ID
        {
            get
            {
                if (guid == Guid.Empty)
                    guid = Guid.Parse(guidText);

                return guid;
            }
            private set
            {
                guidText = value.ToString();
                guid = value;
            }
        }

        private Guid guid = Guid.Empty;

        private void OnValidate()
        {
            if(string.IsNullOrEmpty(guidText))
                SetNewGuid();
        }

        internal void SetNewGuid() =>  ID = Guid.NewGuid();
    }
}