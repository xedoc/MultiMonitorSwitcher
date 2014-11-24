using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MultiMonitorSwitcher.Model
{
    public class MonitorService
    {
        private List<Monitor> monitors;

        public MonitorService()
        {
            monitors = new List<Monitor>();
        }

        public void SwitchMonitorOff( string id )
        {
            //(string lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd, ChangeDisplaySettingsFlags dwflags, IntPtr lParam

            var display = monitors.FirstOrDefault(d => d.DeviceId == id);
            
            if (display == null)
                return;


            NativeMethods.DEVMODE deleteScreenMode = new NativeMethods.DEVMODE();

            deleteScreenMode.dmSize = (short)Marshal.SizeOf(deleteScreenMode);
            deleteScreenMode.dmDriverExtra = 0;
            deleteScreenMode.dmFields = NativeMethods.DM.Position | NativeMethods.DM.PelsHeight | NativeMethods.DM.PelsWidth;
            deleteScreenMode.dmPelsWidth = 0;
            deleteScreenMode.dmPelsHeight = 0;

            NativeMethods.POINTL deletion;
            deletion.x = 0;
            deletion.y = 0;
            deleteScreenMode.dmPosition = deletion;

            NativeMethods.ChangeDisplaySettingsEx(
                id,
                ref deleteScreenMode,
                IntPtr.Zero,
                NativeMethods.ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY |
                NativeMethods.ChangeDisplaySettingsFlags.CDS_NORESET,
                IntPtr.Zero);

            NativeMethods.ChangeDisplaySettingsEx(null, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);




        }
        public void SwitchMonitorOn(string id)
        {
            var display = monitors.FirstOrDefault(d => d.DeviceId == id);
            
            if (display == null)
                return;

            var hdc = NativeMethods.GetDC(IntPtr.Zero);
            var width = NativeMethods.GetDeviceCaps(hdc, 8);
            NativeMethods.ReleaseDC(IntPtr.Zero, hdc);

            NativeMethods.DEVMODE defaultMode = new NativeMethods.DEVMODE();
            defaultMode.dmSize = (short)Marshal.SizeOf(defaultMode);
            defaultMode.dmPosition.x += width;
            defaultMode.dmFields = NativeMethods.DM.Position;
            NativeMethods.ChangeDisplaySettingsEx(display.DeviceId,
                                ref defaultMode,
                                IntPtr.Zero,
                                NativeMethods.ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY | 
                                NativeMethods.ChangeDisplaySettingsFlags.CDS_NORESET,
                                IntPtr.Zero);

            NativeMethods.ChangeDisplaySettings( IntPtr.Zero, IntPtr.Zero); 
        }
        public void GetMonitors( Action<string, List<Monitor>> callback )
        {
            NativeMethods.DISPLAY_DEVICE display = new NativeMethods.DISPLAY_DEVICE();
            uint deviceId = 0;

            var error = Try(() => {
                display.cb = Marshal.SizeOf(display);
                monitors.Clear();
                while (NativeMethods.EnumDisplayDevices(null, deviceId, ref display, 0))
                {

                    if (display.StateFlags == NativeMethods.DisplayDeviceStateFlags.MirroringDriver)
                    {
                        deviceId++;
                        continue;
                    }

                    monitors.Add(new Monitor()
                    {
                        DeviceId = display.DeviceName,
                        Description = display.DeviceString + " " + display.DeviceName,
                        IsAttached = display.StateFlags.HasFlag( NativeMethods.DisplayDeviceStateFlags.AttachedToDesktop),
                        IsPrimary = display.StateFlags.HasFlag(NativeMethods.DisplayDeviceStateFlags.PrimaryDevice),
                      
                    });
                    Debug.Print("{0},{1}", display.DeviceName, display.StateFlags);
                    deviceId++;
                }            
            });
            callback(error, monitors);
        }

        private string Try( Action action )
        {
            if (action == null)
                return null;

            try
            {
                action();
            }
            catch (Exception e )
            {
                return e.Message;
            }
            return null;
        }
    }

    
}
