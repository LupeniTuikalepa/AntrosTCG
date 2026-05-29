namespace Helteix.Tools.Phases
{
    public enum PhaseResultType
    {
        Success,
        Cancel,
        Failure,
    }

    public readonly struct PhaseResult<T>
    {
        public readonly T value;
        public readonly PhaseResultType type;

        internal PhaseResult(T value, PhaseResultType type)
        {
            this.value = value;
            this.type = type;
        }

        public static implicit operator T(PhaseResult<T> source) => source.value;

        public static implicit operator PhaseResultType(PhaseResult<T> source) => source.type;
    }
}