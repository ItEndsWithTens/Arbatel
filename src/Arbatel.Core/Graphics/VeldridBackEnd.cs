using Arbatel.Controls;
using Arbatel.Formats;
using Arbatel.UI;
using Eto;
using Eto.Forms;
using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Veldrid;
using Veldrid.SPIRV;

namespace Arbatel.Graphics
{
	public class VeldridBuffers : Buffers
	{
		public DeviceBuffer VertexBuffer { get; set; }
		public DeviceBuffer IndexBuffer { get; set; }
		public DeviceBuffer LineLoopIndexBuffer { get; set; }

		public DeviceBuffer ProjectionMatrixBuffer { get; set; }
		public DeviceBuffer ViewMatrixBuffer { get; set; }
		public DeviceBuffer ModelMatrixBuffer { get; set; }

		public VeldridBuffers()
		{
		}

		public override void CleanUp()
		{
			VertexBuffer.Dispose();
			IndexBuffer.Dispose();
			LineLoopIndexBuffer.Dispose();

			ProjectionMatrixBuffer.Dispose();
			ViewMatrixBuffer.Dispose();
			ModelMatrixBuffer.Dispose();
		}
	}

	public class VeldridBackEnd : BackEnd
	{
		public VeldridSurface Surface { get; set; }

		public new Dictionary<(Map, View), VeldridBuffers> Buffers { get; } = new Dictionary<(Map, View), VeldridBuffers>();

		public new Dictionary<string, (Veldrid.Texture t, TextureView v, ResourceSet s)> Textures { get; } = new Dictionary<string, (Veldrid.Texture t, TextureView v, ResourceSet s)>();

		public Dictionary<ShadingStyle, Pipeline> Pipelines { get; } = new Dictionary<ShadingStyle, Pipeline>();

		public ResourceFactory Factory { get; set; }

		public CommandList CommandList { get; set; }

		public VertexLayoutDescription VertexLayout { get; private set; }
		public ResourceLayout ProjectionMatrixLayout { get; private set; }
		public ResourceLayout ViewMatrixLayout { get; private set; }
		public ResourceLayout ModelMatrixLayout { get; private set; }
		public ResourceLayout PointSamplerLayout { get; private set; }
		public ResourceLayout TextureLayout { get; private set; }

		public ResourceSet ProjectionMatrixSet { get; set; }
		public ResourceSet ViewMatrixSet { get; set; }
		public ResourceSet ModelMatrixSet { get; set; }
		public ResourceSet PointSamplerSet { get; set; }

		public void SetUpWireframe(Control control)
		{
			if (control is VeldridView v && v.VeldridReady)
			{
				v.ShadingStyle = ShadingStyle.Wireframe;

				v.ClearColor = RgbaFloat.Red;
			}
		}
		public void SetUpFlat(Control control)
		{
			if (control is VeldridView v && v.VeldridReady)
			{
				v.ShadingStyle = ShadingStyle.Flat;

				v.ClearColor = RgbaFloat.Green;
			}
		}
		public void SetUpTextured(Control control)
		{
			if (control is VeldridView v && v.VeldridReady)
			{
				v.ShadingStyle = ShadingStyle.Textured;

				v.ClearColor = RgbaFloat.Blue;
			}
		}

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

			VeldridBuffers b = Buffers[(map, view)];

			CommandList.SetPipeline(Pipelines[ShadingStyle.Wireframe]);

			CommandList.SetGraphicsResourceSet(0, ProjectionMatrixSet);
			CommandList.SetGraphicsResourceSet(1, ViewMatrixSet);
			CommandList.SetGraphicsResourceSet(2, ModelMatrixSet);

			CommandList.UpdateBuffer(b.ProjectionMatrixBuffer, 0, camera.ProjectionMatrix);
			CommandList.UpdateBuffer(b.ViewMatrixBuffer, 0, camera.ViewMatrix);

			CommandList.UpdateBuffer(b.ModelMatrixBuffer, 0, Matrix4.Identity);
			foreach (Renderable r in wireframeWorld)
			{
				CommandList.SetVertexBuffer(0, b.VertexBuffer, (uint)r.VertexOffset);

				foreach (Polygon p in r.Polygons)
				{
					CommandList.SetIndexBuffer(b.LineLoopIndexBuffer, IndexFormat.UInt32, (uint)p.LineLoopIndexOffset);
					CommandList.DrawIndexed((uint)p.LineLoopIndices.Count);
				}
			}

			foreach (Renderable r in wireframeModel)
			{
				CommandList.UpdateBuffer(b.ModelMatrixBuffer, 0, r.ModelMatrix);
				CommandList.SetVertexBuffer(0, b.VertexBuffer, (uint)r.VertexOffset);

				foreach (Polygon p in r.Polygons)
				{
					CommandList.SetIndexBuffer(b.LineLoopIndexBuffer, IndexFormat.UInt32, (uint)p.LineLoopIndexOffset);
					CommandList.DrawIndexed((uint)p.LineLoopIndices.Count);
				}
			}
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

			VeldridBuffers b = Buffers[(map, view)];

			CommandList.SetPipeline(Pipelines[ShadingStyle.Flat]);

			CommandList.SetGraphicsResourceSet(0, ProjectionMatrixSet);
			CommandList.SetGraphicsResourceSet(1, ViewMatrixSet);
			CommandList.SetGraphicsResourceSet(2, ModelMatrixSet);

			CommandList.UpdateBuffer(b.ProjectionMatrixBuffer, 0, camera.ProjectionMatrix);
			CommandList.UpdateBuffer(b.ViewMatrixBuffer, 0, camera.ViewMatrix);

			CommandList.UpdateBuffer(b.ModelMatrixBuffer, 0, Matrix4.Identity);
			foreach (Renderable r in flatWorld)
			{
				CommandList.SetVertexBuffer(0, b.VertexBuffer, (uint)r.VertexOffset);
				CommandList.SetIndexBuffer(b.IndexBuffer, IndexFormat.UInt32, (uint)r.IndexOffset);

				CommandList.DrawIndexed((uint)r.Indices.Count);
			}

			foreach (Renderable r in flatModel)
			{
				CommandList.UpdateBuffer(b.ModelMatrixBuffer, 0, r.ModelMatrix);

				CommandList.SetVertexBuffer(0, b.VertexBuffer, (uint)r.VertexOffset);
				CommandList.SetIndexBuffer(b.IndexBuffer, IndexFormat.UInt32, (uint)r.IndexOffset);

				CommandList.DrawIndexed((uint)r.Indices.Count);
			}
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

			VeldridBuffers b = Buffers[(map, view)];

			CommandList.UpdateBuffer(b.ProjectionMatrixBuffer, 0, camera.ProjectionMatrix);
			CommandList.UpdateBuffer(b.ViewMatrixBuffer, 0, camera.ViewMatrix);

			CommandList.SetPipeline(Pipelines[ShadingStyle.Textured]);
			CommandList.SetGraphicsResourceSet(0, ProjectionMatrixSet);
			CommandList.SetGraphicsResourceSet(1, ViewMatrixSet);
			CommandList.SetGraphicsResourceSet(2, ModelMatrixSet);
			CommandList.SetGraphicsResourceSet(3, PointSamplerSet);

			IEnumerable<IGrouping<Texture, (Polygon, Renderable)>> worldByTexture =
				camera.GetVisiblePolygons(texturedWorld)
				.GroupBy(pair => pair.Item1.Texture)
				.OrderBy(t => t.Key.Translucent);

			CommandList.UpdateBuffer(b.ModelMatrixBuffer, 0, Matrix4.Identity);
			foreach (IGrouping<Texture, (Polygon, Renderable)> t in worldByTexture)
			{
				CommandList.SetGraphicsResourceSet(4, Textures[t.Key.Name.ToLower()].s);

				foreach ((Polygon p, Renderable r) in t)
				{
					CommandList.SetVertexBuffer(0, b.VertexBuffer, (uint)r.VertexOffset);
					CommandList.SetIndexBuffer(b.IndexBuffer, IndexFormat.UInt32, (uint)p.IndexOffset);

					CommandList.DrawIndexed((uint)p.Indices.Count);
				}
			}

			IEnumerable<IGrouping<Texture, (Polygon, Renderable)>> modelByTexture =
				camera.GetVisiblePolygons(texturedModel)
				.GroupBy(pair => pair.Item1.Texture)
				.OrderBy(t => t.Key.Translucent);

			foreach (IGrouping<Texture, (Polygon, Renderable)> t in modelByTexture)
			{
				CommandList.SetGraphicsResourceSet(4, Textures[t.Key.Name.ToLower()].s);

				IEnumerable<IGrouping<Renderable, (Polygon, Renderable)>> byRenderable =
					t
					.GroupBy(pair => pair.Item2);

				foreach (IGrouping<Renderable, (Polygon, Renderable)> pair in byRenderable)
				{
					Renderable r = pair.Key;

					CommandList.UpdateBuffer(b.ModelMatrixBuffer, 0, r.ModelMatrix);
					CommandList.SetVertexBuffer(0, b.VertexBuffer, (uint)r.VertexOffset);

					foreach ((Polygon p, _) in pair)
					{
						CommandList.SetIndexBuffer(b.IndexBuffer, IndexFormat.UInt32, (uint)p.IndexOffset);

						CommandList.DrawIndexed((uint)p.Indices.Count);
					}
				}
			}

			if (!flat.Any())
			{
				return;
			}

			CommandList.SetPipeline(Pipelines[ShadingStyle.Flat]);
			CommandList.SetGraphicsResourceSet(0, ProjectionMatrixSet);
			CommandList.SetGraphicsResourceSet(1, ViewMatrixSet);
			CommandList.SetGraphicsResourceSet(2, ModelMatrixSet);

			CommandList.UpdateBuffer(b.ModelMatrixBuffer, 0, Matrix4.Identity);
			foreach (Renderable r in flatWorld)
			{
				CommandList.SetVertexBuffer(0, b.VertexBuffer, (uint)r.VertexOffset);
				CommandList.SetIndexBuffer(b.IndexBuffer, IndexFormat.UInt32, (uint)r.IndexOffset);

				CommandList.DrawIndexed((uint)r.Indices.Count);
			}

			foreach (Renderable r in flatModel)
			{
				CommandList.UpdateBuffer(b.ModelMatrixBuffer, 0, r.ModelMatrix);

				CommandList.SetVertexBuffer(0, b.VertexBuffer, (uint)r.VertexOffset);
				CommandList.SetIndexBuffer(b.IndexBuffer, IndexFormat.UInt32, (uint)r.IndexOffset);

				CommandList.DrawIndexed((uint)r.Indices.Count);
			}
		}

		public void SetUp(VeldridSurface surface)
		{
			Surface = surface;

			Factory = Surface.GraphicsDevice.ResourceFactory;

			// Veldrid.SPIRV, when cross-compiling to HLSL, will always produce
			// TEXCOORD semantics; VertexElementSemantic.TextureCoordinate thus
			// becomes necessary to let D3D11 work alongside Vulkan and OpenGL.
			//
			// For more details: https://github.com/mellinoe/veldrid/issues/121
			VertexLayout = new VertexLayoutDescription(
				new VertexElementDescription("position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
				new VertexElementDescription("normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
				new VertexElementDescription("color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
				new VertexElementDescription("texCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));

			ProjectionMatrixLayout = Factory.CreateResourceLayout(
				new ResourceLayoutDescription(
					new ResourceLayoutElementDescription(
						"ProjectionMatrix",
						ResourceKind.UniformBuffer,
						ShaderStages.Vertex)));

			ViewMatrixLayout = Factory.CreateResourceLayout(
				new ResourceLayoutDescription(
					new ResourceLayoutElementDescription(
						"ViewMatrix",
						ResourceKind.UniformBuffer,
						ShaderStages.Vertex)));

			ModelMatrixLayout = Factory.CreateResourceLayout(
				new ResourceLayoutDescription(
					new ResourceLayoutElementDescription(
						"ModelMatrix",
						ResourceKind.UniformBuffer,
						ShaderStages.Vertex)));

			PointSamplerLayout = Factory.CreateResourceLayout(
				new ResourceLayoutDescription(
					new ResourceLayoutElementDescription(
						"point",
						ResourceKind.Sampler,
						ShaderStages.Fragment)));

			TextureLayout = Factory.CreateResourceLayout(
				new ResourceLayoutDescription(
					new ResourceLayoutElementDescription(
						"Texture",
						ResourceKind.TextureReadOnly,
						ShaderStages.Fragment)));

			Pipelines.Add(ShadingStyle.Wireframe, CreatePipeline(ShadingStyle.Wireframe, VertexLayout));
			Pipelines.Add(ShadingStyle.Flat, CreatePipeline(ShadingStyle.Flat, VertexLayout));
			Pipelines.Add(ShadingStyle.Textured, CreatePipeline(ShadingStyle.Textured, VertexLayout));

			Sampler point = Factory.CreateSampler(
				new SamplerDescription(
					SamplerAddressMode.Wrap,
					SamplerAddressMode.Wrap,
					SamplerAddressMode.Wrap,
					SamplerFilter.MinPoint_MagPoint_MipLinear,
					null,
					0,
					0,
					0,
					0,
					SamplerBorderColor.OpaqueBlack));

			PointSamplerSet = Factory.CreateResourceSet(
				new ResourceSetDescription(PointSamplerLayout, point));

			CommandList = Factory.CreateCommandList();
		}

		protected override void InitMap(Map map, View view)
		{
			base.InitMap(map, view);

			var buffers = new VeldridBuffers();
			Buffers.Add((map, view), buffers);

			buffers.ProjectionMatrixBuffer = Factory.CreateBuffer(
				new BufferDescription(64, BufferUsage.UniformBuffer));
			ProjectionMatrixSet = Factory.CreateResourceSet(new ResourceSetDescription(
				ProjectionMatrixLayout, buffers.ProjectionMatrixBuffer));

			buffers.ViewMatrixBuffer = Factory.CreateBuffer(
				new BufferDescription(64, BufferUsage.UniformBuffer));
			ViewMatrixSet = Factory.CreateResourceSet(new ResourceSetDescription(
				ViewMatrixLayout, buffers.ViewMatrixBuffer));

			buffers.ModelMatrixBuffer = Factory.CreateBuffer(
				new BufferDescription(64, BufferUsage.UniformBuffer));
			ModelMatrixSet = Factory.CreateResourceSet(new ResourceSetDescription(
				ModelMatrixLayout, buffers.ModelMatrixBuffer));

			InitRenderables(buffers, map.AllObjects.GetAllRenderables());

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

		private Pipeline CreatePipeline(ShadingStyle shadingStyle, VertexLayoutDescription vertexLayout)
		{
			var specializations = new SpecializationConstant[1];
			if (Surface.GraphicsDevice.BackendType == GraphicsBackend.Vulkan)
			{
				specializations[0] = new SpecializationConstant(0, true); // flip_vertical
			}

			var depthStencilState = new DepthStencilStateDescription(
				depthTestEnabled: true,
				depthWriteEnabled: true,
				comparisonKind: ComparisonKind.Less);

			var rasterizerState = new RasterizerStateDescription(
				cullMode: FaceCullMode.Back,
				fillMode: PolygonFillMode.Solid,
				frontFace: FrontFace.CounterClockwise,
				depthClipEnabled: true,
				scissorTestEnabled: false);

			PrimitiveTopology primitiveTopology = PrimitiveTopology.TriangleList;

			if (shadingStyle == ShadingStyle.Wireframe)
			{
				depthStencilState = DepthStencilStateDescription.Disabled;

				rasterizerState.CullMode = FaceCullMode.None;
				rasterizerState.FillMode = PolygonFillMode.Wireframe;
				rasterizerState.DepthClipEnabled = false;

				primitiveTopology = PrimitiveTopology.LineStrip;
			}

			var shaderSet = new ShaderSetDescription(
				vertexLayouts: new[] { vertexLayout },
				shaders: LoadShader(shadingStyle.ToString(), specializations),
				specializations: specializations);

			var resourceLayouts = new List<ResourceLayout>
			{
				ProjectionMatrixLayout,
				ViewMatrixLayout,
				ModelMatrixLayout
			};
			if (shadingStyle == ShadingStyle.Textured)
			{
				resourceLayouts.Add(PointSamplerLayout);
				resourceLayouts.Add(TextureLayout);
			}

			var description = new GraphicsPipelineDescription
			{
				BlendState = BlendStateDescription.SingleAlphaBlend,
				DepthStencilState = depthStencilState,
				RasterizerState = rasterizerState,
				PrimitiveTopology = primitiveTopology,
				ResourceLayouts = resourceLayouts.ToArray(),
				ShaderSet = shaderSet,
				Outputs = Surface.Swapchain.Framebuffer.OutputDescription
			};

			return Factory.CreateGraphicsPipeline(description);
		}

		public override void InitRenderables(Buffers buffers, IEnumerable<Renderable> renderables)
		{
			InitRenderables((VeldridBuffers)buffers, renderables);
		}
		private void InitRenderables(VeldridBuffers buffers, IEnumerable<Renderable> renderables)
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

			buffers.VertexBuffer = Factory.CreateBuffer(
				new BufferDescription((uint)totalVertexBytes, BufferUsage.VertexBuffer));
			buffers.IndexBuffer = Factory.CreateBuffer(
				new BufferDescription((uint)totalIndexBytes, BufferUsage.IndexBuffer));
			buffers.LineLoopIndexBuffer = Factory.CreateBuffer(
				new BufferDescription((uint)totalLineLoopBytes, BufferUsage.IndexBuffer));

			int verticesSoFar = 0;
			IntPtr vboOffset = IntPtr.Zero;

			IntPtr eboOffset = IntPtr.Zero;
			IntPtr lineLoopEboOffset = IntPtr.Zero;

			double progress = 50.0;
			double progressStep = (100.0 - progress) / renderableCount;

			for (int i = 0; i < renderableCount; i++)
			{
				OnProgressUpdated(this, new ProgressEventArgs(
					(int)progress,
					$"Initializing renderable {i + 1} of {renderableCount}"));

				progress += progressStep;

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

				Surface.GraphicsDevice.UpdateBuffer(buffers.VertexBuffer, (uint)vboOffset, r.Vertices.ToArray());
				Surface.GraphicsDevice.UpdateBuffer(buffers.IndexBuffer, (uint)eboOffset, r.Indices.ToArray());
				Surface.GraphicsDevice.UpdateBuffer(buffers.LineLoopIndexBuffer, (uint)lineLoopEboOffset, r.LineLoopIndices.ToArray());

				verticesSoFar += r.Vertices.Count;
				vboOffset += renderableVerticesBytes;

				eboOffset += renderableIndicesBytes;
				lineLoopEboOffset += renderableLineLoopIndicesBytes;
			}

			OnProgressUpdated(this, new ProgressEventArgs(100, "Map loaded!"));
		}

		public override void UpdateRenderable(Buffers buffers, Renderable renderable)
		{
			UpdateRenderable((VeldridBuffers)buffers, renderable);
		}
		protected void UpdateRenderable(VeldridBuffers b, Renderable r)
		{
			int renderableVerticesBytes = Vertex.MemorySize * r.Vertices.Count;
			int renderableIndicesBytes = sizeof(int) * r.Indices.Count;
			int renderableLineLoopIndicesBytes = sizeof(int) * r.LineLoopIndices.Count;

			Surface.GraphicsDevice.UpdateBuffer(b.VertexBuffer, (uint)r.VertexOffset, r.Vertices.ToArray());
			Surface.GraphicsDevice.UpdateBuffer(b.IndexBuffer, (uint)r.IndexOffset, r.Indices.ToArray());
			Surface.GraphicsDevice.UpdateBuffer(b.LineLoopIndexBuffer, (uint)r.LineLoopIndexOffset, r.LineLoopIndices.ToArray());
		}

		public override void InitTexture(Texture texture)
		{
			PixelFormat format;
			if (Platform.Instance.IsWinForms || Platform.Instance.IsWpf)
			{
				format = PixelFormat.B8_G8_R8_A8_UNorm;
			}
			else
			{
				format = PixelFormat.R8_G8_B8_A8_UNorm;
			}

			var description = new TextureDescription(
				(uint)texture.Width,
				(uint)texture.Height,
				1,
				1,
				1,
				format,
				TextureUsage.Sampled,
				TextureType.Texture2D);

			Veldrid.Texture created = Factory.CreateTexture(description);

			using (Eto.Drawing.BitmapData data = texture.Bitmap.Lock())
			{
				Surface.GraphicsDevice.UpdateTexture(
					created,
					data.Data,
					(uint)(data.ScanWidth * texture.Height),
					0,
					0,
					0,
					(uint)texture.Width,
					(uint)texture.Height,
					1,
					0,
					0);
			}

			TextureView view = Factory.CreateTextureView(created);

			ResourceSet set = Factory.CreateResourceSet(new ResourceSetDescription(
				TextureLayout, view));

			Textures.Add(texture.Name, (created, view, set));
		}
		public override void DeleteTexture(string name)
		{
			Textures[name].s.Dispose();
			Textures[name].v.Dispose();
			Textures[name].t.Dispose();
			Textures.Remove(name);
		}

		protected override void Renderable_Updated(object sender, EventArgs e)
		{
			if (sender is Renderable r)
			{
				foreach (KeyValuePair<(Map, View), VeldridBuffers> pair in Buffers)
				{
					UpdateRenderable(pair.Value, r);
				}
			}
		}

		private Veldrid.Shader[] LoadShader(string name, SpecializationConstant[] specializations)
		{
			byte[] vertexShaderSpirvBytes = LoadSpirvBytes(name, ShaderStages.Vertex);
			byte[] fragmentShaderSpirvBytes = LoadSpirvBytes(name, ShaderStages.Fragment);

			var vertex = new ShaderDescription(ShaderStages.Vertex, vertexShaderSpirvBytes, "main", true);
			var fragment = new ShaderDescription(ShaderStages.Fragment, fragmentShaderSpirvBytes, "main", true);

			var options = new CrossCompileOptions(false, false, specializations);

			return Factory.CreateFromSpirv(vertex, fragment, options);
		}

		private static byte[] LoadSpirvBytes(string name, ShaderStages stage)
		{
			byte[] bytes;

			string shaderDir = Path.Combine(AppContext.BaseDirectory, "shaders", "vulkan");
			string fileName = $"{name}450.{stage.ToString().Substring(0, 4).ToLower()}";
			string full = Path.Combine(shaderDir, fileName);

			try
			{
				bytes = File.ReadAllBytes($"{full}.spv");
			}
			catch (FileNotFoundException)
			{
				bytes = File.ReadAllBytes($"{full}");
			}

			return bytes;
		}
	}
}
