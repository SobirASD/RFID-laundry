﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using PALMS.Data.Objects.ClientModel;
using PALMS.Settings.ViewModel.Common;
using PALMS.Settings.ViewModel.EntityViewModels;
using PALMS.Settings.ViewModel.Windows;
using PALMS.ViewModels.Common;
using PALMS.ViewModels.Common.Services;

namespace PALMS.Settings.ViewModel.ViewModels
{
    public class VendorDetailsViewModel : ViewModelBase, ISettingsContent, IInitializationAsync
    {
        public string Name => "PrimeInfo";
        public bool HasChanges()
        {
            throw new NotImplementedException();
        }

        private readonly IDispatcher _dispatcher;
        private readonly IDataService _dataService;
        private readonly IDialogService _dialogService;
        private readonly IResolver _resolverService;
        public ManualResetEvent Plc1Thread { get; set; }
        public ManualResetEvent RfidThread { get; set; }


        #region parameters

        private int _plc1Error, _plc2Error, _plc3Error;
        private FinsTcp _plc1;
        private FinsTcp _belt1;
        private FinsTcp _belt2;
        private string _localIp;
        private string _plc3Ip;
        private string _plc2Ip;
        private string _plc1Ip;
        private int _getBelt1SlotNumb;
        private int _getBelt2SlotNumb;
        private ObservableCollection<ConveyorItemViewModel> _beltItems;
        private bool _isAutoMode;
        private int _setBelt1SlotNumb;
        private int _setBelt2SlotNumb;
        private RfidCommon _impinj;
        private ObservableCollection<ClientLinenEntityViewModel> _clientLinens;
        private bool _isItemPrepared;
        private ClientLinenEntityViewModel _waitingLinen;
        private ConveyorItemViewModel _hangingLinen;
        private ObservableCollection<MasterLinenEntityViewModel> _masterLinens;
        private ObservableCollection<ClientStaffEntityViewModel> _staff;
        private ObservableCollection<string> _tags;
        private bool _isAutoPackMode;

        public bool IsAutoPackMode
        {
            get => _isAutoPackMode;
            set => Set(() => IsAutoPackMode, ref _isAutoPackMode, value);
        }
        public ObservableCollection<string> Tags
        {
            get => _tags;
            set => Set(() => Tags, ref _tags, value);
        }
        public ObservableCollection<ClientStaffEntityViewModel> Staff
        {
            get => _staff;
            set => Set(() => Staff, ref _staff, value);
        }
        public ObservableCollection<MasterLinenEntityViewModel> MasterLinens
        {
            get => _masterLinens;
            set => Set(() => MasterLinens, ref _masterLinens, value);
        }
        public ClientLinenEntityViewModel WaitingLinen
        {
            get => _waitingLinen;
            set => Set(() => WaitingLinen, ref _waitingLinen, value);
        }
        public bool IsItemPrepared
        {
            get => _isItemPrepared;
            set => Set(() => IsItemPrepared, ref _isItemPrepared, value);
        }
        public ObservableCollection<ClientLinenEntityViewModel> ClientLinens
        {
            get => _clientLinens;
            set => Set(() => ClientLinens, ref _clientLinens, value);
        }
        public RfidCommon Impinj
        {
            get => _impinj;
            set => Set(() => Impinj, ref _impinj, value);
        }
        public ConveyorItemViewModel HangingLinen
        {
            get => _hangingLinen;
            set => Set(() => HangingLinen, ref _hangingLinen, value);
        }
        public int SetBelt2SlotNumb
        {
            get => _setBelt2SlotNumb;
            set => Set(() => SetBelt2SlotNumb, ref _setBelt2SlotNumb, value);
        }
        public int SetBelt1SlotNumb
        {
            get => _setBelt1SlotNumb;
            set => Set(() => SetBelt1SlotNumb, ref _setBelt1SlotNumb, value);
        }
        public bool IsAutoMode
        {
            get => _isAutoMode;
            set => Set(() => IsAutoMode, ref _isAutoMode, value);
        }
        public ObservableCollection<ConveyorItemViewModel> BeltItems
        {
            get => _beltItems;
            set => Set(() => BeltItems, ref _beltItems, value);
        }
        public int GetBelt2SlotNumb
        {
            get => _getBelt2SlotNumb;
            set => Set(() => GetBelt2SlotNumb, ref _getBelt2SlotNumb, value);
        }
        public int GetBelt1SlotNumb
        {
            get => _getBelt1SlotNumb;
            set => Set(() => GetBelt1SlotNumb, ref _getBelt1SlotNumb, value);
        }
        public int Plc3Error
        {
            get => _plc3Error;
            set => Set(() => Plc3Error, ref _plc3Error, value);
        }
        public int Plc2Error
        {
            get => _plc2Error;
            set => Set(() => Plc2Error, ref _plc2Error, value);
        }
        public int Plc1Error
        {
            get => _plc1Error;
            set => Set(() => Plc1Error, ref _plc1Error, value);
        }
        public string LocalIp
        {
            get => _localIp;
            set => Set(() => LocalIp, ref _localIp, value);
        }
        public FinsTcp Plc1
        {
            get => _plc1;
            set => Set(() => Plc1, ref _plc1, value);
        }
        public FinsTcp Belt1
        {
            get => _belt1;
            set => Set(() => Belt1, ref _belt1, value);
        }
        public FinsTcp Belt2
        {
            get => _belt2;
            set => Set(() => Belt2, ref _belt2, value);
        }
        public string Plc1Ip
        {
            get => _plc1Ip;
            set => Set(() => Plc1Ip, ref _plc1Ip, value);
        }
        public string Plc2Ip
        {
            get => _plc2Ip;
            set => Set(() => Plc2Ip, ref _plc2Ip, value);
        }
        public string Plc3Ip
        {
            get => _plc3Ip;
            set => Set(() => Plc3Ip, ref _plc3Ip, value);
        }

        public ObservableCollection<ConveyorItemViewModel> Belt1Items => SortBeltItems(1);
        public ObservableCollection<ConveyorItemViewModel> Belt2Items => SortBeltItems(2);

        public RelayCommand ConnectConveyorCommand { get; }
        public RelayCommand StartConveyorCommand { get; }
        public RelayCommand StopConveyorCommand { get; }
        public RelayCommand AutoModeCommand { get; }
        public RelayCommand ResetClothCountCommand { get; }
        public RelayCommand SendToBelt1Command { get; }
        public RelayCommand SendToBelt2Command { get; }
        public RelayCommand AutoPackModeCommand { get; }
        public RelayCommand TakeClothBelt1Command { get; }
        public RelayCommand TakeClothBelt2Command { get; }
        public RelayCommand PackClothCommand { get; }
        public RelayCommand RemoveAllSLotsCommand { get; }

        #endregion


        public async Task InitializeAsync()
        {
            var linen = await _dataService.GetAsync<ClientLinen>();
            var linens = linen.Select(x => new ClientLinenEntityViewModel(x));
            _dispatcher.RunInMainThread(() => ClientLinens = linens.ToObservableCollection());

            var master = await _dataService.GetAsync<MasterLinen>();
            var masters = master.Select(x => new MasterLinenEntityViewModel(x));
            _dispatcher.RunInMainThread(() => MasterLinens = masters.ToObservableCollection());

            var staff = await _dataService.GetAsync<ClientStaff>();
            var staffs = staff.Select(x => new ClientStaffEntityViewModel(x));
            _dispatcher.RunInMainThread(() => Staff = staffs.ToObservableCollection());

            var conveyorItem = await _dataService.GetAsync<ConveyorItem>();
            var conveyorItems = conveyorItem.Select(x => new ConveyorItemViewModel(x));
            _dispatcher.RunInMainThread(() => BeltItems = conveyorItems.ToObservableCollection());

            BeltItems.ForEach(SubscribeItem);

            foreach (var item in BeltItems.Where(x=>x.ClientLinenId != null))
            {
                item.ClientLinen = ClientLinens.FirstOrDefault(x => x.Id == item.ClientLinenId);
            }


            HangingLinen = new ConveyorItemViewModel();
            WaitingLinen = new ClientLinenEntityViewModel();
            Tags = new ObservableCollection<string>();
        }
        
        public VendorDetailsViewModel(IDispatcher dispatcher, IDataService dataService, IDialogService dialogService, IResolver resolver)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _resolverService = resolver ?? throw new ArgumentNullException(nameof(resolver));

            SendToBelt1Command = new RelayCommand(ManualSendToBelt1);
            SendToBelt2Command = new RelayCommand(ManualSendToBelt2);
            ConnectConveyorCommand = new RelayCommand(ConnectConveyor);
            StartConveyorCommand = new RelayCommand(StartConveyor);
            StopConveyorCommand = new RelayCommand(StopConveyor);
            AutoModeCommand = new RelayCommand(AutoMode);
            ResetClothCountCommand = new RelayCommand(ResetClothCount);
            AutoPackModeCommand = new RelayCommand(AutoPacking);
            TakeClothBelt1Command = new RelayCommand(TakeClothBelt1);
            TakeClothBelt2Command = new RelayCommand(TakeClothBelt2);
            PackClothCommand = new RelayCommand(PackCloth);
            RemoveAllSLotsCommand = new RelayCommand(RemoveAllSlots);

            InitializeAsync();

            Plc1Ip = "192.168.250.1";
            Plc2Ip = "192.168.250.2";
            Plc3Ip = "192.168.250.3";
            LocalIp = GetLocalIp();

            Plc1Error = 99;
            Plc2Error = 99;
            Plc3Error = 99;
            Plc1Thread = new ManualResetEvent(true);
            RfidThread = new ManualResetEvent(true);
            Impinj = new RfidCommon();
            PropertyChanged += OnPropertyChanged;

        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsAutoMode))
            {
                if (IsAutoMode)
                {
                    Task.Factory.StartNew(RunAutoMode);
                }
            }

            if (e.PropertyName == nameof(IsAutoPackMode))
            {
                if (IsAutoPackMode)
                {
                    Task.Factory.StartNew(RunAutoPacking);
                }
            }

            //if (e.PropertyName == nameof(IsItemPrepared))
            //{
            //    if (!IsItemPrepared)
            //    {
            //        Task.Factory.StartNew(RunCircleCheckCloth);
            //    }
            //}
        }

        private void ItemOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is ConveyorItemViewModel item)) return;

            if (e.PropertyName == nameof(ConveyorItemViewModel.ClientLinenId))
            {
                SaveConveyorItem(sender as ConveyorItemViewModel);
            }

            RaisePropertyChanged(() => Belt1Items);
            RaisePropertyChanged(() => Belt2Items);
        }

        private void UnSubscribeItem(ConveyorItemViewModel item)
        {
            item.PropertyChanged -= ItemOnPropertyChanged;
        }

        private void SubscribeItem(ConveyorItemViewModel item)
        {
            item.PropertyChanged += ItemOnPropertyChanged;
        }

        private void SaveConveyorItem(ConveyorItemViewModel item)
        {
            if (item.HasChanges())
            {
                item.AcceptChanges();
                _dataService.AddOrUpdateAsync(item.OriginalObject);
            }
        }

        private void RemoveAllSlots()
        {
            var items = BeltItems.Where(x => x.HasItem).ToList();
            RemoveBeltItems(items);
        }

        private ObservableCollection<ConveyorItemViewModel> SortBeltItems(int beltNumb)
        {
            var beltItems = BeltItems?.Where(x => x.HasItem && x.BeltNumber == beltNumb).ToObservableCollection();
            var ordered = beltItems?.OrderBy(x => x.SlotNumber).ToObservableCollection();
            return ordered;
        }


#region Conveyor Start
        public void StartConveyor()
        {
            Plc1.Start();
            Belt1.Start();
            Belt2.Start();

            Task.Factory.StartNew(RunCircleCheckCloth);
        }

        public void StopConveyor()
        {
            Plc1.Stop();
            Belt1.Stop();
            Belt2.Stop();
        }

        public void ConnectConveyor()
        {
            Plc1 = new FinsTcp(LocalIp, Plc1Ip, 9600);
            Belt1 = new FinsTcp(LocalIp, Plc2Ip, 9600);
            Belt2 = new FinsTcp(LocalIp, Plc3Ip, 9600);

            CheckConveyorConnection();
        }

        private void CheckConveyorConnection()
        {
            Plc1Error = Plc1.conn(LocalIp, Plc1Ip, 9600);
            Plc2Error = Belt1.conn(LocalIp, Plc2Ip, 9600);
            Plc3Error = Belt2.conn(LocalIp, Plc3Ip, 9600);

            GetBelt1SlotNumb = Belt1.GetNowPoint();
            GetBelt2SlotNumb = Belt2.GetNowPoint();

            Belt1.GetBasePoint();
            Belt1.GetHanginpoint();

            Belt2.GetBasePoint();
            Belt2.GetHanginpoint();
        }

        private string GetLocalIp()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        #endregion


#region Common Methods

        private void ResetClothCount()
        {
            Plc1.ResetWaitHangNum();
            IsItemPrepared = false;
        }

        private bool IsSlotHasItem(int beltNumb, int slotNumb)
        {
            var first = BeltItems.FirstOrDefault(x => x.BeltNumber == beltNumb && x.SlotNumber == slotNumb);

            return first != null && first.HasItem;
        }

        private bool IsBeltFull(int beltNumb)
        {
            var belt = GetBeltItems(beltNumb);

            return belt.All(x => x.HasItem);
        }

        private ObservableCollection<ConveyorItemViewModel> GetBeltItems(int beltNumb)
        {
            var items = new ObservableCollection<ConveyorItemViewModel>();

            items.AddRange(BeltItems.Where(x => x.BeltNumber == beltNumb));

            return items;
        }

        private ObservableCollection<ConveyorItemViewModel> GetBeltEmptyItems(int beltNumb)
        {
            var items = new ObservableCollection<ConveyorItemViewModel>();

            items.AddRange(BeltItems.Where(x => x.BeltNumber == beltNumb && !x.HasItem));

            var order = items.OrderBy(x => x.SlotNumber).ToObservableCollection();

            return order;
        }

        private async Task<bool> UpdateClientLinenEntity()
        {
            ClientLinens = new ObservableCollection<ClientLinenEntityViewModel>();
            var linen = await _dataService.GetAsync<ClientLinen>();
            var linens = linen.Select(x => new ClientLinenEntityViewModel(x));
            ClientLinens = linens.ToObservableCollection();

            return true;
        }

        private void SetWaitingLinen(ClientLinenEntityViewModel linen)
        {
            WaitingLinen = linen;
            IsItemPrepared = true;
        }

        private void SetHangingLinen(int beltNumb, int slotNumb)
        {
            var beltItem = BeltItems.FirstOrDefault(x => x.BeltNumber == beltNumb && x.SlotNumber == slotNumb);

            _dispatcher.RunInMainThread((() =>
            {
                beltItem.ClientLinen = WaitingLinen;
                HangingLinen = beltItem;
                IsItemPrepared = false;

                WaitingLinen = new ClientLinenEntityViewModel();
            }));
        }

        #endregion

        
#region Plc1 Waiting 

        private void RunCircleCheckCloth()
        {
            while (true)
            {
                Thread.Sleep(1000);

                if(IsItemPrepared) continue;

                if (!Plc1.GetClotheReady()) continue;

                if (!CheckClothWaitingNumb()) continue;
                    
                var tag = CheckLinenRfid();
                if (String.IsNullOrWhiteSpace(tag))
                    continue;

                var linen = CheckLinenByTag(tag);
                if (linen == null)
                    continue;

                SetWaitingLinen(linen);

                if (IsAutoMode)
                {
                    Task.Factory.StartNew(RunAutoMode);
                }
            }
        }

        /// <summary>
        /// проверка на наличие количество вешелок
        /// </summary>
        /// <returns></returns>
        private bool CheckClothWaitingNumb()
        {
            var i = Plc1.GetWaitHangNum();

            if (i > 1)
            {
                ShowDialogWaitingNumb();

                return false;
            }
            return true;
        }

        /// <summary>
        /// вызвать метод считывателя и проверика
        /// </summary>
        public string CheckLinenRfid()
        {
            Tags = new AsyncObservableCollection<string>();
            Impinj.ReadDuringTime(2500);
            Tags = Impinj.GetAntennaTags(1).ToObservableCollection();

            if (Tags.Count == 0)
            {
                ShowDialogTagNumbZero();
                return null;
            }

            if (Tags.Count > 1)
            {
                ShowDialogTagNumbMore();

                return null;
            }

            return Tags.FirstOrDefault();
        }

        /// <summary>
        /// метод проверяет наличия белья в списке. если бельё не найдено то вызывает окно для добавление нового белья
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        private ClientLinenEntityViewModel CheckLinenByTag(string tag)
        {
            var clientLinen = ClientLinens?.FirstOrDefault(x => x.Tag == tag);
            //check tag in existing in list of linen, if false give option to add item
            
            if (clientLinen == null)
            {
                if (ShowDialogAddLinen())
                {
                   var finish = UpdateClientLinenEntity();
                }

                CheckLinenByTag(tag);
            }
            return clientLinen;
        }
        
        #endregion

#region Waiting ShowDialogs

        private void ShowDialogWaitingNumb()
        {
            _dispatcher.RunInMainThread(() =>
            {
                Plc1Thread.Reset();

                if (_dialogService.ShowWarnigDialog(
                    "There are more then 1 hanger in belt sorting point\n Please remove all hangers and pass again " +
                    "\n\n Press ok once all done"))
                {
                    ResetClothCount();
                }

                Plc1Thread.Set();
            });

            Plc1Thread.WaitOne();
        }

        private bool ShowDialogAddLinen()
        {
            var linenAdded = false;
            _dispatcher.RunInMainThread(() =>
            {
                RfidThread.Reset();

                if (_dialogService.ShowQuestionDialog("No linen with current Tag \n Want to add Linen?"))
                {
                    var addNew = _resolverService.Resolve<AddNewLinenViewModel>();

                    addNew.InitializeAsync();
                    var showDialog = _dialogService.ShowDialog(addNew);
                    if (!showDialog) return;

                    linenAdded = true;
                };

                RfidThread.Set();
            });
            RfidThread.WaitOne();

            return linenAdded;
        }

        private void ShowDialogTagNumbMore()
        {
            _dispatcher.RunInMainThread(() =>
            {
                RfidThread.Reset();

                _dialogService.ShowWarnigDialog("More then 1 chip in Antenna 1");

                RfidThread.Set();
            });

            RfidThread.WaitOne();
        }

        private void ShowDialogTagNumbZero()
        {
            _dispatcher.RunInMainThread(() =>
            {
                RfidThread.Reset();

                _dialogService.ShowWarnigDialog("No Tag in linen");

                RfidThread.Set();
            });

            RfidThread.WaitOne();
        }
        #endregion

#region Hanging Method

        private void SendToBelt(int beltNumb, int slotNumb)
        {
            FinsTcp belt = null;

            switch (beltNumb)
            {
                case 1:
                    belt = Belt1;
                    break;
                case 2:
                    belt = Belt2;
                    break;
            }

            Plc1.Sorting(beltNumb);

            SetHangingLinen(beltNumb, slotNumb);

            HangToBeltSlot(belt, slotNumb);
            
            HangingLinen = new ConveyorItemViewModel();
        }

        private void HangToBeltSlot(FinsTcp belt, int slotNumb)
        {

            //this.Hang_In();
            //this.Delay(100);
            //while (!this.Hang_In_State())
            //    this.Delay(500);
            //return 0;

            // Подготовка слота 
            if (belt.GetNowPoint() != slotNumb)
            {
                belt.SetNowPoint(slotNumb);
                Thread.Sleep(500);
                belt.GotoPoint();

                // ожыдание окончание подготовки слота в линии
                while (belt.DialState())
                {
                    Thread.Sleep(500);
                }
            }

            // начала загрузки в слот

            var isHangWorking = false;
            while (!isHangWorking)
            {
                belt.Hang_In();
                Thread.Sleep(500);
                isHangWorking = belt.GetClotheInHook();
            }

            // ожыдание окончание загрузки в слот
            //while (belt.GetClotheInHook())
            //{
            //    Thread.Sleep(500);
            //}
        }

        #endregion


#region Hanging Manual/Auto Mode

        private void ManualSendToBelt1()
        {
            ManualSendToBelt(1, SetBelt1SlotNumb);
        }

        private void ManualSendToBelt2()
        {
            ManualSendToBelt(1, SetBelt1SlotNumb);
        }

        private void ManualSendToBelt(int beltNumb, int slotNumb)
        {
            if (IsItemPrepared == false)
            {
                _dialogService.ShowInfoDialog("Linen is not ready");
                return;
            }

            if (IsSlotHasItem(1, SetBelt1SlotNumb))
            {
                _dialogService.ShowInfoDialog("Selected slot is not empty");

                return;
            }

            SendToBelt(beltNumb, slotNumb);
        }

        private void AutoMode()
        {
            if (IsAutoPackMode)
            {
                _dialogService.ShowWarnigDialog("Auto Packing mode is ON!!");
                return;
            }
            IsAutoMode = !IsAutoMode;
        }

        private void RunAutoMode()
        {
            while (!IsItemPrepared)
            {
                return;
            }

            //Belt 1 проверка слота
            if (!IsBeltFull(1))
            {
                var currentSlot = Belt1.GetNowPoint();

                var slotsByOrder = GetBeltEmptyItems(1).OrderByDescending(x => x.SlotNumber == currentSlot)
                    .ThenBy(x => x.SlotNumber < currentSlot).ToList();

                foreach (var beltItem in slotsByOrder)
                {
                    SendToBelt(1, beltItem.SlotNumber);
                }
            }

            //TODO: Belt 2 проверка слота
        }

        #endregion


#region Packing Methods

        private void TakeCloth(FinsTcp belt, string linenList)
        {
            // Take out clothes
            belt.TakeOutClothes(linenList);

            //Working state of the clothes taking device
            while (belt.GetTakeOutClothesState())
            {
                Thread.Sleep(1000);
            }

            //PackCloth();
            Thread.Sleep(2000);
        }

        private void PackCloth()
        {
            Plc1.Packclothes();
        }

        #endregion

#region Paking Manual/Auto Mode

        private void TakeClothBelt1()
        {
            var items = Belt1Items.Where(x => x.IsSelected).ToList();

            if (items == null || items.Count == 0)
            {
                _dialogService.ShowInfoDialog("No linen was selected");
                return;
            }

            var stringList = "";
            foreach (var item in items)
            {
                stringList += $"{item.SlotNumber}, ";
            }

            stringList = stringList.Substring(0, stringList.Length - 2);

            TakeCloth(Belt1, stringList);

            RemoveBeltItems(items);
        }

        private void TakeClothBelt2()
        {
            var items = Belt2Items.Where(x => x.IsSelected).ToList();

            if (items == null || items.Count == 0)
            {
                _dialogService.ShowInfoDialog("No linen was selected");
                return;
            }

            var stringList = "";
            foreach (var item in items)
            {
                stringList += $"{item.SlotNumber},";
            }

            TakeCloth(Belt2, stringList);

            RemoveBeltItems(items);
        }

        private void AutoPacking()
        {
            if (IsAutoMode)
            {
                _dialogService.ShowWarnigDialog("Auto Hanging Mode is ON!");
                return;
            }

            IsAutoPackMode = !IsAutoPackMode;
        }

        private void RunAutoPacking()
        {
            while (IsAutoPackMode)
            {
                var staffId = GetStaffList();

                if (staffId == null) break;


                var linens = BeltItems?.Where(x => x.StaffId == staffId).ToList();

                if(linens == null || linens.Count == 0) continue;

                var slotsBelt1 = linens.Where(x => x.BeltNumber == 1).ToList();
                var slotsBelt2 = linens.Where(x => x.BeltNumber == 2).ToList();

                //TODO: запустить два метода линии 1 и 2 в паралельных потоках

                if (slotsBelt1.Count != 0)
                {
                    var beltString = GetUniformSlots(slotsBelt1);
                    TakeCloth(Belt1, beltString);

                    RemoveBeltItems(slotsBelt1);
                }

                if (slotsBelt2.Count != 0)
                {
                    var beltString = GetUniformSlots(slotsBelt2);
                    TakeCloth(Belt1, beltString);

                    RemoveBeltItems(slotsBelt2);
                }
            }
        }

        private int? GetStaffList()
        {
            var staffId = BeltItems?.FirstOrDefault().StaffId;

            return staffId;
        }

        private string GetUniformSlots(List<ConveyorItemViewModel> linens)
        {
            var slotList = "";

            foreach (var linen in linens)
            {
                slotList += $"{linen.SlotNumber}, ";
            }

            return slotList;
        }

        private void RemoveBeltItems(List<ConveyorItemViewModel> beltItems)
        {
            foreach(var item in beltItems)
            {
                item.ClientLinen = null;
                item.ClientLinenId = null;
            }

            RaisePropertyChanged(() => Belt1Items);
            RaisePropertyChanged(() => Belt2Items);
        }

        #endregion
        
    }
}
