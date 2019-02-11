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
		public int UboTextureInfo;

		public Buffers()
		{
			GL.GenVertexArrays(1, out Vao);
			GL.GenBuffers(1, out Vbo);
			GL.GenBuffers(1, out Ebo);
			GL.GenBuffers(1, out UboMatrices);
			GL.GenBuffers(1, out UboTextureInfo);
		}

		public void CleanUp()
		{
			GL.DeleteBuffer(UboTextureInfo);
			GL.DeleteBuffer(UboMatrices);
			GL.DeleteBuffer(Ebo);
			GL.DeleteBuffer(Vbo);
			GL.DeleteVertexArray(Vao);
		}
	}

	public class OpenGL4BackEnd : BackEnd
	{
		public Dictionary<(Map, View), Buffers> Buffers { get; } = new Dictionary<(Map, View), Buffers>();

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

			GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, b.UboMatrices);

			shaders[ShadingStyle.Flat].Use();
			shaders[ShadingStyle.Flat].DrawWorld(flatWorld, camera);
			shaders[ShadingStyle.Flat].DrawModel(flatModel, camera);

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
				where r.ShadingStyleDict[view.ShadingStyle] <= ShadingStyle.Flat
				select r;

			IEnumerable<Renderable> flatWorld =
				from r in flat
				where r.ModelMatrix == Matrix4.Identity
				select r;

			IEnumerable<Renderable> flatModel =
				from r in flat
				where r.ModelMatrix != Matrix4.Identity
				select r;

			Buffers b = Buffers[(map, view)];

			GL.BindVertexArray(b.Vao);

			shaders[ShadingStyle.Textured].Use();
			shaders[ShadingStyle.Textured].DrawWorld(texturedWorld, camera);
			shaders[ShadingStyle.Textured].DrawModel(texturedModel, camera);

			shaders[ShadingStyle.Flat].Use();
			shaders[ShadingStyle.Flat].DrawWorld(flatWorld, camera);
			shaders[ShadingStyle.Flat].DrawModel(flatModel, camera);

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

				if (shader.Value is SingleTextureShader t)
				{
					(bindingPoint, index, name) = shader.Value.Ubos["TextureInfo"];
					name = buffers.UboTextureInfo;
					shader.Value.Ubos["TextureInfo"] = (bindingPoint, index, name);
				}
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

			IEnumerable<Renderable> renderables = map.AllObjects.GetAllRenderables();

			InitRenderables(buffers, renderables, map, view);

			GL.BindVertexArray(0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
		}
		protected override void DeleteMap(Map map, View view)
		{
			if (!Buffers.ContainsKey((map, view)))
			{
				return;
			}

			Buffers[(map, view)].CleanUp();
			Buffers.Remove((map, view));
		}

		public struct QuakeTextureInfo
		{
			public float Width;
			public float Height;
			public Vector4 BasisS;
			public Vector4 BasisT;
			public Vector2 Offset;
			public Vector2 Scale;
		}

		public override void InitRenderables(Buffers buffers, IEnumerable<Renderable> renderables, Map map, View view)
		{
			GL.GetInteger(GetPName.UniformBufferOffsetAlignment, out int uboAlignment);

			int totalVertexBytes = 0;
			int totalIndexBytes = 0;
			int totalMatrixBytes = Vector4.SizeInBytes * 4 * 2; // projectionMatrix, viewMatrix
			int totalTextureInfoBytes = 0;

			int minimumTextureInfoBytes =
				sizeof(float) * 2 + // textureWidth, textureHeight
				Vector4.SizeInBytes * 2 + // basisS, basisT (padded vec3s)
				Vector2.SizeInBytes * 2; // offset, scale
			int textureInfoBytesStep = uboAlignment;
			while (minimumTextureInfoBytes % textureInfoBytesStep != minimumTextureInfoBytes)
			{
				textureInfoBytesStep += uboAlignment;
			}

			foreach (Renderable r in renderables)
			{
				totalVertexBytes += Vertex.MemorySize * r.Vertices.Count;
				totalIndexBytes += sizeof(int) * r.Indices.Count;
				for (int p = 0; p < r.Polygons.Count; p++)
				{
					totalTextureInfoBytes += textureInfoBytesStep;
				}
			}

			GL.BufferData(BufferTarget.ArrayBuffer, totalVertexBytes, IntPtr.Zero, BufferUsageHint.StaticDraw);
			GL.BufferData(BufferTarget.ElementArrayBuffer, totalIndexBytes, IntPtr.Zero, BufferUsageHint.StaticDraw);

			GL.BindBuffer(BufferTarget.UniformBuffer, buffers.UboMatrices);
			GL.BufferData(BufferTarget.UniformBuffer, totalMatrixBytes, IntPtr.Zero, BufferUsageHint.StaticDraw);
			GL.BindBuffer(BufferTarget.UniformBuffer, 0);

			GL.BindBuffer(BufferTarget.UniformBuffer, buffers.UboTextureInfo);
			GL.BufferData(BufferTarget.UniformBuffer, totalTextureInfoBytes, IntPtr.Zero, BufferUsageHint.StaticDraw);
			GL.BindBuffer(BufferTarget.UniformBuffer, 0);

			GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 1, buffers.UboTextureInfo);

			int verticesSoFar = 0;
			IntPtr vboOffset = IntPtr.Zero;

			int indicesSoFar = 0;
			IntPtr eboOffset = IntPtr.Zero;

			IntPtr textureInfoBufferOffset = IntPtr.Zero;
			foreach (Renderable r in renderables)
			{
				r.VertexOffset = vboOffset;
				r.IndexOffset = eboOffset;

				IntPtr polygonIndexOffset = r.IndexOffset;
				foreach (Polygon p in r.Polygons)
				{
					p.IndexOffset = polygonIndexOffset;
					polygonIndexOffset += p.Indices.Count * sizeof(int);
					p.TextureInfoOffset = textureInfoBufferOffset;

					if (r.ShadingStyleDict[ShadingStyle.Textured] == ShadingStyle.Textured)
					{
						var textureInfo = new QuakeTextureInfo
						{
							Width = p.Texture.Width,
							Height = p.Texture.Height,
							BasisS = new Vector4(p.BasisS),
							BasisT = new Vector4(p.BasisT),
							Offset = p.Offset,
							Scale = p.Scale
						};

						unsafe
						{
							GL.BufferSubData(
								BufferTarget.UniformBuffer,
								textureInfoBufferOffset + 0,
								sizeof(float),
								(IntPtr)(&textureInfo.Width));

							GL.BufferSubData(
								BufferTarget.UniformBuffer,
								textureInfoBufferOffset + 4,
								sizeof(float),
								(IntPtr)(&textureInfo.Height));

							GL.BufferSubData(
								BufferTarget.UniformBuffer,
								textureInfoBufferOffset + 16,
								Vector4.SizeInBytes,
								(IntPtr)(&textureInfo.BasisS));

							GL.BufferSubData(
								BufferTarget.UniformBuffer,
								textureInfoBufferOffset + 32,
								Vector4.SizeInBytes,
								(IntPtr)(&textureInfo.BasisT));

							GL.BufferSubData(
								BufferTarget.UniformBuffer,
								textureInfoBufferOffset + 48,
								Vector2.SizeInBytes,
								(IntPtr)(&textureInfo.Offset));

							GL.BufferSubData(
								BufferTarget.UniformBuffer,
								textureInfoBufferOffset + 56,
								Vector2.SizeInBytes,
								(IntPtr)(&textureInfo.Scale));
						}
					}

					textureInfoBufferOffset += textureInfoBytesStep;
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

			GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 1, 0);
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
