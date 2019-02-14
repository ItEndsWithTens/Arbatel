using Arbatel.Controls;
using Arbatel.Formats;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Arbatel.Graphics
{
	public class Buffers
	{
		public int Vao;
		public int Vbo;
		public int Ebo;
		public int UboMatrices;

		public Buffers()
		{
			GL.GenVertexArrays(1, out Vao);
			GL.GenBuffers(1, out Vbo);
			GL.GenBuffers(1, out Ebo);
			GL.GenBuffers(1, out UboMatrices);
		}

		public void CleanUp()
		{
			GL.DeleteBuffer(UboMatrices);
			GL.DeleteBuffer(Ebo);
			GL.DeleteBuffer(Vbo);
			GL.DeleteVertexArray(Vao);
		}
	}

	public class OpenGL4BackEnd : BackEnd
	{
		public Dictionary<(Map, View), Buffers> Buffers { get; } = new Dictionary<(Map, View), Buffers>();

		public void DrawMapWireframe(Map map, Dictionary<ShadingStyle, Shader> shaders, ShadingStyle style, View view, Camera camera)
		{
			IEnumerable<MapObject> visible = camera.GetVisibleMapObjects(map.AllObjects);

			IEnumerable<Renderable> renderables = visible.GetAllRenderables();

			IEnumerable<Renderable> wireframeWorld =
				from r in renderables
				where r.ModelMatrix == Matrix4.Identity
				select r;

			IEnumerable<Renderable> wireframeModel =
				from r in renderables
				where r.ModelMatrix != Matrix4.Identity
				select r;

			Buffers b = Buffers[(map, view)];

			GL.BindVertexArray(b.Vao);

			shaders[ShadingStyle.Wireframe].Draw(wireframeWorld, wireframeModel, camera);

			GL.BindVertexArray(0);
		}

		public void DrawMapFlat(Map map, Dictionary<ShadingStyle, Shader> shaders, ShadingStyle style, View view, Camera camera)
		{
			IEnumerable<MapObject> visible = camera.GetVisibleMapObjects(map.AllObjects);

			IEnumerable<Renderable> renderables = visible.GetAllRenderables();

			IEnumerable<Renderable> flatWorld =
				from r in renderables
				where r.ModelMatrix == Matrix4.Identity
				select r;

			IEnumerable<Renderable> flatModel =
				from r in renderables
				where r.ModelMatrix != Matrix4.Identity
				select r;

			Buffers b = Buffers[(map, view)];

			GL.BindVertexArray(b.Vao);

			shaders[ShadingStyle.Flat].Draw(flatWorld, flatModel, camera);

			GL.BindVertexArray(0);
		}
		public void DrawMapTextured(Map map, Dictionary<ShadingStyle, Shader> shaders, ShadingStyle style, View view, Camera camera)
		{
			IEnumerable<MapObject> visible = camera.GetVisibleMapObjects(map.AllObjects);

			IEnumerable<Renderable> renderables = visible.GetAllRenderables();

			IEnumerable<Renderable> textured =
				from r in renderables
				where r.ShadingStyleDict[view.ShadingStyle] == ShadingStyle.Textured
				select r;

			IEnumerable<Renderable> texturedWorld =
				from r in textured
				where r.ModelMatrix == Matrix4.Identity
				select r;

			IEnumerable<Renderable> texturedModel =
				from r in textured
				where r.ModelMatrix != Matrix4.Identity
				select r;

			IEnumerable<Renderable> flat =
				from r in renderables
				where r.ShadingStyleDict[view.ShadingStyle] == ShadingStyle.Flat
				select r;

			IEnumerable<Renderable> flatWorld =
				from r in flat
				where r.ModelMatrix == Matrix4.Identity
				select r;

			IEnumerable<Renderable> flatModel =
				from r in flat
				where r.ModelMatrix != Matrix4.Identity
				select r;

			IEnumerable<Renderable> wireframe =
				from r in renderables
				where r.ShadingStyleDict[view.ShadingStyle] == ShadingStyle.Wireframe
				select r;

			IEnumerable<Renderable> wireframeWorld =
				from r in wireframe
				where r.ModelMatrix == Matrix4.Identity
				select r;

			IEnumerable<Renderable> wireframeModel =
				from r in wireframe
				where r.ModelMatrix != Matrix4.Identity
				select r;

			Buffers b = Buffers[(map, view)];

			GL.BindVertexArray(b.Vao);

			shaders[ShadingStyle.Textured].Draw(texturedWorld, texturedModel, camera);
			shaders[ShadingStyle.Flat].Draw(flatWorld, flatModel, camera);
			shaders[ShadingStyle.Wireframe].Draw(wireframeWorld, wireframeModel, camera);

			GL.BindVertexArray(0);
		}

		protected override void InitMap(Map map, View view)
		{
			var buffers = new Buffers();
			Buffers.Add((map, view), buffers);

			foreach (KeyValuePair<ShadingStyle, Shader> shader in view.Shaders)
			{
				(int bindingPoint, int index, int name) = shader.Value.Ubos["Matrices"];
				name = buffers.UboMatrices;
				shader.Value.Ubos["Matrices"] = (bindingPoint, index, name);
			}

			GL.BindVertexArray(buffers.Vao);
			GL.BindBuffer(BufferTarget.ArrayBuffer, buffers.Vbo);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, buffers.Ebo);

			GL.VertexAttribPointer(Shader.Locations["position"], 3, VertexAttribPointerType.Float, false, Vertex.MemorySize, 0);
			GL.EnableVertexAttribArray(Shader.Locations["position"]);

			GL.VertexAttribPointer(Shader.Locations["normal"], 3, VertexAttribPointerType.Float, false, Vertex.MemorySize, sizeof(float) * 3);
			GL.EnableVertexAttribArray(Shader.Locations["normal"]);

			GL.VertexAttribPointer(Shader.Locations["color"], 4, VertexAttribPointerType.Float, false, Vertex.MemorySize, sizeof(float) * 6);
			GL.EnableVertexAttribArray(Shader.Locations["color"]);

			GL.VertexAttribPointer(Shader.Locations["texCoords"], 2, VertexAttribPointerType.Float, false, Vertex.MemorySize, sizeof(float) * 10);
			GL.EnableVertexAttribArray(Shader.Locations["texCoords"]);

			IEnumerable<Renderable> renderables = map.AllObjects.GetAllRenderables();

			InitRenderables(buffers, renderables, map, view);

			GL.BindVertexArray(0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
		}
		protected override void DeleteMap(Map map, View view)
		{
			base.DeleteMap(map, view);

			if (!Buffers.ContainsKey((map, view)))
			{
				return;
			}

			Buffers[(map, view)].CleanUp();
			Buffers.Remove((map, view));
		}

		public override void InitRenderables(Buffers buffers, IEnumerable<Renderable> renderables, Map map, View view)
		{
			int totalVertexBytes = 0;
			int totalIndexBytes = 0;
			int totalMatrixBytes = Vector4.SizeInBytes * 4 * 2; // projectionMatrix, viewMatrix

			foreach (Renderable r in renderables)
			{
				totalVertexBytes += Vertex.MemorySize * r.Vertices.Count;
				totalIndexBytes += sizeof(int) * r.Indices.Count;
			}

			GL.BufferData(BufferTarget.ArrayBuffer, totalVertexBytes, IntPtr.Zero, BufferUsageHint.StaticDraw);
			GL.BufferData(BufferTarget.ElementArrayBuffer, totalIndexBytes, IntPtr.Zero, BufferUsageHint.StaticDraw);

			GL.BindBuffer(BufferTarget.UniformBuffer, buffers.UboMatrices);
			GL.BufferData(BufferTarget.UniformBuffer, totalMatrixBytes, IntPtr.Zero, BufferUsageHint.StaticDraw);
			GL.BindBuffer(BufferTarget.UniformBuffer, 0);

			int verticesSoFar = 0;
			IntPtr vboOffset = IntPtr.Zero;

			int indicesSoFar = 0;
			IntPtr eboOffset = IntPtr.Zero;

			foreach (Renderable r in renderables)
			{
				r.VertexOffset = vboOffset;
				r.IndexOffset = eboOffset;

				IntPtr polygonIndexOffset = r.IndexOffset;
				foreach (Polygon p in r.Polygons)
				{
					p.IndexOffset = polygonIndexOffset;
					polygonIndexOffset += p.Indices.Count * sizeof(int);
				}

				int totalVerticesBytes = Vertex.MemorySize * r.Vertices.Count;
				GL.BufferSubData(
					BufferTarget.ArrayBuffer,
					vboOffset,
					totalVerticesBytes,
					r.Vertices.ToArray());

				int totalIndicesBytes = sizeof(int) * r.Indices.Count;
				IEnumerable<int> shiftedIndices = r.Indices.Select(i => verticesSoFar + i);
				GL.BufferSubData(
					BufferTarget.ElementArrayBuffer,
					eboOffset,
					totalIndicesBytes,
					shiftedIndices.ToArray());

				verticesSoFar += r.Vertices.Count;
				vboOffset += totalVerticesBytes;

				indicesSoFar += r.Indices.Count;
				eboOffset += totalIndicesBytes;
			}
		}

		public override void InitTextures(TextureDictionary dictionary)
		{
			foreach (Texture t in dictionary.Values)
			{
				if (Textures.ContainsKey(t.Name))
				{
					continue;
				}

				GL.GenTextures(1, out int id);
				Textures.Add(t.Name, id);

				GL.BindTexture(TextureTarget.Texture2D, id);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, t.Width, t.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, t.ToUncompressed(Eto.Drawing.PixelFormat.Format32bppRgba, flip: true));
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
				GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
				GL.BindTexture(TextureTarget.Texture2D, 0);
			}
		}
		public override void DeleteTextures(TextureDictionary dictionary)
		{
			foreach (Texture t in dictionary.Values)
			{
				GL.DeleteTexture(Textures[t.Name]);

				Textures.Remove(t.Name);
			}
		}
	}
}
