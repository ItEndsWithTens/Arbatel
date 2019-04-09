using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
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
using System.IO;
using System.Linq;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.Git.GitTasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Common.Tools.NUnit.NUnitTasks;
using static Nuke.Common.Tools.VSWhere.VSWhereTasks;

class Build : NukeBuild
{
	const string ProductName = "Arbatel";

	// Required when building the custom copy of OpenTK.
	const string FSharpComponent = "Microsoft.VisualStudio.Component.FSharp";

	string[] EtoPlatformsWin = new string[] { "WinForms", "Wpf" };
	string[] EtoPlatformsLin = new string[] { "Gtk" };
	string[] EtoPlatformsMac = new string[] { "Mac", "XamMac" };

	AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
	AbsolutePath SourceDirectory => RootDirectory / "src";
	AbsolutePath TestSourceDirectory => RootDirectory / "test" / "src";
	AbsolutePath CustomMsBuildPath;

	AbsolutePath EtoViewportRoot = RootDirectory / "lib" / "thirdparty" / "etoViewport";

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
	/// Get the absolute path to a given project's compile output directory.
	/// </summary>
	/// <param name="name">The name of the project.</param>
	/// <returns>The full, absolute path to the directory specified by a
	/// project's "OutputPath" property.</returns>
	public AbsolutePath GetOutputPath(string name)
	{
		Project n = Solution.GetProject(name);

		// The OutputPath property has a different value depending on the active
		// build configuration, so it's necessary to take that into account.
		MSBuildProject parsed = MSBuildParseProject(n, settings => settings
			.SetConfiguration(Configuration)
			.When(CustomMsBuildPath != null, s => s
				.SetToolPath(CustomMsBuildPath)));

		return n.Directory / parsed.Properties["OutputPath"];
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
			// to clean all bin and obj folders, then use SetTargets("Build") on
			// each GUI project to trigger an incremental build. The core will
			// be built anew, with no time wasted rebuilding it for every GUI.
			EnsureCleanDirectories(GlobDirectories(SourceDirectory, "**/bin", "**/obj"));
			EnsureCleanDirectories(GlobDirectories(TestSourceDirectory, "**/bin", "**/obj"));
		});

	Target SetVisualStudioPaths => _ => _
		.Executes(() =>
		{
			if (EnvironmentInfo.IsWin)
			{
				Logger.Info("Windows build; setting Visual Studio paths.");

				VSWhereSettings vswhereSettings = new VSWhereSettings()
					.EnableLatest()
					.AddRequires(MsBuildComponent)
					.AddRequires(FSharpComponent);

				IReadOnlyCollection<Output> output = VSWhere(s => vswhereSettings).Output;

				Output outputPath = output.FirstOrDefault(o => o.Text.StartsWith("installationPath"));
				Output outputVersion = output.FirstOrDefault(o => o.Text.StartsWith("installationVersion"));

				// A list of component IDs and friendly names can be found at
				// https://docs.microsoft.com/en-us/visualstudio/install/workload-and-component-ids
				if (String.IsNullOrEmpty(outputPath.Text) || String.IsNullOrEmpty(outputVersion.Text))
				{
					throw new Exception(
						"Couldn't find a suitable Visual Studio installation! " +
						"Either VS is not installed, or no available version " +
						"has all of the following components installed:" +
						"\n" +
						"\n" +
						$"MSBuild ({MsBuildComponent})\n" +
						$"F# language support ({FSharpComponent})");
				}

				string vsPath = outputPath.Text.Replace("installationPath: ", "");
				string vsVersion = outputVersion.Text.Replace("installationVersion: ", "");
				Int32.TryParse(vsVersion.Split('.')[0], out int vsMajor);

				if (vsMajor < 15)
				{
					throw new Exception("Can't build with less than VS 2017!");
				}

				// Windows developers with Visual Studio installed to a directory
				// other than System.Environment.SpecialFolder.ProgramFilesX86 need
				// to tell Nuke the path to MSBuild.exe themselves.
				CustomMsBuildPath = (AbsolutePath)GlobFiles(Path.Combine(vsPath, "MSBuild"), "**/Bin/MSBuild.exe").First();
			}
			else
			{
				Logger.Info("Mono build; no Visual Studio paths to set.");
			}
		});

	private void CompileEtoGl(params string[] targets)
	{
		if (!DirectoryExists(EtoViewportRoot) || !Directory.EnumerateFileSystemEntries(EtoViewportRoot).Any())
		{
			Git("submodule update --init lib/thirdparty/etoViewport");
		}

		DotNetTasks.DotNetRun(s => s
			.SetWorkingDirectory(EtoViewportRoot)
			.SetProjectFile(EtoViewportRoot / "build" / "_build.csproj")
			.SetApplicationArguments($"--configuration Release --target CompileLibrary {String.Join(' ', targets)}"));
	}

	Target CompileWindowsDependencies => _ => _
		.DependsOn(Clean, SetVisualStudioPaths)
		.Before(CompileCore)
		.Executes(() =>
		{
			CompileEtoGl("CompileWindows");
		});

	Target CompileLinuxDependencies => _ => _
		.DependsOn(Clean, SetVisualStudioPaths)
		.Before(CompileCore)
		.Executes(() =>
		{
			CompileEtoGl("CompileLinux");
		});

	Target CompileMacDependencies => _ => _
		.DependsOn(Clean, SetVisualStudioPaths)
		.Before(CompileCore)
		.Executes(() =>
		{
			CompileEtoGl("CompileMac");
		});

	private void Compile(string[] projects)
	{
		MSBuild(settings => settings
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
			.CombineWith(projects, (s, p) => s
				.SetProjectFile(Solution.GetProject($"{p}"))));
	}

	Target CompileCore => _ => _
		.DependsOn(Clean, SetVisualStudioPaths)
		.Executes(() =>
		{
			Compile(new string[] { $"{ProductName}.Core" });
		});

	Target CompileWindows => _ => _
		.DependsOn(CompileCore, CompileWindowsDependencies)
		.Executes(() =>
		{
			Compile(EtoPlatformsWin.Select(p => $"{ProductName}.{p}").ToArray());
		});

	Target CompileLinux => _ => _
		.DependsOn(CompileCore, CompileLinuxDependencies)
		.Executes(() =>
		{
			Compile(EtoPlatformsLin.Select(p => $"{ProductName}.{p}").ToArray());
		});

	Target CompileMac => _ => _
		.DependsOn(CompileCore, CompileMacDependencies)
		.Executes(() =>
		{
			Compile(EtoPlatformsMac.Select(p => $"{ProductName}.{p}").ToArray());
		});

	Target CompileTests => _ => _
		.DependsOn(CompileCore)
		.Executes(() =>
		{
			Compile(new string[] { $"{ProductName}Test.Core", $"{ProductName}Test.Rendering" });
		});

	Target Test => _ => _
		.DependsOn(CompileTests)
		.Executes(() =>
		{
			string project = $"{ProductName}Test.Core";

			// Nuke supports passing project file names directly into NUnit to
			// specify which test assemblies to load. The NUnit console runner
			// can't load .csproj files out of the box, but the developers offer
			// an extension that does the trick. Unfortunately, NUnit currently
			// loads extensions by looking in your NuGet package directory for
			// anything matching the pattern NUnit.Extension.*, whereas NuGet
			// restores everything in lowercase these days. Works in Windows,
			// fails in Linux and macOS. Passing in the output assembly path
			// instead is the next best thing.
			NUnit3(settings => settings
				.SetConfiguration(Configuration)
				.SetInputFiles(GetOutputPath(project) / $"{project}.dll")
				.AddParameter("dataDirectory", RootDirectory / "test/data")
				.AddParameter("fgdDirectory", RootDirectory / "extras")
				.When(Configuration != "Release",
					s => s.SetWhereExpression("cat != Performance")));
		});

	private void Package(string[] etoPlatforms, Action<AbsolutePath, AbsolutePath, string> save)
	{
		foreach (string platform in etoPlatforms)
		{
			AbsolutePath source = GetOutputPath($"{ProductName}.{platform}") / (EnvironmentInfo.IsOsx ? $"{ProductName}.{platform}.app " : "");
			AbsolutePath dest = ArtifactsDirectory / platform;

			// Cleaning the entire artifacts directory is undesirable, since
			// usually this script is only building for one OS at a time. If
			// other platforms' packages exist already, leave them be.
			EnsureCleanDirectory(dest);

			string name = String.Join('-', ProductName, GitVersion.MajorMinorPatch, OsFriendlyName[EnvironmentInfo.Platform]);

			save.Invoke(source, dest, name);
		}
	}

	Target PackageWindows => _ => _
		.DependsOn(CompileWindows, Test)
		.Executes(() =>
		{
			Package(EtoPlatformsWin, (source, dest, name) =>
			{
				using (var archive = ZipArchive.Create())
				{
					archive.DeflateCompressionLevel = CompressionLevel.BestCompression;
					archive.AddAllFromDirectory(source);
					archive.SaveTo(dest / name + ".zip", new WriterOptions(CompressionType.Deflate));
				}
			});
		});

	Target PackageLinux => _ => _
		.DependsOn(CompileLinux, Test)
		.Executes(() =>
		{
			Package(EtoPlatformsLin, (source, dest, name) =>
			{
				using (var tarball = TarArchive.Create())
				{
					tarball.AddAllFromDirectory(source);
					tarball.SaveTo(dest / name + ".tar.gz", new WriterOptions(CompressionType.GZip));
				}
			});
		});

	Target PackageMac => _ => _
		.DependsOn(CompileMac, Test)
		.Executes(() =>
		{
			Package(EtoPlatformsMac, (source, dest, name) =>
			{
				ProcessTasks.StartProcess(
					"dmgbuild",
					"-s " + BuildProjectDirectory / "dmgbuild-settings.py " +
					"-D app=" + source +
					$"{ProductName} " +
					dest / name + ".dmg");
			});
		});
}
