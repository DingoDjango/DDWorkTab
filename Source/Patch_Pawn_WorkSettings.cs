using Harmony;
using RimWorld;
using Verse;

namespace DD_WorkTab
{
	[HarmonyPatch(typeof(Pawn_WorkSettings))]
	[HarmonyPatch("SetPriority")]
	public class Patch_Pawn_WorkSettings
	{
		public static void Prefix(ref object __state, ref int priority)
		{
			if (priority > Pawn_WorkSettings.LowestPriority)
			{
				__state = priority;
				priority = Pawn_WorkSettings.LowestPriority;
			}
		}

		public static void Postfix(Pawn_WorkSettings __instance, object __state, WorkTypeDef w)
		{
			if (__state != null)
			{
				DefMap<WorkTypeDef, int> prios = (DefMap<WorkTypeDef, int>)AccessTools.Field(__instance.GetType(), "priorities").GetValue(__instance);

				prios[w] = (int)__state;
			}
		}
	}
}
