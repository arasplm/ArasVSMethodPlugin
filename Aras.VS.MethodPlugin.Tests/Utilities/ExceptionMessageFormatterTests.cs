using System;
using System.Diagnostics;
using Aras.VS.MethodPlugin.Utilities;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.Utilities
{
	[TestFixture]
	public class ExceptionMessageFormatterTests
	{
		[Test]
		public void Format_ExceptionWithInnerException_SeparatesSummaryAndTechnicalDetails()
		{
			var innerException = new InvalidOperationException("The server rejected the request.");
			var exception = new ApplicationException("Saving the method failed.", innerException);

			ExceptionMessage message = ExceptionMessageFormatter.Format(exception, "AVS-TEST-001", "Test operation");

			StringAssert.Contains("Reason: The server rejected the request.", message.Summary);
			StringAssert.Contains("Error code: AVS-TEST-001", message.Summary);
			StringAssert.Contains("Operation: Test operation", message.Summary);
			StringAssert.DoesNotContain(typeof(ApplicationException).FullName, message.Summary);
			StringAssert.DoesNotContain("Technical details:", message.Summary);
			StringAssert.Contains(typeof(ApplicationException).FullName, message.Details);
			StringAssert.Contains("Saving the method failed.", message.Details);
			StringAssert.Contains(typeof(InvalidOperationException).FullName, message.Details);
			StringAssert.Contains("The server rejected the request.", message.Details);
			string pluginVersion = FileVersionInfo.GetVersionInfo(typeof(ExceptionMessageFormatter).Assembly.Location).FileVersion;
			StringAssert.Contains($"Plugin version: {pluginVersion}", message.Details);
			StringAssert.Contains("Technical details:", message.Details);
		}

		[Test]
		public void Format_MultilineReason_ShowsOnlyFirstLineInSummaryAndPreservesFullDetails()
		{
			var exception = new InvalidOperationException("Request failed.\r\n<Response>Technical payload</Response>");

			ExceptionMessage message = ExceptionMessageFormatter.Format(exception, "AVS-TEST-002", "Test operation");

			StringAssert.Contains("Reason: Request failed.", message.Summary);
			StringAssert.DoesNotContain("Technical payload", message.Summary);
			StringAssert.Contains("<Response>Technical payload</Response>", message.Details);
		}
	}
}