using System;
using System.Collections.Generic;
using System.IO;
using AbinLibs;

namespace SourceCounter.BLL
{
	class DataRules
	{
		public static readonly string ProjectFilters = AppConfig.ReadAppSettings("ProjectFilters", null);
		public static readonly int MaxCopyLength = AppConfig.ReadAppSettings("MaxCopyLength", 0);
		private readonly HashSet<string> m_folderBlackList = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		private readonly HashSet<string> m_fileTyles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		public DataRules()
		{
			string[] items = AppConfig.ReadAppSettings("FolderBlackList", string.Empty).Split(',');
			foreach (var item in items)
			{
				string name = item.Trim();
				if (name.Length != 0)
				{
					m_folderBlackList.Add(name);
				}
			}

			items = AppConfig.ReadAppSettings("FileTypes", string.Empty).Split(',');
			foreach (var item in items)
			{
				string name = item.Trim();
				if (name.Length != 0)
				{
					m_fileTyles.Add(name);
				}
			}
		}

		public bool TestFolder(DirectoryInfo di)
		{
			return !m_folderBlackList.Contains(di.Name);
		}

		public bool TestFile(FileInfo fi)
		{
			return m_fileTyles.Contains(fi.Extension);
		}
	}
}
