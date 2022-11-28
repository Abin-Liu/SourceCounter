/////////////////////////////////////////////////////////////
// CryptoHelper
//
// Cryptography的简易封装类，提供AES字符串加密解密和MD5校验码计算。
//
// API:
//
// Encrypt(string text, string key = null) -- 使用密钥(key)来加密字符串(text)，返回密文字符串
// Decrypt(string text, string key = null) -- 使用密钥(key)来解密字符串(text)，如果密钥正确则返回明文字符串，错误则返回null
// MD5(string text) -- 计算字符串(text)的MD5校验码（返回32位小写字符串）
/////////////////////////////////////////////////////////////

using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace AbinLibs
{
	/// <summary>
	/// Cryptography的简易封装类，提供DES字符串加密解密和MD5校验码计算。
	/// </summary>
	public sealed class CryptoHelper
	{
		/// <summary> 
		/// DES加密，使用密钥(key)来加密字符串(text)
		/// </summary> 
		/// <param name="text">明文字符串</param> 
		/// <param name="key">密钥</param> 
		/// <returns>密文字符串</returns> 
		public static string Encrypt(string text, string key = null)
		{
			if (string.IsNullOrEmpty(key))
			{
				key = DefaultKey; // 使用默认密钥
			}			

			byte[] bKey = new byte[32];
			Array.Copy(Encoding.UTF8.GetBytes(key.PadRight(bKey.Length)), bKey, bKey.Length);
			byte[] bVector = new byte[16];
			Array.Copy(Encoding.UTF8.GetBytes(Vector.PadRight(bVector.Length)), bVector, bVector.Length);

			byte[] Cryptograph = null; // 加密后的密文

			Rijndael Aes = Rijndael.Create();
			try
			{
				byte[] plainBytes = Encoding.UTF8.GetBytes(text);

				// 开辟一块内存流
				using (MemoryStream Memory = new MemoryStream())
				{
					// 把内存流对象包装成加密流对象
					using (CryptoStream Encryptor = new CryptoStream(Memory, Aes.CreateEncryptor(bKey, bVector), CryptoStreamMode.Write))
					{
						// 明文数据写入加密流
						Encryptor.Write(plainBytes, 0, plainBytes.Length);
						Encryptor.FlushFinalBlock();
						Cryptograph = Memory.ToArray();
					}
				}
			}
			catch
			{
				Cryptograph = null;
			}

			if (Cryptograph == null)
			{
				return "";
			}

			return Convert.ToBase64String(Cryptograph);
		}

		/// <summary> 
		/// DES解密，使用密钥(key)来解密字符串(text)
		/// </summary> 
		/// <param name="text">密文字符串</param> 
		/// <param name="key">密钥</param> 
		/// <returns>如果密钥正确则返回明文字符串，密钥错误则返回null</returns> 
		public static string Decrypt(string text, string key = null)
		{
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}

			if (string.IsNullOrEmpty(key))
			{
				key = DefaultKey; // 使用默认密钥
			}
			
			byte[] bKey = new byte[32];			
			Array.Copy(Encoding.UTF8.GetBytes(key.PadRight(bKey.Length)), bKey, bKey.Length);
			byte[] bVector = new byte[16];
			Array.Copy(Encoding.UTF8.GetBytes(Vector.PadRight(bVector.Length)), bVector, bVector.Length);
			byte[] original = null; // 解密后的明文			

			Rijndael Aes = Rijndael.Create();
			try
			{
				byte[] encryptedBytes = Convert.FromBase64String(text);

				// 开辟一块内存流，存储密文
				using (MemoryStream Memory = new MemoryStream(encryptedBytes))
				{
					// 把内存流对象包装成加密流对象
					using (CryptoStream Decryptor = new CryptoStream(Memory, Aes.CreateDecryptor(bKey, bVector), CryptoStreamMode.Read))
					{
						// 明文存储区
						using (MemoryStream originalMemory = new MemoryStream())
						{
							byte[] Buffer = new byte[1024];
							int readBytes = 0;
							while ((readBytes = Decryptor.Read(Buffer, 0, Buffer.Length)) > 0)
							{
								originalMemory.Write(Buffer, 0, readBytes);
							}

							original = originalMemory.ToArray();
						}
					}
				}
			}
			catch
			{
				original = null;
			}

			if (original == null)
			{
				return null;
			}

			return Encoding.UTF8.GetString(original);
		}

		/// <summary> 
		/// MD5计算
		/// </summary> 
		/// <param name="text">字符串</param> 
		/// <returns>MD5校验码（32位字符串）</returns> 
		public static string MD5(string text)
		{			
			byte[] data = Encoding.UTF8.GetBytes(text ?? "");
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] output = md5.ComputeHash(data);
			return BitConverter.ToString(output).Replace("-", "").ToLower();			
		}		

		static readonly byte[] DataBlock = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF, 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
		static readonly string Vector = Encoding.UTF8.GetString(DataBlock);
		const string DefaultKey = "adw#$sdw%s*sdd=="; // 默认密钥
	}
}
