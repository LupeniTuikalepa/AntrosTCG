using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Entities
{
    public unsafe struct ComponentMask
    {
        private const int SIZE = 4;
        private const int BIT_SIZE = 64;

        //4 × 64 = 256 bits max.
        //I could up the number more, but it should be enough for the game.
        //We'll see
        private fixed ulong bits[SIZE];

        public bool IsEmpty
        {
            get
            {
                for (int i = 0; i < SIZE; i++)
                    if (bits[i] != 0)
                        return false;

                return true;
            }
        }

        public void Set(int bit)
        {
            bits[bit / BIT_SIZE] |= 1UL << (bit % BIT_SIZE);
        }

        public void Clear(int bit)
        {
            bits[bit / BIT_SIZE] &= ~(1UL << (bit % BIT_SIZE));
        }

        public readonly bool Has(int bit)
        {
            return (bits[bit / BIT_SIZE] & (1UL << (bit % BIT_SIZE))) != 0;
        }

        public readonly bool MatchesAll(in ComponentMask mask)
        {
            for (int i = 0; i < 4; i++)
                if ((bits[i] & mask.bits[i]) != mask.bits[i])
                    return false;
            return true;
        }

        public readonly bool MatchesAny(in ComponentMask mask)
        {
            for (int i = 0; i < 4; i++)
                if ((bits[i] & mask.bits[i]) != 0)
                    return true;
            return false;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public static ComponentMaskBuilder With<T>() where T : struct, IEntityComponent
        {
            ComponentMaskBuilder builder = new();
            return builder.With<T>();
        }


        public ref struct Enumerator
        {
            private readonly ComponentMask mask;
            private int index;

            public Enumerator(ComponentMask mask)
            {
                this.mask = mask;
                index = -1;
            }

            public bool MoveNext()
            {
                while (++index < SIZE * BIT_SIZE)
                    if (mask.Has(index))
                        return true;

                return false;
            }

            public int Current => index;
        }
    }
}