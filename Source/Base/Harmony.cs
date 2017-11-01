using Harmony;
using RimWorld;
using Verse;

namespace DD_WorkTab
{
	class Harmony
	{
		[HarmonyPatch(typeof(Pawn_WorkSettings))]
		[HarmonyPatch(nameof(Pawn_WorkSettings.SetPriority))]
		class Patch_Pawn_WorkSettings
		{
			static void Prefix(ref object __state, ref int priority)
			{
				if (priority > Pawn_WorkSettings.LowestPriority)
				{
					__state = priority;
					priority = Pawn_WorkSettings.LowestPriority;
				}
			}

			static void Postfix(Pawn_WorkSettings __instance, object __state, WorkTypeDef w)
			{
				if (__state != null)
				{
					DefMap<WorkTypeDef, int> prios = (DefMap<WorkTypeDef, int>)AccessTools.Field(__instance.GetType(), "priorities").GetValue(__instance);

					prios[w] = (int)__state;
				}
			}
		}

		[HarmonyPatch(typeof(SkillRecord))]
		[HarmonyPatch(nameof(SkillRecord.Learn))]
		class Patch_SkillRecord
		{
			static void Prefix(SkillRecord __instance, ref object __state)
			{
				__state = __instance.levelInt;
			}

			static void Postfix(SkillRecord __instance, object __state)
			{
				if ((int)__state != __instance.levelInt)
				{
					Pawn p = AccessTools.Field(typeof(SkillRecord), "pawn").GetValue(__instance) as Pawn;

					Controller.GetManager.GetPawnSurface(p)?.RecacheDraggableOutlines();
				}
			}
		}
	}
}
