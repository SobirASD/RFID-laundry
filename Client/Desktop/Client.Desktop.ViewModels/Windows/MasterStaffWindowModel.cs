﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using Client.Desktop.ViewModels.Common.EntityViewModels;
using Client.Desktop.ViewModels.Common.Extensions;
using Client.Desktop.ViewModels.Common.Services;
using Client.Desktop.ViewModels.Common.ViewModels;
using Client.Desktop.ViewModels.Common.Windows;
using Storage.Laundry.Models;

namespace Client.Desktop.ViewModels.Windows
{
    public class MasterStaffWindowModel : ViewModelBase, IWindowDialogViewModel
    {
        private readonly ILaundryService _laundryService;
        private readonly IDialogService _dialogService;
        private readonly IMainDispatcher _dispatcher;
        private bool _hasChanges;
        private StaffEntityViewModel _selectedStaff;
        private ObservableCollection<ClientStaffEntity> _staffs;

        public ObservableCollection<ClientStaffEntity> Staffs
        {
            get => _staffs;
            set => Set(ref _staffs, value);
        }
        public StaffEntityViewModel SelectedStaff
        {
            get => _selectedStaff;
            set => Set(ref _selectedStaff, value);
        }
        public bool HasChanges
        {
            get => _hasChanges;
            set => Set(ref _hasChanges, value);
        }

        public Action<bool> CloseAction { get; set; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand CloseCommand { get; }
        public RelayCommand InitializeCommand { get; }

        public MasterStaffWindowModel(ILaundryService laundryService, IDialogService dialogService, IMainDispatcher dispatcher)
        {
            _laundryService = laundryService ?? throw new ArgumentNullException(nameof(laundryService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

            SaveCommand = new RelayCommand(Save);
            CloseCommand = new RelayCommand(Close);
            InitializeCommand = new RelayCommand(Initialize);

        }

        public void SetSelectedStaff(ClientStaffEntity staff, DepartmentEntity department)
        {
            SelectedStaff = null;

            if (staff != null)
            {
                SelectedStaff = new StaffEntityViewModel(staff);
                return;
            }

            SelectedStaff = new StaffEntityViewModel(new ClientStaffEntity()
            {
                DepartmentId = department.Id,
            });
        }

        private async void Initialize()
        {
            _dialogService.ShowBusy();

            try
            {
                var staff = await _laundryService.GetAllAsync<ClientStaffEntity>();
                Staffs = staff.ToObservableCollection();

            }
            catch (Exception e)
            {
                _dialogService.HideBusy();
            }
            finally
            {
                _dialogService.HideBusy();
            }

            HasChanges = false;
            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }

        private void Save()
        {
            if (!SelectedStaff.IsValid)
            {
                return;
            }

            if (!SelectedStaff.HasChanges())
            {
                return;
            }

            SelectedStaff.AcceptChanges();

            _laundryService.AddOrUpdateAsync(SelectedStaff.OriginalObject);
            HasChanges = true;

            if (_dialogService.ShowQuestionDialog("Saved! \n Do you want to close window ? "))
            {
                CloseWindow();
            }
        }

        private bool CanExecuteParentIdClearCommand()
        {
            return true;
        }

        private void Close()
        {
            if (!HasChanges)
            {
                if (_dialogService.ShowQuestionDialog($"Do you want to close window ? \n \"All changes will be canceled\""))
                {
                    CloseWindow();
                }
            }
            else
            {
                CloseWindow();
            }

        }

        private void CloseWindow()
        {
            CloseAction?.Invoke(HasChanges);
        }
    }
}
