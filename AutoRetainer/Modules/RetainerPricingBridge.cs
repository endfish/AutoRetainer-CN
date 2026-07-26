using AutoRetainer.Internal;
using AutoRetainer.Scheduler.Handlers;
using Dalamud.Game.ClientState.Conditions;
using ECommons.Automation.NeoTaskManager;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Modules;

/// <summary>
/// CN-only IPC bridge that lets a companion plugin process a caller-selected set of retainers.
/// AutoRetainer remains responsible for entering and leaving retainers; the caller must return
/// the game UI to the retainer menu before completing each retainer.
/// </summary>
internal static unsafe class RetainerPricingBridge
{
    internal const int ProtocolVersion = 1;

    private const string ProtocolVersionIpc = "AutoRetainer.CN.RetainerPricing.ProtocolVersion";
    private const string StartJobIpc = "AutoRetainer.CN.RetainerPricing.StartJob";
    private const string CancelJobIpc = "AutoRetainer.CN.RetainerPricing.CancelJob";
    private const string HeartbeatIpc = "AutoRetainer.CN.RetainerPricing.Heartbeat";
    private const string CompleteRetainerIpc = "AutoRetainer.CN.RetainerPricing.CompleteRetainer";
    private const string RetainerReadyIpc = "AutoRetainer.CN.RetainerPricing.RetainerReady";
    private const string JobEndedIpc = "AutoRetainer.CN.RetainerPricing.JobEnded";

    private const int HeartbeatTimeoutMs = 30_000;
    private const int UiTransitionTimeoutMs = 10_000;

    private static PricingJob job;
    private static bool initialized;

    /// <summary>
    /// Suspends AutoRetainer automation for the full lifetime of an accepted pricing job.
    /// The configured scheduler and MultiMode states are left untouched so they can resume
    /// after normal completion or recovery releases the job.
    /// </summary>
    internal static bool IsAutomationLocked => job != null;

    private enum StartResult
    {
        Success = 0,
        Busy = 1,
        RetainerListUnavailable = 2,
        InvalidSelection = 3,
        UnsupportedState = 4,
        InternalError = 5,
    }

    private enum EndResult
    {
        Completed = 0,
        Cancelled = 100,
        HeartbeatTimeout = 101,
        EnvironmentChanged = 102,
        UiTimeout = 103,
        ClientUnavailable = 104,
        InternalError = 105,
    }

    private sealed class RetainerTarget
    {
        internal required ulong Id;
        internal required string Name;
    }

    private sealed class PricingJob
    {
        internal required string ClientId;
        internal required string JobId;
        internal required ulong CharacterId;
        internal required uint TerritoryId;
        internal required List<RetainerTarget> Targets;
        internal long LastHeartbeatAt = Environment.TickCount64;
        internal RetainerTarget Current;
        internal bool CurrentCompleted;
        internal bool ReadySent;
        internal bool CancelRequested;
        internal bool RecoveryQueued;
        internal EndResult Result = EndResult.Completed;
    }

    internal static void Init()
    {
        if(initialized)
        {
            return;
        }

        Svc.PluginInterface.GetIpcProvider<int>(ProtocolVersionIpc).RegisterFunc(() => ProtocolVersion);
        Svc.PluginInterface.GetIpcProvider<string, string, ulong[], int>(StartJobIpc).RegisterFunc(StartJob);
        Svc.PluginInterface.GetIpcProvider<string, string, bool>(CancelJobIpc).RegisterFunc(CancelJob);
        Svc.PluginInterface.GetIpcProvider<string, string, ulong, bool>(HeartbeatIpc).RegisterFunc(Heartbeat);
        Svc.PluginInterface.GetIpcProvider<string, string, ulong, bool>(CompleteRetainerIpc).RegisterFunc(CompleteRetainer);
        Svc.Framework.Update += Tick;
        initialized = true;
        DebugLog("[RetainerPricingBridge] Initialized protocol v1");
    }

    internal static void Shutdown()
    {
        if(!initialized)
        {
            return;
        }

        Svc.Framework.Update -= Tick;
        if(job != null)
        {
            var endingJob = job;
            job = null;
            TrySendJobEnded(endingJob, EndResult.ClientUnavailable);
        }

        Svc.PluginInterface.GetIpcProvider<int>(ProtocolVersionIpc).UnregisterFunc();
        Svc.PluginInterface.GetIpcProvider<string, string, ulong[], int>(StartJobIpc).UnregisterFunc();
        Svc.PluginInterface.GetIpcProvider<string, string, bool>(CancelJobIpc).UnregisterFunc();
        Svc.PluginInterface.GetIpcProvider<string, string, ulong, bool>(HeartbeatIpc).UnregisterFunc();
        Svc.PluginInterface.GetIpcProvider<string, string, ulong, bool>(CompleteRetainerIpc).UnregisterFunc();
        initialized = false;
        DebugLog("[RetainerPricingBridge] Shutdown");
    }

    private static int StartJob(string clientId, string jobId, ulong[] retainerIds)
    {
        try
        {
            if(job != null || P.TaskManager.IsBusy || MultiMode.Enabled)
            {
                DebugLog(
                    $"[RetainerPricingBridge] Rejected busy job {jobId} from {clientId}: "
                    + $"pricingJob={job != null}, taskManager={P.TaskManager.IsBusy}, "
                    + $"schedulerEnabled={SchedulerMain.PluginEnabledInternal}, multiMode={MultiMode.Enabled}");
                return (int)StartResult.Busy;
            }

            if(clientId.IsNullOrEmpty() || jobId.IsNullOrEmpty() || retainerIds == null || retainerIds.Length == 0)
            {
                return (int)StartResult.InvalidSelection;
            }

            if(!Svc.ClientState.IsLoggedIn || !Player.Available || !Player.IsInHomeWorld)
            {
                return (int)StartResult.UnsupportedState;
            }

            if(!Svc.Condition[ConditionFlag.OccupiedSummoningBell]
               || !TryGetAddonByName<AtkUnitBase>("RetainerList", out var retainerList)
               || !IsAddonReady(retainerList)
               || !GameRetainerManager.Ready)
            {
                return (int)StartResult.RetainerListUnavailable;
            }

            var requestedIds = retainerIds.Where(x => x != 0).Distinct().ToArray();
            if(requestedIds.Length != retainerIds.Length)
            {
                return (int)StartResult.InvalidSelection;
            }

            var available = GameRetainerManager.Retainers
                .Where(x => x.Available)
                .ToDictionary(x => x.RetainerID);
            if(requestedIds.Any(x => !available.ContainsKey(x)))
            {
                return (int)StartResult.InvalidSelection;
            }

            var targets = requestedIds
                .Select(x => new RetainerTarget
                {
                    Id = x,
                    Name = available[x].Name,
                })
                .ToList();

            job = new PricingJob
            {
                ClientId = clientId,
                JobId = jobId,
                CharacterId = Svc.ClientState.LocalContentId,
                TerritoryId = Svc.ClientState.TerritoryType,
                Targets = targets,
            };

            EnqueueJob(job);
            DebugLog($"[RetainerPricingBridge] Accepted job {jobId} from {clientId}: {targets.Select(x => x.Name).Print()}");
            return (int)StartResult.Success;
        }
        catch(Exception ex)
        {
            ex.Log();
            job = null;
            return (int)StartResult.InternalError;
        }
    }

    private static bool CancelJob(string clientId, string jobId)
    {
        if(!MatchesJob(clientId, jobId))
        {
            return false;
        }

        RequestEnd(EndResult.Cancelled);
        return true;
    }

    private static bool Heartbeat(string clientId, string jobId, ulong retainerId)
    {
        if(!MatchesJob(clientId, jobId))
        {
            return false;
        }

        if(retainerId != 0 && job.Current != null && retainerId != job.Current.Id)
        {
            return false;
        }

        job.LastHeartbeatAt = Environment.TickCount64;
        return true;
    }

    private static bool CompleteRetainer(string clientId, string jobId, ulong retainerId)
    {
        if(!MatchesJob(clientId, jobId)
           || job.CancelRequested
           || job.Current == null
           || job.Current.Id != retainerId
           || !job.ReadySent)
        {
            return false;
        }

        job.LastHeartbeatAt = Environment.TickCount64;
        job.CurrentCompleted = true;
        return true;
    }

    private static bool MatchesJob(string clientId, string jobId)
    {
        return job != null
               && string.Equals(job.ClientId, clientId, StringComparison.Ordinal)
               && string.Equals(job.JobId, jobId, StringComparison.Ordinal);
    }

    private static void Tick(object _)
    {
        var currentJob = job;
        if(currentJob == null)
        {
            return;
        }

        if(!Svc.ClientState.IsLoggedIn
           || Svc.ClientState.LocalContentId != currentJob.CharacterId
           || Svc.ClientState.TerritoryType != currentJob.TerritoryId
           || !Svc.Condition[ConditionFlag.OccupiedSummoningBell]
           || MultiMode.Enabled)
        {
            RequestEnd(EndResult.EnvironmentChanged);
        }
        else if(Environment.TickCount64 - currentJob.LastHeartbeatAt > HeartbeatTimeoutMs)
        {
            RequestEnd(EndResult.HeartbeatTimeout);
        }

        if(job != null && !P.TaskManager.IsBusy && !job.RecoveryQueued)
        {
            job.RecoveryQueued = true;
            if(!job.CancelRequested)
            {
                RequestEnd(EndResult.InternalError);
            }
            EnqueueRecoveryAndFinish();
        }
    }

    private static void RequestEnd(EndResult result)
    {
        if(job == null)
        {
            return;
        }

        if(!job.CancelRequested || job.Result == EndResult.Completed)
        {
            job.Result = result;
        }
        job.CancelRequested = true;
        job.CurrentCompleted = true;
    }

    private static void EnqueueJob(PricingJob acceptedJob)
    {
        foreach(var target in acceptedJob.Targets)
        {
            P.TaskManager.Enqueue(() => PrepareTarget(acceptedJob, target), $"RetainerPricingBridge.Prepare({target.Name})");
            P.TaskManager.Enqueue(
                WithTimeout(
                    acceptedJob,
                    target,
                    () => RetainerListHandlers.SelectRetainerByName(target.Name),
                    UiTransitionTimeoutMs),
                $"RetainerPricingBridge.Select({target.Name})");
            P.TaskManager.Enqueue(
                WithTimeout(
                    acceptedJob,
                    target,
                    () => Utils.TryGetCurrentRetainer(out var name) && name == target.Name,
                    UiTransitionTimeoutMs),
                $"RetainerPricingBridge.WaitSelected({target.Name})");
            P.TaskManager.Enqueue(() => FireRetainerReady(acceptedJob, target), $"RetainerPricingBridge.Ready({target.Name})");
            P.TaskManager.Enqueue(
                () => WaitForRetainerCompletion(acceptedJob, target),
                $"RetainerPricingBridge.WaitClient({target.Name})",
                new TaskManagerConfiguration(timeLimitMS: int.MaxValue, abortOnTimeout: false));
            P.TaskManager.Enqueue(
                WithTimeout(acceptedJob, target, RecoverToRetainerMenu, UiTransitionTimeoutMs, true),
                $"RetainerPricingBridge.RecoverMenu({target.Name})");
            P.TaskManager.Enqueue(
                WithTimeout(acceptedJob, target, QuitCurrentRetainer, UiTransitionTimeoutMs, true),
                $"RetainerPricingBridge.Quit({target.Name})");
            P.TaskManager.Enqueue(
                WithTimeout(acceptedJob, target, ConfirmQuitOrUnavailable, UiTransitionTimeoutMs, true),
                $"RetainerPricingBridge.ConfirmQuit({target.Name})");
            P.TaskManager.Enqueue(
                WithTimeout(acceptedJob, target, IsRetainerListReady, UiTransitionTimeoutMs),
                $"RetainerPricingBridge.WaitList({target.Name})");
            P.TaskManager.Enqueue(() => ClearTarget(acceptedJob, target), $"RetainerPricingBridge.Clear({target.Name})");
        }

        P.TaskManager.Enqueue(() => FinishJob(acceptedJob), "RetainerPricingBridge.Finish");
    }

    private static void EnqueueRecoveryAndFinish()
    {
        var acceptedJob = job;
        if(acceptedJob == null)
        {
            return;
        }

        P.TaskManager.Enqueue(
            WithTimeout(acceptedJob, acceptedJob.Current, RecoverToRetainerMenu, UiTransitionTimeoutMs, true),
            "RetainerPricingBridge.EmergencyRecover");
        P.TaskManager.Enqueue(
            WithTimeout(acceptedJob, acceptedJob.Current, QuitCurrentRetainer, UiTransitionTimeoutMs, true),
            "RetainerPricingBridge.EmergencyQuit");
        P.TaskManager.Enqueue(
            WithTimeout(acceptedJob, acceptedJob.Current, ConfirmQuitOrUnavailable, UiTransitionTimeoutMs, true),
            "RetainerPricingBridge.EmergencyConfirmQuit");
        P.TaskManager.Enqueue(() => FinishJob(acceptedJob), "RetainerPricingBridge.EmergencyFinish");
    }

    private static bool PrepareTarget(PricingJob acceptedJob, RetainerTarget target)
    {
        if(job != acceptedJob || acceptedJob.CancelRequested)
        {
            return true;
        }

        acceptedJob.Current = target;
        acceptedJob.CurrentCompleted = false;
        acceptedJob.ReadySent = false;
        acceptedJob.LastHeartbeatAt = Environment.TickCount64;
        return true;
    }

    private static Func<bool?> WithTimeout(
        PricingJob acceptedJob,
        RetainerTarget target,
        Func<bool?> action,
        int timeoutMs,
        bool runWhenCancelled = false)
    {
        var startedAt = 0L;
        return () =>
        {
            if(job != acceptedJob
               || (!runWhenCancelled && acceptedJob.CancelRequested)
               || (target != null && acceptedJob.Current != target))
            {
                return true;
            }

            startedAt = startedAt == 0 ? Environment.TickCount64 : startedAt;
            bool? result;
            try
            {
                result = action();
            }
            catch(Exception ex)
            {
                ex.Log();
                RequestEnd(EndResult.InternalError);
                return true;
            }
            if(result != false)
            {
                return result;
            }

            if(Environment.TickCount64 - startedAt >= timeoutMs)
            {
                DebugLog($"[RetainerPricingBridge] UI transition timed out for {target?.Name ?? "recovery"}");
                RequestEnd(EndResult.UiTimeout);
                return true;
            }
            return false;
        };
    }

    private static bool FireRetainerReady(PricingJob acceptedJob, RetainerTarget target)
    {
        if(job != acceptedJob || acceptedJob.CancelRequested || acceptedJob.Current != target)
        {
            return true;
        }

        if(acceptedJob.ReadySent)
        {
            return true;
        }

        try
        {
            acceptedJob.ReadySent = true;
            acceptedJob.LastHeartbeatAt = Environment.TickCount64;
            Svc.PluginInterface
                .GetIpcProvider<string, string, ulong, string, object>(RetainerReadyIpc)
                .SendMessage(acceptedJob.ClientId, acceptedJob.JobId, target.Id, target.Name);
        }
        catch(Exception ex)
        {
            ex.Log();
            RequestEnd(EndResult.ClientUnavailable);
        }
        return true;
    }

    private static bool WaitForRetainerCompletion(PricingJob acceptedJob, RetainerTarget target)
    {
        return job != acceptedJob
               || acceptedJob.CancelRequested
               || acceptedJob.Current != target
               || acceptedJob.CurrentCompleted;
    }

    private static bool? RecoverToRetainerMenu()
    {
        if(!Svc.Condition[ConditionFlag.OccupiedSummoningBell] || IsRetainerListReady() == true)
        {
            return true;
        }

        foreach(var addonName in new[] { "ItemSearchResult", "RetainerSell", "ContextMenu", "RetainerSellList" })
        {
            if(TryGetAddonByName<AtkUnitBase>(addonName, out var addon) && addon->IsVisible)
            {
                addon->Close(true);
                return false;
            }
        }

        return TryGetAddonByName<AtkUnitBase>("SelectString", out var selectString)
               && IsAddonReady(selectString);
    }

    private static bool? QuitCurrentRetainer()
    {
        if(!Svc.Condition[ConditionFlag.OccupiedSummoningBell] || IsRetainerListReady() == true)
        {
            return true;
        }
        return RetainerHandlers.SelectQuit();
    }

    private static bool? ConfirmQuitOrUnavailable()
    {
        if(!Svc.Condition[ConditionFlag.OccupiedSummoningBell] || IsRetainerListReady() == true)
        {
            return true;
        }
        return RetainerHandlers.ConfirmCantBuyback();
    }

    private static bool? IsRetainerListReady()
    {
        return TryGetAddonByName<AtkUnitBase>("RetainerList", out var retainerList)
               && IsAddonReady(retainerList);
    }

    private static bool ClearTarget(PricingJob acceptedJob, RetainerTarget target)
    {
        if(job == acceptedJob && acceptedJob.Current == target)
        {
            acceptedJob.Current = null;
            acceptedJob.CurrentCompleted = false;
            acceptedJob.ReadySent = false;
        }
        return true;
    }

    private static bool FinishJob(PricingJob acceptedJob)
    {
        if(job != acceptedJob)
        {
            return true;
        }

        job = null;
        TrySendJobEnded(acceptedJob, acceptedJob.Result);
        DebugLog($"[RetainerPricingBridge] Finished job {acceptedJob.JobId}: {acceptedJob.Result}");
        return true;
    }

    private static void TrySendJobEnded(PricingJob acceptedJob, EndResult result)
    {
        try
        {
            Svc.PluginInterface
                .GetIpcProvider<string, string, int, object>(JobEndedIpc)
                .SendMessage(acceptedJob.ClientId, acceptedJob.JobId, (int)result);
        }
        catch(Exception ex)
        {
            ex.Log();
        }
    }
}
