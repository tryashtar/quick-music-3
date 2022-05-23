using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QuickMusic3.Core;

public class RelayCommand : ICommand
{
    public event EventHandler CanExecuteChanged { add { } remove { } }

    private readonly Action ExecuteAction;
    public RelayCommand(Action execute)
    {
        ExecuteAction = execute;
    }

    public bool CanExecute(object parameter)
    {
        return true;
    }

    public void Execute(object parameter)
    {
        ExecuteAction();
    }
}

public class RelayCommand<T> : ICommand
{
    public event EventHandler CanExecuteChanged { add { } remove { } }

    private readonly Action<T> ExecuteAction;
    public RelayCommand(Action<T> execute)
    {
        ExecuteAction = execute;
    }

    public bool CanExecute(object parameter)
    {
        return true;
    }

    public void Execute(object parameter)
    {
        ExecuteAction((T)parameter);
    }
}
