using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.OperationProgress;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.NodejsTools.Project
{
    // Based on typescript implementation: https://devdiv.visualstudio.com/DevDiv/_git/TypeScript-VS?path=%2FVS%2FLanguageService%2FTypeScriptVisualStudio%2FUtilities%2FOperationProgressService.cs&version=GBmaster&_a=contents
    internal class OperationProgressService
    {
        private readonly Dictionary<string, TaskCompletionSource<bool>> loadCompletionMap = new Dictionary<string, TaskCompletionSource<bool>>();

        public void RegisterAndStartStage(string stageId, string message)
        {
            var loadCompletion = new TaskCompletionSource<bool>();
            this.loadCompletionMap.Add(stageId, loadCompletion);

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
                        // Check if ProjectLoadingFinishEvent has already fired and nulled this out
                        if (loadCompletion == null)
                        {
                            return null;
                        }

                        stageAccess.RegisterTask(
                              new OperationProgressTask(
                                  ThreadHelper.JoinableTaskFactory.RunAsync(() => Task.FromResult(string.Empty)),
                                  "NTVSTask" /* not used elsewhere */,
                                  () => Task.FromResult(string.Empty)));

                        return loadCompletion.Task;
                    });
            }).Task.Forget();
        }

        public void CompleteAndCleanupStage(string stageId)
        {
            if (this.loadCompletionMap.TryGetValue(stageId, out var loadCompletion))
            {
                loadCompletion.TrySetResult(true);
                this.loadCompletionMap.Remove(stageId);
            }
        }
    }
}
