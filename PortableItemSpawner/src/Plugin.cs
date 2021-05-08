using Deli.Setup;

namespace PortableItemSpawner
{
	public class Plugin : DeliBehaviour
	{
		private readonly Hooks _hooks;

		public Plugin()
		{
			_hooks = new Hooks();
			_hooks.Hook();
		}
	}
}