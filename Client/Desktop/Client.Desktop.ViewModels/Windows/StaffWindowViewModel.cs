﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Client.Desktop.ViewModels.Common.EntityViewModels;
using Client.Desktop.ViewModels.Common.Extensions;
using Client.Desktop.ViewModels.Common.Services;
using Client.Desktop.ViewModels.Common.ViewModels;
using Storage.Laundry.Models.Abstract;

namespace Client.Desktop.ViewModels.Windows
{
    public class StaffWindowViewModel : ViewModelBase
    {
        private readonly ILaundryService _laundryService;
        private readonly IDialogService _dialogService;
        private List<ClientEntityViewModel> _clients;
        private ClientEntityViewModel _selectedClient;
        private List<DepartmentEntityViewModel> _departments;
        private DepartmentEntityViewModel _selectedDepartment;
        private ObservableCollection<StaffEntityViewModel> _staff;
        private StaffEntityViewModel _selectedStaff;
        private ObservableCollection<MasterLinenEntityViewModel> _masterLinens;
        private ObservableCollection<LinenEntityViewModel> _linens;
        private LinenEntityViewModel _selectedLinen;

        public LinenEntityViewModel SelectedLinen
        {
            get => _selectedLinen;
            set => Set(() => SelectedLinen, ref _selectedLinen, value);
        }
        public ObservableCollection<LinenEntityViewModel> Linens
        {
            get => _linens;
            set => Set(() => Linens, ref _linens, value);
        }
        public ObservableCollection<MasterLinenEntityViewModel> MasterLinens
        {
            get => _masterLinens;
            set => Set(() => MasterLinens, ref _masterLinens, value);
        }
        public StaffEntityViewModel SelectedStaff
        {
            get => _selectedStaff;
            set => Set(() => SelectedStaff, ref _selectedStaff, value);
        }
        public ObservableCollection<StaffEntityViewModel> Staff
        {
            get => _staff;
            set => Set(() => Staff, ref _staff, value);
        }
        public DepartmentEntityViewModel SelectedDepartment
        {
            get => _selectedDepartment;
            set => Set(() => SelectedDepartment, ref _selectedDepartment, value);
        }
        public List<DepartmentEntityViewModel> Departments
        {
            get => _departments;
            set => Set(() => Departments, ref _departments, value);
        }
        public ClientEntityViewModel SelectedClient
        {
            get => _selectedClient;
            set => Set(() => SelectedClient, ref _selectedClient, value);
        }
        public List<ClientEntityViewModel> Clients
        {
            get => _clients;
            set => Set(() => Clients, ref _clients, value);
        }

        public ObservableCollection<DepartmentEntityViewModel> SortedDepartments =>
            Departments?.Where(x => x.ClientId == SelectedClient?.Id).ToObservableCollection();

        public ObservableCollection<StaffEntityViewModel> SortedStaff =>
            Staff?.Where(x => x.DepartmentId == SelectedDepartment?.Id).ToObservableCollection();

        public ObservableCollection<LinenEntityViewModel> SortedLinens => SortLinen();

        public RelayCommand SaveCommand { get; }
        public RelayCommand AddStaffCommand { get; }
        public RelayCommand DeleteStaffCommand { get; }
        public RelayCommand AddLinenCommand { get; }
        public RelayCommand DeleteLinenCommand { get; }



        public StaffWindowViewModel(ILaundryService dataService, IDialogService dialogService)
        {
            _laundryService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            SaveCommand = new RelayCommand(Save);
            AddStaffCommand = new RelayCommand(AddStaff, (() => SelectedDepartment != null));
            DeleteStaffCommand = new RelayCommand(DeleteStaff, (() => SelectedStaff != null));

            AddLinenCommand = new RelayCommand(AddLinen, (() => SelectedDepartment != null));
            DeleteLinenCommand = new RelayCommand(DeleteLinen, (() => SelectedLinen != null));


            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedClient))
            {
                RaisePropertyChanged(() => SortedDepartments);
            }

            if (e.PropertyName == nameof(SelectedDepartment))
            {
                RaisePropertyChanged(()=> SortedStaff);
                RaisePropertyChanged(() => SortedLinens);

                AddStaffCommand.RaiseCanExecuteChanged();
                AddLinenCommand.RaiseCanExecuteChanged();
            }

            if (e.PropertyName == nameof(SelectedStaff))
            {
                RaisePropertyChanged(() => SortedLinens);

                DeleteStaffCommand.RaiseCanExecuteChanged();
            }

            if (e.PropertyName == nameof(SelectedLinen))
            {
                DeleteLinenCommand.RaiseCanExecuteChanged();
            }

        }

        private ObservableCollection<LinenEntityViewModel> SortLinen()
        {
            var linens = new ObservableCollection<LinenEntityViewModel>();

            if (SelectedStaff == null)
            {
                if (SelectedDepartment == null) return linens;

                return Linens.Where(x => x.DepartmentId == SelectedDepartment.Id).ToObservableCollection();
            }

            linens = Linens.Where(x => x.OriginalObject.ClientStaffEntity == SelectedStaff.OriginalObject).ToObservableCollection();
            return linens;
        }

        private void Save()
        {
            if (Staff.Any(x => x.HasChanges()))
            {
                var items = Staff.Where(x => x.HasChanges()).ToList();

                foreach (var item in items)
                {
                    item.AcceptChanges();
                    SaveEntity(item.OriginalObject);
                }
            }

            if (Linens.Any(x => x.HasChanges()))
            {
                var items = Linens.Where(x => x.HasChanges()).ToList();

                foreach (var item in items)
                {
                    item.AcceptChanges();
                    SaveEntity(item.OriginalObject);
                }
            }

            _dialogService.ShowInfoDialog("All changes saved");
        }

        private void SaveEntity<T>(T entity) where T : EntityBase
        {
            _laundryService.AddOrUpdate(entity);
        }

        private void DeleteEntity<T>(T entity) where T : EntityBase
        {
            //TODO: Delete Entity
        }

        private void AddStaff()
        {
            if(SelectedDepartment == null) return;
            if (!_dialogService.ShowQuestionDialog("Do you want to add new Staff?"))return;

            var newStaff = new StaffEntityViewModel()
            {
                DepartmentId = SelectedDepartment.Id,
                PhoneNumber = "+971",
            };

            Staff.Add(newStaff);

            RaisePropertyChanged(()=> SortedStaff);
            SelectedStaff = newStaff;
        }

        private void DeleteStaff()
        {
            var staff = SelectedStaff;

            if (staff == null) return;
            if (!_dialogService.ShowQuestionDialog($"Do you want to DELETE {staff.StaffName}?")) return;

            DeleteEntity(staff.OriginalObject);
            Staff.Remove(staff);

            if (!_dialogService.ShowQuestionDialog($"Do you want to DELETE {staff.StaffName} linens also?")) return;

            var linens = Linens.Where(x => x.StaffId == staff.Id).ToList();

            foreach (var linen in linens)
            {
                DeleteEntity(linen.OriginalObject);
                Linens.Remove(linen);
            }

        }

        private void AddLinen()
        {
            if (SelectedDepartment == null) return;
            if (!_dialogService.ShowQuestionDialog("Do you want to add new Linen?")) return;

            var newLinen = new LinenEntityViewModel()
            {
                ClientId =  SelectedClient.Id,
                DepartmentId = SelectedDepartment.Id,
                MasterLinenId = 1,
                StatusId = (int) LinenStatusEnum.InUse,
            };

            if (SelectedStaff != null)
            {
                newLinen.OriginalObject.ClientStaffEntity = SelectedStaff.OriginalObject;
            }

            Linens.Add(newLinen);

            RaisePropertyChanged(()=> SortedLinens);
            SelectedLinen = newLinen;
        }

        private void DeleteLinen()
        {
            var linen = SelectedLinen;

            if (linen == null) return;
            if (!_dialogService.ShowQuestionDialog(
                $"Do you want to DELETE {MasterLinens?.FirstOrDefault(x => x.Id == SelectedLinen?.MasterLinenId)?.Name}?")
            ) return;

            DeleteEntity(linen.OriginalObject);
            Linens.Remove(linen);

            RaisePropertyChanged(()=> SortedLinens);
        }
    }
}
