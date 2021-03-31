//------------------------------------------------------------------------------
// <copyright file="RelayCommand.cs" company="Aras Corporation">
//     © 2017-2021 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Windows.Input;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class RelayCommand<T> : ICommand
	{
		private readonly Action<T> execute = null;
		private readonly Predicate<T> canExecute = null;

		public RelayCommand(Action<T> execute) : this(execute, null)
		{
		}

		public RelayCommand(Action<T> execute, Predicate<T> canExecute)
		{
			if (execute == null)
			{
				throw new ArgumentNullException(nameof(execute));
			}

			this.execute = execute;
			this.canExecute = canExecute;
		}

		public bool CanExecute(object parameter)
		{
			return canExecute == null ? true : canExecute((T)parameter);
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public void Execute(object parameter)
		{
			execute((T)parameter);
		}
	}

	public class RelayCommand : ICommand
	{
		private readonly Action execute;
		private readonly Func<bool> canExecute;

		public RelayCommand(Action execute) : this(execute, null)
		{
		}

		public RelayCommand(Action execute, Func<bool> canExecute)
		{
			if (execute == null)
			{
				throw new ArgumentNullException(nameof(execute));
			}

			this.execute = execute;
			this.canExecute = canExecute;
		}

		public bool CanExecute(object parameter)
		{
			return canExecute == null ? true : canExecute();
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public void Execute(object parameter)
		{
			execute();
		}
	}
}
