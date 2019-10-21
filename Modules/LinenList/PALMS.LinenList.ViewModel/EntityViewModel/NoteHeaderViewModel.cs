﻿using System;
using System.ComponentModel;
using GalaSoft.MvvmLight;
using PALMS.Data.Objects.NoteModel;

namespace PALMS.LinenList.ViewModel.EntityViewModel
{
    public class NoteHeaderViewModel : ViewModelBase, IDataErrorInfo
    {
        private int _id;
        private int _clientId;
        private int _departmentId;
        private DateTime _collectionDate;
        private DateTime _deliveryDate;
        private DateTime _clientReceivedDate;
        private int _deliveryTypeId;
        private string _comment;
        private double _expressCharge;
        private double _collectionWeight;
        private double _deliveryWeight;
        private int? _invoiceId;
        private double _weightPrice;
        private string _name;
        private int? _noteStatus;
        private int _colorStatus;

        public string DepartmentName { get; set; }
        public bool IsSelected { get; set; }

        public int ColorStatus
        {
            get => _colorStatus;
            set => Set(ref _colorStatus, value);
        }
        public int? NoteStatus
        {
            get => _noteStatus;
            set => Set(ref _noteStatus, value);
        }
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }
        public double WeightPrice
        {
            get => _weightPrice;
            set => Set(ref _weightPrice, value);
        }
        public int Id
        {
            get => _id;
            set => Set(ref _id, value);
        }
        public int ClientId
        {
            get => _clientId;
            set => Set(ref _clientId, value);
        }
        public int DepartmentId
        {
            get => _departmentId;
            set => Set(ref _departmentId, value);
        }
        public DateTime CollectionDate
        {
            get => _collectionDate;
            set => Set(ref _collectionDate, value);
        }
        public DateTime DeliveryDate
        {
            get => _deliveryDate;
            set => Set(ref _deliveryDate, value);
        }
        public DateTime ClientReceivedDate
        {
            get => _clientReceivedDate;
            set => Set(ref _clientReceivedDate, value);
        }
        public int DeliveryTypeId
        {
            get => _deliveryTypeId;
            set => Set(ref _deliveryTypeId, value);
        }
        public string Comment
        {
            get => _comment;
            set => Set(ref _comment, value);
        }
        public double ExpressCharge
        {
            get => _expressCharge;
            set => Set(ref _expressCharge, value);
        }
        public double CollectionWeight
        {
            get => _collectionWeight;
            set => Set(ref _collectionWeight, value);
        }

        public double DeliveryWeight
        {
            get => _deliveryWeight;
            set => Set(ref _deliveryWeight, value);
        }

        public int? InvoiceId
        {
            get => _invoiceId;
            set => Set(ref _invoiceId, value);
        }
        public NoteHeader OriginalObject { get; set; }
        public string Error { get; }

        public bool IsNew => OriginalObject == null || OriginalObject.IsNew;

        public NoteHeaderViewModel()
        {
            OriginalObject = new NoteHeader();
        }

        public NoteHeaderViewModel(NoteHeader originalObject) : this()
        {
            Update(originalObject);
        }

        public void SetName()
        {
            if (OriginalObject.Id == 0) return;

            Name = $"PAL {OriginalObject.Id:D6}/{DateTime.Now:yy}";
        }

        public void Reset()
        {
            Update(OriginalObject);
        }

        private void Update(NoteHeader noteHeader)
        {
            OriginalObject = noteHeader;

            Id = OriginalObject.Id;
            Name = OriginalObject.Name;
            ClientId = OriginalObject.ClientId;
            DepartmentId = OriginalObject.DepartmentId;
            CollectionDate = OriginalObject.CollectionDate;
            DeliveryDate = OriginalObject.DeliveredDate;
            ClientReceivedDate = OriginalObject.ClientReceivedDate;
            DeliveryTypeId = OriginalObject.DeliveryTypeId;
            Comment = OriginalObject.Comment;
            ExpressCharge = OriginalObject.ExpressCharge;
            CollectionWeight = OriginalObject.CollectionWeight;
            DeliveryWeight = OriginalObject.DeliveryWeight;
            InvoiceId = OriginalObject.InvoiceId;
            WeightPrice = OriginalObject.WeightPrice;
            NoteStatus = OriginalObject.NoteStatus;
        }

        public void AcceptChanges()
        {
            if (OriginalObject == null) return;

            OriginalObject.ClientId = ClientId;
            OriginalObject.DepartmentId = DepartmentId;
            OriginalObject.CollectionDate = CollectionDate;
            OriginalObject.ClientReceivedDate = ClientReceivedDate;
            OriginalObject.DeliveryTypeId = DeliveryTypeId;
            OriginalObject.Comment = Comment;
            OriginalObject.ExpressCharge = ExpressCharge;
            OriginalObject.CollectionWeight = CollectionWeight;
            OriginalObject.DeliveryWeight = DeliveryWeight;
            OriginalObject.InvoiceId = InvoiceId;
            OriginalObject.WeightPrice = WeightPrice;
            OriginalObject.Name = Name;
            OriginalObject.NoteStatus = NoteStatus;

            //if (DeliveryComment != null)
            //{
            //    OriginalObject.Comment += $"{Environment.NewLine}{Environment.NewLine}{DeliveryComment}";
            //}
            OriginalObject.DeliveredDate = DeliveryDate;
        }

        public bool HasChanges() => OriginalObject == null ||
                                    OriginalObject.IsNew ||
                                    ClientId != OriginalObject.ClientId ||
                                    DepartmentId != OriginalObject.DepartmentId ||
                                    CollectionDate != OriginalObject.CollectionDate ||
                                    DeliveryDate != OriginalObject.DeliveredDate ||
                                    ClientReceivedDate != OriginalObject.ClientReceivedDate ||
                                    DeliveryTypeId != OriginalObject.DeliveryTypeId ||
                                    Comment != OriginalObject.Comment ||
                                    Equals(CollectionWeight , OriginalObject.CollectionWeight) ||
                                    Equals (DeliveryWeight , OriginalObject.DeliveryWeight) ||
                                    InvoiceId != OriginalObject.InvoiceId ||
                                    NoteStatus != OriginalObject.NoteStatus ||
                                    !Equals(WeightPrice, OriginalObject.WeightPrice) ||
                                    Name != OriginalObject.Name;
              
        public string this[string columnName] => Validate(columnName);

        private string Validate(string columnName)
        {

            return null;
        }
    }
}
