using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace DD_WorkTab
{
	public struct DD_WorkTypeInfo
	{
		public Texture2D texture;

		public Texture2D primaryTexture;

		public HashSet<PawnCapacityDef> allRequiredCapacities;

		public DD_WorkTypeInfo(Texture2D tex, Texture2D primaryTex, HashSet<PawnCapacityDef> capacities)
		{
			this.texture = tex;
			this.primaryTexture = primaryTex;
			this.allRequiredCapacities = capacities;
		}
	}
}
