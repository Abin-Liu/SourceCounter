using System;
using System.Configuration;

namespace AbinLibs
{
	/// <summary>
	/// 配置文件帮助类，用来读写app.config或web.configp文件
	/// </summary>
	public class AppConfig
	{
		#region ConnectionStrings
		/// <summary>
		/// 读取*.config中ConnectionStrings段的值，阻止exception
		/// </summary>
		/// <param name="key">配置名</param>
		/// <param name="decrypt">字符串是否需要AES解密</param>
		/// <param name="password">AES解密密码</param>
		/// <returns>连接字符串，不存在则返回null</returns>
		public static string ReadConnectionString(string key, bool decrypt = false, string password = null)
		{
			string value = null;
			try
			{
				value = ConfigurationManager.ConnectionStrings[key].ConnectionString;
			}
			catch
			{
			}

			if (decrypt)
			{
				value = CryptoHelper.Decrypt(value, password);
			}

			return value;
		}
		#endregion

		#region AppSettings
		/// <summary>
		/// 读取*.config中AppSettings段的值，阻止exception
		/// </summary>
		/// <param name="key">配置名</param>
		/// <param name="defaultVal">默认值</param>
		/// <param name="decrypt">字符串是否需要AES解密</param>
		/// <param name="password">AES解密密码</param>
		/// <returns>返回配置值，不存在则返回默认值</returns>
		public static string ReadAppSettings(string key, string defaultVal = "", bool decrypt = false, string password = null)
		{
			

			string value = null;
			try
			{
				value = ConfigurationManager.AppSettings[key];
			}
			catch
			{
			}

			if (decrypt)
			{
				value = CryptoHelper.Decrypt(value, password);
			}

			return value ?? defaultVal;
		}		

		/// <summary>
		/// 读取*.config中AppSettings段的值并转化为int类型
		/// </summary>
		/// <param name="key">配置名</param>
		/// <param name="defaultVal">默认值</param>
		/// <returns>返回配置值，不存在则返回默认值</returns>
		public static int ReadAppSettings(string key, int defaultVal)
		{
			string value = ReadAppSettings(key, "", false);
			try
			{
				return Convert.ToInt32(value);
			}
			catch
			{
				return defaultVal;
			}
		}

		/// <summary>
		/// 读取*.config中AppSettings段的值并转化为double类型
		/// </summary>
		/// <param name="key">配置名</param>
		/// <param name="defaultVal">默认值</param>
		/// <returns>返回配置值，不存在则返回默认值</returns>
		public static double ReadAppSettings(string key, double defaultVal)
		{
			string value = ReadAppSettings(key, "", false);
			try
			{
				return Convert.ToDouble(value);
			}
			catch
			{
				return defaultVal;
			}
		}

		/// <summary>
		/// 读取*.config中AppSettings段的值并转化为bool类型
		/// </summary>
		/// <param name="key">配置名</param>
		/// <param name="defaultVal">默认值</param>
		/// <returns>返回配置值，不存在则返回默认值</returns>
		public static bool ReadAppSettings(string key, bool defaultVal)
		{
			int value = ReadAppSettings(key, defaultVal ? 1 : 0);
			return value != 0;
		}

		/// <summary>
		/// 读取*.config中AppSettings段的值并转化为DateTime类型
		/// </summary>
		/// <param name="key">配置名</param>
		/// <param name="defaultVal">默认值</param>
		/// <returns>返回配置值，不存在则返回DateTime.MinValue</returns>
		public static DateTime ReadAppSettings(string key, DateTime defaultVal)
		{
			string value = ReadAppSettings(key, "", false);
			try
			{
				return Convert.ToDateTime(value);
			}
			catch
			{
				return defaultVal;
			}
		}

		/// <summary>
		/// 向*.config中AppSettings段写入string类型值
		/// </summary>
		/// <param name="key">配置名</param>
		/// <param name="value">写入值</param>
		/// <param name="encrypt">写入前是否需要AES加密</param>
		/// <param name="password">AES加密密码</param>
		public static void WriteAppSettings(string key, string value, bool encrypt = false, string password = null)
		{
			Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			if (value == null)
			{
				config.AppSettings.Settings.Remove(key);
			}
			else
			{
				if (encrypt)
				{
					value = CryptoHelper.Encrypt(value, password);
				}

				config.AppSettings.Settings.Add(key, value);
			}

			config.Save();
		}

		/// <summary>
		/// 向*.config中AppSettings段写入int类型值
		/// </summary>
		/// <param name="key">配置名</param>
		/// <param name="value">写入值</param>
		public static void WriteAppSettings(string key, int value)
		{
			WriteAppSettings(key, string.Format("{0}", value), false);
		}

		/// <summary>
		/// 向*.config中AppSettings段写入double类型值
		/// </summary>
		/// <param name="key">配置名</param>
		/// <param name="value">写入值</param>
		public static void WriteAppSettings(string key, double value)
		{
			WriteAppSettings(key, string.Format("{0}", value), false);
		}

		/// <summary>
		/// 向*.config中AppSettings段写入bool类型值
		/// </summary>
		/// <param name="key">配置名</param>
		/// <param name="value">写入值</param>
		public static void WriteAppSettings(string key, bool value)
		{
			WriteAppSettings(key, value ? "1" : "0", false);
		}

		/// <summary>
		/// 向*.config中AppSettings段写入DateTime类型值
		/// </summary>
		/// <param name="key">配置名</param>
		/// <param name="value">写入值</param>
		/// <param name="format">写入格式</param>
		public static void WriteAppSettings(string key, DateTime value, string format = "yyyy-MM-dd")
		{
			WriteAppSettings(key, value.ToString(format), false);
		}
		#endregion		
	}
}