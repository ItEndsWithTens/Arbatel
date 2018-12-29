using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.Nunit;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Writers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Common.Tools.NuGet.NuGetTasks;
using static Nuke.Common.Tools.Nunit.NunitTasks;

class Build : NukeBuild
{
	public static string GetMsBuildPath()
	{
		// Visual Studio 2017 Update 2 and newer install vswhere by default.
		return GetMsBuildPath("vswhere.exe");
	}
	public static string GetMsBuildPath(string vswherePath)
	{
		string args = "-latest -products * -requires Microsoft.Component.MSBuild";

		var vswhere = new Process();
		vswhere.StartInfo.FileName = vswherePath;
		vswhere.StartInfo.UseShellExecute = false;
		vswhere.StartInfo.RedirectStandardOutput = true;
		vswhere.StartInfo.CreateNoWindow = true;

		vswhere.StartInfo.Arguments = String.Join(' ', args, "-property installationPath");
		vswhere.Start();
		var output = new StringBuilder();
		while (!vswhere.StandardOutput.EndOfStream)
		{
			output.Append(vswhere.StandardOutput.ReadLine());
		}
		string vsPath = output.ToString();

		vswhere.StartInfo.Arguments = String.Join(' ', args, "-property installationVersion");
		vswhere.Start();
		output.Clear();
		while (!vswhere.StandardOutput.EndOfStream)
		{
			output.Append(vswhere.StandardOutput.ReadLine());
		}
		Int32.TryParse(output.ToString().Split('.')[0], out int vsMajor);

		if (vsMajor < 15)
		{
			throw new Exception("Can't build with less than VS 2017!");
		}

		return Path.Combine(vsPath, "MSBuild", vsMajor.ToString() + ".0", "Bin", "MSBuild.exe");
	}

	public static int Main()
	{
		return Execute<Build>(x => x.PackageWindows);
	}

	[Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
	readonly string Configuration = IsLocalBuild ? "Debug" : "Release";

	const string ProjectName = "Arbatel";

	[Solution(ProjectName + ".sln")]
	readonly Solution Solution;

	[GitRepository]
	readonly GitRepository GitRepository;

	[GitVersion]
	readonly GitVersion GitVersion;

	AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
	AbsolutePath StagingDirectory => RootDirectory / "staging";
	AbsolutePath SourceDirectory => RootDirectory / "src";

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

			// Make sure all files are gone first; can't remove empty folders.
			foreach (string file in GlobFiles(SourceDirectory, "**/bin/**/*", "**/obj/**/*"))
			{
				DeleteFile(file);
			}

			IEnumerable<string> directories = GlobDirectories(SourceDirectory, "**/bin/**", "**/obj/**");

			// Sort paths by length, and reverse to delete the deepest first;
			// the parent levels will then be empty and cleanly deletable.
			var list = new List<string>(directories);
			list.Sort((a, b) => a.Length.CompareTo(b.Length));
			list.Reverse();
			DeleteDirectories(list);

			EnsureCleanDirectory(StagingDirectory);
		});

	Target CompileCore => _ => _
		.DependsOn(Clean)
		.Executes(() =>
		{
			string msbuildPath = GetMsBuildPath();

			AbsolutePath projectDir = RootDirectory / "src" / $"{ProjectName}.Core";
			AbsolutePath project = projectDir / $"{ProjectName}.Core.csproj";

			MSBuildProject parsed = MSBuildParseProject(project, s => s.SetToolPath(msbuildPath));

			AbsolutePath buildDir = projectDir / parsed.Properties["OutputPath"];

			NuGetRestore(project);

			MSBuild(s => new MSBuildSettings()
				.SetToolPath(msbuildPath)
				.SetProjectFile(project)
				.SetTargets("Build")
				.SetConfiguration(Configuration)
				.SetAssemblyVersion(GitVersion.GetNormalizedAssemblyVersion())
				.SetFileVersion(GitVersion.GetNormalizedFileVersion())
				.SetInformationalVersion(GitVersion.InformationalVersion)
				.SetMaxCpuCount(Environment.ProcessorCount)
				.SetNodeReuse(IsLocalBuild));
		});

	Target CompileWindows => _ => _
		.DependsOn(CompileCore)
		.Executes(() =>
		{
			// Currently, Nuke can only find C drive installations of VS.
			string msbuildPath = GetMsBuildPath();

			foreach (string platform in new string[] { "WinForms" })//, "Wpf" })
			{
				AbsolutePath projectDir = RootDirectory / "src" / "gui" / $"{ProjectName}.{platform}";
				AbsolutePath project = projectDir / $"{ProjectName}.{platform}.csproj";

				MSBuildProject parsed = MSBuildParseProject(project, s => s.SetToolPath(msbuildPath));

				AbsolutePath buildDir = projectDir / parsed.Properties["OutputPath"];

				NuGetRestore(project);

				MSBuild(s => new MSBuildSettings()
					.SetToolPath(msbuildPath)
					.SetProjectFile(project)
					.SetTargets("Build")
					.SetConfiguration(Configuration)
					.SetAssemblyVersion(GitVersion.GetNormalizedAssemblyVersion())
					.SetFileVersion(GitVersion.GetNormalizedFileVersion())
					.SetInformationalVersion(GitVersion.InformationalVersion)
					.SetMaxCpuCount(Environment.ProcessorCount)
					.SetNodeReuse(IsLocalBuild));

				CopyDirectoryRecursively(buildDir, StagingDirectory);
			}
		});

	Target CompileTests => _ => _
		.DependsOn(CompileCore)
		.Executes(() =>
		{
			string project = RootDirectory / "test" / "src" / $"{ProjectName}Test.Core" / $"{ProjectName}Test.Core.csproj";

			NuGetRestore(project);

			MSBuildSettings settings = new MSBuildSettings()
				.SetProjectFile(project)
				.SetTargets("Rebuild")
				.SetConfiguration(Configuration)
				.SetAssemblyVersion(GitVersion.GetNormalizedAssemblyVersion())
				.SetFileVersion(GitVersion.GetNormalizedFileVersion())
				.SetInformationalVersion(GitVersion.InformationalVersion)
				.SetMaxCpuCount(Environment.ProcessorCount)
				.SetNodeReuse(IsLocalBuild);

			if (EnvironmentInfo.IsWin)
			{
				settings = settings.SetToolPath(GetMsBuildPath());
			}

			MSBuild(s => settings);
		});

	Target TestWindows => _ => _
		.DependsOn(CompileWindows, CompileTests)
		.Executes(() =>
		 {
			 Nunit3(s => new Nunit3Settings()
				.SetInputFiles(RootDirectory / "test" / $"{ProjectName}Test.Core" / ".csproj"));
		 });

	Target PackageWindows => _ => _
		//.DependsOn(TestWindows)
		.DependsOn(CompileWindows)
		.Executes(() =>
		{
			EnsureExistingDirectory(ArtifactsDirectory);

			using (var archive = ZipArchive.Create())
			{
				archive.DeflateCompressionLevel = CompressionLevel.BestCompression;
				archive.AddAllFromDirectory(StagingDirectory);

				string name = String.Join('-', ProjectName, GitVersion.SemVer, EnvironmentInfo.Platform);
				archive.SaveTo(ArtifactsDirectory / name + ".zip", new WriterOptions(CompressionType.Deflate));
			}
		});
}
