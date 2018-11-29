using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Security.Permissions;
using MDLog;

namespace WFCTestLib.Util
{
	/// <summary>
	/// Wrapper class for getting information from the MDLog.  The MauiDriver writes information
	/// to the log such as TCID, Context ID, Run ID, run flags, etc.
	/// </summary>
	public class MDLogWrapper
	{
		public const string LogFilename = "results.xml";
		private const string FlagsSettingsType = "RunBuildFlags";

		private static MDLogWrapper m_instance = null;

		Settings m_runFlags;

        [FileIOPermission(SecurityAction.Assert, Unrestricted=true)]
		private MDLogWrapper()
		{
			if (!File.Exists(LogFilename))
				throw new FileNotFoundException(LogFilename + " was not found!");

			using (MDLog.Log log = MDLog.Log.OpenLog(LogFilename))
			{
				// Grab any other info here.
				m_runFlags = log.RunInfo.get_Settings(FlagsSettingsType);
			}
		}

		/// <summary>
		/// Returns the singleton instance of the wrapper.  If the log file could not be
		/// opened, Instance will return null.
		/// </summary>
		public static MDLogWrapper GetInstance()
		{
			if (m_instance == null)
			{
				try
				{
					m_instance = new MDLogWrapper();
				}
				catch (Exception e) {
					Debug.WriteLine("MDLogWrapper.Instance: Threw exception opening MDLog: " + e.ToString());

                    // We used to eat exceptions here, but it just made issues too hard to debug.
                    throw;
				}
			}

			return m_instance;
		}

		/// <summary>
		/// Case-insensitive search for a run flag.
		/// </summary>
		/// <param name="flagName">Name of the flag to search for.</param>
		/// <returns>True if the flag is present, false if it isn't.</returns>
		public bool IsRunFlagPresent(string flagName)
		{
			if (m_runFlags == null)
				return false;

			foreach (SettingsItem item in m_runFlags.Items)
			{
				if (item.Name != null && item.Name.ToLowerInvariant() == flagName.ToLowerInvariant())
					return true;
			}

			return false;
		}
	}
}
