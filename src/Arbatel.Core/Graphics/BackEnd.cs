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
			for (int i = 0; i < map.MapObjects.Count; i++)
			{
				DrawMapObject(map.MapObjects[i], shaders, style, view, camera);
			}
		}

		public virtual void DrawMapObject(MapObject mapObject, Dictionary<ShadingStyle, Shader> shaders, ShadingStyle style, View view, Camera camera)
		{
			if (!camera.CanSee(mapObject))
			{
				return;
			}

			for (int i = 0; i < mapObject.Children.Count; i++)
			{
				DrawMapObject(mapObject.Children[i], shaders, style, view, camera);
			}

			for (int i = 0; i < mapObject.Renderables.Count; i++)
			{
				DrawRenderable(mapObject.Renderables[i], shaders, style, view, camera);
			}
		}

		public virtual void DrawRenderable(Renderable r, Dictionary<ShadingStyle, Shader> shaders, ShadingStyle style, View view, Camera camera)
		{
		}

		public virtual void InitMap(Map map, List<View> views)
		{
			foreach (View view in views)
			{
				InitMap(map, view);
			}
		}
		public virtual void InitMap(Map map, View view)
		{
			foreach (MapObject mo in map.MapObjects)
			{
				InitMapObject(mo, view);
			}
		}

		public virtual void InitMapObject(MapObject mapObject, View view)
		{
			foreach (MapObject child in mapObject.Children)
			{
				InitMapObject(child, view);
			}

			foreach (Renderable r in mapObject.Renderables)
			{
				InitRenderable(r, view);
			}
		}

		public virtual void InitRenderable(Renderable renderable, View view)
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
