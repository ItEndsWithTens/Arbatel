using System;
using System.IO;
using System.Threading;

namespace Arbatel
{
	public class AutoReloader
	{
		private bool _enabled = false;
		public bool Enabled
		{
			get { return _enabled; }
			set
			{
				_enabled = value;

				Watcher.EnableRaisingEvents = Enabled;

				if (!Enabled)
				{
					ReloadTimer.Change(Timeout.Infinite, Timeout.Infinite);
				}
			}
		}

		private string _file;
		public string File
		{
			get { return _file; }
			set
			{
				_file = value;

				_lastWriteTimeUtc = new FileInfo(File).LastWriteTimeUtc;

				Watcher.Path = Path.GetDirectoryName(File);
				Watcher.Filter = Path.GetFileName(File);
			}
		}

		/// <summary>
		/// The timeout before reloading, in seconds.
		/// </summary>
		public int Interval { get; set; } = 1;

		private DateTime _lastWriteTimeUtc = DateTime.MinValue;
		public DateTime LastWriteTimeUtc
		{
			get { return _lastWriteTimeUtc; }
			private set
			{
				// Check for any kind of difference, not just newer revisions;
				// any difference in write time represents a file change.
				if (value == _lastWriteTimeUtc)
				{
					return;
				}

				// Stopping a System.Threading.Timer isn't exactly intuitive.
				ReloadTimer.Change(Timeout.Infinite, Timeout.Infinite);

				_lastWriteTimeUtc = value;

				ReloadTimer.Change(Interval * 1000, Timeout.Infinite);
			}
		}

		public Action<string> ReloadAction { get; private set; }

		public Timer ReloadTimer { get; }

		public FileSystemWatcher Watcher { get; } = new FileSystemWatcher
		{
			EnableRaisingEvents = false,

			// FileSystemWatcher is a low level feature, and basically just lets
			// you access file system events from the OS. Unfortunately not all
			// operating systems, or all applications, handle file modification
			// the same way, so just watch this directory/file pair for any kind
			// of change at all, and use that as a pretext to check the file's
			// last write time when the event handler is called.
			NotifyFilter =
				NotifyFilters.Attributes |
				NotifyFilters.CreationTime |
				NotifyFilters.DirectoryName |
				NotifyFilters.FileName |
				NotifyFilters.LastWrite |
				NotifyFilters.LastAccess |
				NotifyFilters.Security |
				NotifyFilters.Size
		};

		public AutoReloader(Action<string> action)
		{
			ReloadAction = action;

			Watcher.Changed += Watcher_Changed;

			ReloadTimer = new Timer((state) =>
			{
				ReloadTimer.Change(Timeout.Infinite, Timeout.Infinite);

				ReloadAction.Invoke(File);
			});
		}
		public AutoReloader(string file, Action<string> action) : this(action)
		{
			File = file;
		}

		private void Watcher_Changed(object sender, FileSystemEventArgs e)
		{
			LastWriteTimeUtc = new FileInfo(e.FullPath).LastWriteTimeUtc;
		}
	}
}
