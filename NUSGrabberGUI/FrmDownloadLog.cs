using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NUSGrabberGUI
{
    public partial class FrmDownloadLog : Form
    {
        public FrmDownloadLog()
        {
            InitializeComponent();
        }

        public void ClearLog()
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                edtLog.Text = string.Empty;
            });
        }

        public void AppendLog(string _Text)
        {
            if (this.Visible)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    if (chkAutoScroll.Checked) edtLog.AppendText(_Text + Environment.NewLine);
                    else edtLog.Text += _Text + Environment.NewLine;
                });
            }
        }

        private void FrmDownloadLog_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = (e.CloseReason == CloseReason.UserClosing);
            Hide();
        }
    }
}
