using System;
using System.Collections.Generic;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace LatihasExport;

public unsafe class AchievementService : IDisposable {
    private readonly Queue<uint> _queue = new();
    internal readonly Dictionary<uint, uint> Current = new();
    internal readonly Dictionary<uint, uint> Max = new();

    [Signature("C7 81 ?? ?? ?? ?? ?? ?? ?? ?? 89 91 ?? ?? ?? ?? 44 89 81", DetourName = nameof(ReceiveAchievementProgress))]
    private readonly Hook<ReceiveAchievementProgressDelegate>? receiveAchievementProgressDelegate = null!;
    private uint ForTheHoardAchievementId;
    private bool isRunning;

    internal AchievementService() {
        Plugin.GameInteropProvider.InitializeFromAttributes(this);
        receiveAchievementProgressDelegate?.Enable();
    }

    public void Dispose() {
        receiveAchievementProgressDelegate?.Disable();
        receiveAchievementProgressDelegate?.Dispose();
        GC.SuppressFinalize(this);
    }

    public void UpdateProgress(uint id) => _queue.Enqueue(id);

    public void Reset() {
        _queue.Clear();
        isRunning = false;
    }

    internal void ProcNext() {
        if (isRunning || _queue.Count == 0) return;
        isRunning = true;
        ForTheHoardAchievementId = _queue.Peek();
        Achievement.Instance()->RequestAchievementProgress(ForTheHoardAchievementId);
    }

    private void ReceiveAchievementProgress(Achievement* self, uint id, uint current, uint max) {
        if (ForTheHoardAchievementId == id) {
            Current[ForTheHoardAchievementId] = current;
            Max[ForTheHoardAchievementId] = max;
            _queue.Dequeue();
            isRunning = false;
        }
        receiveAchievementProgressDelegate!.Original(self, id, current, max);
    }

    private delegate void ReceiveAchievementProgressDelegate(Achievement* self, uint id, uint current, uint max);
}