using ATCG.Battle.Entities.Core.Components;
using Unity.Cinemachine;

namespace ATCG.Battle.Entities.Core
{
    public unsafe struct ComponentMask
    {
        //4 × 64 = 256 bits max.
        //I could up the number more, but it should be enough for the game.
        //We'll see
        private fixed ulong bits[4];

        public void Set(int bit) => bits[bit / 64] |= (1UL << (bit % 64));
        public void Clear(int bit) => bits[bit / 64] &= ~(1UL << (bit % 64));

        public readonly bool Has(int bit) => (bits[bit / 64] & (1UL << (bit % 64))) != 0;

        public readonly bool MatchesAll(in ComponentMask query)
        {
            for (int i = 0; i < 4; i++)
            {
                if ((bits[i] & query.bits[i]) != query.bits[i])
                    return false;
            }
            return true;
        }

        public readonly bool MatchesAny(in ComponentMask query)
        {
            for (int i = 0; i < 4; i++)
            {
                if ((bits[i] & query.bits[i]) != 0)
                    return true;
            }
            return false;
        }

        public static ComponentMask With<T>() where T : struct, IEntityComponent
        {
            ComponentMaskBuilder builder = new ComponentMaskBuilder();
            return builder.With<T>();
        }
    }
}