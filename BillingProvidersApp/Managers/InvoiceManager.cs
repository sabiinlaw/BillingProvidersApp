using SuiteTalk;
using System;
using System.Collections;
using System.Collections.Generic;

namespace BillingProvidersApp.Managers
{
    public class InvoiceManager: NetSuiteEntityManager
    {
        public InvoiceManager(Action<string> logMessage)
    : base(RecordType.invoice, logMessage)
        {
        }

        public InvoiceManager()
    : base(RecordType.invoice, null)
        {
        }

        public static Invoice CreateInvoice(
            string entityId,
            RecordType entityType,
            DateTime startDate,
            DateTime endDate,
            DateTime transactionDate,
            PriceLevel priceLevel,
            bool toBeEMailed,
            int customFormId,
            string providerDisplayName,
            IEnumerable<PriceListItem> priceListItems)
        {
            Invoice invoice = new Invoice();

            invoice.account = Helper.CreateRecordRefWithId(Helper.GetDefaultAccountReceivableId());

            invoice.entity = new RecordRef();
            invoice.entity.type = entityType;
            invoice.entity.typeSpecified = true;
            invoice.entity.internalId = entityId;

            invoice.tranDate = transactionDate;
            invoice.tranDateSpecified = true;

            invoice.terms = Helper.CreateRecordRefWithId(Helper.GetDefaultDiscountTermId());

            invoice.startDate = startDate;
            invoice.startDateSpecified = (startDate != DateTime.MinValue);

            invoice.endDate = endDate;
            invoice.endDateSpecified = (endDate != DateTime.MinValue);

            ArrayList invoiceItems = new ArrayList();

            // automate adding upsell upsell modules to invoice
            if (priceListItems != null)
            {
                foreach (PriceListItem priceListItem in priceListItems)
                {
                    InvoiceItem invoiceItemModule = new InvoiceItem();
                    invoiceItemModule.item = Helper.CreateRecordRefWithId(priceListItem.NsInternalId);
                    if (priceListItem.Quantity.HasValue)
                    {
                        invoiceItemModule.quantity = (double)priceListItem.Quantity.Value;
                        invoiceItemModule.quantitySpecified = true;
                    }

                    if (priceListItem.Amount.HasValue)
                    {
                        invoiceItemModule.amount = (double)priceListItem.Amount.Value;
                        invoiceItemModule.amountSpecified = true;
                    }

                    if (!string.IsNullOrEmpty(priceListItem.NsPriceId))
                    {
                        invoiceItemModule.price = Helper.CreateRecordRefWithId(priceListItem.NsPriceId);
                    }

                    invoiceItemModule.description = priceListItem.ExtraDescription;

                    invoiceItems.Add(invoiceItemModule);
                }
            }

            invoice.itemList = new InvoiceItemList();
            invoice.itemList.item = (InvoiceItem[])invoiceItems.ToArray(typeof(InvoiceItem));
            invoice.customForm = Helper.CreateRecordRefWithId(customFormId.ToString());
            invoice.customFieldList = new CustomFieldRef[]
            {
                new StringCustomFieldRef()
                {
                    scriptId = Helper.GetWonInvoiceInstanceFieldId(),
                    value = providerDisplayName
                }
            };

            if (toBeEMailed)
            {
                invoice.toBeEmailedSpecified = true;
                invoice.toBeEmailed = true;
            }

            return invoice;
        }
    }
    public class PriceListItem
    {
        public string NsInternalId { get; set; }

        public decimal? Quantity { get; set; }

        public decimal? Amount { get; set; }

        public string NsPriceId { get; set; }

        public string ExtraDescription { get; set; }
    }
}
