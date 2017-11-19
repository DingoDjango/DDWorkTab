using Harmony;
using RimWorld;
using Verse;

namespace DD_WorkTab.Base
{
	//Patch to allow any priority value in Pawn_WorkSettings
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
				DefMap<WorkTypeDef, int> pawnPriorities = AccessTools.Field(typeof(Pawn_WorkSettings), "priorities").GetValue(__instance) as DefMap<WorkTypeDef, int>;

				pawnPriorities[w] = (int)__state;
			}
		}
	}

	//Saves some performance by recaching draggable colours only when skills change in value
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
				Pawn pawn = AccessTools.Field(typeof(SkillRecord), "pawn").GetValue(__instance) as Pawn;

				if (pawn.IsColonist)
				{
					Controller.GetManager.GetPawnSurface(pawn).RecacheDraggableOutlines();
				}
			}
		}
	}
}
