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
		public int LocationViewMatrix { get; set; }
		public int LocationProjectionMatrix { get; set; }

		public static Dictionary<string, int> Locations { get; } = new Dictionary<string, int>
		{
			{ "position", 0 },
			{ "normal", 1 },
			{ "color", 2 }
		};

		public Shader()
		{
			LocationModelMatrix = 0;
			LocationViewMatrix = 0;
			LocationProjectionMatrix = 0;
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

			string log = GL.GetShaderInfoLog(vertexShader);
			if (log != "")
			{
				Console.Write("Error! Vertex shader compilation failed: " + log);
			}

			int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
			GL.ShaderSource(fragmentShader, fragment);
			GL.CompileShader(fragmentShader);

			log = GL.GetShaderInfoLog(fragmentShader);
			if (log != "")
			{
				Console.Write("Error! Fragment shader compilation failed: " + log);
			}

			int shaderProgram = GL.CreateProgram();
			GL.AttachShader(shaderProgram, vertexShader);
			GL.AttachShader(shaderProgram, fragmentShader);
			GL.LinkProgram(shaderProgram);

			log = GL.GetProgramInfoLog(shaderProgram);
			if (log != "")
			{
				Console.Write("Error! Shader program linking failed: " + log);
			}

			GL.DeleteShader(vertexShader);
			GL.DeleteShader(fragmentShader);

			Program = shaderProgram;

			LocationModelMatrix = GL.GetUniformLocation(Program, "model");
			LocationViewMatrix = GL.GetUniformLocation(Program, "view");
			LocationProjectionMatrix = GL.GetUniformLocation(Program, "projection");

			foreach (KeyValuePair<string, int> location in Locations)
			{
				GL.BindAttribLocation(Program, location.Value, location.Key);
			}
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

		/// <summary>
		/// Draw the specified renderables, respecting their model matrices.
		/// </summary>
		public virtual void DrawModel(IEnumerable<Renderable> renderables, Camera camera)
		{
			Use();

			SetUniform(LocationViewMatrix, camera.ViewMatrix);
			SetUniform(LocationProjectionMatrix, camera.ProjectionMatrix);
		}

		/// <summary>
		/// Draw the specified renderables, ignoring their model matrices.
		/// </summary>
		public virtual void DrawWorld(IEnumerable<Renderable> renderables, Camera camera)
		{
			Use();

			SetUniform(LocationModelMatrix, Matrix4.Identity);
			SetUniform(LocationViewMatrix, camera.ViewMatrix);
			SetUniform(LocationProjectionMatrix, camera.ProjectionMatrix);
		}
	}
}
