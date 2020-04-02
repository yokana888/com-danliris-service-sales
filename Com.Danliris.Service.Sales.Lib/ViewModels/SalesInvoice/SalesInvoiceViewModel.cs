﻿using Com.Danliris.Service.Sales.Lib.Utilities;
using Com.Danliris.Service.Sales.Lib.ViewModels.IntegrationViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Com.Danliris.Service.Sales.Lib.ViewModels.SalesInvoice
{
    public class SalesInvoiceViewModel : BaseViewModel, IValidatableObject
    {
        [MaxLength(255)]
        public string Code { get; set; }
        public long AutoIncreament { get; set; }
        [MaxLength(255)]
        public string SalesInvoiceNo { get; set; }
        [MaxLength(255)]
        public string SalesInvoiceType { get; set; }
        public DateTimeOffset? SalesInvoiceDate { get; set; }
        public DateTimeOffset? DueDate { get; set; }
        [MaxLength(255)]
        public string DeliveryOrderNo { get; set; }
        //[MaxLength(255)]
        //public string DebtorIndexNo { get; set; }
        public BuyerViewModel Buyer { get; set; }
        [MaxLength(255)]
        public string IDNo { get; set; }
        public CurrencyViewModel Currency { get; set; }
        [MaxLength(255)]
        public string VatType { get; set; }
        public double TotalPayment { get; set; }
        public double TotalPaid { get; set; }
        public bool IsPaidOff { get; set; }
        [MaxLength(1000)]
        public string Remark { get; set; }


        public ICollection<SalesInvoiceDetailViewModel> SalesInvoiceDetails { get; set; }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(SalesInvoiceType))
                yield return new ValidationResult("Kode Faktur Penjualan harus diisi", new List<string> { "SalesInvoiceType" });

            if (!SalesInvoiceDate.HasValue || SalesInvoiceDate.Value > DateTimeOffset.Now)
                yield return new ValidationResult("Tgl Faktur Penjualan harus diisi & lebih kecil  atau sama dengan hari ini", new List<string> { "SalesInvoiceDate" });

            if (string.IsNullOrWhiteSpace(DeliveryOrderNo))
                yield return new ValidationResult("No. Surat Jalan harus diisi", new List<string> { "DeliveryOrderNo" });

            //if (string.IsNullOrWhiteSpace(DebtorIndexNo))
            //    yield return new ValidationResult("No. Index Debitur harus diisi", new List<string> { "DebtorIndexNo" });

            if (string.IsNullOrWhiteSpace(Currency.Code))
                yield return new ValidationResult("Kurs harus diisi", new List<string> { "CurrencyCode" });

            if (!DueDate.HasValue || Id == 0 && DueDate.Value < DateTimeOffset.Now.AddDays(-1))
                yield return new ValidationResult("Tanggal jatuh tempo harus diisi & lebih besar dari hari ini", new List<string> { "DueDate" });
            
            if (string.IsNullOrWhiteSpace(VatType))
                yield return new ValidationResult("Jenis PPN harus diisi", new List<string> { "VatType" });

            if (TotalPayment <= 0)
                yield return new ValidationResult("Total termasuk PPN kosong", new List<string> { "TotalPayment" });

            if (TotalPaid < 0)
                yield return new ValidationResult("Total Paid harus lebih besar atau sama dengan 0", new List<string> { "TotalPayment" });

            int Count = 0;
            string DetailErrors = "[";

            if (SalesInvoiceDetails != null && SalesInvoiceDetails.Count > 0)
            {
                foreach (SalesInvoiceDetailViewModel detail in SalesInvoiceDetails)
                {
                    DetailErrors += "{";

                    var rowErrorCount = 0;

                    if (string.IsNullOrWhiteSpace(detail.ShipmentDocumentCode))
                    {
                        Count++;
                        rowErrorCount++;
                        DetailErrors += "UomUnit : 'Satuan harus diisi',";
                    }

                    foreach (SalesInvoiceItemViewModel item in detail.SalesInvoiceItems)
                    {

                        if (string.IsNullOrWhiteSpace(item.ProductCode))
                        {
                            Count++;
                            rowErrorCount++;
                            DetailErrors += "ProductCode : 'Kode harus diisi',";
                        }
                        if (string.IsNullOrWhiteSpace(item.ProductName))
                        {
                            Count++;
                            rowErrorCount++;
                            DetailErrors += "ProductName : 'Kode harus diisi',";
                        }
                        if (string.IsNullOrWhiteSpace(item.Quantity))
                        {
                            Count++;
                            rowErrorCount++;
                            DetailErrors += "Quantity : 'Kuantitas harus diisi',";
                        }
                        if (!item.Total.HasValue || item.Total.Value <= 0)
                        {
                            Count++;
                            rowErrorCount++;
                            DetailErrors += "Total : 'Jumlah harus lebih besar dari 0',";
                        }
                        if (string.IsNullOrWhiteSpace(item.Uom.Unit))
                        {
                            Count++;
                            rowErrorCount++;
                            DetailErrors += "UomUnit : 'Satuan harus diisi',";
                        }
                        if (!item.Price.HasValue || item.Price.Value <= 0)
                        {
                            Count++;
                            rowErrorCount++;
                            DetailErrors += "Price : 'Harga barang harus lebih besar dari 0',";
                        }
                        if (item.Amount <= 0)
                        {
                            Count++;
                            rowErrorCount++;
                            DetailErrors += "Amount : 'Jumlah & Harga Satuan harus diisi',";
                        }

                        if (rowErrorCount == 0)
                        {
                            var duplicateDetails = detail.SalesInvoiceItems.Where(f =>
                                    f.ProductCode.Equals(item.ProductCode) &&
                                    f.ProductName.Equals(item.ProductName) &&
                                    f.Price.GetValueOrDefault().Equals(item.Price.GetValueOrDefault()) &&
                                    f.Uom.Id.GetValueOrDefault().Equals(item.Uom.Id.GetValueOrDefault())
                                ).ToList();

                            if (duplicateDetails.Count > 1)
                            {
                                Count++;
                                DetailErrors += "ProductCode : 'Kode, Nama Barang, dan Harga Satuan tidak boleh duplikat',";
                                DetailErrors += "ProductName : 'Kode, Nama Barang, dan Harga Satuan tidak boleh duplikat',";
                                DetailErrors += "Price : 'Kode, Nama Barang, dan Harga Satuan tidak boleh duplikat',";
                            }
                        }
                    }
                    DetailErrors += "}, ";
                }
            }
            else
            {
                yield return new ValidationResult("Detail harus diisi", new List<string> { "SalesInvoiceDetail" });
            }

            DetailErrors += "]";

            if (Count > 0)
                yield return new ValidationResult(DetailErrors, new List<string> { "SalesInvoiceDetails" });

        }
    }
}
