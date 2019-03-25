//------------------------------------------------------------------------------
// <copyright file="ViewAdaper.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Windows;

namespace Aras.VS.MethodPlugin.Dialogs
{
	public abstract class ViewAdaper<TView, TResult> : IViewAdaper<TView, TResult>
		where TResult : ViewResult, new()
		where TView : Window
	{
		protected TView view;

		public ViewAdaper(TView view)
		{
			if (view == null) throw new ArgumentNullException(nameof(view));
			this.view = view;
		}

		public abstract TResult ShowDialog();
	}

	public interface IViewAdaper<TView, TResult>
	{
		TResult ShowDialog();
	}
}

