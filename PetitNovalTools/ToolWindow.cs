using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetitNovalTools
{
    public class ToolWindow : Window
    {
        private bool isForceClosing = false;

        public void ForceClose()
        {
            this.isForceClosing = true;
            this.Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!isForceClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }
    }
}
