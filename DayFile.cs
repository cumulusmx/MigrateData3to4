using System;
using System.IO;
using System.Linq;

namespace MigrateData3to4
{
	static class DayFile
	{

		private static readonly string inpFilename = Program.Src + Path.DirectorySeparatorChar + "dayfile.txt";
		private static readonly string outFilename = Program.Dst + Path.DirectorySeparatorChar + "dayfile.txt";


		static internal void Convert()
		{
			Console.WriteLine($"Migrating v3 dayfile.txt to {outFilename}");

			if (File.Exists(inpFilename))
			{
				Utils.LogMessage($"Dayfile: Found input file: {inpFilename}");
			}
			else
			{
				Console.WriteLine("Dayfile: Warning! Input dayfile.txt not found");
				Utils.LogMessage("Dayfile: Warning! Input dayfile.txt not found");
				return;
			}

			var lineNum = 1;

			try
			{

				// read the first line to determine format
				var lines = File.ReadLines(inpFilename).ToArray();
				char sepInp = Utils.GetLogFileSeparator(lines[0], ',');
				Utils.LogMessage($"LogFile: File {inpFilename} is using the separator: {sepInp}");

				Utils.TryDetectNewLine(inpFilename, out string endOfLine);
				Utils.LogMessage($"LogFile: File {inpFilename} is using the line ending: {(endOfLine == "\n" ? "\\n" : "\\r\\n")}");

				using var sw = new StreamWriter(outFilename) { NewLine = endOfLine };

				foreach (var inpLine in lines)
				{
					var fields = inpLine.Split(sepInp);

					// Do the date
					fields[0] = Utils.DdmmyyStrToStr(fields[0]);

					// do the rest of the fields, converting comma decimals to dot
					for (var i = 1; i < fields.Length; i++)
					{
						fields[i] = fields[i].Replace(',', '.');
					}

					// Write the output
					sw.WriteLine(string.Join(',', fields));

					lineNum++;
				}
				sw.Flush();
				sw.Close();
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Error at line {lineNum} - {ex.Message}\n");
				Console.ResetColor();
			}
		}
	}
}
