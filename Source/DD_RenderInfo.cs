using UnityEngine;

namespace DD_WorkTab
{
	public struct DD_RenderInfo
	{
		public Texture2D texture;

		public Texture2D primaryTexture;

		public DD_RenderInfo(Texture2D tex, Texture2D primaryTex)
		{
			this.texture = tex;
			this.primaryTexture = primaryTex;
		}
	}
}
