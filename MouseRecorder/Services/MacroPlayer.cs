using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MouseRecorder.Models;
using MouseRecorder.Native;

namespace MouseRecorder.Services
{
    public class MacroPlayer
    {
        private CancellationTokenSource _cts;

        public bool IsPlaying { get; private set; }
        public event Action<string> StatusChanged;
        public event Action PlaybackFinished;
        public event Action<int> StepExecuting;

        public async Task PlayAsync(Macro macro)
        {
            if (IsPlaying) return;
            if (macro == null || macro.Steps.Count == 0) return;

            IsPlaying = true;
            _cts = new CancellationTokenSource();

            try
            {
                int totalRuns = macro.RepeatCount <= 0 ? int.MaxValue : macro.RepeatCount;
                string totalLabel = macro.RepeatCount <= 0 ? "\u221E" : totalRuns.ToString();

                for (int run = 0; run < totalRuns; run++)
                {
                    _cts.Token.ThrowIfCancellationRequested();
                    RaiseStatus($"Running '{macro.Name}' — iteration {run + 1}/{totalLabel}");

                    for (int i = 0; i < macro.Steps.Count; i++)
                    {
                        _cts.Token.ThrowIfCancellationRequested();
                        StepExecuting?.Invoke(i);
                        ExecuteStep(macro.Steps[i]);
                    }
                }

                RaiseStatus("Macro finished");
            }
            catch (OperationCanceledException)
            {
                RaiseStatus("Macro stopped");
            }
            catch (Exception ex)
            {
                RaiseStatus($"Error: {ex.Message}");
            }
            finally
            {
                IsPlaying = false;
                PlaybackFinished?.Invoke();
            }
        }

        public void Stop()
        {
            _cts?.Cancel();
        }

        private void ExecuteStep(MacroStep step)
        {
            switch (step.Type)
            {
                case StepType.LeftClick:
                    PerformClick(step.X, step.Y);
                    break;
                case StepType.LeftDoubleClick:
                    PerformDoubleClick(step.X, step.Y);
                    break;
                case StepType.RightClick:
                    PerformRightClick(step.X, step.Y);
                    break;
                case StepType.KeyboardShortcut:
                    PerformKeyCombo(step.Keys);
                    break;
                case StepType.Keystroke:
                    PerformKeyCombo(step.Keys);
                    break;
                case StepType.TypeText:
                    PerformTypeText(step.Text);
                    break;
                case StepType.Wait:
                    WaitWithCancel(step.DelayMs);
                    break;
            }
        }

        private void PerformClick(int x, int y)
        {
            Win32.SetCursorPos(x, y);
            Thread.Sleep(30);
            Win32.mouse_event(Win32.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, IntPtr.Zero);
            Thread.Sleep(20);
            Win32.mouse_event(Win32.MOUSEEVENTF_LEFTUP, 0, 0, 0, IntPtr.Zero);
            Thread.Sleep(30);
        }

        private void PerformDoubleClick(int x, int y)
        {
            Win32.SetCursorPos(x, y);
            Thread.Sleep(30);
            Win32.mouse_event(Win32.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, IntPtr.Zero);
            Thread.Sleep(20);
            Win32.mouse_event(Win32.MOUSEEVENTF_LEFTUP, 0, 0, 0, IntPtr.Zero);
            Thread.Sleep(20);
            Win32.mouse_event(Win32.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, IntPtr.Zero);
            Thread.Sleep(20);
            Win32.mouse_event(Win32.MOUSEEVENTF_LEFTUP, 0, 0, 0, IntPtr.Zero);
            Thread.Sleep(30);
        }

        private void PerformRightClick(int x, int y)
        {
            Win32.SetCursorPos(x, y);
            Thread.Sleep(30);
            Win32.mouse_event(Win32.MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, IntPtr.Zero);
            Thread.Sleep(20);
            Win32.mouse_event(Win32.MOUSEEVENTF_RIGHTUP, 0, 0, 0, IntPtr.Zero);
            Thread.Sleep(30);
        }

        private void PerformKeyCombo(List<string> keys)
        {
            if (keys == null || keys.Count == 0) return;

            var vkCodes = new List<ushort>();
            foreach (var key in keys)
            {
                var vk = KeyMapper.GetVirtualKeyCode(key);
                if (vk != 0) vkCodes.Add(vk);
            }

            if (vkCodes.Count == 0) return;

            int inputSize = Marshal.SizeOf(typeof(Win32.INPUT));
            var inputs = new Win32.INPUT[vkCodes.Count * 2];

            // Press all keys down
            for (int i = 0; i < vkCodes.Count; i++)
            {
                inputs[i].type = Win32.INPUT_KEYBOARD;
                inputs[i].u.ki.wVk = vkCodes[i];
                inputs[i].u.ki.dwFlags = 0;
            }

            // Release all keys in reverse order
            for (int i = 0; i < vkCodes.Count; i++)
            {
                int releaseIdx = vkCodes.Count + i;
                int keyIdx = vkCodes.Count - 1 - i;
                inputs[releaseIdx].type = Win32.INPUT_KEYBOARD;
                inputs[releaseIdx].u.ki.wVk = vkCodes[keyIdx];
                inputs[releaseIdx].u.ki.dwFlags = Win32.KEYEVENTF_KEYUP;
            }

            Win32.SendInput((uint)inputs.Length, inputs, inputSize);
            Thread.Sleep(50);
        }

        private void PerformTypeText(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            int inputSize = Marshal.SizeOf(typeof(Win32.INPUT));

            foreach (char c in text)
            {
                var inputs = new Win32.INPUT[2];

                // Key down
                inputs[0].type = Win32.INPUT_KEYBOARD;
                inputs[0].u.ki.wVk = 0;
                inputs[0].u.ki.wScan = c;
                inputs[0].u.ki.dwFlags = Win32.KEYEVENTF_UNICODE;

                // Key up
                inputs[1].type = Win32.INPUT_KEYBOARD;
                inputs[1].u.ki.wVk = 0;
                inputs[1].u.ki.wScan = c;
                inputs[1].u.ki.dwFlags = Win32.KEYEVENTF_UNICODE | Win32.KEYEVENTF_KEYUP;

                Win32.SendInput(2, inputs, inputSize);
                Thread.Sleep(15);
            }

            Thread.Sleep(30);
        }

        private void WaitWithCancel(int ms)
        {
            // Sleep in small chunks so we can respond to cancel quickly
            int remaining = ms;
            while (remaining > 0)
            {
                _cts.Token.ThrowIfCancellationRequested();
                int chunk = Math.Min(remaining, 100);
                Thread.Sleep(chunk);
                remaining -= chunk;
            }
        }

        private void RaiseStatus(string text)
        {
            StatusChanged?.Invoke(text);
        }
    }
}
