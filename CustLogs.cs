using System;
using System.Collections.Generic;
using System.IO;

namespace MigrateData3to4
{
	class CustLogs
	{
		public List<string> DailyLogs = [];
		public List<string> IntvLogs = [];


		public CustLogs()
		{
			// Get all the stuff we need from Cumulus.ini
			ReadIniFile();
		}

		private void ReadIniFile()
		{
			var pwd = Directory.GetCurrentDirectory();

			if (!System.IO.File.Exists(pwd + Path.DirectorySeparatorChar + "Cumulus.ini"))
			{
				Utils.LogMessage("Failed to find Cumulus.ini file!");
				Console.WriteLine("Failed to find Cumulus.ini file!");
				Environment.Exit(1);
			}

			Utils.LogMessage("Reading Cumulus.ini file");

			IniFile ini = new IniFile("Cumulus.ini");

			// Custom Log Settings
			for (var i = 0; i < 10; i++)
			{
				if (ini.ValueExists("CustomLogs", "DailyFilename" + i))
				{
					var name = ini.GetValue("CustomLogs", "DailyFilename" + i, string.Empty);
					if (!string.IsNullOrEmpty(name))
					{
						DailyLogs.Add(name + ".txt");
					}
				}

				if (ini.ValueExists("CustomLogs", "IntervalFilename" + i))
				{
					var name = ini.GetValue("CustomLogs", "IntervalFilename" + i, string.Empty);
					if (!string.IsNullOrEmpty(name))
					{
						IntvLogs.Add(name);
					}
				}
			}
		}
	}
}
