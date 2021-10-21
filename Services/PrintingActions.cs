/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/

namespace Microsoft.Dynamics.Retail.Pos.Printing
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Windows.Forms;
    using LSRetailPosis;
    using LSRetailPosis.POSControls;
    using LSRetailPosis.Settings.HardwareProfiles;
    using LSRetailPosis.Settings.FunctionalityProfiles;
    using LSRetailPosis.Transaction;
    using Diagnostics;
    using Contracts.DataEntity;
    using Contracts.Services;

    /// <summary>
    /// Static helper class for printing related actions.
    /// </summary>
    internal static class PrintingActions
    {
        // Get all text through the Translation function in the ApplicationLocalizer
        //
        // TextID's for PrintSystem are reserved at 10050 - 10099
        // Last Id used: 10076
        internal delegate string GetTextHandler(FormModulation formMod, FormInfo formInfo);

        internal static readonly string SingleLine = string.Empty.PadLeft(55, '-');
        internal static readonly string DoubleLine = string.Empty.PadLeft(55, '=');
        internal const string LineFormat = "{0}:{1}";
        internal const char DottedPadding = '.';

        /// <summary>
        /// Print form layout with specified content
        /// </summary>
        /// <param name="formType">Form type.</param>
        /// <param name="copyReceipt">Flag to identify if this is a copy.</param>
        /// <param name="formInfoTemplateRequired">Form Template required</param>
        /// <param name="getPrintTextHandler">Handler that is called to construct text for printing.</param>
        internal static void Print(
            FormType formType,
            bool copyReceipt,
            bool formInfoTemplateRequired,
            GetTextHandler getPrintTextHandler)
        {
            FormModulation formMod = new FormModulation(Printing.InternalApplication.Settings.Database.Connection);
            
            if (Printing.InternalApplication.Services.Peripherals.FiscalPrinter.FiscalPrinterEnabled() &&
                Functions.CountryRegion == SupportedCountryRegion.BR &&
                FormTypeAllowedToBrazilianFiscalPrinter(formType))
            {
                FormInfo formInfo = formMod.GetInfoForForm(formType, copyReceipt, Printer.ReceiptProfileId);
                Printing.InternalApplication.Services.Peripherals.FiscalPrinter.PrintReceipt(getPrintTextHandler(formMod, formInfo));
            }
            
            IList<PrinterAssociation> printerMapping = PrintingActions.GetActivePrinters(formMod, formType, copyReceipt);

            foreach (PrinterAssociation printerMap in printerMapping)
            {
                PrintFormLayout(printerMap, formMod, formType, copyReceipt, formInfoTemplateRequired, getPrintTextHandler);
            }
        }

        /// <summary>
        /// Prints to printer.
        /// </summary>
        /// <param name="printerMap">The printer map.</param>
        /// <param name="formMod">The form mod.</param>
        /// <param name="formType">Type of the form.</param>
        /// <param name="posTransaction">The pos transaction.</param>
        /// <param name="copyReceipt">if set to <c>true</c> [copy receipt].</param>
        /// <returns></returns>
        internal static bool PrintFormTransaction(
            PrinterAssociation printerMap,
            FormModulation formMod,
            FormType formType,
            IPosTransaction posTransaction,
            bool copyReceipt)
        {
            FormInfo formInfo = printerMap.PrinterFormInfo;
            bool result = false;

            // Checking for header only.
            if (formInfo.HeaderTemplate == null)
            {
                // Note: This is allowed now that we have multiple printers...
                result = false;
            }
            else if (PrintingActions.ShouldWePrint(formInfo, formType, copyReceipt, printerMap))
            {
                result = true;
                try
                {
                    formMod.GetTransformedTransaction(formInfo, (RetailTransaction)posTransaction);

                    if (formInfo.PrintAsSlip)
                    {
                        printerMap.Printer.PrintSlip(formInfo.Header, formInfo.Details, formInfo.Footer);
                    }
                    else
                    {
                        // Note: In this API Errors are handled in the printer and exceptions do not bubble up.
                        printerMap.Printer.PrintReceipt(formInfo.Header + formInfo.Details + formInfo.Footer);
                    }
                }
                catch (Exception ex)
                {
                    NetTracer.Warning("Printing [Printing] - Exception while printing receipt");

                    POSFormsManager.ShowPOSErrorDialog(new PosisException(1003, ex));
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the printers.
        /// </summary>
        /// <param name="formMod">The form mod.</param>
        /// <param name="formType">Type of the form.</param>
        /// <param name="copyReceipt">if set to <c>true</c> [copy receipt].</param>
        /// <returns>
        /// list of all available printers with associated data
        /// </returns>
        internal static IList<PrinterAssociation> GetActivePrinters(FormModulation formMod, FormType formType, bool copyReceipt)
        {
            IList<PrinterAssociation> list = new List<PrinterAssociation>(2);
            FormInfo formInfo;
            DeviceTypes type;

            if (Printing.InternalApplication.Services.Peripherals.Printer.IsActive)
            {   // Add printer #1
                formInfo = formMod.GetInfoForForm(formType, copyReceipt, LSRetailPosis.Settings.HardwareProfiles.Printer.ReceiptProfileId);
                type = LSRetailPosis.Settings.HardwareProfiles.Printer.DeviceType;

                list.Add(new PrinterAssociation(Printing.InternalApplication.Services.Peripherals.Printer, formInfo, type));
            }

            if (Printing.InternalApplication.Services.Peripherals.Printer2.IsActive)
            {   // Add printer #2
                formInfo = formMod.GetInfoForForm(formType, copyReceipt, LSRetailPosis.Settings.HardwareProfiles.Printer2.ReceiptProfileId);
                type = LSRetailPosis.Settings.HardwareProfiles.Printer2.DeviceType;

                list.Add(new PrinterAssociation(Printing.InternalApplication.Services.Peripherals.Printer2, formInfo, type));
            }

            return list;
        }

        /// <summary>
        /// Prepare the header part of a receipt
        /// </summary>
        internal static void PrepareReceiptHeader(StringBuilder reportLayout, IPosTransaction posTransaction, int reportTitleStringId, bool reprint)
        {
            if (LSRetailPosis.Settings.ApplicationSettings.Terminal.TrainingMode == true)
            {
                reportLayout.AppendLine(ApplicationLocalizer.Language.Translate(true, null, 13042));
                reportLayout.AppendLine("***********************************");
            }

            reportLayout.AppendLine();
            if (reprint)
            {
                reportLayout.AppendLine(ApplicationLocalizer.Language.Translate(true, null, 13039)); // COPY
                reportLayout.AppendLine();
            }

            reportLayout.AppendLine(ApplicationLocalizer.Language.Translate(reportTitleStringId)); // Report Title
            reportLayout.AppendLine();
            reportLayout.AppendLine(PrintingActions.DoubleLine);

            reportLayout.Append(FormatHeaderLine(10072, posTransaction.OperatorId, true));
            reportLayout.AppendLine(FormatHeaderLine(10075, ((IPosTransactionV1)posTransaction).EndDateTime.ToShortDateString(), false));
            reportLayout.Append(FormatHeaderLine(10073, posTransaction.StoreId, true));
            reportLayout.AppendLine(FormatHeaderLine(10076, ((IPosTransactionV1)posTransaction).EndDateTime.ToShortTimeString(), false));
            reportLayout.AppendLine(FormatHeaderLine(10074, posTransaction.TerminalId, true));
        }

        /// <summary>
        /// Prepare the footer part of a receipt
        /// </summary>
        internal static void PrepareReceiptFooter(StringBuilder reportLayout)
        {
            if (LSRetailPosis.Settings.ApplicationSettings.Terminal.TrainingMode == true)
            {
                reportLayout.AppendLine(ApplicationLocalizer.Language.Translate(true, null, 13042));
                reportLayout.AppendLine("***********************************");
            }
        }

        /// <summary>
        /// Prepare the tender part of receipt
        /// </summary>
        /// <param name="reportLayout"></param>
        /// <param name="posTransaction"></param>
        internal static void PrepareReceiptTenders(StringBuilder reportLayout, IPosTransaction posTransaction)
        {
            string tenderName = string.Empty;
            string amount = string.Empty;

            reportLayout.AppendLine();

            foreach (ITenderLineItem tenderLine in ((TenderCountTransaction)posTransaction).TenderLines)
            {
                if (tenderLine.CurrencyCode == posTransaction.StoreCurrencyCode)
                {
                    // Tenders in the store currency
                    amount = Printing.InternalApplication.Services.Rounding.RoundForDisplay(tenderLine.Amount, true, false);
                    tenderName = tenderLine.Description;
                }
                else
                {
                    // Foreign currency {Currency - CAD}
                    amount = Printing.InternalApplication.Services.Rounding.RoundForDisplay(tenderLine.Amount, tenderLine.CurrencyCode, false, false);
                    tenderName = string.Format("{0} - {1}", tenderLine.Description, tenderLine.CurrencyCode);
                }

                // {Credit Card:......$50}
                reportLayout.AppendLine(FormatTenderLine(tenderName, amount));
            }

            reportLayout.AppendLine();
        }

        /// <summary>
        /// Format the header line of Receipt.
        /// </summary>
        /// <param name="title">Title part of line</param>
        /// <param name="value">Value part of line</param>
        /// <param name="firstPart">True for first part of header, false for second.</param>
        /// <returns></returns>
        internal static string FormatHeaderLine(int titleResourceId, string value, bool firstPart)
        {
            string title = ApplicationLocalizer.Language.Translate(titleResourceId);

            if (firstPart)
            {
                return string.Format(PrintingActions.LineFormat, title.PadRight(15, PrintingActions.DottedPadding), value.PadLeft(8));
            }
            else
            {
                return string.Format(PrintingActions.LineFormat, title.PadRight(7, PrintingActions.DottedPadding), value.PadLeft(10)).PadLeft(22);
            }
        }

        /// <summary>
        /// Format the tender line of Receipt
        /// </summary>
        /// <param name="title">Title of tender item</param>
        /// <param name="value">Value of tender item</param>
        /// <returns></returns>
        internal static string FormatTenderLine(string title, string value)
        {
            return string.Format(PrintingActions.LineFormat, title, value.PadLeft(35 - title.Length, PrintingActions.DottedPadding));
        }

        /// <summary>
        /// Validates if the formType is allowed for Braziilian Fiscal Printer
        /// </summary>
        /// <param name="formType">form type to be validate</param>
        /// <returns>Return true if the formtype is not already managed by Brazilian fiscal printer, otherwise false</returns>
        private static bool FormTypeAllowedToBrazilianFiscalPrinter(FormType formType)
        {
            return formType != FormType.Receipt &&
                   formType != FormType.CardReceiptForShop &&
                   formType != FormType.CardReceiptForCust &&
                   formType != FormType.CardReceiptForShopReturn &&
                   formType != FormType.CardReceiptForCustReturn &&
                   formType != FormType.EFTMessage &&
                   formType != FormType.RemoveTender &&
                   formType != FormType.FloatEntry &&
                   formType != FormType.BankDrop &&
                   formType != FormType.SafeDrop &&
                   formType != FormType.EFDocDANFESimplified &&
                   formType != FormType.EFDocDANFEDetailed;
        }


        /// <summary>
        /// Print form layout with specified content.
        /// </summary>
        /// <param name="formMod">Form modulation.</param>
        /// <param name="formInfo">Form info.</param>
        /// <param name="formType">Form type.</param>
        /// <param name="copyReceipt">Flag to identify if this is a copy.</param>
        /// <param name="getPrintTextHandler">Handler that is called to construct text for printing.</param>
        private static void PrintFormLayout(
            PrinterAssociation printerMap,
            FormModulation formMod,
            FormType formType,
            bool copyReceipt,
            bool formInfoTemplateRequired,
            GetTextHandler getPrintTextHandler)
        {
            FormInfo formInfo = printerMap.PrinterFormInfo;

            // Checking if formType requires template.
            if (formInfoTemplateRequired && (formInfo.HeaderTemplate == null))
            {
                // Note: This is allowed now that we have multiple printers since receipt profile id may not be specified for all of them
            }
            else if (PrintingActions.ShouldWePrint(formInfo, formType, copyReceipt, printerMap))
            {
                try
                {
                    string textToPrint = getPrintTextHandler(formMod, formInfo);

                    if (formInfo.PrintAsSlip)
                    {
                        printerMap.Printer.PrintSlip(textToPrint);
                    }
                    else
                    {
                        printerMap.Printer.PrintReceipt(textToPrint);
                    }
                }
                catch (Exception ex)
                {
                    NetTracer.Warning("Printing [Print] - Exception while printing receipt");
                    POSFormsManager.ShowPOSErrorDialog(new PosisException(1003, ex));
                }
            }
        }

        /// <summary>
        /// Checks if the application should ask whether to print out the respective form and then asks the question.
        /// Returns true if it should not as the question.
        /// Returns the users choice if it should ask the question.
        /// </summary>
        /// <param name="formInfo">The form info.</param>
        /// <param name="formType">Type of the form.</param>
        /// <param name="copyReceipt">if set to <c>true</c> [copy receipt].</param>
        /// <param name="printerAssociation">The printer association.</param>
        /// <returns></returns>
        private static bool ShouldWePrint(FormInfo formInfo, FormType formType, bool copyReceipt, PrinterAssociation printerAssociation)
        {
            bool retval = true;

            // If this is a copy, then we always print everything without asking.
            if (copyReceipt)
            {
                NetTracer.Information("Printing::ShouldWePrint: This is a copy. We always print everything without asking.");
                return retval;
            }

            string printerIdentifier = GetPrinterIdentifier(printerAssociation);

            if (formInfo.PrintBehaviour == (int)PrintBehaviour.PromptUser)
            {
                string formatMsg = string.Format(
                    ApplicationLocalizer.Language.Translate(10081), // {0} \n Printer: {1}
                    ApplicationLocalizer.Language.Translate(GetPromptTextId(formType)), // e.g. Do you want to print a receipt?
                    printerIdentifier);

                DialogResult dialogResult = Printing.InternalApplication.Services.Dialog.ShowMessage(
                    formatMsg, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.No)
                {
                    retval = false;
                }
            }
            else if (formInfo.PrintBehaviour == (int)PrintBehaviour.DoNotPrint ||
                formInfo.PrintBehaviour == (int)PrintBehaviour.AsRequired)
            {
                retval = false;
            }

            return retval;
        }

        /// <summary>
        /// Gets the printer identifier.
        /// </summary>
        /// <param name="printerAssociation">The printer association.</param>
        /// <returns>a printer identifier string</returns>
        private static string GetPrinterIdentifier(PrinterAssociation printerAssociation)
        {
            string result;

            if ((printerAssociation != null) && (printerAssociation.Printer != null))
            {
                result = printerAssociation.Printer.DeviceDescription;
                if (string.IsNullOrWhiteSpace(result))
                {   // Fallback to device name if no description provided
                    result = printerAssociation.Printer.DeviceName;

                    if (string.IsNullOrWhiteSpace(result))
                    {   // Fallback to "printer" if no device description or name is provided.
                        result = ApplicationLocalizer.Language.Translate(10080); // "Unidentified printer"
                    }
                }
            }
            else
            {   // Fallback to "printer" if no device description or name is provided.
                result = ApplicationLocalizer.Language.Translate(10080); // "Unidentified printer"
            }

            return result;
        }

        /// <summary>
        /// Returns the correct id for the text that the user is prompted with, when the application should
        /// ask the user whether to print out the respective form.
        /// </summary>
        /// <param name="formType">The form type.</param>
        /// <returns>The message identifier.</returns>
        private static int GetPromptTextId(FormType formType)
        {
            int retval = 7101; // Do you want to print

            switch (formType)
            {

                case FormType.Receipt:                              // 1
                    retval = 10050;                                 //Do you want to print the receipt?
                    break;
                case FormType.CardReceiptForShop:                   // 2
                    retval = 10051;                                 //Do you want to print the stores's card receipt?
                    break;
                case FormType.CardReceiptForCust:                   // 3
                    retval = 10052;                                 //Do you want to print the customers's card receipt?
                    break;
                case FormType.CardReceiptForShopReturn:             // 4
                    retval = 10053;                                 //Do you want to print the store's return card receipt?
                    break;
                case FormType.CardReceiptForCustReturn:             // 5
                    retval = 10054;                                 //Do you want to print the customers's return card receipt?
                    break;
                case FormType.CustAcntReceiptForShop:               // 6
                    retval = 10055;                                 //Do you want to print the store's customer account receipt?
                    break;
                case FormType.CustAcntReceiptForCust:               // 7
                    retval = 10056;                                 //Do you want to print the customers's account receipt?
                    break;
                case FormType.CustAcntReceiptForShopReturn:         // 8
                    retval = 10057;                                 //Do you want to print the stores's return customer account receipt?
                    break;
                case FormType.CustAcntReceiptForCustReturn:         // 9
                    retval = 10058;                                 //Do you want to print the customers's account return receipt?
                    break;
                case FormType.Invoice:                              // 12
                    retval = 7100;                                  //Do you want to print the invoice?
                    break;
                case FormType.GiftReceipt:                          // 33
                    retval = 99904;                                 //Do you want to print the gift receipt?
                    break;
                default:
                    NetTracer.Information("Printing [Printing] - Prompt text does not exist for form type: {0}", formType.ToString());
                    break;
            }

            return retval;
        }
    }
}
