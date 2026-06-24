namespace Cheats.Core
{
	public interface ICheat 
	{
		public string Name { get; }
		public string Description { get; }
		
		public void Execute(in CheatContext context);
		
	}
}
