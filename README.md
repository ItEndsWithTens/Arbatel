
  Arbatel
  =======

  A Quake .map viewer with support for func_instance entities.

  Not ready for end users! No releases are currently available, as this project is still in its infancy. Curious developers are welcome to clone the repository and follow the build instructions further below.

  Developers and adventurous users would do well to expect major bugs.



  ## Requirements
  
  #### Windows
  - [.NET Framework 4.7.1](https://dotnet.microsoft.com/download)

  #### Linux
  - [Mono](https://www.mono-project.com/download/stable/#download-lin)
  - Gtk# 2.12
    - Ubuntu and derivatives: `sudo apt install gtk-sharp2`

  #### macOS
  - [Mono](https://www.mono-project.com/download/stable/#download-mac)



  ## Usage

  Configure program settings with Edit|Preferences (Ctrl+,) and ensure appropriate FGDs and textures are loaded. WAD2 format texture collections will need to be generated or downloaded elsewhere, but sample FGDs are included in the 'extras' directory, covering vanilla Quake entities with features provided by [ericw's compiling tools](http://ericwa.github.io/ericw-tools/) (a big thank you to [DaZ](https://twitter.com/tdDaz) for curating quake4ericwTools.fgd, and the func_instance entity.
  
  With your preferences set, open a .map file with File|Open (Ctrl+O), toggle fly mode with Z, and use WASDEQ for first person mouselook controls. Save the collapsed version of a map containing instances from the Instancing menu.



  ## Development

  With the repository cloned, to work on the project you'll need to:

  ### Install prerequisites

  #### Windows
  - [Visual Studio 2017](https://visualstudio.microsoft.com/vs/community/)
  - Up to date PowerShell (part of the [Windows Management Framework](https://docs.microsoft.com/en-us/powershell/wmf/overview))
    - WMF 5.1 is included with Windows 10, and works as-is, but earlier versions of the OS may have outdated framework installations, preventing the Nuke build script from properly passing some arguments down into MSBuild.

  #### Linux
  - libcurl3 (GitVersion is currently broken in *nix without this)
    - Ubuntu and derivatives: `sudo apt-get install libcurl3`

  #### macOS
  - [dmgbuild](https://github.com/al45tair/dmgbuild)
    - `pip install dmgbuild`

  ### Build dependencies

  Most of this project's dependencies are simply NuGet packages, but a couple are custom versions of third-party libraries and need to be built from source before this project will work properly from an IDE. If you know what you're doing, you're welcome to build them by hand, but I'd recommend using the Nuke script.

  If you have the Nuke global tool installed, it's as easy as changing to the root solution directory and calling
  ```Shell
  nuke
  ```

  Alternatively, there are bootstrapping scripts included that can get you up and running; in PowerShell run `.\build.ps1`, or in Bash run `./build.sh`. Any of those three options, run at least once, will get the project set up for development.

  ### Develop
  
  With the dependencies ready to go, you can easily edit, build, and debug from Visual Studio, MonoDevelop, or VS for Mac, as appropriate for a given platform.

  The suite of automated NUnit tests will run during a command line build with no special setup. To run them using the NUnit test adapter in Visual Studio, go to Test|Test Settings->Select Test Settings File, and choose test/src/vsadapter.runsettings.

  Final packaging is also handled by the Nuke script, and is the default target. Should you want to run the script from Visual Studio, open the Task Runner Explorer and give it a moment to refresh. Running any of the listed targets is the same as using `--target X` at the command line.



  ## Credits

  Built with:
  - [Eto.Forms](https://github.com/picoe/Eto) - Cross platform, native look GUIs in C#
  - [etoViewport](https://github.com/philstopford/etoViewport) - OpenGL in Eto.Forms by way of OpenTK
  - [JsonSettings](https://github.com/Nucs/JsonSettings) - Simple, lightweight app settings
  - [Nuke](https://nuke.build) - Cross platform build automation with a C# DSL



  ## Contact
  If you have any questions or comments, feel free to get in touch!

  robert.martens@gmail.com  
  [@ItEndsWithTens](https://twitter.com/ItEndsWithTens)
