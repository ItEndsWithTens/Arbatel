using Eto.Gl;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Arbatel.Graphics
{
	public class Shader
	{
		public BackEnd BackEnd { get; set; }

		public int Program;

		public string VertexShaderSource;
		public string FragmentShaderSource;

		public int LocationModelMatrix;
		public int LocationViewMatrix;
		public int LocationProjectionMatrix;

		public Shader()
		{
			LocationModelMatrix = 0;
			LocationViewMatrix = 0;
			LocationProjectionMatrix = 0;
		}
		public Shader(string vertexPath, string fragmentPath) : this()
		{
			Compile(File.ReadAllText(vertexPath), File.ReadAllText(fragmentPath));
		}
		public Shader(string[] vertexArray, string[] fragmentArray) : this()
		{
			Compile(VertexShaderSource, FragmentShaderSource);
		}

		public void Compile(string[] vertexArray, string[] fragmentArray)
		{
			VertexShaderSource = String.Join(Environment.NewLine, vertexArray);
			FragmentShaderSource = String.Join(Environment.NewLine, fragmentArray);

			Compile(VertexShaderSource, FragmentShaderSource);
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
		}

		public static void GetGlslVersion(out int major, out int minor)
		{
			// Not sure if there's another way to get the GLSL version; the GL version is easy, but I can't find any
			// info about determining the shader language version number beside this string parsing stuff.
			var raw = GL.GetString(StringName.ShadingLanguageVersion);

			var regex = new Regex(@"(\d+?\.\d+?)\s*.*");

			var trimmed = regex.Match(raw).Groups[1].Value.TrimEnd('0');

			int.TryParse(trimmed.Substring(0, trimmed.IndexOf('.')), out major);
			int.TryParse(trimmed.Substring(trimmed.IndexOf('.') + 1), out minor);
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

		public void Use()
		{
			GL.UseProgram(Program);
		}

		virtual public void Draw(Renderable renderable, GLSurface surface, Camera camera)
		{
			Use();
			SetUniform(LocationModelMatrix, renderable.ModelMatrix);
			SetUniform(LocationViewMatrix, camera.ViewMatrix);
			SetUniform(LocationProjectionMatrix, camera.ProjectionMatrix);
		}
	}
}
