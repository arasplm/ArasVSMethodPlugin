//------------------------------------------------------------------------------
// <copyright file="CommonData.cs" company="Aras Corporation">
//     © 2017-2021 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Aras.Method.Libs.Code
{
	public class CommonData
	{
		static CommonData()
		{
			EventSpecificDataTypeList = new List<EventSpecificDataType>()
			{
				new EventSpecificDataType() {  EventSpecificData = EventSpecificData.None, EventDataClass = "", InterfaceName="Aras.Server.Core.IInnovatorServerMethod" },
				new EventSpecificDataType() {  EventSpecificData = EventSpecificData.SuccessfulLogin, EventDataClass ="Aras.Server.Core.SuccessfulLoginEventArgs", InterfaceName="Aras.Server.Core.IOnSuccessfulLoginInnovatorServerMethod" },
				new EventSpecificDataType() {  EventSpecificData = EventSpecificData.FailedLogin, EventDataClass = "Aras.Server.Core.FailedLoginEventArgs", InterfaceName="Aras.Server.Core.IOnFailedLoginInnovatorServerMethod" },
				new EventSpecificDataType() {  EventSpecificData = EventSpecificData.Logout, EventDataClass = "Aras.Server.Core.LogoutEventArgs", InterfaceName="Aras.Server.Core.IOnLogoutInnovatorServerMethod" },
				new EventSpecificDataType() {  EventSpecificData = EventSpecificData.OnAfterVersion, EventDataClass = "Aras.Server.Core.AfterVersionEventArgs", InterfaceName="Aras.Server.Core.IOnAfterVersionServerMethod" },
				new EventSpecificDataType() {  EventSpecificData = EventSpecificData.OnActivate, EventDataClass = "Aras.Workflow.OnActivateEventArgs", InterfaceName="Aras.Workflow.IOnActivateServerMethod" },
				new EventSpecificDataType() {  EventSpecificData = EventSpecificData.OnAssign, EventDataClass = "Aras.Workflow.OnAssignEventArgs", InterfaceName="Aras.Workflow.IOnAssignServerMethod" },
				new EventSpecificDataType() {  EventSpecificData = EventSpecificData.OnClose, EventDataClass = "Aras.Workflow.OnCloseEventArgs", InterfaceName="Aras.Workflow.IOnCloseServerMethod" },
				new EventSpecificDataType() {  EventSpecificData = EventSpecificData.OnDelegate, EventDataClass = "Aras.Workflow.OnDelegateEventArgs", InterfaceName="Aras.Workflow.IOnDelegateServerMethod" },
				new EventSpecificDataType() {  EventSpecificData = EventSpecificData.OnDue, EventDataClass = "Aras.Workflow.OnDueEventArgs", InterfaceName="Aras.Workflow.IOnDueServerMethod" },
				new EventSpecificDataType() {  EventSpecificData = EventSpecificData.OnEscalate, EventDataClass = "Aras.Workflow.OnEscalateEventArgs", InterfaceName="Aras.Workflow.IOnEscalateServerMethod" },
				new EventSpecificDataType() {  EventSpecificData = EventSpecificData.OnRefuse, EventDataClass = "Aras.Workflow.OnRefuseEventArgs", InterfaceName="Aras.Workflow.IOnRefuseServerMethod" },
				new EventSpecificDataType() {  EventSpecificData = EventSpecificData.OnRemind, EventDataClass = "Aras.Workflow.OnRemindEventArgs", InterfaceName="Aras.Workflow.IOnRemindServerMethod" },
				new EventSpecificDataType() {  EventSpecificData = EventSpecificData.OnVote, EventDataClass = "Aras.Workflow.OnVoteEventArgs", InterfaceName="Aras.Workflow.IOnVoteServerMethod" },
				new EventSpecificDataType() {  EventSpecificData = EventSpecificData.OnGet, EventDataClass = "Aras.Server.Core.OnGetEventArgs", InterfaceName="Aras.Server.Core.IOnGetServerMethod" },
				new EventSpecificDataType() {  EventSpecificData = EventSpecificData.OnAfterAdd, EventDataClass = "Aras.Server.Core.OnAfterAddEventArgs", InterfaceName="Aras.Server.Core.IOnAfterAddServerMethod" },
				new EventSpecificDataType() {  EventSpecificData = EventSpecificData.OnAfterCopy, EventDataClass = "Aras.Server.Core.OnAfterCopyEventArgs", InterfaceName="Aras.Server.Core.IOnAfterCopyServerMethod" },
				new EventSpecificDataType() {  EventSpecificData = EventSpecificData.OnBeforeUpdate, EventDataClass = "Aras.Server.Core.OnBeforeUpdateEventArgs", InterfaceName="Aras.Server.Core.IOnBeforeUpdateServerMethod" },
				new EventSpecificDataType() {  EventSpecificData = EventSpecificData.OnAfterUpdate, EventDataClass = "Aras.Server.Core.OnAfterUpdateEventArgs", InterfaceName="Aras.Server.Core.IOnAfterUpdateServerMethod" },
				new EventSpecificDataType() {  EventSpecificData = EventSpecificData.OnBeforeDelete, EventDataClass = "Aras.Server.Core.OnBeforeDeleteEventArgs", InterfaceName="Aras.Server.Core.IOnBeforeDeleteServerMethod" },
				new EventSpecificDataType() {  EventSpecificData = EventSpecificData.OnAfterDelete, EventDataClass = "Aras.Server.Core.OnAfterDeleteEventArgs", InterfaceName="Aras.Server.Core.IOnAfterDeleteServerMethod" },
				new EventSpecificDataType() {  EventSpecificData = EventSpecificData.OnDelete, EventDataClass = "Aras.Server.Core.OnDeleteEventArgs", InterfaceName="Aras.Server.Core.IOnDeleteServerMethod" },
				new EventSpecificDataType() {  EventSpecificData = EventSpecificData.OnUpdate, EventDataClass = "Aras.Server.Core.OnUpdateEventArgs", InterfaceName="Aras.Server.Core.IOnUpdateServerMethod" },
				new EventSpecificDataType() {  EventSpecificData = EventSpecificData.OnBeforePromote, EventDataClass = "Aras.Server.Core.OnBeforePromoteEventArgs", InterfaceName="Aras.Server.Core.IOnBeforePromoteServerMethod" },
				new EventSpecificDataType() {  EventSpecificData = EventSpecificData.OnPromote, EventDataClass = "Aras.Server.Core.OnPromoteEventArgs", InterfaceName="Aras.Server.Core.IOnPromoteServerMethod" },
				new EventSpecificDataType() {  EventSpecificData = EventSpecificData.OnAfterPromote, EventDataClass = "Aras.Server.Core.OnAfterPromoteEventArgs", InterfaceName="Aras.Server.Core.IOnAfterPromoteServerMethod" },
				new EventSpecificDataType() {  EventSpecificData = EventSpecificData.OnAfterResetLifecycle, EventDataClass = "Aras.Server.Core.OnAfterResetLifecycleEventArgs", InterfaceName="Aras.Server.Core.IOnAfterResetLifecycleServerMethod" }
			};
		}

		public static List<EventSpecificDataType> EventSpecificDataTypeList { get; private set; }
	}
}
