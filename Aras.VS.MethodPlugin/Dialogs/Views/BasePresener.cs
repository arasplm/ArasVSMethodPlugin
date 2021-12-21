//------------------------------------------------------------------------------
// <copyright file="BasePresener.cs" company="Aras Corporation">
//     © 2017-2022 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace OfficeConnector.Dialogs
{
	public abstract class BasePresener<TView> : IPresenter where TView : IView
	{
		protected TView View { get; private set; }

		protected BasePresener(TView view)
		{
			if (view == null) throw new ArgumentNullException(nameof(view));

			View = view;
		}

		public virtual void Run()
		{
			View.Show();
		}
	}

	public abstract class BasePresener<TView, TArg> : IPresenter<TArg> where TView : IView
	{
		protected TView View { get; private set; }

		protected BasePresener(TView view)
		{
			if (view == null) throw new ArgumentNullException(nameof(view));

			View = view;
		}

		public abstract void Run(TArg argument);
	}

	public abstract class BasePresener<TView, TArg, Tresult> : IPresenter<TArg, Tresult> where TView : IView
	{
		protected TView View { get; private set; }

		protected BasePresener(TView view)
		{
			if (view == null) throw new ArgumentNullException(nameof(view));

			View = view;
		}

		public abstract Tresult Run(TArg argument);
	}
}
