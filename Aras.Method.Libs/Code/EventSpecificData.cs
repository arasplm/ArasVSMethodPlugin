//------------------------------------------------------------------------------
// <copyright file="EventSpecificData.cs" company="Aras Corporation">
//     © 2017-2021 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Aras.Method.Libs.Code
{
	/// <summary>
	/// Enumeration of events with event-specific data
	/// </summary>
	/// <remarks></remarks>
	public enum EventSpecificData : int
	{
		None = 0,
		FailedLogin,
		SuccessfulLogin,
		Logout,
		OnVote,
		OnRefuse,
		OnDue,
		OnAssign,
		OnClose,
		OnActivate,
		OnRemind,
		OnEscalate,
		OnDelegate,
		OnGet,
		OnAfterAdd,
		OnBeforeUpdate,
		OnAfterUpdate,
		OnAfterVersion,
		OnBeforeDelete,
		OnAfterDelete,
		OnAfterCopy,
		OnUpdate,
		OnDelete,
		OnBeforePromote,
		OnPromote,
		OnAfterPromote,
		OnAfterResetLifecycle,
	}
}
