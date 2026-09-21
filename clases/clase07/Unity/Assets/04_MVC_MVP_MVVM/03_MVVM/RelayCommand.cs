using System;

namespace Clase07.Mvx.Mvvm
{
    public class RelayCommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute() => _canExecute?.Invoke() ?? true;
        public void Execute() { if (CanExecute()) _execute(); }
    }
}
