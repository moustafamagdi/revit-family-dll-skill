using System;
using System.Linq;
using Autodesk.Revit.DB;

namespace RevitFamilyGenerator
{
    internal sealed class LoggedFailures : IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor accessor)
        {
            bool error = false;
            foreach (FailureMessageAccessor failure in accessor.GetFailureMessages())
            {
                RunLog.Write("REVIT severity=" + failure.GetSeverity() +
                    " id=" + failure.GetFailureDefinitionId().Guid +
                    " description=" + failure.GetDescriptionText() +
                    " failing=" + string.Join(",", failure.GetFailingElementIds().Select(x => x.ToString())) +
                    " additional=" + string.Join(",", failure.GetAdditionalElementIds().Select(x => x.ToString())));
                if (failure.GetSeverity() != FailureSeverity.Warning) error = true;
            }
            // Warnings remain visible to the user. Unknown errors are never auto-resolved.
            return error ? FailureProcessingResult.ProceedWithRollBack : FailureProcessingResult.Continue;
        }
    }

    internal static class Transactions
    {
        internal static void Run(Document doc, string name, Action action)
        {
            using (var tx = new Transaction(doc, name))
            {
                RunLog.Write("TRANSACTION START " + name);
                if (tx.Start() != TransactionStatus.Started)
                    throw new InvalidOperationException("Transaction could not start: " + name);
                tx.SetFailureHandlingOptions(tx.GetFailureHandlingOptions()
                    .SetFailuresPreprocessor(new LoggedFailures()).SetClearAfterRollback(true));
                try
                {
                    action();
                    doc.Regenerate();
                    TransactionStatus status = tx.Commit();
                    RunLog.Write("TRANSACTION COMMIT " + name + " status=" + status);
                    if (status != TransactionStatus.Committed)
                        throw new InvalidOperationException("Transaction did not commit: " + name + ", " + status);
                }
                catch (Exception ex)
                {
                    string logFailure = RunLog.TryWrite("TRANSACTION FAILED " + name + "\n" + ex);
                    if (tx.GetStatus() == TransactionStatus.Started)
                    {
                        try
                        {
                            TransactionStatus rollback = tx.RollBack();
                            string rollbackLogFailure = RunLog.TryWrite("TRANSACTION ROLLBACK " + name + " status=" + rollback);
                            if (rollbackLogFailure != null) ex.Data["RollbackLogFailure"] = rollbackLogFailure;
                        }
                        catch (Exception cleanup) { ex.Data["RollbackFailure"] = cleanup.ToString(); }
                    }
                    if (logFailure != null) ex.Data["TransactionLogFailure"] = logFailure;
                    throw;
                }
            }
        }
    }
}
