using System;
using System.Drawing;
using System.Windows.Forms;

namespace AbinLibs
{
	/// <summary>
	/// 日志种类
	/// </summary>
	public enum RichLogType
	{
		/// <summary>
		/// 正常（黑色）
		/// </summary>
		Normal = 0,

		/// <summary>
		/// 高亮（蓝色）
		/// </summary>
		Highlight,

		/// <summary>
		/// 错误（红色）
		/// </summary>
		Error,

		/// <summary>
		/// 关键（紫色）
		/// </summary>
		Critical,

		/// <summary>
		/// 警示（橙色）
		/// </summary>
		Warning,

		/// <summary>
		/// 冗余（灰色）
		/// </summary>
		Verbose,
	}

	/// <summary>
	/// RichTextBox的日志显示控件使用的基础行
	/// </summary>
	public class RichLogEntry
	{
		/// <summary>
		/// 文字
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// 颜色类型
		/// </summary>
		public RichLogType Type { get; set; }

		/// <summary>
		/// RGB颜色
		/// </summary>
		public Color Color => GetColor(Type);

		/// <summary>
		/// 颜色类型转换为颜色
		/// </summary>
		/// <returns>RGB颜色</returns>
		public static Color GetColor(RichLogType type)
		{
			Color color;
			switch (type)
			{
				case RichLogType.Error:
					color = Color.Red;
					break;

				case RichLogType.Highlight:
					color = Color.Blue;
					break;

				case RichLogType.Critical:
					color = Color.Purple;
					break;

				case RichLogType.Warning:
					color = Color.Orange;
					break;

				case RichLogType.Verbose:
					color = Color.Gray;
					break;

				default:
					color = Color.Empty;
					break;
			}

			return color;
		}
	}

	/// <summary>
	/// 基于RichTextBox的日志显示控件，可同时接收来自不同线程的日志
	/// </summary>
	public class RichTextLogHandler
	{
		/// <summary>
		/// RichTextBox控件
		/// </summary>
		public RichTextBox TextBox { get; set; }

		/// <summary>
		/// 最大文本长度，超出则自动清空
		/// </summary>
		public int MaxLength { get; set; } = 32767;		

		/// <summary>
		/// 是否允许连续添加相同文字内容，默认为true
		/// </summary>
		public bool AllowRepetitive { get; set; } = true;

		/// <summary>
		/// 获取控件中的文本
		/// </summary>
		public string Text => GetText();		

		/// <summary>
		/// 滚动到文本框最底端
		/// </summary>
		public virtual void ScrollToBottom()
		{
			TextBox.Select(TextBox.Text.Length, 0);
			TextBox.ScrollToCaret();
		}

		/// <summary>
		/// 向控件内添加一行文字并指定颜色（线程安全）
		/// </summary>
		/// <param name="line">日志行</param>
		public virtual void AppendText(RichLogEntry line)
		{
			if (line == null)
			{
				AppendText("");
			}
			else
			{
				AppendText(line.Text, line.Type);
			}			
		}		

		/// <summary>
		/// 向控件内添加一行文字并指定颜色（线程安全）
		/// </summary>
		/// <param name="text">待添加的文字</param>
		/// <param name="type">日志种类</param>
		public virtual void AppendText(string text, RichLogType type = RichLogType.Normal)
		{
			if (TextBox.InvokeRequired)
			{
				TextBox.Invoke(new AppendTextDelegate(InvokeAppendText), new object[] { text, type });
			}
			else
			{
				InvokeAppendText(text, type);
			}
		}

		/// <summary>
		/// 清空控件中的文本
		/// </summary>
		public virtual void Clear()
		{
			m_lastMessage = null;
			if (TextBox.InvokeRequired)
			{
				TextBox.Invoke(new ClearDelegate(InvokeClear));
			}
			else
			{
				InvokeClear();
			}
		}	
		
		/// <summary>
		/// 获取控件中的文本
		/// </summary>
		/// <returns>控件文字</returns>
		public string GetText()
		{
			if (TextBox.InvokeRequired)
			{
				return (string)TextBox.Invoke(new GetTextDelegate(InvokeGetText));
			}

			return InvokeGetText();
		}

		#region 跨线程操作及回调
		private string m_lastMessage = null;
		private delegate void AppendTextDelegate(string text, RichLogType type);		
		private void InvokeAppendText(string text, RichLogType type)
		{
			if (!AllowRepetitive && m_lastMessage == text)
			{
				return;
			}

			m_lastMessage = text;

			// 规范化文字
			string line = string.Format("{0}\n", text ?? "");

			int caret = TextBox.SelectionStart;

			// 如果添加后将会超出最大长度限制，则先清空文本框原内容
			int curLength = TextBox.Text.Length;
			int length = line.Length;
			if (length + curLength > MaxLength)
			{
				TextBox.Clear();
				caret = 0;
			}

			// 用户是否正在查看文本框中的内容，如果是则停止自动滚动
			bool reviewing = caret < curLength - 1;

			// 添加文字行
			TextBox.AppendText(line);

			// 更改文字行颜色
			if (type != RichLogType.Normal)
			{
				Color color = RichLogEntry.GetColor(type);
				TextBox.Select(TextBox.Text.Length - length, length);
				TextBox.SelectionColor = color;
				TextBox.Select(caret, 0);
			}

			if (!reviewing)
			{
				ScrollToBottom();
			}
		}

		private delegate void ClearDelegate();
		private void InvokeClear()
		{
			TextBox.Clear();
		}

		private delegate string GetTextDelegate();
		private string InvokeGetText()
		{
			return TextBox.Text;
		}
		#endregion
	}
}
