﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Client.Desktop.ViewModels.Common.EntityViewModels;
using Client.Desktop.ViewModels.Common.Extensions;
using Client.Desktop.ViewModels.Common.ViewModels;
using Impinj.OctaneSdk;
using Storage.Laundry.Models;

namespace Client.Desktop.ViewModels.Common.Services
{
    public class RfidService : ViewModelBase
    {
        public ImpinjReader Reader = new ImpinjReader();
        private Settings settings;
        private readonly ILaundryService _laundryService;
        private readonly IDialogService _dialogService;

        private string _connectionStatus;
        private string _startStopButton;
        private ObservableCollection<RfidReaderEntityViewModel> _rfidReaders;
        private ConcurrentDictionary<string, int> _data;
        private RfidReaderEntityViewModel _selectedRfidReader;
        private ObservableCollection<RfidAntennaEntityViewModel> _antennas;

        public ConcurrentDictionary<string, int> Data
        {
            get => _data;
            set => Set(ref _data, value);
        }
        public ObservableCollection<RfidAntennaEntityViewModel> Antennas
        {
            get => _antennas;
            set => Set(ref _antennas, value);
        }
        public RfidReaderEntityViewModel SelectedRfidReader
        {
            get => _selectedRfidReader;
            set => Set(ref _selectedRfidReader, value);
        }
        public ObservableCollection<RfidReaderEntityViewModel> RfidReaders
        {
            get => _rfidReaders;
            set => Set(ref _rfidReaders, value);
        }
        public string StartStopButton
        {
            get => _startStopButton;
            set => Set(() => StartStopButton, ref _startStopButton, value);
        }
        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => Set(() => ConnectionStatus, ref _connectionStatus, value);
        }

        public RelayCommand StartStopReaderCommand { get; }


        public RfidService(ILaundryService laundryService, IDialogService dialogService)
        {
            _laundryService = laundryService ?? throw new ArgumentNullException(nameof(laundryService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            StartStopReaderCommand = new RelayCommand(StartStopRead, CheckConnection);

            StartStopButton = "Start";
            _data = new ConcurrentDictionary<string, int>();
            Initialize();

            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedRfidReader))
            {
                StartStopReaderCommand.RaiseCanExecuteChanged();
            }
        }

        private async void Initialize()
        {
            _dialogService.ShowBusy();

            try
            {
                var reader = await _laundryService.GetAllAsync<RfidReaderEntity>();
                var readers = reader.Select(x => new RfidReaderEntityViewModel(x));
                RfidReaders = readers.ToObservableCollection();

                var antenna = await _laundryService.GetAllAsync<RfidAntennaEntity>();
                var antennas = antenna.Select(x => new RfidAntennaEntityViewModel(x));
                Antennas = antennas.ToObservableCollection();
            }
            catch (Exception e)
            {
                _dialogService.HideBusy();
            }
            finally
            {
                _dialogService.HideBusy();
            }
        }

        public void Connect()
        {
            if (SelectedRfidReader == null) return;
            var antennas = Antennas.Where(x => x.RfidReaderId == SelectedRfidReader.Id).ToList();
            if (!antennas.Any()) return;

            Connection(SelectedRfidReader, antennas);
        }

        public void Disconnect()
        {
            if(Reader == null || !Reader.IsConnected) return;

            Reader.Stop();
            Reader.TagsReported -= DisplayTag;
            Reader.Disconnect();
        }

        private bool Connection(RfidReaderEntityViewModel newReader, List<RfidAntennaEntityViewModel> antennas)
        {
            try
            {
                Disconnect();

                Reader.Connect(SelectedRfidReader.ReaderIp);
                Reader.Stop();
            }

            catch (OctaneSdkException ee)
            {
                Console.WriteLine("Octane SDK exception: Reader #1" + ee.Message, "error");
            }
            catch (Exception ee)
            {
                Console.WriteLine("Exception : Reader #1" + ee.Message, "error");
                Console.WriteLine(ee.StackTrace);
            }

            if (!Reader.IsConnected) return false;
            SetSettings(SelectedRfidReader.TagPopulation);
            SetAntennaSettings(antennas);
            Reader.ApplySettings(settings);

            CheckConnection();
            return Reader.IsConnected;
        }

        private void SetSettings(ushort tagPopulation)
        {
            settings = Reader.QueryDefaultSettings();

            settings.Report.IncludeAntennaPortNumber = true;
            settings.Report.IncludePhaseAngle = true;
            settings.Report.IncludeChannel = true;
            settings.Report.IncludeDopplerFrequency = true;
            settings.Report.IncludeFastId = true;
            settings.Report.IncludeFirstSeenTime = true;
            settings.Report.IncludeLastSeenTime = true;
            settings.Report.IncludePeakRssi = true;
            settings.Report.IncludeSeenCount = true;
            settings.Report.IncludePcBits = true;
            settings.Report.IncludeSeenCount = true;

            //ReaderMode.AutoSetDenseReaderDeepScan | Rx = -70 | Tx = 15/20
            //ReaderMode.MaxThrouput | Rx = -80 | Tx = 15

            settings.ReaderMode = ReaderMode.AutoSetDenseReaderDeepScan;//.AutoSetDenseReader;
            settings.SearchMode = SearchMode.DualTarget;//.DualTarget;
            settings.Session = 1;
            settings.TagPopulationEstimate = tagPopulation;
            settings.Report.Mode = ReportMode.Individual;
        }

        private void SetAntennaSettings(List<RfidAntennaEntityViewModel> antennas)
        {
            settings.Antennas.DisableAll();
            foreach (var antenna in antennas)
            {
                settings.Antennas.GetAntenna((ushort)antenna.AntennaNumb).IsEnabled = true;
                settings.Antennas.GetAntenna((ushort)antenna.AntennaNumb).TxPowerInDbm = antenna.TxPower;
                settings.Antennas.GetAntenna((ushort)antenna.AntennaNumb).RxSensitivityInDbm = antenna.RxSensitivity;
            }
        }
        
        public bool CheckConnection()
        {
            var isConnected = Reader.IsConnected;

            try
            {
                ConnectionStatus = isConnected ? "Connected" : "Disconnected";
            }

            catch (OctaneSdkException ee)
            {
                Console.WriteLine("Octane SDK exception: Reader #1" + ee.Message, "error");
            }
            catch (Exception ee)
            {
                Console.WriteLine("Exception : Reader #1" + ee.Message, "error");
                Console.WriteLine(ee.StackTrace);
            }

            return isConnected;
        }

        public void StartStopRead()
        {
            if (StartStopButton == "Start")
            {
                StartRead();
                StartStopButton = "Stop";
            }
            else
            {
                StopRead();
                StartStopButton = "Start";
            }
        }

        public void StartRead()
        {
            if (!Reader.IsConnected) return;

            Reader.TagsReported += DisplayTag;
            Reader.Start();
        }

        public void StopRead()
        {
            if (!Reader.IsConnected) return;

            Reader.Stop();
            Reader.TagsReported -= DisplayTag;
        }

        private void DisplayTag(ImpinjReader reader, TagReport report)
        {
            _data = new ConcurrentDictionary<string, int>();

            foreach (Tag tag in report)
            {
                AddData(tag.Epc.ToString(), tag.AntennaPortNumber);
            }
        }

        private void AddData(string epc, int antenna)
        {
            if (!_data.TryGetValue(epc, out int val))
            {
                _data.TryAdd(epc, antenna);
            }
            else
            {
                _data.TryUpdate(epc, antenna, val);
            }
        }

    }

}
