
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

  Configure program settings with Edit|Preferences (Ctrl+,) and ensure appropriate FGDs and textures are loaded. WAD2 format texture collections will need to be generated or downloaded elsewhere, but sample FGDs are included in the 'extras' directory, covering vanilla Quake entities with features provided by [ericw's compiling tools](http://ericwa.github.io/ericw-tools/) (a big thank you to [DaZ](https://twitter.com/tdDaz) for curating [quake4ericwTools.fgd](https://dl.dropboxusercontent.com/u/33279452/quake4ericwTools.fgd)), and the func_instance entity.
  
  With your preferences set, open a .map file with File|Open (Ctrl+O), toggle fly mode with Z, and use WASDEQ for first person mouselook controls. Save the collapsed version of a map containing instances from the Instancing menu.



  ## Development

  Building and packaging this project requires some additional software:

  #### Windows
  - [Visual Studio 2017, Update 2 or newer](https://visualstudio.microsoft.com/vs/community/)

  #### Linux
  - libcurl3 (GitVersion is currently broken in *nix without this)
    - Ubuntu and derivatives: `sudo apt-get install libcurl3`

  #### macOS
  - [dmgbuild](https://github.com/al45tair/dmgbuild)
    - `pip install dmgbuild`

  Once those are installed, open a PowerShell or Bash terminal as appropriate, and run build.ps1 or build.sh, respectively. The main solution file can of course be opened in Visual Studio, VS for Mac, or MonoDevelop to easily edit, build, and debug, but the full end-to-end compile and package process is handled by the Nuke bootstrapping scripts.

  ## Credits

  Built with:
  - [Eto.Forms](https://github.com/picoe/Eto) - Cross platform, native look GUI library
  - [etoViewport](https://github.com/philstopford/etoViewport) - An OpenTK based OpenGL control for Eto.Forms
  - [JsonSettings](https://github.com/Nucs/JsonSettings) - Simple, lightweight app settings library
  - [Nuke](https://nuke.build) - Cross platform build automation with a C# DSL



  ## Contact
  If you have any questions or comments, feel free to get in touch!

  robert.martens@gmail.com  
  [@ItEndsWithTens](https://twitter.com/ItEndsWithTens)
