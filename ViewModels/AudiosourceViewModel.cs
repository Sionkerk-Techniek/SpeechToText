using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Windows.Devices.Enumeration;
using static SpeechToText.Logging;

namespace SpeechToText.ViewModels
{
    public class AudiosourceViewModel
    {
        public static AudiosourceViewModel Instance { get; private set; }

        public Dictionary<string, DeviceInformation> Devices { get; } = new();
        public DeviceInformation SelectedDevice { get; private set; }
        
        private DeviceWatcher _watcher;

        public AudiosourceViewModel()
        {
            Instance = this;
        }

        public void Initialise()
        {
            CreateDeviceWatcher();
        }

        /// <summary>
        /// Update the devices dropdown
        /// </summary>
        public void UpdateDevices()
        {
            MenuFlyout flyout = App.MainWindow.AudiosourceMenu;

            flyout.Items.Clear();
            foreach (DeviceInformation device in Devices.Values)
                flyout.Items.Add(new RadioMenuFlyoutItem()
                {
                    Text = device.Name,
                    Tag = device,
                    IsChecked = device == SelectedDevice
                });
        }

        public void SelectDevice(object sender, object e)
        {
            SelectedDevice = (sender as MenuFlyout).Items.FirstOrDefault(
                item => item is RadioMenuFlyoutItem radioItem && radioItem.IsChecked
                )?.Tag as DeviceInformation;

            if (SelectedDevice != null)
            {
                Settings.Instance.LastAudioDeviceId = SelectedDevice.Id;
                Log("Selected device: " + SelectedDevice.Name);
            }
        }

        public void CreateDeviceWatcher()
        {
            // TODO: place in own method? with initialise? fix async stuff
            _watcher = DeviceInformation.CreateWatcher(DeviceClass.AudioCapture);
            _watcher.Added += delegate (DeviceWatcher sender, DeviceInformation device)
            {
                Log($"Device found: {device.Name} ({device.Id})");
                Devices[device.Id] = device;
            };
            _watcher.Removed += delegate (DeviceWatcher sender, DeviceInformationUpdate device)
            {
                Log($"Device removed: {Devices[device.Id].Name} ({device.Id})");
                Devices.Remove(device.Id);

                if (device.Id == SelectedDevice?.Id)
                {
                    SelectedDevice = null;
                }
            };
            _watcher.EnumerationCompleted += delegate
            {
                Log($"Device enumeration completed");
            };
            _watcher.Start();
        }
    }
}
