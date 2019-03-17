using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.IO;

namespace Arbatel.Graphics
{
	public class Shader
	{
		public static string ShaderDirectory => Path.Combine(Core.Location, "..", "shaders", "glsl");

		public BackEnd BackEnd { get; set; }

		public int Program { get; set; }

		public int LocationModelMatrix { get; set; }

		public Dictionary<string, (int bindingPoint, int blockIndex, int name)> Ubos { get; } = new Dictionary<string, (int, int, int)>();

		public static Dictionary<string, int> Locations { get; } = new Dictionary<string, int>
		{
			{ "position", 0 },
			{ "normal", 1 },
			{ "color", 2 },
			{ "texCoords", 3 }
		};

		public Shader()
		{
			LocationModelMatrix = 0;
		}
		public Shader(string vertexPath, string fragmentPath) : this()
		{
			vertexPath = Path.Combine(ShaderDirectory, vertexPath);
			fragmentPath = Path.Combine(ShaderDirectory, fragmentPath);

			Compile(File.ReadAllText(vertexPath), File.ReadAllText(fragmentPath));
		}
		public Shader(string[] vertexArray, string[] fragmentArray) : this()
		{
			string vertexSource = String.Join(Environment.NewLine, vertexArray);
			string fragmentSource = String.Join(Environment.NewLine, fragmentArray);

			Compile(vertexSource, fragmentSource);
		}

		public void Compile(string vertex, string fragment)
		{
			int vertexShader = GL.CreateShader(ShaderType.VertexShader);
			GL.ShaderSource(vertexShader, vertex);
			GL.CompileShader(vertexShader);

			GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out int compileSuccess);
			if (compileSuccess == 0)
			{
				string log = GL.GetShaderInfoLog(vertexShader);

				throw new GraphicsException($"Vertex shader compilation failed! Error message:\n\n{log}");
			}

			int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
			GL.ShaderSource(fragmentShader, fragment);
			GL.CompileShader(fragmentShader);

			GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out compileSuccess);
			if (compileSuccess == 0)
			{
				string log = GL.GetShaderInfoLog(fragmentShader);

				throw new GraphicsException($"Fragment shader compilation failed! Error message:\n\n{log}");
			}

			Program = GL.CreateProgram();
			GL.AttachShader(Program, vertexShader);
			GL.AttachShader(Program, fragmentShader);

			foreach (KeyValuePair<string, int> location in Locations)
			{
				GL.BindAttribLocation(Program, location.Value, location.Key);
			}

			GL.LinkProgram(Program);

			GL.GetProgram(Program, GetProgramParameterName.LinkStatus, out int linkSuccess);
			if (linkSuccess == 0)
			{
				string log = GL.GetProgramInfoLog(Program);

				throw new GraphicsException($"Shader program linking failed! Error message:\n\n{log}");
			}

			GL.DeleteShader(vertexShader);
			GL.DeleteShader(fragmentShader);

			LocationModelMatrix = GL.GetUniformLocation(Program, "model");

			Ubos.Add("Matrices", (0, GL.GetUniformBlockIndex(Program, "Matrices"), -1));

			GL.UniformBlockBinding(Program, Ubos["Matrices"].blockIndex, Ubos["Matrices"].bindingPoint);
		}

		public static (int major, int minor) GetGlslVersion()
		{
			string raw = GL.GetString(StringName.ShadingLanguageVersion);

			string[] split = raw.Split('.', ' ');

			bool gotMajor = Int32.TryParse(split[0], out int major);
			bool gotMinor = Int32.TryParse(split[1].TrimEnd('0'), out int minor);

			if (!(gotMajor && gotMinor))
			{
				throw new GraphicsException("Couldn't parse GLSL version string!");
			}

			return (major, minor);
		}

		public static void SetUniform(int location, int value)
		{
			GL.Uniform1(location, value);
		}
		public static void SetUniform(int location, float value)
		{
			GL.Uniform1(location, value);
		}
		public static void SetUniform(int location, Vector2 value)
		{
			GL.Uniform2(location, value);
		}
		public static void SetUniform(int location, Vector3 value)
		{
			GL.Uniform3(location, value);
		}
		public static void SetUniform(int location, Matrix4 value)
		{
			GL.UniformMatrix4(location, false, ref value);
		}

		public virtual void Use()
		{
			GL.UseProgram(Program);
		}

		public virtual void Draw(IEnumerable<Renderable> world, IEnumerable<Renderable> model, Camera camera)
		{
			Use();
			SetMatrices(camera);
			DrawWorld(world, camera);
			DrawModel(model, camera);
		}

		/// <summary>
		/// Draw the specified renderables, respecting their model matrices.
		/// </summary>
		public virtual void DrawModel(IEnumerable<Renderable> renderables, Camera camera)
		{
		}

		/// <summary>
		/// Draw the specified renderables, ignoring their model matrices.
		/// </summary>
		public virtual void DrawWorld(IEnumerable<Renderable> renderables, Camera camera)
		{
			SetUniform(LocationModelMatrix, Matrix4.Identity);
		}

		private void SetMatrices(Camera camera)
		{
			GL.BindBufferBase(
				BufferRangeTarget.UniformBuffer,
				Ubos["Matrices"].bindingPoint,
				Ubos["Matrices"].name);

			IntPtr pointer = IntPtr.Zero;
			GL.BufferSubData(
				BufferTarget.UniformBuffer,
				pointer + 0,
				Vector4.SizeInBytes,
				(IntPtr)camera.ProjectionMatrix.Row0);

			GL.BufferSubData(
				BufferTarget.UniformBuffer,
				pointer + 16,
				Vector4.SizeInBytes,
				(IntPtr)camera.ProjectionMatrix.Row1);

			GL.BufferSubData(
				BufferTarget.UniformBuffer,
				pointer + 32,
				Vector4.SizeInBytes,
				(IntPtr)camera.ProjectionMatrix.Row2);

			GL.BufferSubData(
				BufferTarget.UniformBuffer,
				pointer + 48,
				Vector4.SizeInBytes,
				(IntPtr)camera.ProjectionMatrix.Row3);

			pointer += 64;
			GL.BufferSubData(
				BufferTarget.UniformBuffer,
				pointer + 0,
				Vector4.SizeInBytes,
				(IntPtr)camera.ViewMatrix.Row0);

			GL.BufferSubData(
				BufferTarget.UniformBuffer,
				pointer + 16,
				Vector4.SizeInBytes,
				(IntPtr)camera.ViewMatrix.Row1);

			GL.BufferSubData(
				BufferTarget.UniformBuffer,
				pointer + 32,
				Vector4.SizeInBytes,
				(IntPtr)camera.ViewMatrix.Row2);

			GL.BufferSubData(
				BufferTarget.UniformBuffer,
				pointer + 48,
				Vector4.SizeInBytes,
				(IntPtr)camera.ViewMatrix.Row3);

			GL.BindBuffer(BufferTarget.UniformBuffer, 0);
		}
	}
}
