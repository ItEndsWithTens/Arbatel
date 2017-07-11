using System;
using Eto;
using Eto.Forms;
using Eto.Gl;
using Eto.Gl.Windows;

namespace Temblor.WinForms
{
	public class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			new Application(Platforms.WinForms).Run(new MainForm());
		}
	}
}
