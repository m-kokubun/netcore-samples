using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace MLNetWpfApp.Inputs
{
    public class RelayCommand : ICommand
    {
        private Action execute;
        private Func<bool> canExecute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand() : this(() => { }) { }

        public RelayCommand(Action execute) : this(execute, () => true) { }

        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute()
            => CanExecute(null);

        public bool CanExecute(object parameter)
            => canExecute();

        public void Execute()
            => Execute(null);

        public void Execute(object parameter)
            => execute();
    }
}
