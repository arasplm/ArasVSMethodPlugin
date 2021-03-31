//------------------------------------------------------------------------------
// <copyright file="IPresenter.cs" company="Aras Corporation">
//     © 2017-2021 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace OfficeConnector.Dialogs
{
	public interface IPresenter
	{
		void Run();
	}

	public interface IPresenter<in TArg>
	{
		void Run(TArg argument);
	}

	public interface IPresenter<in TArg, Tresult>
	{
		Tresult Run(TArg argument);
	}
}
