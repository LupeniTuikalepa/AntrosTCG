using System;
using ATCG.Passives.Datas;
using UnityEngine;

namespace ATCG.Battle.PassiveSystem.Runtimes
{
    public abstract class RuntimePassive : MonoBehaviour
    {
        public abstract void Apply(RuntimePassiveContext context);

        public abstract void Remove(RuntimePassiveContext context);

        public abstract void Tick(RuntimePassiveContext context);
    }
}