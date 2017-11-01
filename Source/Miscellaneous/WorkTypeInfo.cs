using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace DD_WorkTab.Miscellaneous
{
	public class WorkTypeInfo
	{
		public readonly Texture2D texture64;

		public readonly Texture2D texture64_Disabled;

		public readonly Texture2D texture64_Greyscale;

		public readonly HashSet<PawnCapacityDef> allRequiredCapacities;

		public WorkTypeInfo(Texture2D texture64, Texture2D texture64_Disabled, Texture2D texture64_Greyscale, HashSet<PawnCapacityDef> capacities)
		{
			this.texture64 = texture64;
			this.texture64_Disabled = texture64_Disabled;
			this.texture64_Greyscale = texture64_Greyscale;
			this.allRequiredCapacities = capacities;
		}
	}
}
