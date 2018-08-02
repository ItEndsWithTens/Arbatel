using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Temblor.Graphics
{
	public class Shader
	{
		public int Program;

		public string VertexShaderSource;
		public string FragmentShaderSource;

		public Shader()
		{
		}

		public Shader(string vertexPath, string fragmentPath)
		{
			Compile(File.ReadAllText(vertexPath), File.ReadAllText(fragmentPath));
		}

		public Shader(string[] vertexArray, string[] fragmentArray)
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

		public void Use()
		{
			GL.UseProgram(Program);
		}

		public void SetMatrix4(string name, ref Matrix4 matrix)
		{
			GL.UniformMatrix4(GL.GetUniformLocation(Program, name), false, ref matrix);
		}
	}
}
