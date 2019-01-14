using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.NUnit;
using Nuke.Common.Tools.VSWhere;
using SharpCompress.Archives;
using SharpCompress.Archives.Tar;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Writers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Common.Tools.NUnit.NUnitTasks;
using static Nuke.Common.Tools.VSWhere.VSWhereTasks;

class Build : NukeBuild
{
	const string ProductName = "Arbatel";

	string[] EtoPlatformsWin = new string[] { "WinForms", "Wpf" };
	string[] EtoPlatformsLin = new string[] { "Gtk" };
	string[] EtoPlatformsMac = new string[] { "Mac", "XamMac" };

	AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
	AbsolutePath SourceDirectory => RootDirectory / "src";
	AbsolutePath TestSourceDirectory => RootDirectory / "test" / "src";
	AbsolutePath CustomMsBuildPath;

	[Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
	readonly string Configuration = IsLocalBuild ? "Debug" : "Release";

	[Solution(ProductName + ".sln")]
	readonly Solution Solution;

	[GitRepository]
	readonly GitRepository GitRepository;

	[GitVersion]
	readonly GitVersion GitVersion;

	public static Dictionary<PlatformFamily, string> OsFriendlyName = new Dictionary<PlatformFamily, string>
	{
		{ PlatformFamily.Unknown, "" },
		{ PlatformFamily.Windows, "Windows" },
		{ PlatformFamily.Linux, "Linux" },
		{ PlatformFamily.OSX, "macOS" },
	};

	public static int Main()
	{
		if (EnvironmentInfo.IsOsx)
		{
			return Execute<Build>(x => x.PackageMac);
		}
		else if (EnvironmentInfo.IsLinux)
		{
			return Execute<Build>(x => x.PackageLinux);
		}
		else
		{
			return Execute<Build>(x => x.PackageWindows);
		}
	}

	/// <summary>
	/// Find the full path of a copy of MSBuild on a Windows system.
	/// </summary>
	/// <returns>The full path to MSBuild.exe</returns>
	public static string GetMsBuildPath()
	{
		if (!EnvironmentInfo.IsWin)
		{
			throw new PlatformNotSupportedException("GetMsBuildPath only works in Windows!");
		}

		VSWhereSettings vswhereSettings = new VSWhereSettings()
			.EnableLatest()
			.AddRequires(MsBuildComponent);

		IReadOnlyCollection<Output> output = VSWhere(s => vswhereSettings).Output;

		string vsPath = output.FirstOrDefault(o => o.Text.StartsWith("installationPath")).Text.Replace("installationPath: ", "");
		string vsVersion = output.FirstOrDefault(o => o.Text.StartsWith("installationVersion")).Text.Replace("installationVersion: ", "");
		Int32.TryParse(vsVersion.Split('.')[0], out int vsMajor);

		if (vsMajor < 15)
		{
			throw new Exception("Can't build with less than VS 2017!");
		}

		return Path.Combine(vsPath, "MSBuild", vsMajor.ToString() + ".0", "Bin", "MSBuild.exe");
	}

	/// <summary>
	/// Get the full path to a given project's compile output directory.
	/// </summary>
	/// <param name="name">The name of the project.</param>
	/// <returns>The full, absolute path to the directory specified by a
	/// project's "OutputPath" property.</returns>
	public string GetOutputPath(string name)
	{
		Project n = Solution.GetProject(name);

		// The OutputPath property has a different value depending on the active
		// build configuration, so it's necessary to take that into account.
		Microsoft.Build.Evaluation.Project m = n.GetMSBuildProject(Configuration);
		Microsoft.Build.Evaluation.ProjectProperty o = m.GetProperty("OutputPath");

		return Path.Combine(n.Directory, o.EvaluatedValue);
	}

	Target Clean => _ => _
		.Executes(() =>
		{
			// Using SetTargets("Rebuild") in the compile targets below would
			// ensure that everything got cleaned and built fresh every time the
			// build scripts were run. Unfortunately, project references in
			// .csproj files mean that dependencies would also be rebuilt, which
			// in this case means the core class library. To start fresh every
			// time, but still build the core library only once, it's important
			// to clean all the bin and obj folders along with the staging
			// directory. Then use SetTargets("Build") on each GUI project to
			// trigger an incremental build. The core will have been built anew,
			// and no time will be wasted rebuilding it for every GUI app.

			var files = new List<string>();
			files.AddRange(GlobFiles(SourceDirectory, "**/bin/**/*", "**/obj/**/*"));
			files.AddRange(GlobFiles(TestSourceDirectory, "**/bin/**/*", "**/obj/**/*"));

			// Make sure all files are gone first; can't remove empty folders.
			foreach (string file in files)
			{
				DeleteFile(file);
			}

			var directories = new List<string>();
			directories.AddRange(GlobDirectories(SourceDirectory, "**/bin/**", "**/obj/**"));
			directories.AddRange(GlobDirectories(TestSourceDirectory, "**/bin/**", "**/obj/**"));

			// Sort paths by length, and reverse to delete the deepest first;
			// the parent levels will then be empty and cleanly deletable.
			var list = new List<string>(directories);
			list.Sort((a, b) => a.Length.CompareTo(b.Length));
			list.Reverse();
			DeleteDirectories(list);
		});

	private void Compile(string[] platforms)
	{
		MSBuild(settings => new MSBuildSettings()
			.EnableRestore()
			.SetTargets("Build")
			.SetConfiguration(Configuration)
			.When(CustomMsBuildPath != null, s => s
				.SetToolPath(CustomMsBuildPath))
			.SetAssemblyVersion(GitVersion.GetNormalizedAssemblyVersion())
			.SetFileVersion(GitVersion.GetNormalizedFileVersion())
			.SetInformationalVersion(GitVersion.InformationalVersion)
			.SetMaxCpuCount(Environment.ProcessorCount)
			.SetNodeReuse(IsLocalBuild)
			.CombineWith(platforms, (s, p) => s
				.SetProjectFile(Solution.GetProject($"{p}"))));
	}

	Target CompileCore => _ => _
		.DependsOn(Clean)
		.Executes(() =>
		{
			if (EnvironmentInfo.IsWin)
			{
				// Windows developers with Visual Studio installed to a directory
				// other than System.Environment.SpecialFolder.ProgramFilesX86 need
				// to tell Nuke the path to MSBuild.exe themselves.
				CustomMsBuildPath = (AbsolutePath)GetMsBuildPath();
			}

			Compile(new string[] { $"{ProductName}.Core" });
		});

	Target CompileWindows => _ => _
		.DependsOn(CompileCore)
		.Executes(() =>
		{
			Compile(EtoPlatformsWin.Select(p => $"{ProductName}.{p}").ToArray());
		});

	Target CompileLinux => _ => _
		.DependsOn(CompileCore)
		.Executes(() =>
		{
			Compile(EtoPlatformsLin.Select(p => $"{ProductName}.{p}").ToArray());
		});

	Target CompileMac => _ => _
		.DependsOn(CompileCore)
		.Executes(() =>
		{
			Compile(EtoPlatformsMac.Select(p => $"{ProductName}.{p}").ToArray());
		});

	Target CompileTests => _ => _
		.DependsOn(CompileCore)
		.Executes(() =>
		{
			Compile(new string[] { $"{ProductName}Test.Core" });
		});

	private void Test(string executable)
	{
		string binDir = GetOutputPath($"{ProductName}Test.Core");

		NUnit3(settings => new NUnit3Settings()
			.SetToolPath(Path.Combine(binDir, executable))
			.AddParameter("dataDirectory", RootDirectory / "test/data")
			.AddParameter("fgdDirectory", RootDirectory / "extras")
			.When(Configuration != "Release",
				s => s.SetWhereExpression("cat != Performance")));
	}

	Target TestWindows => _ => _
		.DependsOn(CompileWindows, CompileTests)
		.Executes(() =>
		{
			Test($"{ProductName}Test.Core.exe");
		});

	Target TestLinux => _ => _
		.DependsOn(CompileLinux, CompileTests)
		.Executes(() =>
		{
			Test($"{ProductName}Test.Core.exe");
		});

	Target TestMac => _ => _
		.DependsOn(CompileMac, CompileTests)
		.Executes(() =>
		{
			Test($"{ProductName}Test.Core.app/Contents/MacOS/{ProductName}Test.Core");
		});

	Target PackageWindows => _ => _
		.DependsOn(TestWindows)
		.Executes(() =>
		{
			foreach (string platform in EtoPlatformsWin)
			{
				AbsolutePath finalDir = ArtifactsDirectory / platform;

				EnsureCleanDirectory(finalDir);

				using (var archive = ZipArchive.Create())
				{
					archive.DeflateCompressionLevel = CompressionLevel.BestCompression;
					archive.AddAllFromDirectory(GetOutputPath($"{ProductName}.{platform}"));

					string name = String.Join('-', ProductName, GitVersion.MajorMinorPatch, OsFriendlyName[EnvironmentInfo.Platform]);
					archive.SaveTo(finalDir / name + ".zip", new WriterOptions(CompressionType.Deflate));
				}
			}
		});

	Target PackageLinux => _ => _
		.DependsOn(TestLinux)
		.Executes(() =>
		{
			foreach (string platform in EtoPlatformsLin)
			{
				AbsolutePath finalDir = ArtifactsDirectory / platform;

				EnsureCleanDirectory(finalDir);

				string name = String.Join('-', ProductName, GitVersion.MajorMinorPatch, OsFriendlyName[EnvironmentInfo.Platform]);

				using (var tarball = TarArchive.Create())
				{
					tarball.AddAllFromDirectory(GetOutputPath($"{ProductName}.{platform}"));
					tarball.SaveTo(finalDir / name + ".tar.gz", new WriterOptions(CompressionType.GZip));
				}
			}
		});

	Target PackageMac => _ => _
		.DependsOn(TestMac)
		.Executes(() =>
		{
			foreach (string platform in EtoPlatformsMac)
			{
				AbsolutePath finalDir = ArtifactsDirectory / platform;

				EnsureCleanDirectory(finalDir);

				string name = String.Join('-', ProductName, GitVersion.MajorMinorPatch, OsFriendlyName[EnvironmentInfo.Platform]);

				var dmgbuild = new Process();
				dmgbuild.StartInfo.FileName = "dmgbuild";
				dmgbuild.StartInfo.Arguments =
					$"-s " + BuildProjectDirectory / "dmgbuild-settings.py" +
					" -D app=" + Path.Combine(GetOutputPath($"{ProductName}.{platform}"), $"{platform}.app") +
					$" {ProductName} " +
					finalDir / name + ".dmg";

				dmgbuild.StartInfo.UseShellExecute = false;
				dmgbuild.StartInfo.CreateNoWindow = true;

				dmgbuild.Start();
				dmgbuild.WaitForExit();
			}
		});
}
