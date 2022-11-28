using System;
using System.Windows.Forms;
using AbinLibs;
using SourceCounter.BLL;

namespace SourceCounter
{
	public partial class MainForm : MessageThreadForm
	{
		string m_selectedFile = null;
		ParseThread m_thread = new ParseThread();
		RichTextLogHandler m_log = new RichTextLogHandler();

		public MainForm()
		{
			InitializeComponent();
			SetThread(m_thread);
			m_log.TextBox = richTextBox1;
			m_thread.Log = m_log;
		}

		private void MainForm_Load(object sender, EventArgs e)
		{

		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = DataRules.ProjectFilters;
			if (!string.IsNullOrEmpty(m_selectedFile))
			{
				dlg.FileName = m_selectedFile;
			}
			
			if (dlg.ShowDialog(this) != DialogResult.OK)
			{
				return;
			}

			if (string.Compare(m_selectedFile, dlg.FileName, true) == 0)
			{
				return;
			}

			m_selectedFile = dlg.FileName;
			txtFileName.Text = m_selectedFile;

			btnParse.Enabled = true;
			btnCopySource.Enabled = false;
		}

		private void btnParse_Click(object sender, EventArgs e)
		{			
			m_thread.StartParse(m_selectedFile);
		}

		private void btnCopySource_Click(object sender, EventArgs e)
		{
			m_thread.StartExtract();
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		protected override void OnThreadStart()
		{
			base.OnThreadStart();

			btnBrowse.Enabled = false;
			btnParse.Enabled = false;
			btnCopySource.Enabled = false;
		}

		protected override void OnThreadStop()
		{
			base.OnThreadStop();

			if (m_thread.Mode == ParseThread.RunMode.Copy)
			{
				Clipboard.SetText(m_thread.Contents);
				m_log.AppendText("Source text copied to clip-board.");
			}

			btnBrowse.Enabled = true;
			btnParse.Enabled = true;
			btnCopySource.Enabled = m_thread.TotalFiles > 0;
		}
	}
}
