using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

// Plan GUIDs obtained from running powercfg -aliases.
namespace AutoPowerScheme {
    class Program {
       static void Main(string[] args) {
            UpdatePowerSchemeOnPowerModeChange();
            UpdatePowerSchemeOnTimer();

           Application.Run();
        }

        private static void UpdatePowerSchemeOnTimer() {
            const int TimerFrequencyMs = 60000;

            var timer = new Timer {
                Enabled = true,
                Interval = TimerFrequencyMs,
            };

            timer.Tick += (sender, eventArgs) => UpdatePowerScheme();
            timer.Start();
        }

        private static void UpdatePowerSchemeOnPowerModeChange() => 
            SystemEvents.PowerModeChanged += (sender, eventArgs) => {
                if (eventArgs.Mode == PowerModes.StatusChange || eventArgs.Mode == PowerModes.Resume) {
                    UpdatePowerScheme();
                }
            };

        private static void UpdatePowerScheme() {
            var powerLineStatus = SystemInformation.PowerStatus.PowerLineStatus;
            Console.WriteLine($"Powerline status is {powerLineStatus}.");

            if (powerLineStatus == PowerLineStatus.Online) {
                SwitchToMaximumPerformance();
            } else if (powerLineStatus == PowerLineStatus.Offline) {
                SwitchToBatterySaving();
            }
        }

        private static void SwitchToBatterySaving() {
            var batterySavingPowerScheme = new Guid("a1841308-3541-4fab-bc81-f71556f20b4a");
            Console.WriteLine("Enabling battery saving power scheme.");
            PowerSetActiveScheme(IntPtr.Zero, ref batterySavingPowerScheme);
        }

        private static void SwitchToMaximumPerformance() {
            var maximumPerformanceScheme = new Guid("8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");
            Console.WriteLine("Enabling maximum performance power scheme.");
            PowerSetActiveScheme(IntPtr.Zero, ref maximumPerformanceScheme);
        }

        [DllImport("powrprof.dll", EntryPoint = "PowerSetActiveScheme")]
        private static extern uint PowerSetActiveScheme(IntPtr UserPowerKey, ref Guid ActivePolicyGuid);
    }
}
