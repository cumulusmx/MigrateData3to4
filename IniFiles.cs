using System;
using System.IO;

namespace MigrateData3to4
{
	static class IniFiles
	{
		static internal void Copy()
		{
			string[] files = { "alltime.ini", "month.ini", "today.ini", "year.ini", "yesterday.ini", "diary.db" };
			var fileSep = Path.DirectorySeparatorChar;

			Console.WriteLine("\nCopying ini files");
			Utils.LogMessage("Copying ini files");


			foreach (string file in files)
			{
				var src = Program.Src + fileSep + file;
				var dst = Program.Dst + fileSep + file;
				// test if the file exists
				if (File.Exists(src))
				{
					try
					{
						var col = Console.ForegroundColor;
						Console.Write($"Copying {file} to destination... ");
						Utils.LogMessage($"Copying {file} to destination");
						File.Copy(src, dst);
						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine($"Copied OK");
						Utils.LogMessage($"Copied {file} to destination");
						Console.ForegroundColor = col;
					}
					catch (Exception ex )
					{
						var col = Console.ForegroundColor;
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine($"Error copying file {file} - {ex.Message}\n");
						Console.ForegroundColor = col;
					}
				}
				else
				{
					var col = Console.ForegroundColor;
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine($"File {file} not found: Manually copy this file to the \\datav4 folder if required");
					Utils.LogMessage($"File {file} not found");
					Console.ForegroundColor = col;
				}
			}
		}
	}
}
