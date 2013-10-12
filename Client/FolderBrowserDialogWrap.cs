using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public interface IFolderBrowserDialogWrap
    {
        string PromptFolderBrowserDialog();
    }

    public class FolderBrowserDialogWrap : IFolderBrowserDialogWrap
    {
        public string PromptFolderBrowserDialog()
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();

            return dialog.SelectedPath;
        }
    }
}
