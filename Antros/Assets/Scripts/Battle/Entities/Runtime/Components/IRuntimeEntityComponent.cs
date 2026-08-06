namespace ATCG.Battle.Entities.Runtime.Components
{
	public interface IRuntimeEntityComponent<T> where T : IEntityAspect
	{
		void Connect(T aspect,RuntimeEntity<T> runtimeEntity);
		
		void Disconnect(T aspect,RuntimeEntity<T> runtimeEntity);
	}
}