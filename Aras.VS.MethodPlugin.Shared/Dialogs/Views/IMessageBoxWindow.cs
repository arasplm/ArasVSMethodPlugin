using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
    public interface IMessageBoxWindow
    {

        MessageDialogResult ShowDialog(Window owner, string message, string title, MessageButtons buttons, MessageIcon icon);

        MessageDialogResult ShowDialog(string message, string title, MessageButtons buttons, MessageIcon icon);


    }
}
