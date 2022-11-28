using System;
using System.Collections.Generic;
using System.IO;
using AbinLibs;

namespace SourceCounter.BLL
{
	class ParseThread : MessageThread
	{
		public enum RunMode { Parse, Copy }
		public RunMode Mode { get; private set; }
		public IEnumerable<string> FileList => m_fileList;
		public int TotalFiles => m_fileList?.Count ?? 0;
		public int TotalLines { get; private set; }
		public string Contents { get; private set; }
		public RichTextLogHandler Log { get; set; }		

		private DataRules m_rules = new DataRules();
		private string m_folder = null;
		private List<string> m_fileList = null;

		public void StartParse(string fileName)
		{
			if (IsAlive)
			{
				return;
			}

			Mode = RunMode.Parse;
			m_folder = Path.GetDirectoryName(fileName);
			Log.AppendText($"Parsing folder: {m_folder}");			
			m_fileList = new List<string>();
			TotalLines = 0;
			base.Start();
		}

		public void StartExtract()
		{
			if (IsAlive)
			{
				return;
			}

			Log.AppendText("Extracting source code ...");
			Mode = RunMode.Copy;
			Contents = string.Empty;
			base.Start();
		}

		protected override void ThreadProc()
		{
			try
			{
				if (Mode == RunMode.Parse)
				{
					DirectoryInfo di = new DirectoryInfo(m_folder);
					GetFiles(di);
					Log.AppendText($"Total files: {m_fileList.Count}");
					TotalLines = CalcLines();
					Log.AppendText($"Total lines: {TotalLines}");
				}
				else
				{
					int length = CopyText();
					Log.AppendText($"Source text extracted: {length}");
				}				
			}
			catch (Exception ex)
			{
				Log.AppendText(ex.Message, RichLogType.Error);
			}			
		}

		protected override void OnStop()
		{
			base.OnStop();
			Log.AppendText("");
		}

		private void GetFiles(DirectoryInfo di)
		{
			if (!m_rules.TestFolder(di))
			{
				return;
			}

			var fileList = di.GetFiles();
			foreach ( var file in fileList )
			{
				if (m_rules.TestFile(file))
				{
					m_fileList.Add(file.FullName);
				}
			}

			var folderList = di.GetDirectories();
			foreach (var subFolder in folderList)
			{
				GetFiles(subFolder);
			}
		}

		private int CalcLines()
		{
			int count = 0;
			foreach ( var file in m_fileList )
			{
				var lines = File.ReadAllLines(file);
				count += lines.Length;
			}
			return count;
		}

		private int CopyText()
		{
			string contents = string.Empty;

			foreach (var file in m_fileList)
			{
				var text = File.ReadAllText(file);
				if (DataRules.MaxCopyLength > 0 && contents.Length + text.Length + 2 > DataRules.MaxCopyLength)
				{
					break;
				}

				contents += "\n\n" + text;
			}

			Contents = contents;
			return contents.Length;
		}
	}
}
