using Arbatel.Controls;
using Arbatel.Formats;
using Arbatel.UI;
using Eto;
using Eto.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Arbatel.Graphics
{
	public class OpenGLBuffers : Buffers
	{
		public int Vao { get; }
		public int Vbo { get; }
		public int Ebo { get; }
		public int LineLoopEbo { get; }
		public int UboMatrices { get; }

		public OpenGLBuffers()
		{
			int vao = 0;
			int vbo = 0;
			int ebo = 0;
			int lineLoopEbo = 0;
			int uboMatrices = 0;

			Application.Instance.Invoke(() =>
			{
				GL.GenVertexArrays(1, out vao);
				GL.GenBuffers(1, out vbo);
				GL.GenBuffers(1, out ebo);
				GL.GenBuffers(1, out lineLoopEbo);
				GL.GenBuffers(1, out uboMatrices);
			});

			Vao = vao;
			Vbo = vbo;
			Ebo = ebo;
			LineLoopEbo = lineLoopEbo;
			UboMatrices = uboMatrices;
		}

		public override void CleanUp()
		{
			Application.Instance.Invoke(() =>
			{
				GL.DeleteBuffer(UboMatrices);
				GL.DeleteBuffer(LineLoopEbo);
				GL.DeleteBuffer(Ebo);
				GL.DeleteBuffer(Vbo);
				GL.DeleteVertexArray(Vao);
			});
		}
	}

	public class OpenGLBackEnd : BackEnd
	{
		// OpenGL 3.0 is the lowest version that has all the features this
		// project needs built in. With the appropriate extensions 2.X is also
		// usable; unfortunately, requesting a context of less than 3.2 in macOS
		// will produce a 2.1 context without said extensions.
		public static int DesiredMajorVersion { get; } = EtoEnvironment.Platform.IsMac ? 3 : 2;
		public static int DesiredMinorVersion { get; } = EtoEnvironment.Platform.IsMac ? 2 : 0;

		/// <summary>
		/// Extensions required to run under OpenGL versions below 3.0.
		/// </summary>
		public static ReadOnlyCollection<string> RequiredExtensions { get; } =
			new List<string>
			{
				"ARB_vertex_array_object",
				"ARB_framebuffer_object",
				"ARB_uniform_buffer_object"
			}.AsReadOnly();

		public new Dictionary<(Map, View), OpenGLBuffers> Buffers { get; } = new Dictionary<(Map, View), OpenGLBuffers>();

		public Dictionary<string, int> Textures { get; } = new Dictionary<string, int>();

		public Dictionary<ShadingStyle, Shader> Shaders { get; } = new Dictionary<ShadingStyle, Shader>();

		public static Action<Control> SetUpWireframe { get; } = new Action<Control>(control =>
		{
			if (control is OpenGLView o && o.OpenGLReady)
			{
				o.ShadingStyle = ShadingStyle.Wireframe;

				GL.Disable(EnableCap.CullFace);

				GL.Disable(EnableCap.Blend);

				GL.ClearColor(1.0f, 0.0f, 0.0f, 1.0f);
			}
		});

		public static Action<Control> SetUpFlat { get; } = new Action<Control>(control =>
		{
			if (control is OpenGLView o && o.OpenGLReady)
			{
				o.ShadingStyle = ShadingStyle.Flat;

				GL.Enable(EnableCap.CullFace);
				GL.CullFace(CullFaceMode.Back);

				GL.Enable(EnableCap.Blend);
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

				GL.ClearColor(0.0f, 1.0f, 0.0f, 1.0f);
			}
		});

		public static Action<Control> SetUpTextured { get; } = new Action<Control>(control =>
		{
			if (control is OpenGLView o && o.OpenGLReady)
			{
				o.ShadingStyle = ShadingStyle.Textured;

				GL.Enable(EnableCap.CullFace);
				GL.CullFace(CullFaceMode.Back);

				GL.Enable(EnableCap.Blend);
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

				GL.ClearColor(0.0f, 0.0f, 1.0f, 1.0f);
			}
		});

		public void DrawMapWireframe(Map map, View view, Camera camera)
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

			OpenGLBuffers b = Buffers[(map, view)];

			GL.BindVertexArray(b.Vao);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, b.LineLoopEbo);

			Shaders[ShadingStyle.Wireframe].Draw(wireframeWorld, wireframeModel, camera);

			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
			GL.BindVertexArray(0);
		}
		public void DrawMapFlat(Map map, View view, Camera camera)
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

			OpenGLBuffers b = Buffers[(map, view)];

			GL.BindVertexArray(b.Vao);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, b.Ebo);

			Shaders[ShadingStyle.Flat].Draw(flatWorld, flatModel, camera);

			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
			GL.BindVertexArray(0);
		}
		public void DrawMapTextured(Map map, View view, Camera camera)
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

			OpenGLBuffers b = Buffers[(map, view)];

			GL.BindVertexArray(b.Vao);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, b.Ebo);

			Shaders[ShadingStyle.Textured].Draw(texturedWorld, texturedModel, camera);
			Shaders[ShadingStyle.Flat].Draw(flatWorld, flatModel, camera);
			Shaders[ShadingStyle.Wireframe].Draw(wireframeWorld, wireframeModel, camera);

			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
			GL.BindVertexArray(0);
		}

		public void SetUp()
		{
			string version = GL.GetString(StringName.Version);

			string[] split = version.Split('.', ' ');

			bool gotMajor = Int32.TryParse(split[0], out int glMajor);
			bool gotMinor = Int32.TryParse(split[1], out int glMinor);

			if (!(gotMajor && gotMinor))
			{
				throw new GraphicsException("Couldn't parse OpenGL version string!");
			}

			if (glMajor < 3)
			{
				string extensions = GL.GetString(StringName.Extensions);

				var missing = new List<string>();

				foreach (string extension in RequiredExtensions)
				{
					if (!extensions.Contains(extension))
					{
						missing.Add(extension);
					}
				}

				if (missing.Count > 0)
				{
					string message = $"{Core.Name} needs at least OpenGL 3.1, or these missing extensions:\n\n";
					message += String.Join("\n", missing.ToArray());

					throw new GraphicsException(message);
				}
			}

			GL.Enable(EnableCap.DepthTest);

			GL.FrontFace(FrontFaceDirection.Ccw);

			(int glslMajor, int glslMinor) = Shader.GetGlslVersion();

			Shaders.Clear();
			Shaders.Add(ShadingStyle.Wireframe, new WireframeShader(glslMajor, glslMinor) { BackEnd = this });
			Shaders.Add(ShadingStyle.Flat, new FlatShader(glslMajor, glslMinor) { BackEnd = this });
			Shaders.Add(ShadingStyle.Textured, new SingleTextureShader(glslMajor, glslMinor) { BackEnd = this });
		}

		protected override void InitMap(Map map, View view)
		{
			base.InitMap(map, view);

			var buffers = new OpenGLBuffers();
			Buffers.Add((map, view), buffers);

			foreach (KeyValuePair<ShadingStyle, Shader> shader in Shaders)
			{
				(int bindingPoint, int index, int name) = shader.Value.Ubos["Matrices"];
				name = buffers.UboMatrices;
				shader.Value.Ubos["Matrices"] = (bindingPoint, index, name);
			}

			Application.Instance.Invoke(() =>
			{
				GL.BindVertexArray(buffers.Vao);
				GL.BindBuffer(BufferTarget.ArrayBuffer, buffers.Vbo);

				GL.VertexAttribPointer(Shader.Locations["position"], 3, VertexAttribPointerType.Float, false, Vertex.MemorySize, 0);
				GL.EnableVertexAttribArray(Shader.Locations["position"]);

				GL.VertexAttribPointer(Shader.Locations["normal"], 3, VertexAttribPointerType.Float, false, Vertex.MemorySize, sizeof(float) * 3);
				GL.EnableVertexAttribArray(Shader.Locations["normal"]);

				GL.VertexAttribPointer(Shader.Locations["color"], 4, VertexAttribPointerType.Float, false, Vertex.MemorySize, sizeof(float) * 6);
				GL.EnableVertexAttribArray(Shader.Locations["color"]);

				GL.VertexAttribPointer(Shader.Locations["texCoords"], 2, VertexAttribPointerType.Float, false, Vertex.MemorySize, sizeof(float) * 10);
				GL.EnableVertexAttribArray(Shader.Locations["texCoords"]);
			});

			IEnumerable<Renderable> renderables = map.AllObjects.GetAllRenderables();

			InitRenderables(buffers, renderables);

			Application.Instance.Invoke(() =>
			{
				GL.BindVertexArray(0);
				GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			});

			map.InitializedInBackEnd = true;
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

		public override void InitRenderables(Buffers buffers, IEnumerable<Renderable> renderables)
		{
			InitRenderables((OpenGLBuffers)buffers, renderables);
		}
		protected void InitRenderables(OpenGLBuffers buffers, IEnumerable<Renderable> renderables)
		{
			int totalVertexBytes = 0;
			int totalIndexBytes = 0;
			int totalLineLoopBytes = 0;
			int totalMatrixBytes = Vector4.SizeInBytes * 4 * 2; // projectionMatrix, viewMatrix

			int renderableCount = 0;

			foreach (Renderable r in renderables)
			{
				totalVertexBytes += Vertex.MemorySize * r.Vertices.Count;
				totalIndexBytes += sizeof(int) * r.Indices.Count;
				totalLineLoopBytes += sizeof(int) * r.LineLoopIndices.Count;

				renderableCount++;
			}

			Application.Instance.Invoke(() =>
			{
				GL.BufferData(BufferTarget.ArrayBuffer, totalVertexBytes, IntPtr.Zero, BufferUsageHint.StaticDraw);

				GL.BindBuffer(BufferTarget.ElementArrayBuffer, buffers.Ebo);
				GL.BufferData(BufferTarget.ElementArrayBuffer, totalIndexBytes, IntPtr.Zero, BufferUsageHint.StaticDraw);
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

				GL.BindBuffer(BufferTarget.ElementArrayBuffer, buffers.LineLoopEbo);
				GL.BufferData(BufferTarget.ElementArrayBuffer, totalLineLoopBytes, IntPtr.Zero, BufferUsageHint.StaticDraw);
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

				GL.BindBuffer(BufferTarget.UniformBuffer, buffers.UboMatrices);
				GL.BufferData(BufferTarget.UniformBuffer, totalMatrixBytes, IntPtr.Zero, BufferUsageHint.StaticDraw);
				GL.BindBuffer(BufferTarget.UniformBuffer, 0);
			});

			int verticesSoFar = 0;
			IntPtr vboOffset = IntPtr.Zero;

			IntPtr eboOffset = IntPtr.Zero;
			IntPtr lineLoopEboOffset = IntPtr.Zero;

			double progress = 50.0;
			double progressStep = (100.0 - progress) / renderableCount;

			for (int i = 0; i < renderableCount; i++)
			{
				progress += progressStep;

				OnProgressUpdated(this, new ProgressEventArgs(
					(int)progress,
					$"Initializing renderable {i + 1} of {renderableCount}"));

				Renderable r = renderables.ElementAt(i);

				r.VertexOffset = vboOffset;
				r.IndexOffset = eboOffset;
				r.LineLoopIndexOffset = lineLoopEboOffset;

				IntPtr polygonIndexOffset = r.IndexOffset;
				foreach (Polygon p in r.Polygons)
				{
					p.IndexOffset = polygonIndexOffset;
					polygonIndexOffset += sizeof(int) * p.Indices.Count;
				}

				IntPtr polygonLineLoopIndexOffset = r.LineLoopIndexOffset;
				foreach (Polygon p in r.Polygons)
				{
					p.LineLoopIndexOffset = polygonLineLoopIndexOffset;
					polygonLineLoopIndexOffset += sizeof(int) * p.LineLoopIndices.Count;
				}

				int renderableVerticesBytes = Vertex.MemorySize * r.Vertices.Count;
				int renderableIndicesBytes = sizeof(int) * r.Indices.Count;
				int renderableLineLoopIndicesBytes = sizeof(int) * r.LineLoopIndices.Count;

				IEnumerable<int> shiftedIndices = r.Indices.Select(j => verticesSoFar + j);
				IEnumerable<int> shiftedLineLoopIndices = r.LineLoopIndices.Select(j => verticesSoFar + j);

				Application.Instance.Invoke(() =>
				{
					GL.BufferSubData(
						BufferTarget.ArrayBuffer,
						vboOffset,
						renderableVerticesBytes,
						r.Vertices.ToArray());

					GL.BindBuffer(BufferTarget.ElementArrayBuffer, buffers.Ebo);
					GL.BufferSubData(
						BufferTarget.ElementArrayBuffer,
						eboOffset,
						renderableIndicesBytes,
						shiftedIndices.ToArray());
					GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

					GL.BindBuffer(BufferTarget.ElementArrayBuffer, buffers.LineLoopEbo);
					GL.BufferSubData(
						BufferTarget.ElementArrayBuffer,
						lineLoopEboOffset,
						renderableLineLoopIndicesBytes,
						shiftedLineLoopIndices.ToArray());
					GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
				});

				verticesSoFar += r.Vertices.Count;
				vboOffset += renderableVerticesBytes;

				eboOffset += renderableIndicesBytes;
				lineLoopEboOffset += renderableLineLoopIndicesBytes;
			}

			OnProgressUpdated(this, new ProgressEventArgs(100, "Map loaded!"));
		}

		public override void UpdateRenderable(Buffers buffers, Renderable renderable)
		{
			UpdateRenderable(buffers as OpenGLBuffers, renderable);
		}
		protected void UpdateRenderable(OpenGLBuffers buffers, Renderable renderable)
		{
			int totalVerticesBytes = Vertex.MemorySize * renderable.Vertices.Count;

			Application.Instance.Invoke(() =>
			{
				GL.BindBuffer(BufferTarget.ArrayBuffer, buffers.Vbo);

				GL.BufferSubData(
					BufferTarget.ArrayBuffer,
					renderable.VertexOffset,
					totalVerticesBytes,
					renderable.Vertices.ToArray());

				GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			});
		}

		public override void InitTextures(TextureDictionary dictionary)
		{
			foreach (Texture t in dictionary.Values)
			{
				if (Textures.ContainsKey(t.Name))
				{
					DeleteTexture(t.Name);
				}

				OnProgressUpdated(
					this,
					new ProgressEventArgs($"Initializing texture {t.Name}"));

				InitTexture(t);
			}
		}
		public override void InitTexture(Texture t)
		{
			Application.Instance.Invoke(() =>
			{
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
			});
		}
		public override void DeleteTextures()
		{
			foreach (int id in Textures.Values)
			{
				Application.Instance.Invoke(() =>
				{
					GL.DeleteTexture(id);
				});
			}

			Textures.Clear();
		}
		public override void DeleteTexture(string name)
		{
			Application.Instance.Invoke(() =>
			{
				GL.DeleteTexture(Textures[name]);
			});

			Textures.Remove(name);
		}

		protected override void Renderable_Updated(object sender, EventArgs e)
		{
			if (sender is Renderable r)
			{
				foreach (KeyValuePair<(Map, View), OpenGLBuffers> pair in Buffers)
				{
					UpdateRenderable(pair.Value, r);
				}
			}
		}
	}
}
