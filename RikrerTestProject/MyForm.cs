using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RikrerTestProject
{
    public partial class MyForm : Form
    {
        private readonly TaskScheduler m_SyncContextTaskScheduller;
        public MyForm()
        {
            InitializeComponent();
            m_SyncContextTaskScheduller = TaskScheduler.FromCurrentSynchronizationContext();

            Text = "Sunc";
         //   Visible = true;
            Width = 600;
            Height = 100;

        }

        private CancellationTokenSource m_cts;
        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (m_cts != null)
            {
                m_cts.Cancel();
                m_cts = null;
                Text = "Operation canceled";
            }
            else
            {
                Text = "Operation runing";
                m_cts = new CancellationTokenSource();

                Task<int> t = Task.Run(() => Program.Sum(m_cts.Token, 2000), m_cts.Token);

                t.ContinueWith(task => Text = "Result" + task.Result,
                    CancellationToken.None,
                    TaskContinuationOptions.OnlyOnRanToCompletion, m_SyncContextTaskScheduller);

                t.ContinueWith(task => Text = "Operation Canceled",
                    CancellationToken.None,
                    TaskContinuationOptions.OnlyOnCanceled, m_SyncContextTaskScheduller);

                t.ContinueWith(task => Text = "Operation faulted",
                    CancellationToken.None,
                    TaskContinuationOptions.OnlyOnFaulted, m_SyncContextTaskScheduller);

            }

            base.OnMouseClick(e);
        }
    }
}
