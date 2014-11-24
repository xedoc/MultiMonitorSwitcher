using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using MultiMonitorSwitcher.Model;
using System.Linq;
using System.Windows.Threading;
using System;
using GalaSoft.MvvmLight.CommandWpf;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MultiMonitorSwitcher.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        MonitorService monitorService;
        private DispatcherTimer timer = new DispatcherTimer();
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            monitorService = new MonitorService();
            Initialize();
        }

        private void Initialize()
        {
            FillMonitors();
            //timer = new DispatcherTimer()
            //{
            //    Interval = TimeSpan.FromMilliseconds(1000)
            //};

            //timer.Tick += (o,e) => {

            //        FillMonitors();
            //};
            //timer.Start();

        }
        private void FillMonitors()
        {
            monitorService.GetMonitors((error, monitors) =>
            {
                if (!string.IsNullOrWhiteSpace(error))
                {

                }
                else if (monitors != null)
                {
                    //Delete disconnected
                    Monitors.Except(monitors, new LambdaComparer<Monitor>((x, y) => x.DeviceId.Equals(y.DeviceId)))
                        .ToList()
                        .ForEach(deleteMonitor => Monitors.Remove(deleteMonitor));

                    //Add connected
                    monitors.Except(Monitors, new LambdaComparer<Monitor>((x, y) => x.DeviceId.Equals(y.DeviceId)))
                        .ToList()
                        .ForEach(addMonitor => Monitors.Add(addMonitor));
                }
            });
            
        }
        /// <summary>
        /// The <see cref="Monitors" /> property's name.
        /// </summary>
        public const string MonitorsPropertyName = "Monitors";

        private ObservableCollection<Monitor> _monitors = new ObservableCollection<Monitor>();

        /// <summary>
        /// Sets and gets the Monitors property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<Monitor> Monitors
        {
            get
            {
                return _monitors;
            }

            set
            {
                if (_monitors == value)
                {
                    return;
                }

                _monitors = value;
                RaisePropertyChanged(MonitorsPropertyName);
            }
        }

        private RelayCommand<string> _switchMonitorOff;

        /// <summary>
        /// Gets the SwitchMonitorOff.
        /// </summary>
        public RelayCommand<string> SwitchMonitorOff
        {
            get
            {
                return _switchMonitorOff
                    ?? (_switchMonitorOff = new RelayCommand<string>(
                    async (id) =>
                    {
                        await Task.Run( () => monitorService.SwitchMonitorOff(id) );
                    }));
            }
        }

        private RelayCommand<string> _switchMonitorOn;

        /// <summary>
        /// Gets the SwitchMonitorOn.
        /// </summary>
        public RelayCommand<string> SwitchMonitorOn
        {
            get
            {
                return _switchMonitorOn
                    ?? (_switchMonitorOn = new RelayCommand<string>(
                    async (id) =>
                    {
                        await Task.Run( () => monitorService.SwitchMonitorOn(id) );
                    }));
            }
        }

        public override void Cleanup()
        {
            // Clean up if needed

            base.Cleanup();
            timer.Stop();   
        }
    }
}