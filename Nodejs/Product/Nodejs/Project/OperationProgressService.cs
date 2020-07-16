using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Telemetry;
using Microsoft.VisualStudio.OperationProgress;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.NodejsTools.Project
{
    internal class OperationProgressService
    {
        private readonly Dictionary<string, TaskCompletionSource<bool>> taskCompletionSources = new Dictionary<string, TaskCompletionSource<bool>>();

        public void RegisterAndStartStage(string stageId, string message)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            this.taskCompletionSources.Add(stageId, taskCompletionSource);

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                if (!(ServiceProvider.GlobalProvider.GetService(typeof(SVsOperationProgress)) is IVsOperationProgress2 operationProgress))
                {
                    return;
                }

                // Register a stage with unique ID and desired display text
                operationProgress.RegisterStageId(stageId, new OperationProgressStageOptions(message));

                // Register a task to our stage
                _ = operationProgress.RegisterStageOperationTasksAsync(
                    stageId,
                    "NTVSStageTasks" /* not used elsewhere */,
                    0,
                    (stageAccess) =>
                    {
                        // Check if the task completion has already fired and nulled this out
                        if (taskCompletionSource == null)
                        {
                            return null;
                        }

                        stageAccess.RegisterTask(
                              new OperationProgressTask(
                                  ThreadHelper.JoinableTaskFactory.RunAsync(() => Task.FromResult(string.Empty)),
                                  "NTVSTask" /* not used elsewhere */,
                                  () => Task.FromResult(string.Empty)));

                        return taskCompletionSource.Task;
                    });
            }).Task.FileAndForget(TelemetryEvents.OperationRegistrationFaulted);
        }

        public void CompleteAndCleanupStage(string stageId)
        {
            if (this.taskCompletionSources.TryGetValue(stageId, out var taskCompletionSource))
            {
                taskCompletionSource.TrySetResult(true);
                this.taskCompletionSources.Remove(stageId);
            }
        }
    }
}
