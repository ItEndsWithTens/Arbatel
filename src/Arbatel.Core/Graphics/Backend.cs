using Arbatel.Controls;
using Arbatel.Formats;
using System.Collections.Generic;

namespace Arbatel.Graphics
{
	public class BackEnd
	{
		public Dictionary<string, int> Textures { get; } = new Dictionary<string, int>();

		public virtual void DrawMap(Map map, Dictionary<ShadingStyle, Shader> shaders, ShadingStyle style, object surface, Camera camera)
		{
			for (int i = 0; i < map.MapObjects.Count; i++)
			{
				DrawMapObject(map.MapObjects[i], shaders, style, surface, camera);
			}
		}

		public virtual void DrawMapObject(MapObject mapObject, Dictionary<ShadingStyle, Shader> shaders, ShadingStyle style, object surface, Camera camera)
		{
			if (!camera.CanSee(mapObject))
			{
				return;
			}

			for (int i = 0; i < mapObject.Children.Count; i++)
			{
				DrawMapObject(mapObject.Children[i], shaders, style, surface, camera);
			}

			for (int i = 0; i < mapObject.Renderables.Count; i++)
			{
				DrawRenderable(mapObject.Renderables[i], shaders, style, surface, camera);
			}
		}

		public virtual void DrawRenderable(Renderable r, Dictionary<ShadingStyle, Shader> shaders, ShadingStyle style, object surface, Camera camera)
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
			foreach (Shader shader in view.Shaders.Values)
			{
				InitMap(map, shader, view);
			}
		}
		public virtual void InitMap(Map map, Shader shader, object surface)
		{
			foreach (MapObject mo in map.MapObjects)
			{
				InitMapObject(mo, shader, surface);
			}
		}

		public virtual void InitMapObject(MapObject mapObject, Shader shader, object surface)
		{
			foreach (MapObject child in mapObject.Children)
			{
				InitMapObject(child, shader, surface);
			}

			foreach (Renderable r in mapObject.Renderables)
			{
				InitRenderable(r, shader, surface);
			}
		}

		public virtual void InitRenderable(Renderable renderable, Shader shader, object surface)
		{
		}

		public virtual void InitTextures(TextureDictionary dictionary)
		{
		}
	}
}
