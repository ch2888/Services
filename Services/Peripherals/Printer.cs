/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/


using System;
using IronBarCode;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Interop.OposConstants;
using Interop.OposPOSPrinter;
using LSRetailPosis;
using LSRetailPosis.POSControls;
using LSRetailPosis.POSControls.Touch;
using LSRetailPosis.Settings;
using LSRetailPosis.Settings.FunctionalityProfiles;
using LSRetailPosis.Settings.HardwareProfiles;
using Microsoft.Dynamics.Retail.Diagnostics;
using Microsoft.Dynamics.Retail.Pos.Contracts.BusinessLogic;
using Microsoft.Dynamics.Retail.Pos.Contracts.Services;
using QRCoder;

namespace Microsoft.Dynamics.Retail.Pos.Services
{
    /// <summary>
    /// Class implements IPrinter interface.
    /// </summary>
    [Export(typeof(IPrinter))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Design")]



    public sealed class Printer : IPrinter
    {

        #region Fields

        private OPOSPOSPrinterClass oposPrinter;

        private readonly int characterSet = Convert.ToInt32(LSRetailPosis.Settings.HardwareProfiles.Printer.Characaterset);

        internal const string barCodeRegEx = "<B: (.+?)>";
        internal const string logoRegEx = @"<L:\s*(?<imageId>\d+)\s*>";
        internal const string qrCodeRegEx = "<Q: (.+?)>";

        private const float defaultFontCharWidth = 6;
        private const string defaultFontName = "Courier New";
        private const float defaultFontSize = 7;
        private const float defaultLineHeight = 10;
        private const int defaultPageBreakMargin = 30;
        private const int totalNumberOfLines = 40;

        private const int defaultQrcodeCellSize = 2;
        private const QrcodeEncoding defaultQrcodeEncoding = QrcodeEncoding.Iso88591;
        private const ErrorCorrectionLevel defaultQrcodeErrorCorrectionLevel = ErrorCorrectionLevel.Low;
        private const int defaultQrcodeVersion = 0;

        private const int TEXT_MARKER_SIZE = 3;
        private const int textMarkerWidth = 3;
        private const string NORMAL_TEXT_MARKER = "|1C";
        private const string BOLD_TEXT_MARKER = "|2C";
        private const string DOUBLESIZE_TEXT_MARKER = "|3C";
        private const string DOUBLESIZE_BOLD_TEXT_MARKER = "|4C";
        private const string ESC_CHARACTER = "\x1B";

        /// Currently specific to Brazil
        private const float defaultFontCharWidthForThermalPrinter = 6.9f;
        private const string defaultFontNameForThermalPrinter = "Lucida Console";
        private const float defaultFontSizeForThermalPrinter = 8;
        private const int defaultPageBoundsWidthForThermalPrinter = 41;
        private const int defaultPageMarginBottonForThermalPrinter = 0;
        private const int defaultPageMarginLeftForThermalPrinter = 0;
        private const int defaultPageMarginTopForThermalPrinter = 20;

        private Barcode barCode = new BarcodeCode39();
        private string[] printText;
        private int printTextLine;
        private frmMessage dialog;
        private int linesLeftOnCurrentPage;
        private string[] headerLines;

        private DeviceTypes deviceType;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Printer"/> class.
        /// </summary>
        public Printer()
        {
        }

        /// Setup printer object with device settings
        /// </summary>
        /// <param name="deviceTypeName">Type of the device.</param>
        /// <param name="deviceName">Name of the device.</param>
        /// <param name="deviceDescription">The device description.</param>
        public void SetUpPrinter(string deviceTypeName, string deviceName, string deviceDescription)
        {
            DeviceTypes device;
            if (Enum.TryParse<DeviceTypes>(deviceTypeName, out device))
            {
                this.deviceType = device;
                this.DeviceName = deviceName;
                this.DeviceDescription = deviceDescription;
            }
            else
            {
                throw new ArgumentException("deviceType");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Load the device.
        /// </summary>
        /// <exception cref="IOException"></exception>
        public void Load()
        {
            if (this.deviceType == DeviceTypes.None)
                return;

            if (this.deviceType == DeviceTypes.OPOS)
            {
                NetTracer.Information("Peripheral [CashDrawer] - OPOS device loading: {0}", this.DeviceName ?? "<Undefined>");

                oposPrinter = new OPOSPOSPrinterClass();

                // Open
                oposPrinter.Open(this.DeviceName);
                Peripherals.CheckResultCode(this, oposPrinter.ResultCode);

                // Claim
                oposPrinter.ClaimDevice(Peripherals.ClaimTimeOut);
                Peripherals.CheckResultCode(this, oposPrinter.ResultCode);

                // Enable/Configure
                oposPrinter.DeviceEnabled = true;
                oposPrinter.AsyncMode = false;
                oposPrinter.CharacterSet = characterSet;
                oposPrinter.RecLineChars = 56;
                oposPrinter.SlpLineChars = 60;

                // Loading a bitmap for the printer
                string logoFile = Path.Combine(ApplicationSettings.GetAppPath(), "RetailPOSLogo.bmp");

                if (File.Exists(logoFile))
                {
                    NetTracer.Information("Peripheral [Printer] - OPOS printer bitmap load");
                    oposPrinter.SetBitmap(1, (int)OPOSPOSPrinterConstants.PTR_S_RECEIPT, logoFile, (int)OPOSPOSPrinterConstants.PTR_BM_ASIS, (int)OPOSPOSPrinterConstants.PTR_BM_CENTER);
                }
            }

            IsActive = true;
        }

        /// <summary>
        /// Unload the device.
        /// </summary>
        public void Unload()
        {
            if (IsActive && oposPrinter != null)
            {
                NetTracer.Information("Peripheral [Printer] - Device Released");

                oposPrinter.ReleaseDevice();
                oposPrinter.Close();
                IsActive = true;
            }
        }

        /// <summary>
        /// Print text on the OPOS printer.
        /// </summary>
        /// <param name="text"></param>
        public void PrintReceipt(string text)
        {
            if (Peripherals.InternalApplication.Services.Peripherals.FiscalPrinter.FiscalPrinterEnabled())
            {
                if (Peripherals.InternalApplication.Services.Peripherals.FiscalPrinter.SupportPrintingReceiptInNonFiscalMode(false))
                {
                    Peripherals.InternalApplication.Services.Peripherals.FiscalPrinter.PrintReceipt(text);
                }

                if (Peripherals.InternalApplication.Services.Peripherals.FiscalPrinter.ProhibitPrintingReceiptOnNonFiscalPrinters(false))
                {
                    return;
                }
            }

            // Always print to text file if test hook is enabled
            PrinterTestHook(text, "Receipt");

            if (!IsActive)
                return;

            try
            {
                NetTracer.Information("Peripheral [Printer] - Print Receipt");

                switch (this.deviceType)
                {
                    case DeviceTypes.OPOS:
                        OPOSPrinting(text);
                        break;

                    case DeviceTypes.Windows:
                        WindowsPrinting(text, this.DeviceName);
                        break;
                }
            }
            catch (Exception ex)
            {
                NetTracer.Warning("Peripheral [Printer] - Print Receipt Error: {0}", ex.ToString());

                ApplicationExceptionHandler.HandleException(this.ToString(), ex);
                Peripherals.InternalApplication.Services.Dialog.ShowMessage(6212);
            }
        }

        /// <summary>
        /// Print text on the OPOS printer as slip.
        /// </summary>
        /// <param name="text"></param>
        public void PrintSlip(string text)
        {
            PrintSlip(text, string.Empty, string.Empty);
        }

        /// <summary>
        /// Prints a slip containing the text in the textToPrint parameter
        /// </summary>
        /// <param name="header"></param>
        /// <param name="details"></param>
        /// <param name="footer"></param>
        public void PrintSlip(string header, string details, string footer)
        {
            if (Peripherals.InternalApplication.Services.Peripherals.FiscalPrinter.FiscalPrinterEnabled())
            {
                Peripherals.InternalApplication.Services.Peripherals.FiscalPrinter.PrintSlip(header, details, footer);
                return;
            }

            if (!IsActive)
                return;

            NetTracer.Information("Peripheral [Printer] - Print Slip");

            if (this.deviceType == DeviceTypes.OPOS)
            {   // Slip printing is only supported on OPOS printer
                headerLines = GetStringArray(header);
                string[] itemLines = GetStringArray(details);
                string[] footerLines = GetStringArray(footer);

                linesLeftOnCurrentPage = 0;

                if (LoadNextSlipPaper(true))
                {
                    try
                    {
                        PrintArray(itemLines);

                        // if there is not space for the footer on the current page then must be prompted for a new page
                        if ((linesLeftOnCurrentPage < footerLines.Length) && (!LoadNextSlipPaper(false)))
                            return;

                        PrintArray(footerLines);
                        RemoveSlipPaper();
                    }
                    finally
                    {
                        CloseExistingMessageWindow();
                    }
                }
            }
            else
            {   // Slip printing is not supported for this device type
                PrintReceipt(header + details + footer);
            }

        }

        /// <summary>
        /// Print a receipt on the windows printer.
        /// </summary>
        /// <param name="textToPrint"></param>
        /// <param name="printerName"></param>
        public void WindowsPrinting(string textToPrint, string printerName)
        {
            if (!string.IsNullOrWhiteSpace(textToPrint))
            {
                using (PrintDocument printDoc = new PrintDocument())
                {
                    printDoc.PrinterSettings.PrinterName = printerName;

                    string subString = textToPrint.Replace(ESC_CHARACTER, string.Empty);
                    printText = subString.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                    printTextLine = 0;

                    if (SupportedCountryRegion.BR == Functions.CountryRegion)
                    {
                        printDoc.BeginPrint += new System.Drawing.Printing.PrintEventHandler(printDoc_BeginPrintBR);
                        printDoc.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(printDoc_PrintPageByWords);
                    }
                    else
                    {
                        printDoc.BeginPrint += new System.Drawing.Printing.PrintEventHandler(printDoc_BeginPrint);
                        printDoc.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(printDoc_PrintPage);
                    }
                    printDoc.DefaultPageSettings.Margins.Left = 0;
                    printDoc.DefaultPageSettings.Margins.Right = 0;
                    printDoc.DefaultPageSettings.Margins.Top = 0;
                    printDoc.DefaultPageSettings.Margins.Bottom = 0;
                    printDoc.Print();

                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Print a receipt containing the text to the OPOS Printer.
        /// </summary>
        /// <param name="textToPrint"> The text to print on the receipt</param>
        /// 



        private void OPOSPrinting(string textToPrint)
        {
            Match barCodeMarkerMatch = Regex.Match(textToPrint, barCodeRegEx, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Match qrCodeMarkerMatch = Regex.Match(textToPrint, qrCodeRegEx, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            bool printBarcode = false;
            bool printQrcode = false;
            string receiptId = string.Empty;

            if (barCodeMarkerMatch.Success)
            {
                printBarcode = true;

                // Get the receiptId
                receiptId = barCodeMarkerMatch.Groups[1].ToString();

                // Delete the barcode marker from the printed string
                textToPrint = textToPrint.Remove(barCodeMarkerMatch.Index, barCodeMarkerMatch.Length);
            }

            if (qrCodeMarkerMatch.Success)
            {
                printQrcode = true;

                // Get the receiptId
                receiptId = qrCodeMarkerMatch.Groups[1].ToString();

                // Delete the barcode marker from the printed string
                textToPrint = textToPrint.Remove(qrCodeMarkerMatch.Index, qrCodeMarkerMatch.Length);
            }
            // replace ESC with Char(27) and add a CRLF to the end
            textToPrint = textToPrint.Replace("ESC", ((char)27).ToString());

            // Format for default logo unchanged 
            textToPrint = textToPrint.Replace("<L>", "\x1B|1B\x1B|bC");

            // Logos configured in AX will have <L:id> format - we need to parse
            // these and print these logos
            MatchCollection logoMatches = Regex.Matches(textToPrint, logoRegEx);
            int start = 0;
            int matchIndex = 0;
            while (true)
            {
                int substringLength = (matchIndex < logoMatches.Count) ? logoMatches[matchIndex].Index - start : textToPrint.Length - start;
                string subStringToPrint = textToPrint.Substring(start, substringLength);
                if (substringLength > 0)
                {
                    // Print the text other than the Logo
                    if (LSRetailPosis.Settings.HardwareProfiles.Printer.BinaryConversion == true)
                    {
                        oposPrinter.BinaryConversion = 2;  // OposBcDecimal
                        subStringToPrint = Peripherals.ConvertToBCD(subStringToPrint + "\r\n\r\n\r\n", characterSet);
                    }

                    oposPrinter.PrintNormal((int)OPOSPOSPrinterConstants.PTR_S_RECEIPT, subStringToPrint);
                    oposPrinter.BinaryConversion = 0;  // OposBcNone
                }

                // Reached the end of text then bail out
                if (start + substringLength >= textToPrint.Length)
                {
                    break;
                }

                // We have a match for the logo, get the logo id and print that logo
                if (matchIndex < logoMatches.Count)
                {
                    // Make sure there is a valid image id > 0
                    int imageId = 0;
                    if (Int32.TryParse(logoMatches[matchIndex].Groups["imageId"].Value, out imageId) && imageId > 0)
                    {
                        // print specific logo
                        byte[] imageBytes = GetImageForLogo(imageId);
                        oposPrinter.BinaryConversion = 2;
                        oposPrinter.PrintMemoryBitmap(
                                (int)OPOSPOSPrinterConstants.PTR_S_RECEIPT,
                                Peripherals.ConvertToBCD(imageBytes),
                                (int)OPOSPOSPrinterConstants.PTR_BMT_BMP,
                                (int)OPOSPOSPrinterConstants.PTR_BM_ASIS,
                                (int)OPOSPOSPrinterConstants.PTR_BM_CENTER);
                        oposPrinter.BinaryConversion = 0;
                    }
                    start = logoMatches[matchIndex].Index + logoMatches[matchIndex].Length;
                    matchIndex++;
                }
            }

            // Check if we should print the receipt id as a barcode on the receipt
            if (printBarcode == true)
            {
                oposPrinter.PrintBarCode((int)OPOSPOSPrinterConstants.PTR_S_RECEIPT, receiptId, (int)OPOSPOSPrinterConstants.PTR_BCS_Code128,
                        80, 80, (int)OPOSPOSPrinterConstants.PTR_BC_CENTER, (int)OPOSPOSPrinterConstants.PTR_BC_TEXT_BELOW);
                oposPrinter.PrintNormal((int)OPOSPOSPrinterConstants.PTR_S_RECEIPT, "\r\n\r\n\r\n\r\n");


            }

            // Check if we should print the receipt id as a qrcode on the receipt
            if (printQrcode == true)
            {
                oposPrinter.PrintBarCode((int)OPOSPOSPrinterConstants.PTR_S_RECEIPT, receiptId, (int)OPOSPOSPrinterConstants.PTR_BCS_Code128/*PTR_BCS_Code128*/,
                        180, 180, (int)OPOSPOSPrinterConstants.PTR_BC_CENTER, (int)OPOSPOSPrinterConstants.PTR_BC_TEXT_BELOW);
                oposPrinter.PrintNormal((int)OPOSPOSPrinterConstants.PTR_S_RECEIPT, "\r\n\r\n\r\n\r\n");


            }

            oposPrinter.CutPaper(100);
        }

        private void NewStatusWindow(int stringId)
        {
            CloseExistingMessageWindow();
            dialog = new frmMessage(stringId, LSPosMessageTypeButton.NoButtons, MessageBoxIcon.Information);
            POSFormsManager.ShowPOSFormModeless(dialog);
        }

        private void CloseExistingMessageWindow()
        {
            if (dialog != null)
            {
                dialog.Dispose();
            }
        }

        private static string[] GetStringArray(string text)
        {
            string[] sep = new string[] { Environment.NewLine };
            if (text.EndsWith(Environment.NewLine))
                text = text.Substring(0, text.Length - 2);
            return text.Split(sep, StringSplitOptions.None);
        }

        private void PrintArray(string[] array)
        {
            NewStatusWindow(99);

            string textToPrint = string.Empty;

            foreach (string text in array)
            {
                if ((linesLeftOnCurrentPage == 0) && (!LoadNextSlipPaper(false)))
                    return;

                textToPrint = text;

                if (LSRetailPosis.Settings.HardwareProfiles.Printer.BinaryConversion == true)
                {
                    oposPrinter.BinaryConversion = 2;  // OposBcDecimal
                    textToPrint = Peripherals.ConvertToBCD(text + Environment.NewLine, this.characterSet);
                }

                oposPrinter.PrintNormal((int)OPOSPOSPrinterConstants.PTR_S_SLIP, textToPrint);
                oposPrinter.BinaryConversion = 0;  // OposBcNone
                linesLeftOnCurrentPage--;
            }
        }

        private bool LoadNextSlipPaper(bool firstSlip)
        {
            bool result = true;
            bool tryAgain;

            NetTracer.Information("Peripheral [Printer] - Load next slip paper");

            if (!firstSlip)
                RemoveSlipPaper();

            do
            {

                tryAgain = false;
                NewStatusWindow(98);
                oposPrinter.BeginInsertion(LSRetailPosis.Settings.HardwareProfiles.Printer.DocInsertRemovalTimeout * 1000);

                if (oposPrinter.ResultCode == (int)OPOS_Constants.OPOS_SUCCESS)
                {
                    NewStatusWindow(99);
                    oposPrinter.EndInsertion();
                    linesLeftOnCurrentPage = totalNumberOfLines;
                    PrintArray(headerLines);
                }
                else
                {
                    CloseExistingMessageWindow();
                    using (frmMessage errDialog = new frmMessage(13051, MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                    {
                        POSFormsManager.ShowPOSForm(errDialog);
                        if (errDialog.DialogResult == DialogResult.Yes)
                            tryAgain = true;
                        else
                            result = false;
                    }
                }
            } while (tryAgain);

            return result;
        }

        private void RemoveSlipPaper()
        {
            bool tryAgain;

            NetTracer.Information("Peripheral [Printer] - Remove slip paper");

            do
            {
                tryAgain = false;
                NewStatusWindow(100);
                oposPrinter.BeginRemoval(LSRetailPosis.Settings.HardwareProfiles.Printer.DocInsertRemovalTimeout * 1000);

                if (oposPrinter.ResultCode == (int)OPOS_Constants.OPOS_SUCCESS)
                {
                    oposPrinter.EndRemoval();
                }
                else if (oposPrinter.ResultCode == (int)OPOS_Constants.OPOS_E_TIMEOUT)
                {
                    CloseExistingMessageWindow();
                    using (frmMessage errDialog = new frmMessage(13052, MessageBoxButtons.OKCancel, MessageBoxIcon.Information))
                    {
                        POSFormsManager.ShowPOSForm(errDialog);
                        if (errDialog.DialogResult == DialogResult.OK)
                            tryAgain = true;
                    }
                }
            } while (tryAgain);
        }

        /// <summary>
        /// Prints given text to sequential file in %TEMP% if printer hook is enabled
        /// </summary>
        /// <param name="text">Text to print</param>
        static private void PrinterTestHook(string text, string filePrefix)
        {
            if (ApplicationSettings.PrintToDisk)
            {
                string directory = Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.Machine);
                string fileName = NextFileNameForPrinterHook(directory, filePrefix);
                fileName = Path.Combine(directory, fileName);
                using (TextWriter printedFile = new StreamWriter(fileName))
                {
                    text = text.Replace(NORMAL_TEXT_MARKER, string.Empty).Replace(BOLD_TEXT_MARKER, string.Empty).Replace(DOUBLESIZE_TEXT_MARKER, string.Empty).Replace(DOUBLESIZE_BOLD_TEXT_MARKER, string.Empty).Replace(ESC_CHARACTER, string.Empty);
                    printedFile.Write(text);
                }
            }
        }

        /// <summary>
        /// Creates sequential files of format prefix_######.txt in the given directory.
        /// </summary>
        /// <param name="directory">Directory where files are stored/searched</param>
        /// <param name="prefix">What the beginning of the file should be named</param>
        /// <returns>Next sequential file name</returns>
        static private string NextFileNameForPrinterHook(string directory, string prefix)
        {
            DirectoryInfo di = new DirectoryInfo(directory);
            FileInfo[] files = di.GetFiles(prefix + "_*.txt");
            int max = 0;

            foreach (FileInfo file in files)
            {
                string fileName = file.Name;
                string[] pieces = fileName.Split(new char[] { '_', '.' });
                int fileNumber = Int32.Parse(pieces[1]);
                if (fileNumber > max)
                {
                    max = fileNumber;
                }
            }

            string nextNumber = (max + 1).ToString().PadLeft(6, '0');
            string nextName = string.Format("{0}_{1}.txt", prefix, nextNumber);

            return nextName;
        }

        /// <summary>
        /// Get Image from image table by image id stored in the print text. 
        /// </summary>
        /// <param name="imageId"></param>
        /// <returns></returns>
        private static byte[] GetImageForLogo(int imageId)
        {
            SqlConnection sqlCon = Peripherals.InternalApplication.Settings.Database.Connection;
            byte[] imageLogo = null;
            try
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = sqlCon;
                    command.CommandText = "SELECT PICTUREID, PICTURE FROM dbo.RETAILIMAGES WHERE PICTUREID = @PICTUREID";
                    command.Parameters.Add("@PICTUREID", SqlDbType.Int).Value = imageId;

                    if (sqlCon.State != ConnectionState.Open)
                    {
                        sqlCon.Open();
                    }

                    using (System.Data.SqlClient.SqlDataAdapter adapter = new System.Data.SqlClient.SqlDataAdapter(command))
                    {
                        using (System.Data.DataTable table = new System.Data.DataTable())
                        {
                            adapter.Fill(table);

                            if (table.Rows.Count > 0 && table.Rows[0]["PICTURE"] != DBNull.Value)
                            {
                                imageLogo = (byte[])table.Rows[0]["PICTURE"];
                            }
                        }
                    }
                }
            }
            finally
            {
                if (sqlCon.State == ConnectionState.Open)
                {
                    sqlCon.Close();
                }
            }
            return imageLogo;
        }

        #endregion

        #region Events
        private void printDoc_BeginPrint(object sender, PrintEventArgs e)
        {
            TextFontName = defaultFontName;
            TextFontSize = defaultFontSize;
            TextFontCharWidth = defaultFontCharWidth;
        }

        private void printDoc_BeginPrintBR(object sender, PrintEventArgs e)
        {
            var printDocument = sender as PrintDocument;

            if (printDocument != null)
            {
                printDocument.DefaultPageSettings.Margins.Bottom = defaultPageMarginBottonForThermalPrinter;
                printDocument.DefaultPageSettings.Margins.Left = defaultPageMarginLeftForThermalPrinter;
                printDocument.DefaultPageSettings.Margins.Top = defaultPageMarginTopForThermalPrinter;
            }

            TextFontName = defaultFontNameForThermalPrinter;
            TextFontSize = defaultFontSizeForThermalPrinter;
            TextFontCharWidth = defaultFontCharWidthForThermalPrinter;

            printText = PrinterHelper.WrapLinesByEnviromentNewLine(printText, NORMAL_TEXT_MARKER);
            printText = PrinterHelper.WrapLinesByPageWidth(printText, defaultPageBoundsWidthForThermalPrinter, NORMAL_TEXT_MARKER);
        }

        /// <summary>
        /// Prints a page. The supported content types are: pure text, images, QRCodes and barcodes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">The print page event args</param>
        private void printDoc_PrintPageByWords(object sender, PrintPageEventArgs e)
        {
            try
            {
                var currentPageHeight = 0f;
                var dpiXRatio = e.Graphics.DpiX / 96f; // 96dpi = 100%
                var dpiYRatio = e.Graphics.DpiY / 96f; // 96dpi = 100%
                var printTextWidth = PrinterHelper.GetTextWidthInHundredthsOfAnInch(printText, TextFontCharWidth);

                e.HasMorePages = false;

                while (printTextLine < printText.Length)
                {
                    var currentLineWidth = 0f;
                    var lastMarker = NORMAL_TEXT_MARKER;

                    var line = printText[printTextLine];
                    var cleanLine = PrinterHelper.RemoveTextMarkers(line);

                    if (currentPageHeight + defaultLineHeight + defaultPageBreakMargin >= e.PageBounds.Height)
                    {
                        // No more room - advance to next page
                        e.HasMorePages = true;
                        return;
                    }

                    // QRCODE
                    var qrCodeMarkerMatch = Regex.Match(cleanLine, qrCodeRegEx, RegexOptions.Compiled | RegexOptions.IgnoreCase);

                    if (qrCodeMarkerMatch.Success)
                    {
                        var qrCodeMarkerMatchString = qrCodeMarkerMatch.ToString();
                        line = line.Replace(qrCodeMarkerMatchString, new string(' ', qrCodeMarkerMatchString.Length));

                        // Get the QrCode
                        var qrCodeString = qrCodeMarkerMatch.Groups[1].ToString();
                        var qrcodeInfo = Utility.CreateQrcodeInfo(defaultQrcodeCellSize, defaultQrcodeErrorCorrectionLevel, defaultQrcodeVersion, defaultQrcodeEncoding);

                        using (var qrCodeImage = Peripherals.InternalApplication.Services.Qrcode.Encode(qrCodeString, qrcodeInfo))
                        {
                            if (qrCodeImage != null)
                            {
                                float qrcodeHeight = (qrCodeImage.Height / dpiYRatio) * 4;
                                float qrcodeWidth = (qrCodeImage.Width / dpiXRatio) * 4;
                                if (currentPageHeight + qrCodeImage.Height + defaultPageBreakMargin >= e.PageBounds.Height)
                                {   // No more room - advance to next page
                                    e.HasMorePages = true;
                                    return;
                                }

                                // Render qrcode in the center of the text.
                                var qrcodePoint = (printTextWidth - qrCodeImage.Width) / 2;
                                e.Graphics.DrawImage(qrCodeImage, e.MarginBounds.Left + qrcodePoint,
                                    e.MarginBounds.Top + currentPageHeight,
                                    qrcodeWidth,
                                    qrcodeHeight);
                                currentPageHeight += qrcodeHeight;
                            }
                        }
                    }

                    // BARCODE
                    var barCodeMarkerMatch = Regex.Match(cleanLine, barCodeRegEx, RegexOptions.Compiled | RegexOptions.IgnoreCase);

                    if (barCodeMarkerMatch.Success)
                    {
                        var barCodeMarkerMatchString = barCodeMarkerMatch.ToString();
                        line = line.Replace(barCodeMarkerMatchString, new string(' ', barCodeMarkerMatchString.Length));

                        // Get the receiptId
                        var barcodeString = barCodeMarkerMatch.Groups[1].ToString();

                        using (var barcodeImage = barCode.Create(barcodeString, e.Graphics.DpiX, e.Graphics.DpiY))
                        {
                            if (barcodeImage != null)
                            {
                                var barcodeHeight = (barcodeImage.Height / dpiYRatio);

                                if (currentPageHeight + barcodeHeight + defaultPageBreakMargin >= e.PageBounds.Height)
                                {   // No more room - advance to next page
                                    e.HasMorePages = true;
                                    return;
                                }

                                // Render barcode in the center of the text.
                                var barcodePoint = (printTextWidth - (barcodeImage.Width / dpiXRatio)) / 2;
                                e.Graphics.DrawImage(barcodeImage, e.MarginBounds.Left + barcodePoint, e.MarginBounds.Top + currentPageHeight);
                                currentPageHeight += barcodeHeight;
                            }
                        }
                    }

                    // Text and other images
                    var words = PrinterHelper.SplitWordsByMarkers(line);

                    foreach (var word in words)
                    {
                        var logoMatch = Regex.Match(word, logoRegEx);

                        if (logoMatch.Success)
                        {
                            var imageId = Convert.ToInt32(logoMatch.Groups["imageId"].Value);
                            var image = PrinterHelper.GetRetailImage(imageId);

                            if (currentPageHeight + image.Height + defaultPageBreakMargin >= e.PageBounds.Height)
                            {   // No more room - advance to next page
                                e.HasMorePages = true;
                                return;
                            }

                            e.Graphics.DrawImage(image, e.MarginBounds.Left + currentLineWidth, e.MarginBounds.Top + currentPageHeight);
                            currentLineWidth += image.Width;
                            currentPageHeight += image.Height;
                        }
                        else
                        {
                            var cleanTextToPrint = PrinterHelper.RemoveTextMarkers(word);
                            lastMarker = AllTextMarkers.Contains(word) ? word : lastMarker;

                            if (!string.IsNullOrEmpty(cleanTextToPrint))
                            {
                                var linePartFont = CreateFontForMarker(lastMarker, TextFontName, TextFontSize);

                                e.Graphics.DrawString(cleanTextToPrint, linePartFont, Brushes.Black, e.MarginBounds.Left + currentLineWidth, e.MarginBounds.Top + currentPageHeight);
                                currentLineWidth += cleanTextToPrint.Length * TextFontCharWidth;
                            }
                        }
                    }

                    currentPageHeight += defaultLineHeight;
                    printTextLine++;
                }
            }
            catch (Exception ex)
            {
                NetTracer.Warning("Peripheral [Printer] - Exception in print page");

                ApplicationExceptionHandler.HandleException(this.ToString(), ex);
            }
        }

        /// <summary>
        /// Prints the selected page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void printDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            try
            {
                e.HasMorePages = false;
                using (Font textFontRegular = new Font(TextFontName, TextFontSize, FontStyle.Regular))
                {
                    float xCaretPos = 0, yCaretPos = 0;
                    float dpiXRatio = e.Graphics.DpiX / 96f; // 96dpi = 100%
                    float dpiYRatio = e.Graphics.DpiY / 96f; // 96dpi = 100%
                    float contentWidth = printText.Max(str => str.Replace(NORMAL_TEXT_MARKER, string.Empty).Replace(BOLD_TEXT_MARKER, string.Empty).Replace(DOUBLESIZE_TEXT_MARKER, string.Empty).Replace(DOUBLESIZE_BOLD_TEXT_MARKER, string.Empty).Length) * dpiXRatio; // Line with max length = content width

                    while (this.printTextLine < printText.Length)
                    {
                        string printingLine;
                        var heightStep = IsStringContainAnyOfMarkers(printText[this.printTextLine], DOUBLESIZE_TEXT_MARKER, DOUBLESIZE_BOLD_TEXT_MARKER) ? 2 * defaultLineHeight : defaultLineHeight;

                        if (yCaretPos + heightStep >= e.MarginBounds.Height)
                        {   // No more room - advance to next page
                            e.HasMorePages = true;
                            return;
                        }

                        if (IsStringContainAnyOfMarkers(printText[this.printTextLine], BOLD_TEXT_MARKER, DOUBLESIZE_TEXT_MARKER, DOUBLESIZE_BOLD_TEXT_MARKER))
                        {
                            // Text line printing with bold or double size Text in it.
                            xCaretPos = 0;

                            printingLine = printText[this.printTextLine];
                            while (printingLine.Length > 0)
                            {
                                if (IsStringContainAnyOfMarkers(printingLine, BOLD_TEXT_MARKER, DOUBLESIZE_TEXT_MARKER, DOUBLESIZE_BOLD_TEXT_MARKER))
                                {
                                    string firstTextMarker = GetFirstTextMarker(printingLine, NORMAL_TEXT_MARKER, BOLD_TEXT_MARKER,
                                        DOUBLESIZE_TEXT_MARKER, DOUBLESIZE_BOLD_TEXT_MARKER);

                                    using (var textFontForPrint = CreateFontForMarker(firstTextMarker, TextFontName, TextFontSize))
                                    {
                                        int firstMarkerIndex = printingLine.IndexOf(firstTextMarker);
                                        printingLine = printingLine.Remove(firstMarkerIndex, textMarkerWidth);
                                        string textToPrint = printingLine.Substring(0, printingLine.IndexOf(NORMAL_TEXT_MARKER));

                                        string textBeforeFirstMarker = textToPrint.Substring(0, firstMarkerIndex);
                                        e.Graphics.DrawString(textBeforeFirstMarker, textFontRegular, Brushes.Black,
                                            xCaretPos + e.MarginBounds.Left, yCaretPos + e.MarginBounds.Top);
                                        xCaretPos += textBeforeFirstMarker.Length * TextFontCharWidth;

                                        e.Graphics.DrawString(textToPrint.Substring(firstMarkerIndex),
                                            textFontForPrint, Brushes.Black, xCaretPos + e.MarginBounds.Left, yCaretPos + e.MarginBounds.Top);
                                        xCaretPos += textToPrint.Substring(firstMarkerIndex).Length * TextFontCharWidth;

                                        if (new[] { DOUBLESIZE_TEXT_MARKER, DOUBLESIZE_BOLD_TEXT_MARKER, BOLD_TEXT_MARKER }.Contains(firstTextMarker))
                                        {
                                            printingLine = printingLine.Insert(printingLine.IndexOf(NORMAL_TEXT_MARKER) == -1 ?
                                                0 : printingLine.IndexOf(NORMAL_TEXT_MARKER) + NORMAL_TEXT_MARKER.Length,
                                                new string(' ', textToPrint.Substring(firstMarkerIndex).Length));
                                        }

                                        printingLine = printingLine.Substring(printingLine.IndexOf(NORMAL_TEXT_MARKER) == -1 ?
                                            0 : printingLine.IndexOf(NORMAL_TEXT_MARKER) + NORMAL_TEXT_MARKER.Length);
                                    }
                                }
                                else
                                {
                                    printingLine = printingLine.Replace(NORMAL_TEXT_MARKER, string.Empty);
                                    e.Graphics.DrawString(printingLine, textFontRegular, Brushes.Black, xCaretPos + e.MarginBounds.Left, yCaretPos + e.MarginBounds.Top);
                                    printingLine = string.Empty;
                                }
                            }
                        }
                        else
                        {
                            // Text line printing with no bold Text in it.

                            printingLine = printText[this.printTextLine].Replace(NORMAL_TEXT_MARKER, string.Empty);

                            Match barCodeMarkerMatch = Regex.Match(printingLine, barCodeRegEx, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                            Match qrCodeMarkerMatch = Regex.Match(printingLine, qrCodeRegEx, RegexOptions.Compiled | RegexOptions.IgnoreCase);

                            if (qrCodeMarkerMatch.Success)
                            {
                                // Get the receiptId
                                printingLine = qrCodeMarkerMatch.Groups[1].ToString();
                                printingLine = printingLine.Replace("@", "\n");
                                string strPrintUTF8 = Encoding.UTF8.GetString(Encoding.GetEncoding(1256).GetBytes(printingLine));
                                var qrcodeInfo = Utility.CreateQrcodeInfo(defaultQrcodeCellSize, ErrorCorrectionLevel.High,
                                    defaultQrcodeVersion, QrcodeEncoding.Utf8);


                                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                                QRCodeData qrCodeData = qrGenerator.CreateQrCode(printingLine, QRCodeGenerator.ECCLevel.L);
                                QRCode qrCode = new QRCode(qrCodeData);
                                //Bitmap qrCodeImage2 = qrCode.GetGraphic(20);

                                //using (var qrCodeImage = Peripherals.InternalApplication.Services.Qrcode.Encode(strPrintUTF8, qrcodeInfo))
                                using (var qrCodeImage = qrCode.GetGraphic(printingLine.Contains("http") ? 2 : 1))
                                {
                                    if (qrCodeImage != null)
                                    {
                                        float qrcodeHeight = (qrCodeImage.Height / dpiYRatio) * 4;
                                        float qrcodeWidth = (qrCodeImage.Width / dpiXRatio) * 4;
                                        if (yCaretPos + qrcodeHeight >= e.MarginBounds.Height)
                                        {   // No more room - advance to next page
                                            e.HasMorePages = true;
                                            return;
                                        }

                                        // Render barcode in the center of receipt.
                                        e.Graphics.DrawImage(qrCodeImage,
                                            ((contentWidth - qrcodeWidth) / 2) + e.MarginBounds.Left,
                                            yCaretPos + e.MarginBounds.Top,
                                            qrcodeWidth,
                                            qrcodeHeight);
                                        yCaretPos += qrcodeHeight;
                                        //e.Graphics.DrawString(printingLine, textFontRegular, Brushes.Black, ((contentWidth - (qrCodeImage.Width / dpiXRatio)) / 2) + e.MarginBounds.Left, yCaretPos + e.MarginBounds.Top);
                                    }
                                }

                            }
                            else if (barCodeMarkerMatch.Success)
                            {
                                // Get the receiptId
                                printingLine = barCodeMarkerMatch.Groups[1].ToString();

                                using (Image barcodeImage = barCode.Create(printingLine, e.Graphics.DpiX, e.Graphics.DpiY))
                                {
                                    if (barcodeImage != null)
                                    {
                                        float barcodeHeight = (barcodeImage.Height / dpiYRatio);

                                        if (yCaretPos + barcodeHeight >= e.MarginBounds.Height)
                                        {   // No more room - advance to next page
                                            e.HasMorePages = true;
                                            return;
                                        }

                                        // Render barcode in the center of receipt.
                                        e.Graphics.DrawImage(barcodeImage, ((contentWidth - (barcodeImage.Width / dpiXRatio)) / 2) + e.MarginBounds.Left, yCaretPos + e.MarginBounds.Top);
                                        yCaretPos += barcodeHeight;
                                    }
                                }
                            }
                            else
                            {
                                e.Graphics.DrawString(printingLine, textFontRegular, Brushes.Black, e.MarginBounds.Left, yCaretPos + e.MarginBounds.Top);
                            }
                        }
                        yCaretPos = yCaretPos + heightStep;

                        printTextLine += 1;

                    } // of while()
                } // of using()
            } // of try
            catch (Exception ex)
            {
                NetTracer.Warning("Peripheral [Printer] - Exception in print page");
                MessageBox.Show(ex.ToString());
                ApplicationExceptionHandler.HandleException(this.ToString(), ex);
            }
        }

        private string GetFirstTextMarker(string subString, params string[] allTextMarkers)
        {
            int minIndex = subString.Length;
            string retVal = string.Empty;

            foreach (var textMarker in allTextMarkers)
            {
                int ind = subString.IndexOf(textMarker);
                if (ind != -1 && minIndex > ind)
                {
                    retVal = textMarker;
                    minIndex = ind;
                }
            }

            return retVal;
        }

        private Font CreateFontForMarker(string fontMarker, string textFontName, float textFontSize)
        {
            const int fontSizeFactor = 2;
            Font retVal;

            switch (fontMarker)
            {
                case BOLD_TEXT_MARKER:
                    retVal = new Font(textFontName, textFontSize, FontStyle.Bold);
                    break;
                case DOUBLESIZE_TEXT_MARKER:
                    retVal = new Font(textFontName, textFontSize * fontSizeFactor, FontStyle.Regular);
                    break;
                case DOUBLESIZE_BOLD_TEXT_MARKER:
                    retVal = new Font(textFontName, textFontSize * fontSizeFactor, FontStyle.Bold);
                    break;
                default:
                    retVal = new Font(textFontName, textFontSize, FontStyle.Regular);
                    break;
            }

            return retVal;
        }

        private bool IsStringContainAnyOfMarkers(string source, params string[] markers)
        {
            return markers.Any(source.Contains);
        }

        #endregion

        /// <summary>
        /// Gets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive
        {
            get;
            private set;
        }

        /// <summary>
        /// Device Name (may be null or empty)
        /// </summary>
        public string DeviceName
        {
            get;
            private set;
        }

        /// <summary>
        /// Device Description (may be null or empty)
        /// </summary>
        public string DeviceDescription
        {
            get;
            private set;
        }

        /// <summary>
        /// List of text markers used to format the printed text
        /// </summary>
        public static IEnumerable<string> AllTextMarkers
        {
            get { return new[] { NORMAL_TEXT_MARKER, BOLD_TEXT_MARKER, DOUBLESIZE_TEXT_MARKER, DOUBLESIZE_BOLD_TEXT_MARKER }; }
        }

        /// <summary>
        /// Text marker size
        /// </summary>
        public static int TextMarkerSize
        {
            get { return TEXT_MARKER_SIZE; }
        }

        private static IUtility Utility
        {

            get { return Peripherals.InternalApplication.BusinessLogic.Utility; }
        }

        /// <summary>
        /// Text font char width in hundredths of an inch.
        /// </summary>
        private float TextFontCharWidth
        {
            get;
            set;
        }

        private string TextFontName
        {
            get;
            set;
        }

        private float TextFontSize
        {
            get;
            set;
        }
    }
}
