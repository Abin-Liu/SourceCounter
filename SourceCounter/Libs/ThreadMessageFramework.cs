using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace AbinLibs
{
	/// <summary>
	/// MessageThread
	/// </summary>
	public abstract class MessageThread : WorkerThread
	{
		/// <summary>
		/// 消息窗口句柄，通常为main from的Handle
		/// </summary>
		public IntPtr MessageWnd { get; set; }

		/// <summary>
		/// 线程启动
		/// </summary>
		public const int MSG_THREAD_START = 0x4000;

		/// <summary>
		/// 线程停止
		/// </summary>
		public const int MSG_THREAD_STOP = 0x8000;		

		/// <summary>
		/// 向MessageWnd异步发送消息
		/// </summary>
		/// <param name="wParam">wParam</param>
		/// <param name="lParam">lParam</param>
		public virtual void PostMessage(int wParam, int lParam = 0)
		{
			if (MessageWnd != IntPtr.Zero && IsWindow(MessageWnd))
			{
				PostMessage(MessageWnd, MessageThreadForm.ThreadMessageID, wParam, lParam);
			}
		}		

		/// <summary>
		/// OnStart
		/// </summary>
		protected override void OnStart()
		{
			base.OnStart();
			PostMessage(MSG_THREAD_START);
		}

		/// <summary>
		/// OnStop
		/// </summary>
		protected override void OnStop()
		{
			base.OnStop();
			PostMessage(MSG_THREAD_STOP);
		}

		[DllImport("User32.dll")]
		extern static int PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

		[DllImport("user32.dll")]
		extern static bool IsWindow(IntPtr hwnd);
	}

	/// <summary>
	/// MessageThreadForm
	/// </summary>
	public class MessageThreadForm : Form
	{
		/// <summary>
		/// 消息ID
		/// </summary>
		public const int ThreadMessageID = 0x0400 + 3212;

		/// <summary>
		/// 线程
		/// </summary>
		private MessageThread m_thread = null;

		/// <summary>
		/// 初始化线程，建议在Form_Load中调用
		/// </summary>
		/// <param name="thread">继承类中创建的线程实体</param>
		protected virtual void SetThread(MessageThread thread)
		{
			if (thread.MessageWnd == IntPtr.Zero)
			{
				thread.MessageWnd = Handle;
			}			

			m_thread = thread;		
		}

		/// <summary>
		/// 继承类的Form_OnClosing中必须调用base.OnFormClosing(sender, e)
		/// </summary>
		protected virtual void Form_OnClosing(object sender, FormClosingEventArgs e)
		{	
			if (m_thread == null || !m_thread.IsAlive)
			{
				return;
			}

			if (MessageBox.Show(this, "Thread still running, terminate anyway?", Application.ProductName, MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) != DialogResult.OK)
			{
				e.Cancel = true;
				return;
			}

			m_thread.Stop();
		}		

		/// <summary>
		/// 线程启动
		/// </summary>
		protected virtual void OnThreadStart()
		{
		}

		/// <summary>
		/// 线程停止
		/// </summary>
		protected virtual void OnThreadStop()
		{
		}

		/// <summary>
		/// 一般消息
		/// </summary>
		/// <param name="wParam">wParam</param>
		/// <param name="lParam">lParam</param>
		protected virtual void OnThreadMessage(int wParam, int lParam)
		{
		}

		/// <summary>
		/// Override WndProc
		/// <param name="m">Message struct</param>		
		/// </summary>
		protected override void WndProc(ref Message m)
		{
			if (m.Msg == ThreadMessageID)
			{
				int wParam = IntPtrToInt(m.WParam);
				int lParam = IntPtrToInt(m.LParam);

				switch (wParam)
				{
					case MessageThread.MSG_THREAD_START:
						OnThreadStart();
						break;

					case MessageThread.MSG_THREAD_STOP:
						OnThreadStop();
						break;

					default:
						OnThreadMessage(wParam, lParam); // generic messages
						break;
				}
			}			

			base.WndProc(ref m);
		}

		private static int IntPtrToInt(IntPtr p)
		{
			try
			{
				return (int)p;
			}
			catch
			{
				return 0;
			}
		}
	}
}
