using System.Reflection;
using Harmony;
using Verse;

namespace DD_WorkTab
{
	public class Controller : Mod
	{
		public Controller(ModContentPack content) : base(content)
		{
			var harmony = HarmonyInstance.Create("dingo.rimworld.dd_worktab");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}
	}
}
