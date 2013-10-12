using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Client
{
    public interface IWatcherCommands
    {
        ICommand BrowseFolderCommand();
        ICommand RunCommand();
    }

    public class WatcherCommands : IWatcherCommands
    {
        public ICommand BrowseFolderCommand()
        {
            throw new NotImplementedException();
        }

        public ICommand RunCommand()
        {
            throw new NotImplementedException();
        }
    }
}
