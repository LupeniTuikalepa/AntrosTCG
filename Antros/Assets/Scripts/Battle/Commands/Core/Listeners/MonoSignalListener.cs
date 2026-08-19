using ATCG.Battle.Commands.Players;

namespace ATCG.Battle.Commands.Listeners
{
    public abstract class MonoSignalListener : 
        MonoBaseSignalListener<CommandSignal>,
        ISignalListener
    {
        
    }
}