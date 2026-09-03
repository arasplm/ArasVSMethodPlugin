//------------------------------------------------------------------------------
// <copyright file="ExceptionMessageFormatter.cs" company="Aras Corporation">
//     © 2017-2026 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Text;

namespace Aras.VS.MethodPlugin.Utilities
{
	public sealed class ExceptionMessage
	{
		public ExceptionMessage(string summary, string details)
		{
			Summary = summary;
			Details = details;
		}

		public string Summary { get; }
		public string Details { get; }
	}

	public static class ExceptionMessageFormatter
	{
		private const int MaximumSummaryReasonLength = 300;

		public static ExceptionMessage Format(Exception exception, string errorCode, string operation)
		{
			if (exception == null) throw new ArgumentNullException(nameof(exception));
			if (string.IsNullOrWhiteSpace(errorCode)) throw new ArgumentException("An error code is required.", nameof(errorCode));
			if (string.IsNullOrWhiteSpace(operation)) throw new ArgumentException("An operation is required.", nameof(operation));

			Exception currentException = exception;
			while (currentException.InnerException != null)
			{
				currentException = currentException.InnerException;
			}

			var summary = new StringBuilder();
			summary.AppendLine("The operation failed.");
			summary.AppendLine();
			summary.AppendLine($"Reason: {GetSummaryReason(currentException.Message)}");
			summary.AppendLine($"Error code: {errorCode}");
			summary.Append($"Operation: {operation}");

			var details = new StringBuilder();
			details.AppendLine($"Error code: {errorCode}");
			details.AppendLine($"Operation: {operation}");
			details.AppendLine($"Plugin version: {FileVersionInfo.GetVersionInfo(typeof(ExceptionMessageFormatter).Assembly.Location).FileVersion}");
			details.AppendLine();
			details.AppendLine("Exception chain:");

			currentException = exception;
			int level = 0;
			while (currentException != null)
			{
				details.AppendLine($"[{level}] {currentException.GetType().FullName} (0x{currentException.HResult:X8})");
				details.AppendLine(currentException.Message);
				currentException = currentException.InnerException;
				level++;
			}

			details.AppendLine();
			details.AppendLine("Technical details:");
			details.Append(exception);

			return new ExceptionMessage(summary.ToString(), details.ToString());
		}

		private static string GetSummaryReason(string message)
		{
			string reason = message ?? string.Empty;
			int lineBreakIndex = reason.IndexOfAny(new[] { '\r', '\n' });
			if (lineBreakIndex >= 0)
			{
				reason = reason.Substring(0, lineBreakIndex);
			}

			if (reason.Length > MaximumSummaryReasonLength)
			{
				reason = reason.Substring(0, MaximumSummaryReasonLength) + "...";
			}

			return reason;
		}
	}
}