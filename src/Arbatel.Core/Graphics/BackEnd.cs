using Arbatel.Controls;
using Arbatel.Formats;
using System.Collections.Generic;

namespace Arbatel.Graphics
{
	public class BackEnd
	{
		public Dictionary<string, int> Textures { get; } = new Dictionary<string, int>();

		public virtual void DrawMap(Map map, Dictionary<ShadingStyle, Shader> shaders, ShadingStyle style, View view, Camera camera)
		{
		}

		public virtual void InitMap(Map map, List<View> views)
		{
			foreach (View view in views)
			{
				InitMap(map, view);
			}
		}
		protected virtual void InitMap(Map map, View view)
		{
		}
		public virtual void DeleteMap(Map map, IEnumerable<View> views)
		{
			DeleteTextures(map.Textures);

			foreach (View view in views)
			{
				DeleteMap(map, view);
			}
		}
		protected virtual void DeleteMap(Map map, View view)
		{
		}

		public virtual void InitRenderables(IEnumerable<Renderable> renderables, View view)
		{
		}

		public virtual void InitTextures(TextureDictionary dictionary)
		{
		}
		public virtual void DeleteTextures(TextureDictionary dictionary)
		{
		}
	}
}
