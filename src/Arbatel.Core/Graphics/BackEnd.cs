using Arbatel.Controls;
using Arbatel.Formats;
using Arbatel.UI;
using System;
using System.Collections.Generic;

namespace Arbatel.Graphics
{
	/// <summary>
	/// The backing data stores of a given graphics API.
	/// </summary>
	public class Buffers
	{
		public Buffers()
		{
		}

		public virtual void CleanUp()
		{
		}
	}

	public delegate void DrawMap(Map map, View view, Camera camera);

	public class BackEnd : IProgress
	{
		public virtual Dictionary<(Map, View), Buffers> Buffers { get; } = new Dictionary<(Map, View), Buffers>();

		public DrawMap DrawMap { get; set; }

		public event EventHandler<ProgressEventArgs> ProgressUpdated;

		public virtual void InitMap(Map map, List<View> views)
		{
			OnProgressUpdated(this, new ProgressEventArgs(50, "Initializing map in backend..."));

			map.Updated += Map_Updated;

			foreach (View view in views)
			{
				InitMap(map, view);
			}
		}
		protected virtual void InitMap(Map map, View view)
		{
			OnProgressUpdated(this, new ProgressEventArgs(50, "Initializing map in backend..."));

			foreach (Renderable r in map.MapObjects.GetAllRenderables())
			{
				r.Updated += Renderable_Updated;
			}
		}
		public virtual void DeleteMap(Map map, IEnumerable<View> views)
		{
			map.Updated -= Map_Updated;

			foreach (Renderable r in map.MapObjects.GetAllRenderables())
			{
				r.Updated -= Renderable_Updated;
			}

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

		public virtual void InitRenderables(Buffers buffers, IEnumerable<Renderable> renderables)
		{
		}
		public virtual void UpdateRenderables(Buffers buffers, IEnumerable<Renderable> renderables)
		{
			foreach (Renderable r in renderables)
			{
				UpdateRenderable(buffers, r);
			}
		}
		public virtual void UpdateRenderable(Buffers buffers, Renderable renderable)
		{
		}

		public virtual void InitTextures(TextureDictionary dictionary)
		{
		}
		public virtual void InitTexture(Texture texture)
		{
		}
		public virtual void DeleteTextures()
		{
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

		private void Map_Updated(object sender, EventArgs e)
		{
			if (sender is Map m)
			{
				DeleteTextures();

				InitTextures(m.Textures);
			}
		}

		protected virtual void Renderable_Updated(object sender, EventArgs e)
		{
			if (sender is Renderable r)
			{
				foreach (KeyValuePair<(Map, View), Buffers> pair in Buffers)
				{
					UpdateRenderable(pair.Value, r);
				}
			}
		}

		public virtual void OnProgressUpdated(object sender, ProgressEventArgs e)
		{
			ProgressUpdated?.Invoke(this, e);
		}
	}
}
