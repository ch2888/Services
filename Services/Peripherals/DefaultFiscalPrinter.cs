/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/


using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.SqlClient;
using Microsoft.Dynamics.Retail.Pos.Contracts;
using Microsoft.Dynamics.Retail.Pos.Contracts.BusinessObjects;
using Microsoft.Dynamics.Retail.Pos.Contracts.DataEntity;
using Microsoft.Dynamics.Retail.Pos.Contracts.Services;
using Microsoft.Dynamics.Retail.Pos.Contracts.Triggers;

namespace Microsoft.Dynamics.Retail.FiscalPrinter
{
    /// <summary>
    /// This is the default implementation for a FiscalPrinter.  This implementation
    /// will be used when a store is in a country that doesnt require fiscal printers.
    /// 
    /// The FiscalPrinterEnabled() method always returns FALSE and 
    /// any other method will throw an NotSupportedException.
    /// </summary>
    [Export("DEFAULT", typeof(IFiscalPrinter))]
    public sealed class DefaultFiscalPrinter : IFiscalPrinter
    {
        public string DeviceName
        {
            get;
            set;
        }

        public string DeviceDescription
        {
            get;
            set;
        }

        public bool IsActive
        {
            get;
            set;
        }

        public bool Initialized
        {
            get;
            private set;
        }

        public bool IsPrintingCommandSent
        {
            get;
            set;
        }

        public bool CanBeInitialized()
        {            
            return false;
        }

        /// <summary>
        /// Verifies if the fiscal printer extension is enabled 
        /// for the current store.  By default this method returns 
        /// FALSE, therefore Fiscal Printer implementations must
        /// override this method and return TRUE.
        /// </summary>
        /// <returns>Returns true if there is a fiscal printer implementation that 
        /// must be called by the extension libraries; otherwise retuns FALSE.</returns>
        public bool FiscalPrinterEnabled()
        {
            return false;
        }

        /// <summary>
        /// Verifies if there is EFT service integrated
        /// </summary>
        public bool IsThirdPartyCardPaymentEnabled()
        {
            return false;
        }

        /// <summary>
        /// Tries to initialize the class instance.
        /// </summary>
        /// <remarks>Used when creating the instance, so do not throw NotSupportedException.</remarks>
        public void Initialize()
        {
            Initialized = true;
        }

        /// <summary>
        /// Issues a gift card given the current transaction.
        /// </summary>
        /// <param name="posTransaction">Pos transaction</param>
        /// <param name="gcTenderInfo">Tender info</param>
        public void IssueGiftCard(IPosTransaction posTransaction, ITender gcTenderInfo)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Add to gift card given the current transaction.
        /// </summary>
        /// <param name="retailTransaction">Retail transaction</param>
        /// <param name="gcTenderInfo">Tender info</param>
        public void AddToGiftCard(IRetailTransaction retailTransaction, ITender gcTenderInfo)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Processes the card payment for the EFT provider.
        /// </summary>
        /// <param name="info">EFT info</param>
        /// <param name="transaction">Retail transaction</param>
        public void ProcessCardPayment(IEFTInfo info, IRetailTransaction transaction)
        {
            throw new NotSupportedException();
        }

        /// <summary>        
        /// Gets maximum number of symbols on paper for the specific <c>IFiscalPrinterFontStyle</c>.
        /// </summary>
        /// <param name="fontStyle">Font style.</param>        
        public int GetLineLegth(IFiscalPrinterFontStyle fontStyle)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Print custom document.
        /// </summary>
        /// <param name="layoutId">layoutid parameter value.</param>
        /// <param name="textToPrint">List of text blocks with format style to print.</param>
        /// <param name="posTransaction">POS transaction.</param>
        public void PrintCustomDocument(string layoutId, IList<IFiscalPrinterTextData> textToPrint, IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Creates font style based on fontStyle and partner data.
        /// </summary>
        /// <param name="fontStyleType">Style of the text.</param>
        /// <param name="partnerData">Dynamic object that hold partner's data.</param>
        /// <returns>return the object created</returns>
        public IFiscalPrinterFontStyle CreateFiscalPrinterFontStyle(FontStyleType fontStyleType, dynamic partnerData)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Creates font style with text based on text, fontStyle and partner data.
        /// </summary>
        /// <param name="text">Text to print.</param>
        /// <param name="fontStyleType">Style of the text.</param>
        /// <param name="partnerData">Dynamic object that hold partner's data.</param>
        /// <returns>return the object created.</returns>
        public IFiscalPrinterTextData CreateFiscalPrinterTextData(string text, FontStyleType fontStyleType, dynamic partnerData)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Creates font style with text based on text and font style.
        /// </summary>
        /// <param name="text">Text to print.</param>
        /// <param name="fontStyle">Style of the font.</param>        
        /// <returns>return the object created.</returns>
        public IFiscalPrinterTextData CreateFiscalPrinterTextData(string text, IFiscalPrinterFontStyle fontStyle)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Tries to initialize the printer driver, connect to the 
        /// serial port, verifies the printer state, paper status 
        /// and recover any pending transaction
        /// </summary>
        public void InitializePrinter()
        {
        }

        /// <summary>
        /// Denotes whether the fiscal printer supports simultaneous printing on normal printers.
        /// </summary>
        public bool SupportNormalPrinters 
        {
            get { return false; }
        }

        #region IApplicationTriggers

        /// <summary>
        /// Triggers once, whenever the application starts.
        /// </summary>
        public void ApplicationStart()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Triggers after the login operation has been executed.
        /// </summary>
        public void PostLogOn(bool logOnSuccessful, string operatorId, string name)
        {
            throw new NotSupportedException();
        }
        #endregion

        #region IApplication Members

        /// <summary>
        /// Retrieves the next receipt ID to use in the fiscal coupon.
        /// </summary>
        /// <param name="transaction">The PosTransaction that sets the context.</param>
        /// <returns>The string representing the receipt ID.</returns>
        /// <remarks>It's retrieved from the fiscal printer, except when it's a Fiscal document model 2 that follows it's own rule.</remarks>
        public string GetNextReceiptId(IPosTransaction transaction)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Determines whether the fiscal printer implements numbering function
        /// </summary>
        /// <returns>Returns true if the numbering function is implemented</returns>
        public bool HasReceiptIdNumbering()
        {
            return false;
        }

        /// <summary>
        /// Checks if Fiscal printer shift id equals to POS one
        /// </summary>
        /// <param name="transaction">Retail transaction</param>
        /// <returns>Returns true if the fiscal printer shift id equals to POS shift id</returns>
        public bool? HasSameShiftId(IRetailTransaction transaction)
        {
            throw new NotSupportedException();
        }

        #endregion IApplication Members

        #region IBlankOperations Members

        /// <summary>
        /// Displays an alert message according operation id passed.
        /// </summary>
        /// <param name="operationInfo"></param>
        /// <param name="posTransaction"></param>        
        public void BlankOperations(IBlankOperationInfo operationInfo, IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }
        #endregion

        #region Implementation of ICashDrawer

        /// <summary>
        /// Open the cash drawer.
        /// </summary>
        public void OpenDrawer()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Check if cash drawer is open.
        /// </summary>
        /// <returns>True if open, false otherwise.</returns>
        public bool DrawerOpen()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Check if the cash drawer is capable of reporting back whether it's closed or open.
        /// </summary>
        /// <returns>True if capable, false otherwise.</returns>
        public bool CapStatus()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region ICustomerTriggers Members

        /// <summary>
        /// Triggered prior to clearing a customer from the transaction
        /// </summary>
        /// <param name="preTriggerResult"></param>
        /// <param name="posTransaction"></param>
        public void PreCustomerClear(IPreTriggerResult preTriggerResult, IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Triggered prior to adding a customer to the transaction
        /// </summary>
        /// <param name="preTriggerResult"></param>
        /// <param name="posTransaction"></param>
        public void PreCustomer(IPreTriggerResult preTriggerResult, IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Triggered prior to adding a customer to the transaction using Customer search
        /// </summary>
        /// <param name="preTriggerResult"></param>
        /// <param name="posTransaction"></param>
        public void PreCustomerSearch(IPreTriggerResult preTriggerResult, IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Triggered prior to setting a customer to the transaction after it has been set
        /// </summary>
        /// <param name="preTriggerResult"></param>
        /// <param name="posTransaction"></param>
        /// <param name="customerId"></param>
        public void PreCustomerSet(IPreTriggerResult preTriggerResult, IPosTransaction posTransaction, string customerId)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IDiscountTriggers Members

        /// <summary>
        /// Triggered prior to setting a line discount amount to the transaction.
        /// </summary>
        /// <param name="preTriggerResult"></param>
        /// <param name="transaction"></param>
        /// <param name="lineId"></param>
        public void PreLineDiscountAmount(IPreTriggerResult preTriggerResult, IPosTransaction transaction, int lineId)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Triggered prior to setting a line discount percent to the transaction.
        /// </summary>
        /// <param name="preTriggerResult"></param>
        /// <param name="transaction"></param>
        /// <param name="lineId"></param>
        public void PreLineDiscountPercent(IPreTriggerResult preTriggerResult, IPosTransaction transaction, int lineId)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Triggered after setting a line discount amount to the transaction.
        /// </summary>
        /// <param name="posTransaction"></param>
        /// <param name="lineId"></param>
        public void PostLineDiscountAmount(IPosTransaction posTransaction, int lineId)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Triggered after setting a line discount percent to the transaction.
        /// </summary>
        /// <param name="posTransaction"></param>
        /// <param name="lineId"></param>
        public void PostLineDiscountPercent(IPosTransaction posTransaction, int lineId)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Triggered prior to setting a total discount amount to the transaction.
        /// </summary>
        /// <param name="preTriggerResult"></param>
        /// <param name="posTransaction"></param>
        public void PreTotalDiscountAmount(IPreTriggerResult preTriggerResult, IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Triggered prior to setting a total discount percent to the transaction.
        /// </summary>
        /// <param name="preTriggerResult"></param>
        /// <param name="posTransaction"></param>
        public void PreTotalDiscountPercent(IPreTriggerResult preTriggerResult, IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Triggered after setting a total discount amount to the transaction.
        /// </summary>
        /// <param name="posTransaction"></param>
        public void PostTotalDiscountAmount(IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Triggered after setting a total discount percent to the transaction.
        /// </summary>
        /// <param name="posTransaction"></param>
        public void PostTotalDiscountPercent(IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEOD Members
        /// <summary>
        /// Print Report for currently opend batch (X-Report)
        /// </summary>
        /// <param name="transaction"></param>
        public void PrintXReport(IPosTransaction transaction)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Print recently closed batch report (Z-Report)
        /// </summary>
        /// <param name="transaction"></param>.
        public void PrintZReport(IPosTransaction transaction)
        {
            throw new NotSupportedException();
        }
        #endregion

        #region IPrinter Members

        /// <summary>
        /// Load the device.
        /// </summary>
        public void Load()
        {
            throw new NotSupportedException();
        }

        public void Unload()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Print text on the POS printer.
        /// </summary>
        /// <param name="text"></param>
        public void PrintReceipt(string text)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Prints a slip containing the text in the textToPrint parameter
        /// </summary>
        /// <param name="header"></param>
        /// <param name="details"></param>
        /// <param name="footer"></param>
        public void PrintSlip(string header, string details, string footer)
        {
            throw new NotSupportedException();
        }

        /// <summary>Determines whether the fiscal printer supports printing receipt in non fiscal mode.</summary>
        /// <param name="copyReceipt">Denotes that this is a copy of a receipt; optional, false by default.</param>
        /// <returns>True if the fiscal printer supports printing receipt in non fiscal mode; false otherwise.</returns>
        public bool SupportPrintingReceiptInNonFiscalMode(bool copyReceipt)
        {
            return true;
        }

        /// <summary> Determines whether the fiscal printer prohibits printing receipt on non fiscal printers. </summary>
        /// <param name="copyReceipt">Denotes that this is a copy of a receipt; optional, false by default. </param>
        /// <returns>True if the fiscal printer prohibits printing receipt on non fiscal printers; false otherwise. </returns>
        public bool ProhibitPrintingReceiptOnNonFiscalPrinters(bool copyReceipt)
        {
            return false;
        }

        #endregion

        #region IPrinting Members

        /// <summary>
        /// Returns true if print preview ids shown.
        /// </summary>
        /// <param name="formType"></param>
        /// <param name="posTransaction"></param>
        /// <returns></returns>
        public bool ShowPrintPreview(FormType formType, IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Print the standard slip, returns false if printing should be aborted altogether.
        /// </summary>
        /// <param name="formType"></param>
        /// <param name="posTransaction"></param>
        /// <param name="copyReceipt"></param>
        /// <returns></returns>
        public bool PrintReceipt(FormType formType, IPosTransaction posTransaction, bool copyReceipt)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Print Float Entry Receipt
        /// </summary>
        /// <param name="posTransaction">FloatEntryTransaction</param>
        public void PrintFloatEntry(IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Print Tender Removal
        /// </summary>
        /// <param name="posTransaction">RemoveTenderTransaction</param>
        public void PrintRemoveTender(IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Print invoice receipt.
        /// </summary>
        /// <param name="posTransaction"></param>
        /// <param name="copyInvoice"></param>
        /// <param name="printPreview"></param>
        /// <returns></returns>
        public void PrintInvoice(IPosTransaction posTransaction, bool copyInvoice, bool printPreview)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Print Bank drop Receipt
        /// </summary>
        /// <param name="posTransaction">BankDropTransaction</param>
        public void PrintBankDrop(IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Print safe drop Receipt
        /// </summary>
        /// <param name="posTransaction">SafeDropTransaction</param>
        public void PrintSafeDrop(IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Print Starting Amount Declaration Receipt
        /// </summary>
        /// <param name="posTransaction">StartingAmountTransaction</param>
        public void PrintStartingAmount(IPosTransaction posTransaction)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Prints the loyalty card balance.
        /// </summary>
        /// <param name="loyaltyCardData">The loyaltyCardData object containing the information about the loyalty card.</param>
        public void PrintLoyaltyCardBalance(ILoyaltyCardData loyaltyCardData)
        {
            throw new NotImplementedException();
        }

        /// <summary>        
        /// Prints gift cards balance in sale/return transaction.
        /// </summary>
        /// <param name="giftCardsList">The giftCardsList object containing the information the gift cards.</param>
        /// <param name="transaction">Retail transaction</param>        
        public void PrintGiftCardsBalance(IList<IGiftCardLineItem> giftCardsList, IRetailTransaction transaction)
        {
            throw new NotImplementedException();
        } 

        #endregion

        #region IItemTriggers Members

        /// <summary>
        /// Triggered prior to adding a sale line item to the transaction.
        /// </summary>
        /// <param name="preTriggerResult"></param>
        /// <param name="saleLineItem"></param>
        /// <param name="posTransaction"></param>
        public void PreSale(IPreTriggerResult preTriggerResult,
                                     ISaleLineItem saleLineItem,
                                     IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Triggered after adding a sale line item to the transaction.
        /// </summary>
        /// <param name="posTransaction"></param>
        public void PostSale(IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Triggered prior to returning a sale line item to the transaction.
        /// </summary>
        /// <param name="preTriggerResult"></param>
        /// <param name="posTransaction"></param>
        public void PreReturnItem(IPreTriggerResult preTriggerResult, IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Triggered prior to voiding a sale line item at the transaction.
        /// </summary>
        /// <param name="preTriggerResult"></param>
        /// <param name="posTransaction"></param>
        /// <param name="lineId"></param>
        public void PreVoidItem(IPreTriggerResult preTriggerResult, IPosTransaction posTransaction,
                                int lineId)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Triggered after voiding a sale line item at the transaction.
        /// </summary>
        /// <param name="posTransaction"></param>
        /// <param name="lineId"></param>
        public void PostVoidItem(IPosTransaction posTransaction, int lineId)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Triggered prior to setting the quantity of a sale line item at the transaction.
        /// </summary>
        /// <param name="preTriggerResult"></param>
        /// <param name="saleLineItem"></param>
        /// <param name="posTransaction"></param>
        /// <param name="lineId"></param>
        public void PreSetQty(IPreTriggerResult preTriggerResult,
                                       ISaleLineItem saleLineItem,
                                       IPosTransaction posTransaction, int lineId)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Triggered after setting the quantity of a sale line item at the transaction.
        /// </summary>
        /// <param name="posTransaction"></param>
        /// <param name="saleLineItem"></param>
        public void PostSetQty(IPosTransaction posTransaction, ISaleLineItem saleLineItem)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Triggered prior to clearing the quantity of a sale line item at the transaction.
        /// </summary>
        /// <param name="preTriggerResult"></param>
        /// <param name="saleLineItem"></param>
        /// <param name="posTransaction"></param>
        /// <param name="lineId"></param>
        public void PreClearQty(IPreTriggerResult preTriggerResult, ISaleLineItem saleLineItem, IPosTransaction posTransaction, int lineId)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Triggered after clearing the quantity of a sale line item at the transaction.
        /// </summary>
        /// <param name="posTransaction"></param>
        /// <param name="saleLineItem"></param>
        public void PostClearQty(IPosTransaction posTransaction, ISaleLineItem saleLineItem)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Triggered prior to overriding the price of a sale line item at the transaction.
        /// </summary>
        /// <param name="preTriggerResult"></param>
        /// <param name="saleLineItem"></param>
        /// <param name="posTransaction"></param>
        /// <param name="lineId"></param>
        public void PrePriceOverride(IPreTriggerResult preTriggerResult,
                                              ISaleLineItem saleLineItem,
                                              IPosTransaction posTransaction, int lineId)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IPaymentTriggers Members

        /// <summary>
        /// Triggered after a payment.
        /// </summary>
        /// <param name="posTransaction"></param>
        public void OnPayment(IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Triggered prior to a payment.
        /// </summary>
        /// <param name="preTriggerResult"></param>
        /// <param name="posTransaction"></param>
        /// <param name="posOperation"></param>
        /// <param name="tenderId"></param>
        public void PrePayment(IPreTriggerResult preTriggerResult, IPosTransaction posTransaction, object posOperation, string tenderId)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Triggered after voiding of a payment.
        /// </summary>
        /// <param name="posTransaction"></param>
        /// <param name="lineId"> </param>
        public void PostVoidPayment(IPosTransaction posTransaction, int lineId)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Triggered before voiding of a payment.
        /// </summary>
        /// <param name="preTriggerResult"></param>
        /// <param name="posTransaction"></param>
        /// <param name="lineId"> </param>
        public void PreVoidPayment(IPreTriggerResult preTriggerResult, IPosTransaction posTransaction, int lineId)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Triggered before pay card authorization.
        /// </summary>
        /// <param name="preTriggerResult">The pre trigger result class.</param>
        /// <param name="posTransaction">The pos transaction.</param>
        /// <param name="cardInfo">The card info.</param>
        /// <param name="amount">The amount.</param>
        public void PrePayCardAuthorization(IPreTriggerResult preTriggerResult, IPosTransaction posTransaction, ICardInfo cardInfo,
            decimal amount)
        {
            //
            //Left empty on purpose
            //
        }

        #endregion

        #region ITransactionTriggers Members

        /// <summary>
        /// Triggered at the start of a new transaction, but after loading the transaction with initialisation 
        /// data, such as the store, terminal number, date, etc...
        /// </summary>
        /// <param name="posTransaction"></param>
        public void BeginTransaction(IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Triggered at the end a transaction, before saving the transaction and printing of receipts
        /// </summary>
        /// <param name="preTriggerResult"> </param>
        /// <param name="posTransaction"></param>
        public void PreEndTransaction(IPreTriggerResult preTriggerResult, IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Triggered at the end a transaction, after saving the transaction and printing of receipts
        /// </summary>
        /// <param name="posTransaction"></param>
        public void PostEndTransaction(IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Triggered prior to voiding a transaction.
        /// </summary>
        /// <param name="preTriggerResult"></param>
        /// <param name="posTransaction"></param>
        public void PreVoidTransaction(IPreTriggerResult preTriggerResult, IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Triggered after voiding a transaction.
        /// </summary>
        /// <param name="posTransaction"></param>
        public void PostVoidTransaction(IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Triggered prior to returning.
        /// </summary>
        /// <param name="preTriggerResult"></param>
        /// <param name="originalTransaction"></param>
        /// <param name="posTransaction"></param>
        public void PreReturnTransaction(IPreTriggerResult preTriggerResult, IRetailTransaction originalTransaction, IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Triggered during save of a transaction to database.
        /// </summary>
        /// <param name="posTransaction"></param>
        /// <param name="sqlTransaction"></param>
        public void SaveTransaction(IPosTransaction posTransaction, SqlTransaction sqlTransaction)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IDiscount implementation
        /// <summary>
        /// Returns true if total discount amount is authorized.
        /// </summary>
        /// <param name="retail"></param>
        /// <param name="amountValue"></param>
        /// <param name="maxAmountValue"></param>
        /// <returns></returns>
        public bool AuthorizeTotalDiscountAmount(IRetailTransaction retail, decimal amountValue, decimal maxAmountValue)
        {
            return true;
        }

        /// <summary>
        /// Returns true if total discount percent is authorized.
        /// </summary>
        /// <param name="retail"></param>
        /// <param name="percentValue"></param>
        /// <param name="maxPercentValue"></param>
        /// <returns></returns>
        public bool AuthorizeTotalDiscountPercent(IRetailTransaction retail, decimal percentValue, decimal maxPercentValue)
        {
            return true;
        }
        /// <summary>
        /// Returns true if discount amount is authorized.
        /// </summary>
        /// <param name="lineItem"></param>
        /// <param name="discountItem"></param>
        /// <param name="maximumDiscountAmt"></param>
        /// <returns></returns>
        public bool AuthorizeLineDiscountAmount(ISaleLineItem lineItem, ILineDiscountItem discountItem, decimal maximumDiscountAmt)
        {
            return true;
        }

        /// <summary>
        /// Returns true if discount percent is correct.
        /// </summary>
        /// <param name="lineItem"></param>
        /// <param name="discountItem"></param>
        /// <param name="maximumDiscountPct"></param>
        /// <returns></returns>
        public bool AuthorizeLineDiscountPercent(ISaleLineItem lineItem, ILineDiscountItem discountItem, decimal maximumDiscountPct)
        {
            return true;
        }

        /// <summary>
        /// Before the operation is processed this trigger is called.
        /// </summary>
        /// <param name="preTriggerResult"></param>
        /// <param name="posTransaction"></param>
        /// <param name="posisOperation"></param>
        public void PreProcessOperation(IPreTriggerResult preTriggerResult, IPosTransaction posTransaction, PosisOperations posisOperation)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// After the operation has been processed this trigger is called.
        /// </summary>
        /// <param name="posTransaction"></param>
        /// <param name="posisOperation"></param>
        public void PostProcessOperation(IPosTransaction posTransaction, PosisOperations posisOperation)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Suspend Triggers

        public void PreSuspendTransaction(IPreTriggerResult preTriggerResult, IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }

        public void PostSuspendTransaction(IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }

        public void PreRecallTransaction(IPreTriggerResult preTriggerResult, IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }

        public void PostRecallTransaction(IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }

        public void OnSuspendTransaction(IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }

        public void OnRecallTransaction(IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }

        public void PreCancelTransaction(IPreTriggerResult preTriggerResult, IRetailTransaction originalTransaction, IPosTransaction posTransaction)
        {
            throw new NotSupportedException();
        }

        public void PreConfirmReturnTransaction(IPreTriggerResult preTriggerResult, IRetailTransaction originalTransaction)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
