﻿using System.ComponentModel;
using GalaSoft.MvvmLight;
using PALMS.Data.Objects.ClientModel;

namespace PALMS.Settings.ViewModel.EntityViewModels
{
    public class ConveyorItemViewModel : ViewModelBase
    {
        private bool _isSelected;
        private bool _hasItem;
        private int _slotNumber;
        private int _beltNumber;
        private ConveyorItem _originalObject;
        private int _id;
        private int? _clientLinenId;
        private int? _staffId;
        private ClientLinenEntityViewModel _clientLinen;
        private string _tag;

        public string Tag
        {
            get => _tag;
            set => Set(() => Tag, ref _tag, value);
        }
        public ClientLinenEntityViewModel ClientLinen
        {
            get => _clientLinen;
            set => Set(() => ClientLinen, ref _clientLinen, value);
        }
        public int? StaffId
        {
            get => _staffId;
            set => Set(() => StaffId, ref _staffId, value);
        }
        public int? ClientLinenId
        {
            get => _clientLinenId;
            set => Set(() => ClientLinenId, ref _clientLinenId, value);
        }
        public int Id
        {
            get => _id;
            set => Set(() => Id, ref _id, value);
        }
        public ConveyorItem OriginalObject
        {
            get => _originalObject;
            set => Set(() => OriginalObject, ref _originalObject, value);
        }
        public int BeltNumber
        {
            get => _beltNumber;
            set => Set(ref _beltNumber, value);
        }
        public int SlotNumber
        {
            get => _slotNumber;
            set => Set(ref _slotNumber, value);
        }
        public bool HasItem
        {
            get => _hasItem;
            set => Set(ref _hasItem, value);
        }
        public bool IsSelected
        {
            get => _isSelected;
            set => Set(ref _isSelected, value);
        }

        public bool IsNew => OriginalObject == null || OriginalObject.IsNew;

        public ConveyorItemViewModel()
        {
            OriginalObject = new ConveyorItem();
            PropertyChanged += OnPropertyChanged;

        }

        public ConveyorItemViewModel(ConveyorItem originalObject) : this()
        {
            Update(originalObject);
        }

        public void Reset()
        {
            Update(OriginalObject);
        }

        private void Update(ConveyorItem originalObject)
        {
            OriginalObject = originalObject;

            Id = OriginalObject.Id;
            BeltNumber = OriginalObject.BeltNumber;
            SlotNumber = OriginalObject.SlotNumber;
            ClientLinenId = OriginalObject.ClientLinenId;
        }

        public void AcceptChanges()
        {
            if (OriginalObject == null) return;

            OriginalObject.ClientLinenId = ClientLinenId;
        }

        public bool HasChanges() => OriginalObject == null ||
                                    OriginalObject.IsNew ||
                                    !Equals(ClientLinenId, OriginalObject.ClientLinenId);


        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ClientLinenId))
            {
                HasItem = ClientLinenId != null;
            }

            if (e.PropertyName == nameof(ClientLinen))
            {
                if (ClientLinen == null)
                {
                    StaffId = null;
                    Tag = null;
                    ClientLinenId = null;
                }
                else
                {
                    StaffId = ClientLinen.StaffId;
                    Tag = ClientLinen.Tag;
                    ClientLinenId = ClientLinen.Id;

                }
            }

        }
    }
}
