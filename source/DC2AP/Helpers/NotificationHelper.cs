using Archipelago.Core.Util;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DC2AP.Helpers
{
    internal class NotificationHelper
    {
        private const ulong CaveAddr = 0x01F70400;
        private const ulong HookAddr = 0x001914A8;
        private const uint HookJal = 0x0C7DC100; // jal 0x01F70400

        private const ulong DialogFlag = 0x01F70030; // write msg ID here to trigger
        private const ulong DialogMode = 0x01F70038; // preset mode for next dialog
        private const ulong DialogActive = 0x01F7003C; // 1 = dialog currently showing

        private const ulong SystemMessage0 = 0x01E94AC0;
        private const int OffMsgId = 0x17E4;     // current message ID; -1 = none
        private const int OffPosX = 0x198;
        private const int OffPosY = 0x19C;

        private const ulong ScratchTextAddr = 0x01E87EEE;
        private const int ScratchMsgId = 0x81B1;

        private const int DefaultX = 100;
        private const int DefaultY = 350;

        private static readonly byte[] DialogCaveMips =
        {
            0xD0, 0xFF, 0xBD, 0x27, 0x00, 0x00, 0xBF, 0xAF, 0x04, 0x00, 0xB0, 0xAF, 0x08, 0x00, 0xB1, 0xAF, // 0x01F70400
            0x74, 0x08, 0x05, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x0C, 0x00, 0xA2, 0xAF, 0xF7, 0x01, 0x02, 0x3C, // 0x01F70410
            0x30, 0x00, 0x42, 0x8C, 0x22, 0x00, 0x40, 0x10, 0x00, 0x00, 0x00, 0x00, 0x25, 0x80, 0x40, 0x00, // 0x01F70420
            0xF7, 0x01, 0x02, 0x3C, 0x30, 0x00, 0x40, 0xAC, 0xF7, 0x01, 0x02, 0x3C, 0x38, 0x00, 0x51, 0x8C, // 0x01F70430
            0x38, 0x00, 0x40, 0xAC, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // 0x01F70440
            0xE0, 0x59, 0x06, 0x0C, 0x00, 0x00, 0x04, 0x34, 0x10, 0x00, 0xA2, 0xAF, 0x46, 0x00, 0x03, 0x34, // 0x01F70450
            0x2C, 0x1B, 0x43, 0xAC, 0x25, 0x20, 0x40, 0x00, 0xB4, 0x4B, 0x05, 0x0C, 0x25, 0x28, 0x20, 0x02, // 0x01F70460
            0x10, 0x00, 0xA4, 0x8F, 0xDC, 0x4C, 0x05, 0x0C, 0x25, 0x28, 0x20, 0x02, 0x10, 0x00, 0xA2, 0x8F, // 0x01F70470
            0xFF, 0xFF, 0x03, 0x24, 0xE4, 0x17, 0x43, 0xAC, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // 0x01F70480
            0x10, 0x00, 0xA4, 0x8F, 0xC8, 0x62, 0x05, 0x0C, 0x25, 0x28, 0x00, 0x02, 0xF7, 0x01, 0x02, 0x3C, // 0x01F70490
            0x01, 0x00, 0x03, 0x34, 0x3C, 0x00, 0x43, 0xAC, 0x12, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, // 0x01F704A0
            0xF7, 0x01, 0x02, 0x3C, 0x3C, 0x00, 0x42, 0x8C, 0x0E, 0x00, 0x40, 0x10, 0x00, 0x00, 0x00, 0x00, // 0x01F704B0
            0xE0, 0x59, 0x06, 0x0C, 0x00, 0x00, 0x04, 0x34, 0xE4, 0x17, 0x43, 0x8C, 0x25, 0x20, 0x40, 0x00, // 0x01F704C0
            0xFF, 0xFF, 0x02, 0x24, 0x05, 0x00, 0x62, 0x14, 0x00, 0x00, 0x00, 0x00, 0xF7, 0x01, 0x02, 0x3C, // 0x01F704D0
            0x3C, 0x00, 0x40, 0xAC, 0x03, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // 0x01F704E0
            0x00, 0x00, 0x00, 0x00, 0x0C, 0x00, 0xA2, 0x8F, 0x08, 0x00, 0xB1, 0x8F, 0x04, 0x00, 0xB0, 0x8F, // 0x01F704F0
            0x00, 0x00, 0xBF, 0x8F, 0x08, 0x00, 0xE0, 0x03, 0x30, 0x00, 0xBD, 0x27,                         // 0x01F70500
        };

        private static ushort[] EncodeText(string text)
        {
            var result = new System.Collections.Generic.List<ushort>();
            int i = 0;
            while (i < text.Length)
            {
                if (text[i] == '{')
                {
                    int end = text.IndexOf('}', i);
                    if (end != -1)
                    {
                        var tag = text.Substring(i + 1, end - i - 1).ToLower();
                        switch (tag)
                        {
                            case "red": result.Add(0xFC01); i = end + 1; continue;
                            case "reset": result.Add(0xFC00); i = end + 1; continue;
                            case "n": result.Add(0xFF00); i = end + 1; continue;
                        }
                    }
                }
                char c = text[i];
                if (c >= 0x21 && c <= 0x5B) result.Add((ushort)(c - 0x20));
                else if (c >= 0x5C && c <= 0x7E) result.Add((ushort)(c - 0x21));
                else result.Add(0xFF02); // space / unknown
                i++;
            }
            result.Add(0xFF01); // end of text
            result.Add(0xFF00); // end of message
            return result.ToArray();
        }

        private readonly record struct Notification(string Text, float Duration);

        private readonly ConcurrentQueue<Notification> _queue = new();
        private CancellationTokenSource _cts;
        private Task _workerTask;
        private bool _caveInstalled;

        public void Install()
        {
            if (!_caveInstalled)
            {
                Memory.WriteByteArray(CaveAddr, DialogCaveMips);
                _caveInstalled = true;
                Log.Information("[GameNotifier] Dialog cave written to 0x{Addr:X8}", CaveAddr);
            }
            Memory.Write(SystemMessage0 + OffMsgId, -1);

            if (_workerTask == null || _workerTask.IsCompleted)
            {
                _cts = new CancellationTokenSource();
                _workerTask = Task.Run(() => DrainLoop(_cts.Token));
            }
        }

        public void ShowNotification(string text, float duration = 3f)
        {
            _queue.Enqueue(new Notification(text, duration));
        }

        private void EnsureHookInstalled()
        {
            // The hook is a JAL instruction replacement.
            // Original: jal mgGetNowFrameRate  (some opcode)
            // Ours:     jal CaveAddr           (0x0C7DC100)
            var current = (uint)Memory.ReadInt(HookAddr);
            if (current != HookJal)
            {
                Memory.Write(HookAddr, (int)HookJal);
                Log.Information("[GameNotifier] Hook installed, was 0x{Prev:X8}", current);
            }
        }
        private bool IsDialogActive()
        {
            if (Memory.ReadInt(DialogActive) != 0) return true;
            int msgId = Memory.ReadInt(SystemMessage0 + OffMsgId);
            // -1 = no message (our dismissed state)
            //  0 = uninitialised (treat as inactive)
            // >0 = real game dialog showing
            return msgId > 0;
        }
        private async Task DrainLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {

                    if (_queue.TryPeek(out _) && !IsDialogActive())
                    {
                        if (_queue.TryDequeue(out var note))
                            await SendNotification(note, ct);
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning("[GameNotifier] DrainLoop error: {Ex}", ex.Message);
                }

                await Task.Delay(100, ct).ConfigureAwait(false);
            }
        }
        private async Task SendNotification(Notification note, CancellationToken ct)
        {
            try
            {
                // Write text to scratch buffer
                var encoded = EncodeText(note.Text);
                for (int i = 0; i < encoded.Length; i++)
                    Memory.Write(ScratchTextAddr + (ulong)(i * 2), (short)encoded[i]);
                Memory.Write(ScratchTextAddr + (ulong)(encoded.Length * 2), (short)0);

                // Set mode and trigger — pnach keeps hook alive, cave picks this up next frame
                Memory.Write(DialogMode, 4);
                Memory.Write(DialogFlag, ScratchMsgId);

                // Wait for cave to fire — should happen within 1-2 frames (~32ms)
                var deadline = DateTime.UtcNow.AddMilliseconds(500);
                while (DateTime.UtcNow < deadline && !ct.IsCancellationRequested)
                {
                    if (Memory.ReadInt(DialogActive) != 0)
                    {
                        Log.Information("[GameNotifier] Cave fired");
                        break;
                    }
                    await Task.Delay(16, ct);
                }

                if (Memory.ReadInt(DialogActive) == 0)
                {
                    Log.Warning("[GameNotifier] Cave never fired — DialogFlag={F}, DialogMode={M}",
                        Memory.ReadInt(DialogFlag), Memory.ReadInt(DialogMode));
                    return;
                }

                // Position override after Preset sets its defaults
                await Task.Delay(50, ct);
                Memory.Write(SystemMessage0 + OffPosX, DefaultX);
                Memory.Write(SystemMessage0 + OffPosY, DefaultY);

                await Task.Delay(TimeSpan.FromSeconds(note.Duration), ct);
                Dismiss();
            }
            catch (OperationCanceledException) { Dismiss(); }
            catch (Exception ex)
            {
                Log.Warning("[GameNotifier] SendNotification error: {Ex}", ex.Message);
                Dismiss();
            }
        }

        private void Dismiss()
        {
            // Writing -1 to ClsMes+0x17E4 tells the cave the message is done.
            // The cave clears DIALOG_ACTIVE on the next frame it sees this.
            Memory.Write(SystemMessage0 + OffMsgId, -1);
        }
    }
}
