using Arbatel.Controls;
using Arbatel.Formats;
using System;
using System.Collections.Generic;

namespace Arbatel.Graphics
{
	public delegate void DrawMap(Map map, Dictionary<ShadingStyle, Shader> shaders, ShadingStyle style, View view, Camera camera);

	public class BackEnd
	{
		public Dictionary<string, int> Textures { get; } = new Dictionary<string, int>();

		public DrawMap DrawMap { get; set; }

		public virtual void InitMap(Map map, List<View> views)
		{
			map.Updated += Map_Updated;

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
			map.Updated -= Map_Updated;

			DeleteTextures(map.Textures);

			foreach (View view in views)
			{
				DeleteMap(map, view);
			}
		}
		protected virtual void DeleteMap(Map map, View view)
		{
			view.Camera.Clear();
		}

		public virtual void InitRenderables(Buffers buffers, IEnumerable<Renderable> renderables, Map map, View view)
		{
		}

		public virtual void InitTextures(TextureDictionary dictionary)
		{
			foreach (Texture t in dictionary.Values)
			{
				if (Textures.ContainsKey(t.Name))
				{
					continue;
				}

				InitTexture(t);
			}
		}
		public virtual void InitTexture(Texture texture)
		{
		}
		public virtual void DeleteTextures()
		{
			foreach (KeyValuePair<string, int> pair in Textures)
			{
				DeleteTexture(pair.Value);
			}

			Textures.Clear();
		}
		public virtual void DeleteTextures(TextureDictionary dictionary)
		{
			foreach (Texture t in dictionary.Values)
			{
				DeleteTexture(t);
			}
		}
		public virtual void DeleteTexture(Texture texture)
		{
			DeleteTexture(texture.Name);
		}
		public virtual void DeleteTexture(string name)
		{
		}
		public virtual void DeleteTexture(int id)
		{
		}

		private void Map_Updated(object sender, EventArgs e)
		{
			if (sender is Map m)
			{
				DeleteTextures();

				InitTextures(m.Textures);
			}
		}
	}
}
