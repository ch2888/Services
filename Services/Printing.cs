
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
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using LSRetailPosis;
using LSRetailPosis.POSProcesses;
using LSRetailPosis.Settings;
using LSRetailPosis.Settings.FunctionalityProfiles;
using LSRetailPosis.Settings.HardwareProfiles;
using LSRetailPosis.Transaction;
using LSRetailPosis.Transaction.Line.TenderItem;
using Microsoft.Dynamics.Retail.Diagnostics;
using Microsoft.Dynamics.Retail.Pos.Contracts;
using Microsoft.Dynamics.Retail.Pos.Contracts.DataEntity;
using Microsoft.Dynamics.Retail.Pos.Contracts.Services;
using Microsoft.Dynamics.Retail.Pos.Contracts.Triggers;
using Microsoft.Dynamics.Retail.Pos.SystemCore;

namespace Microsoft.Dynamics.Retail.Pos.Printing
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Export(typeof(IPrinting))]
    public class Printing : IPrinting
    {
        #region Fields

        private const string NORMAL_TEXT_MARKER = "|1C";
        private const string BOLD_TEXT_MARKER = "|2C";
        private const string DOUBLESIZE_TEXT_MARKER = "|3C";
        private const string DOUBLESIZE_BOLD_TEXT_MARKER = "|4C";

        #endregion

        /// <summary>
        /// IApplication instance.
        /// </summary>
        private IApplication application;

        /// <summary>
        /// Gets or sets the IApplication instance.
        /// </summary>
        [Import]
        public IApplication Application
        {
            get
            {
                return this.application;
            }
            set
            {
                this.application = value;
                InternalApplication = value;
            }
        }

        /// <summary>
        /// Gets or sets the static IApplication instance.
        /// </summary>
        internal static IApplication InternalApplication { get; private set; }

        /// <summary>
        /// Returns true if print preview ids shown.
        /// </summary>
        /// <param name="formType"></param>
        /// <param name="posTransaction"></param>
        /// <returns></returns>
        public bool ShowPrintPreview(FormType formType, IPosTransaction posTransaction, ISaleLineItem saleItem)
        {
            if (Printing.InternalApplication.Services.Peripherals.FiscalPrinter.FiscalPrinterEnabled())
            {
                return Printing.InternalApplication.Services.Peripherals.FiscalPrinter.ShowPrintPreview(formType, posTransaction);
            }

            FormModulation formMod = new FormModulation(Application.Settings.Database.Connection);
            RetailTransaction retailTransaction = (RetailTransaction)posTransaction;

            FormInfo formInfo = formMod.GetInfoForForm(formType, false, LSRetailPosis.Settings.HardwareProfiles.Printer.ReceiptProfileId);
            if (saleItem == null)
            {
                formMod.GetTransformedTransaction(formInfo, retailTransaction);
            }
            else
            {
                formMod.GetTransformedSaleItem(formInfo, saleItem, posTransaction);
            }

            char esc = Convert.ToChar(27);
            string textForPreview = formInfo.Header;
            textForPreview += formInfo.Details;
            textForPreview += formInfo.Footer;
            textForPreview = textForPreview.Replace(esc + NORMAL_TEXT_MARKER, string.Empty);
            textForPreview = textForPreview.Replace(esc + BOLD_TEXT_MARKER, string.Empty);
            textForPreview = textForPreview.Replace(esc + DOUBLESIZE_TEXT_MARKER, string.Empty);
            textForPreview = textForPreview.Replace(esc + DOUBLESIZE_BOLD_TEXT_MARKER, string.Empty);

            ICollection<Point> signaturePoints = null;
            if (retailTransaction.TenderLines != null
                && retailTransaction.TenderLines.Count > 0
                && retailTransaction.TenderLines.First.Value != null)
            {
                signaturePoints = retailTransaction.TenderLines.First.Value.SignatureData;
            }

            using (frmReportList preview = new frmReportList(textForPreview, signaturePoints))
            {
                this.Application.ApplicationFramework.POSShowForm(preview);
                if (preview.DialogResult == DialogResult.OK)
                {
                    if (LSRetailPosis.Settings.HardwareProfiles.Printer.DeviceType == LSRetailPosis.Settings.HardwareProfiles.DeviceTypes.None)
                    {
                        this.Application.Services.Dialog.ShowMessage(ApplicationLocalizer.Language.Translate(10060), MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return false;
                    }

                    return true;
                }
            }

            return false;
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
            if (copyReceipt)
            {
                // Trigger: PrePrintReceiptCopy trigger for the printing receipt copy operation
                var preTriggerResult = new PreTriggerResult();

                PosApplication.Instance.Triggers.Invoke<IPrintingTrigger>(t => t.PrePrintReceiptCopy(preTriggerResult, posTransaction));

                if (!TriggerHelpers.ProcessPreTriggerResults(preTriggerResult))
                {
                    saveLog("Printing.dll PrintReceipt() failed line153:", posTransaction.TransactionId);
                    return false;//priting failed
                }
            }

            if (Printing.InternalApplication.Services.Peripherals.FiscalPrinter.FiscalPrinterEnabled())
            {
                bool proceedPrinting = false;
                if (Printing.InternalApplication.Services.Peripherals.FiscalPrinter.SupportPrintingReceiptInNonFiscalMode(copyReceipt))
                {
                    proceedPrinting = Printing.InternalApplication.Services.Peripherals.FiscalPrinter.PrintReceipt(formType, posTransaction, copyReceipt);
                }

                if (Printing.InternalApplication.Services.Peripherals.FiscalPrinter.ProhibitPrintingReceiptOnNonFiscalPrinters(copyReceipt))
                {
                    if (proceedPrinting == false)
                    {
                        saveLog("Printing.dll PrintReceipt() failed line170:", posTransaction.TransactionId);
                    }
                    return proceedPrinting;
                }
            }

            FormModulation formMod = new FormModulation(Application.Settings.Database.Connection);
            IList<PrinterAssociation> printerMapping = PrintingActions.GetActivePrinters(formMod, formType, copyReceipt);

            bool result = false;
            foreach (PrinterAssociation printerMap in printerMapping)
            {
                bool printResult = PrintingActions.PrintFormTransaction(printerMap, formMod, formType, posTransaction, copyReceipt);
                //   printResult = PrintingActions.PrintFormTransaction(printerMap, formMod, formType, posTransaction, true);

                result = result || printResult;
            }
            if (result == false)
            {
                saveLog("Printing.dll PrintReceipt() failed line185:", posTransaction.TransactionId);
            }

            return result;
        }


        /// <summary>
        /// Print card slips.
        /// </summary>
        /// <param name="formType"></param>
        /// <param name="posTransaction"></param>
        /// <param name="tenderLineItem"></param>
        /// <param name="copyReceipt"></param>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "only single cast of each type per condition")]
        public void PrintCardReceipt(FormType formType, IPosTransaction posTransaction, ITenderLineItem tenderLineItem, bool copyReceipt)
        {
            PrintingActions.Print(formType, copyReceipt, true,
                delegate (FormModulation formMod, FormInfo formInfo)
                {
                    return formMod.GetTransformedCardTender(formInfo, ((ICardTenderLineItem)tenderLineItem).EFTInfo, (RetailTransaction)posTransaction);
                });
        }

        /// <summary>
        /// Print card slips.
        /// </summary>
        /// <param name="formType"></param>
        /// <param name="posTransaction"></param>
        /// <param name="eftInfo"></param>
        /// <param name="copyReceipt"></param>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "only single cast of each type per condition")]
        public void PrintCardReceipt(FormType formType, IPosTransaction posTransaction, IEFTInfo eftInfo, bool copyReceipt)
        {
            PrintingActions.Print(formType, copyReceipt, true,
                delegate (FormModulation formMod, FormInfo formInfo)
                {
                    return formMod.GetTransformedCardTender(formInfo, eftInfo, (RetailTransaction)posTransaction);
                });
        }

        /// <summary>
        /// Print customer account slips.
        /// </summary>
        /// <param name="formType"></param>
        /// <param name="posTransaction"></param>
        /// <param name="tenderLineItem"></param>
        /// <param name="copyReceipt"></param>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "only single cast of each type per condition")]
        public void PrintCustomerReceipt(FormType formType, IPosTransaction posTransaction, ITenderLineItem tenderLineItem, bool copyReceipt)
        {
            PrintingActions.Print(formType, copyReceipt, true,
                delegate (FormModulation formMod, FormInfo formInfo)
                {
                    return formMod.GetTransformedTender(formInfo, (TenderLineItem)tenderLineItem, (RetailTransaction)posTransaction);
                });
        }

        /// <summary>
        /// Print declare starting amount receipt
        /// </summary>
        /// <param name="posTransaction">FloatEntryTransaction</param>
        public void PrintStartngAmountReceipt(IPosTransaction posTransaction)
        {
            if (Printing.InternalApplication.Services.Peripherals.FiscalPrinter.FiscalPrinterEnabled() &&
                Functions.CountryRegion == SupportedCountryRegion.RU)
            {
                Printing.InternalApplication.Services.Peripherals.FiscalPrinter.PrintStartingAmount(posTransaction);
                return;
            }

            bool copyReceipt = false;

            PrintingActions.Print(FormType.FloatEntry, copyReceipt, false, delegate (FormModulation formMod, FormInfo formInfo)
            {
                StringBuilder reportLayout = new StringBuilder();
                StartingAmountTransaction startingAmountTransaction = (StartingAmountTransaction)posTransaction;

                PrintingActions.PrepareReceiptHeader(reportLayout, posTransaction, 10077, false);
                reportLayout.AppendLine(PrintingActions.SingleLine);

                reportLayout.AppendLine();
                reportLayout.AppendLine(PrintingActions.FormatTenderLine(ApplicationLocalizer.Language.Translate(10078),
                    Printing.InternalApplication.Services.Rounding.Round(startingAmountTransaction.Amount, true)));
                reportLayout.AppendLine(startingAmountTransaction.Description.ToString());
                reportLayout.AppendLine();
                reportLayout.AppendLine(PrintingActions.DoubleLine);

                PrintingActions.PrepareReceiptFooter(reportLayout);

                return reportLayout.ToString();
            });
        }

        /// <summary>
        /// Print Float Entry Receipt
        /// </summary>
        /// <param name="posTransaction">FloatEntryTransaction</param>
        public void PrintFloatEntryReceipt(IPosTransaction posTransaction)
        {
            if (Printing.InternalApplication.Services.Peripherals.FiscalPrinter.FiscalPrinterEnabled())
            {
                Printing.InternalApplication.Services.Peripherals.FiscalPrinter.PrintFloatEntry(posTransaction);
                return;
            }

            bool copyReceipt = false;

            PrintingActions.Print(FormType.FloatEntry, copyReceipt, false, delegate (FormModulation formMod, FormInfo formInfo)
            {
                StringBuilder reportLayout = new StringBuilder();
                FloatEntryTransaction asFloatEntryTransaction = (FloatEntryTransaction)posTransaction;

                PrintingActions.PrepareReceiptHeader(reportLayout, posTransaction, 10061, copyReceipt);
                reportLayout.AppendLine(PrintingActions.SingleLine);

                reportLayout.AppendLine();
                reportLayout.AppendLine(PrintingActions.FormatTenderLine(ApplicationLocalizer.Language.Translate(10062),
                    Printing.InternalApplication.Services.Rounding.Round(asFloatEntryTransaction.Amount, true)));
                reportLayout.AppendLine(asFloatEntryTransaction.Description.ToString());
                reportLayout.AppendLine();
                reportLayout.AppendLine(PrintingActions.DoubleLine);

                PrintingActions.PrepareReceiptFooter(reportLayout);

                return reportLayout.ToString();
            });
        }

        /// <summary>
        /// Print Tender Removal Receipt
        /// </summary>
        /// <param name="posTransaction">RemoveTenderTransaction</param>
        public void PrintRemoveTenderReceipt(IPosTransaction posTransaction)
        {
            if (Printing.InternalApplication.Services.Peripherals.FiscalPrinter.FiscalPrinterEnabled())
            {
                Printing.InternalApplication.Services.Peripherals.FiscalPrinter.PrintRemoveTender(posTransaction);
                return;
            }

            bool copyReceipt = false;

            PrintingActions.Print(FormType.RemoveTender, copyReceipt, false, delegate (FormModulation formMod, FormInfo formInfo)
            {
                StringBuilder reportLayout = new StringBuilder();
                PrintingActions.PrepareReceiptHeader(reportLayout, posTransaction, 10063, copyReceipt);
                reportLayout.AppendLine(PrintingActions.SingleLine);

                reportLayout.AppendLine();
                RemoveTenderTransaction asRemoveTenderTransaction = (RemoveTenderTransaction)posTransaction;
                reportLayout.AppendLine(PrintingActions.FormatTenderLine(ApplicationLocalizer.Language.Translate(10064),
                    Printing.InternalApplication.Services.Rounding.Round(asRemoveTenderTransaction.Amount, true)));
                reportLayout.AppendLine(asRemoveTenderTransaction.Description.ToString());
                reportLayout.AppendLine();
                reportLayout.AppendLine(PrintingActions.DoubleLine);

                PrintingActions.PrepareReceiptFooter(reportLayout);

                formMod = new FormModulation(Application.Settings.Database.Connection);
                formInfo = formMod.GetInfoForForm(FormType.FloatEntry, copyReceipt, LSRetailPosis.Settings.HardwareProfiles.Printer.ReceiptProfileId);

                return reportLayout.ToString();
            });
        }

        /// <summary>
        /// Print credit card memo.
        /// </summary>
        /// <param name="formType"></param>
        /// <param name="posTransaction"></param>
        /// <param name="tenderLineItem"></param>
        /// <param name="copyReceipt"></param>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "only single cast of each type per condition")]
        public void PrintCreditMemo(FormType formType, IPosTransaction posTransaction, ITenderLineItem tenderLineItem, bool copyReceipt)
        {
            PrintingActions.Print(formType, copyReceipt, true,
                delegate (FormModulation formMod, FormInfo formInfo)
                {
                    return formMod.GetTransformedTender(formInfo, (TenderLineItem)tenderLineItem, (RetailTransaction)posTransaction);
                });
        }

        /// <summary>
        /// Print balance of credit card memo.
        /// </summary>
        /// <param name="formType"></param>
        /// <param name="balance"></param>
        /// <param name="copyReceipt"></param>
        public void PrintCreditMemoBalance(FormType formType, Decimal balance, bool copyReceipt)
        {
            PrintingActions.Print(formType, copyReceipt, true,
                delegate (FormModulation formMod, FormInfo formInfo)
                {
                    IRetailTransaction tr = Printing.InternalApplication.BusinessLogic.Utility.CreateRetailTransaction(
                        ApplicationSettings.Terminal.StoreId,
                        ApplicationSettings.Terminal.StoreCurrency,
                        ApplicationSettings.Terminal.TaxIncludedInPrice,
                        Printing.InternalApplication.Services.Rounding);
                    tr.AmountToAccount = balance;
                    formMod.GetTransformedTransaction(formInfo, (RetailTransaction)tr);

                    return formInfo.Header;
                });
        }

        /// <summary>
        /// Print invoice receipt.
        /// </summary>
        /// <param name="posTransaction">The pos transaction.</param>
        /// <param name="copyInvoice">if set to <c>true</c> [copy invoice].</param>
        /// <param name="printPreview">Not supported.</param>
        /// <returns></returns>
        public bool PrintInvoice(IPosTransaction posTransaction, bool copyInvoice, bool printPreview)
        {
            if (Printing.InternalApplication.Services.Peripherals.FiscalPrinter.FiscalPrinterEnabled())
            {
                Printing.InternalApplication.Services.Peripherals.FiscalPrinter.PrintInvoice(posTransaction, copyInvoice, printPreview);
                return true;
            }

            FormModulation formMod = new FormModulation(Application.Settings.Database.Connection);

            bool noPrinterDefined = true; // Initilize as error

            IList<PrinterAssociation> printerMapping = PrintingActions.GetActivePrinters(formMod, FormType.Invoice, copyInvoice);

            bool result = true;
            foreach (PrinterAssociation printerMap in printerMapping)
            {
                noPrinterDefined = noPrinterDefined && (printerMap.Type == DeviceTypes.None);

                if ((printerMap.Type == DeviceTypes.OPOS) || (printerMap.Type == DeviceTypes.Windows))
                {
                    bool printResult = PrintingActions.PrintFormTransaction(printerMap, formMod, FormType.Invoice, posTransaction, copyInvoice);

                    result = result && printResult;
                }
            }

            if (noPrinterDefined)
            {
                // 10060 - No printer type has been defined.
                this.Application.Services.Dialog.ShowMessage(ApplicationLocalizer.Language.Translate(10060), MessageBoxButtons.OK, MessageBoxIcon.Stop);
                result = false;
            }

            return result;

        }

        /// <summary>
        /// Print Tender Decaraton Receipt
        /// </summary>
        /// <param name="posTransaction">TenderDeclarationTransaction</param>
        public void PrintTenderDeclaration(IPosTransaction posTransaction)
        {
            bool copyReceipt = false;

            PrintingActions.Print(FormType.TenderDeclaration, copyReceipt, false, delegate (FormModulation formMod, FormInfo formInfo)
            {
                StringBuilder reportLayout = new StringBuilder();
                PrintingActions.PrepareReceiptHeader(reportLayout, posTransaction, 10065, copyReceipt);
                reportLayout.AppendLine(PrintingActions.SingleLine);

                PrintingActions.PrepareReceiptTenders(reportLayout, posTransaction);
                reportLayout.AppendLine(PrintingActions.DoubleLine);

                PrintingActions.PrepareReceiptFooter(reportLayout);

                return reportLayout.ToString();
            });
        }

        /// <summary>
        /// Print Bank drop Receipt
        /// </summary>
        /// <param name="posTransaction">BankDropTransaction</param>
        public void PrintBankDrop(IPosTransaction posTransaction)
        {
            if (Printing.InternalApplication.Services.Peripherals.FiscalPrinter.FiscalPrinterEnabled())
            {
                Printing.InternalApplication.Services.Peripherals.FiscalPrinter.PrintBankDrop(posTransaction);
                return;
            }

            bool copyReceipt = false;

            PrintingActions.Print(FormType.BankDrop, copyReceipt, false, delegate (FormModulation formMod, FormInfo formInfo)
            {
                StringBuilder reportLayout = new StringBuilder();
                PrintingActions.PrepareReceiptHeader(reportLayout, posTransaction, 10066, copyReceipt);
                reportLayout.AppendLine(PrintingActions.FormatHeaderLine(10069, ((BankDropTransaction)posTransaction).BankBagNo.ToString(), true));
                reportLayout.AppendLine(PrintingActions.SingleLine);

                PrintingActions.PrepareReceiptTenders(reportLayout, posTransaction);
                reportLayout.AppendLine(PrintingActions.DoubleLine);

                PrintingActions.PrepareReceiptFooter(reportLayout);

                return reportLayout.ToString();
            });
        }

        /// <summary>
        /// Print safe drop Receipt
        /// </summary>
        /// <param name="posTransaction">SafeDropTransaction</param>
        public void PrintSafeDrop(IPosTransaction posTransaction)
        {
            if (Printing.InternalApplication.Services.Peripherals.FiscalPrinter.FiscalPrinterEnabled())
            {
                Printing.InternalApplication.Services.Peripherals.FiscalPrinter.PrintSafeDrop(posTransaction);
                return;
            }

            bool copyReceipt = false;

            PrintingActions.Print(FormType.SafeDrop, copyReceipt, false, delegate (FormModulation formMod, FormInfo formInfo)
            {
                StringBuilder reportLayout = new StringBuilder();
                PrintingActions.PrepareReceiptHeader(reportLayout, posTransaction, 10067, copyReceipt);
                reportLayout.AppendLine(PrintingActions.SingleLine);

                PrintingActions.PrepareReceiptTenders(reportLayout, posTransaction);
                reportLayout.AppendLine(PrintingActions.DoubleLine);

                PrintingActions.PrepareReceiptFooter(reportLayout);

                return reportLayout.ToString();
            });
        }

        /// <summary>
        /// Pring Gift Certificate
        /// </summary>
        /// <param name="formType">Currently unused</param>
        /// <param name="posTransaction"></param>
        /// <param name="giftCardItem"></param>
        /// <param name="copyReceipt">True if it is duplicate</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2", Justification = "Grandfather")]
        public void PrintGiftCertificate(FormType formType, IPosTransaction posTransaction, IGiftCardLineItem giftCardLineItem, bool copyReceipt)
        {
            PrintingActions.Print(formType, copyReceipt, false, delegate (FormModulation formMod, FormInfo formInfo)
            {
                StringBuilder reportLayout = new StringBuilder();

                PrintingActions.PrepareReceiptHeader(reportLayout, posTransaction, 10068, copyReceipt);
                reportLayout.AppendLine(PrintingActions.SingleLine);

                reportLayout.AppendLine();
                reportLayout.AppendLine(PrintingActions.FormatTenderLine(ApplicationLocalizer.Language.Translate(10070), giftCardLineItem.SerialNumber));
                reportLayout.AppendLine(PrintingActions.FormatTenderLine(ApplicationLocalizer.Language.Translate(10071),
                    Printing.InternalApplication.Services.Rounding.RoundForDisplay(giftCardLineItem.Balance,
                    true, false)));
                reportLayout.AppendLine();
                reportLayout.AppendLine(PrintingActions.DoubleLine);

                PrintingActions.PrepareReceiptFooter(reportLayout);

                return reportLayout.ToString();
            });
        }

        /// <summary>
        /// Prints the return label.
        /// </summary>
        /// <param name="formType">Type of the form.</param>
        /// <param name="posTransaction">The pos transaction.</param>
        /// <param name="saleItem">The sale line item.</param>
        /// <param name="copyReceipt">if set to <c>true</c> [copy receipt].</param>
        public void PrintReturnLabel(FormType formType, IPosTransaction posTransaction, ISaleLineItem saleItem, bool copyReceipt)
        {
            PrintingActions.Print(formType, copyReceipt, false, delegate (FormModulation formMod, FormInfo formInfo)
            {
                return formMod.GetTransformedSaleItem(formInfo, saleItem, posTransaction);
            });
        }

        /// <summary>
        /// Print pack slip.
        /// </summary>
        /// <param name="posTransaction">Transaction instance.</param>
        public void PrintPackSlip(IPosTransaction posTransaction)
        {
            bool copyReceipt = false;
            FormType formType = FormType.PackingSlip;

            PrintingActions.Print(formType, copyReceipt, true, delegate (FormModulation formMod, FormInfo formInfo)
            {
                formMod.GetTransformedTransaction(formInfo, (RetailTransaction)posTransaction);

                return formInfo.Header + formInfo.Details + formInfo.Footer;
            });
        }

        /// <summary>
        /// Print directly using the printText provided
        /// </summary>
        /// <param name="allowFallback">if set to <c>true</c> [allow fallback].</param>
        /// <param name="printText">The print text.</param>
        public void PrintDefault(bool allowFallback, string printText)
        {
            if (Printing.InternalApplication.Services.Peripherals.Printer.IsActive)
            {   // Print to the default printer (#1)
                Printing.InternalApplication.Services.Peripherals.Printer.PrintReceipt(printText);
            }
            else if (allowFallback && Printing.InternalApplication.Services.Peripherals.Printer2.IsActive)
            {   // Use the fallback printer
                Printing.InternalApplication.Services.Peripherals.Printer2.PrintReceipt(printText);
            }
            else
            {
                NetTracer.Information("Printing.PrintDefault() - printing is skipped as no printer is available.");
            }
        }

        /// <summary>
        /// Prints external receipt, i.e., any document received from an external device or a service.
        /// </summary>
        /// <param name="text">The text to be printed.</param>
        /// <remarks>
        /// Prints external receipt on the fiscal printer if it is enabled; otherwise prints on every available printer (printer 1 and/or printer 2).
        /// </remarks>
        public void PrintExternalReceipt(string text)
        {
            if (Printing.InternalApplication.Services.Peripherals.FiscalPrinter.FiscalPrinterEnabled())
            {
                Printing.InternalApplication.Services.Peripherals.FiscalPrinter.PrintReceipt(text);
            }
            else
            {
                if (Printing.InternalApplication.Services.Peripherals.Printer.IsActive)
                {
                    Printing.InternalApplication.Services.Peripherals.Printer.PrintReceipt(text);
                }
                if (Printing.InternalApplication.Services.Peripherals.Printer2.IsActive)
                {
                    Printing.InternalApplication.Services.Peripherals.Printer2.PrintReceipt(text);
                }
            }
        }

        /// <summary>
        /// Print Decline Receipt
        /// </summary>
        public void PrintDeclineReceipt(IEFTInfo eftInfo)
        {
            if (eftInfo == null)
            {
                throw new ArgumentNullException("eftInfo");
            }

            StringBuilder recLayout = new StringBuilder();

            string storeAddress = ApplicationSettings.Terminal.StoreAddress;
            string storeName = ApplicationSettings.Terminal.StoreName;
            string terminalId = ApplicationSettings.Terminal.TerminalId;
            string merchantId = LSRetailPosis.Settings.HardwareProfiles.EFT.MerchantId;
            string receiptNumber = eftInfo.TransactionNumber.ToString();
            string transactionType = eftInfo.TransactionType.ToString();
            string cardNumber = Printing.InternalApplication.BusinessLogic.Utility.MaskCardNumber(eftInfo.CardNumber);
            string cardType = eftInfo.CardName;
            string amount = Printing.InternalApplication.Services.Rounding.RoundForDisplay(eftInfo.Amount, true, false);
            string referenceNo = eftInfo.RetrievalReferenceNumber;
            string authNo = eftInfo.AuthCode;
            DateTime recDate = DateTime.Now;
            // Provider requirement - Print decline status with provider response code.
            string declineStatus = ApplicationLocalizer.Language.Translate(50069, (object)eftInfo.ProviderResponseCode);

            recLayout.AppendLine(ApplicationLocalizer.Language.Translate(50060)); //Blank Line
            recLayout.AppendLine(storeName);
            recLayout.AppendLine(storeAddress);
            recLayout.AppendLine(ApplicationLocalizer.Language.Translate(50061)); //Blank Line
            recLayout.AppendLine(ApplicationLocalizer.Language.Translate(50063, (object)terminalId));

            if (merchantId.Length > 0)
            {
                recLayout.AppendLine(ApplicationLocalizer.Language.Translate(50064, (object)merchantId));
            }

            recLayout.AppendLine(ApplicationLocalizer.Language.Translate(50065, (object)receiptNumber));
            recLayout.AppendLine(ApplicationLocalizer.Language.Translate(50066, (object)transactionType));
            recLayout.AppendLine(ApplicationLocalizer.Language.Translate(50067, (object)cardNumber));
            recLayout.AppendLine(ApplicationLocalizer.Language.Translate(50079, (object)cardType));

            recLayout.AppendLine(declineStatus);
            recLayout.AppendLine(ApplicationLocalizer.Language.Translate(50080, (object)referenceNo));
            recLayout.AppendLine(ApplicationLocalizer.Language.Translate(50081, (object)authNo));
            recLayout.AppendLine(ApplicationLocalizer.Language.Translate(50070, (object)amount));
            recLayout.AppendLine(ApplicationLocalizer.Language.Translate(50071)); //Blank Line

            recLayout.AppendLine(ApplicationLocalizer.Language.Translate(50073)); //Blank Line
            recLayout.AppendLine(ApplicationLocalizer.Language.Translate(50074, (object)recDate.ToShortDateString()));
            recLayout.AppendLine(ApplicationLocalizer.Language.Translate(50075, (object)recDate.ToShortTimeString()));
            recLayout.AppendLine(ApplicationLocalizer.Language.Translate(50076)); //Blank Line
            recLayout.AppendLine("{0}");
            string receipt = recLayout.ToString();

            //Prints Receipt for Merchant and Customer
            if (((object)Printing.InternalApplication.Services.Printing) is IPrintingV2)
            {   // Print to the default printer
                Printing.InternalApplication.Services.Printing.PrintDefault(true,
                    string.Format(receipt, ApplicationLocalizer.Language.Translate(50078))); //Merchant Copy
                Printing.InternalApplication.Services.Printing.PrintDefault(true,
                    string.Format(receipt, ApplicationLocalizer.Language.Translate(50077))); //Customer Copy
            }
            else
            {   // Legacy support - direct print to printer #1
                NetTracer.Warning("EFT.Printing - Printing service does not support default printer.  Using printer #1");
                Printing.InternalApplication.Services.Peripherals.Printer.PrintReceipt(
                    string.Format(receipt, ApplicationLocalizer.Language.Translate(50078))); //Merchant Copy
                Printing.InternalApplication.Services.Peripherals.Printer.PrintReceipt(
                    string.Format(receipt, ApplicationLocalizer.Language.Translate(50077))); //Customer Copy
            }
        }

        public void saveLog(string strLog, string strTransId)
        {
            try
            {
                System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection();
                sqlConn.ConnectionString = LSRetailPosis.Settings.ApplicationSettings.Database.LocalConnectionString;
                string query = "INSERT INTO ax.exceptionlog (transactionsid, log, TRANSDATATIME)";
                query += " VALUES (@transactionsid,@log, @TRANSDATATIME)";

                SqlDataAdapter da = new SqlDataAdapter();
                da.InsertCommand = new SqlCommand(query, sqlConn);

                da.InsertCommand.Parameters.Add("@transactionsid", SqlDbType.VarChar).Value = strTransId;
                da.InsertCommand.Parameters.Add("@log", SqlDbType.VarChar).Value = strLog;
                da.InsertCommand.Parameters.Add("@TRANSDATATIME", SqlDbType.DateTime).Value = DateTime.Now;


                sqlConn.Open();
                da.InsertCommand.ExecuteNonQuery();
                sqlConn.Close();

            }
            catch (Exception e)
            {
                MessageBox.Show("exception save Log:" + e.ToString());
            }
        }


    }
}
