/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/


namespace Microsoft.Dynamics.Retail.Pos.Printing
{
    using System.Linq;
    using AX.Efd;
    using AX.Efd.Entities;
    using Contracts.DataEntity;
    using LSRetailPosis.Transaction;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Retrieves information for Brazilian electronic fiscal document form types.
    /// </summary>
    internal sealed class FormModulationEfd
    {
        private const string FiscalDocumentProtocolMessage = "Protocolo de Autorização";
        private const string TestingEnvironmentMessage = "EMITIDA EM AMBIENTE DE HOMOLOGAÇÃO - SEM VALOR FISCAL";
        private const string ContingencyModeMessage = "EMITIDA EM CONTINGÊNCIA";
        private const string ConsumerNotIdentifiedMessage = "CONSUMIDOR NÃO IDENTIFICADO";
        private const string ConsumerCopyMessage = "   Via consumidor\r\n";
        private const string StoreCopyMessage = "Via estabelecimento\r\n";
        private const string ThirdPartyAddressFormat = "{0}{1} {2} {3}\r\n{4}{5}";

        private readonly string connectionString;
        private FiscalDocument fiscalDocument;

        internal FormModulationEfd(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Retrives the information for the specific variable created for Brazilian consumer electronic fiscal document
        /// </summary>
        /// <param name="itemInfo">The print page layout item information to be replaced</param>
        /// <param name="retailTransaction">The retail transaction of the current printing</param>
        /// <returns>The value for the specified print page layout item information</returns>
        internal string GetInfoFromFiscalDocument(FormItemInfo itemInfo, RetailTransaction retailTransaction)
        {
            if (retailTransaction == null || retailTransaction.SubType != RetailTransactionSubType.ConsumerEFDocument)
            {
                return null;
            }

            GetFiscalDocumentFromRetailTransaction(retailTransaction);

            if (fiscalDocument == null)
            {
                return null;
            }

            switch (itemInfo.Variable.ToUpperInvariant().Replace(" ", string.Empty))
            {
                case "CCMNUM_BR":
                    return fiscalDocument.FiscalEstablishmentCcmNumber;
                case "CNPJCPFNUM_BR":
                    return fiscalDocument.FiscalEstablishmentCnpjCpf;
                case "EFDADDITIONALFISCALMESSAGECONTINGENCYMODE_BR":
                    if (fiscalDocument.ContingencyMode == (int)IssueType.ContingencyOfflineConsumerEfd)
                    {
                        return ContingencyModeMessage;
                    }
                    return null;
                case "EFDADDITIONALFISCALMESSAGETESTINGENVIRONMENT_BR":
                    if (fiscalDocument.EnvironmentType == EnvironmentType.Test)
                    {
                        return TestingEnvironmentMessage;
                    }
                    return null;
                case "EFDAUTHORIZATIONPROTOCOL_BR":
                    if (fiscalDocument.ContingencyMode == (int)IssueType.Regular)
                    {
                        return fiscalDocument.ProtocolNumber;
                    }
                    return null;
                case "EFDAUTHORIZATIONPROTOCOLDATE_BR":
                    if (fiscalDocument.ContingencyMode == (int)IssueType.Regular)
                    {
                        return fiscalDocument.ProtocolDate.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    }
                    return null;
                case "EFDCOPYINFORMATION_BR":
                    if (fiscalDocument.ContingencyMode == (int)IssueType.ContingencyOfflineConsumerEfd)
                    {
                        switch(retailTransaction.ReceiptCopyType)
                        {
                            case ReceiptCopyType.CustomerCopy:
                                return ConsumerCopyMessage;
                            case ReceiptCopyType.StoreCopy:
                                return StoreCopyMessage;
                        }
                    }
                    return null;
                case "EFDINQUIRYURL":
                    return fiscalDocument.ConsumerEFDocInquiryUrl;
                case "EFDNOTIDENTIFIEDCUSTOMERMESSAGE_BR":
                    if (string.IsNullOrWhiteSpace(fiscalDocument.ThirdPartyCnpjCpf)
                        && string.IsNullOrWhiteSpace(fiscalDocument.ThirdPartyForeignerId))
                    {
                        return ConsumerNotIdentifiedMessage;
                    }
                    return null;
                case "EFDQRCODE_BR":
                    if (string.IsNullOrWhiteSpace(fiscalDocument.QrcodeText))
                    {
                        return null;
                    }
                    return "<Q: " + fiscalDocument.QrcodeText + ">";
                case "EFDOCACCESSKEY_BR":
                    return Regex.Replace(fiscalDocument.AccessKey, ".{4}", "$0 ");
                case "EFDTOTALITEMQUANTITY_BR":
                    return fiscalDocument.Lines.Count.ToString(CultureInfo.InvariantCulture);
                case "FISCALDOCUMENTCUSTOMERADDRESS_BR":
                    return GetCustomerAddress();
                case "FISCALDOCUMENTCUSTOMERDOCUMENT_BR":
                    if(!string.IsNullOrWhiteSpace(fiscalDocument.ThirdPartyCnpjCpf))
                    {
                        return fiscalDocument.ThirdPartyCnpjCpf;
                    }
                    else if (!string.IsNullOrWhiteSpace(fiscalDocument.ThirdPartyForeignerId))
                    {
                        return fiscalDocument.ThirdPartyForeignerId;
                    }
                    return null;
                case "FISCALDOCUMENTCUSTOMERDOCUMENTTYPE_BR":
                    if (!string.IsNullOrWhiteSpace(fiscalDocument.ThirdPartyCnpjCpf))
                    {
                        return "CNPJ/CPF:";
                    }
                    else if (!string.IsNullOrWhiteSpace(fiscalDocument.ThirdPartyForeignerId))
                    {
                        return "ID Estrangeiro:";
                    }
                    return null;
                case "FISCALDOCUMENTCUSTOMERNAME_BR":
                    return fiscalDocument.ThirdPartyName;
                case "FISCALDOCUMENTDATE_BR":
                    return fiscalDocument.FiscalDocumentDateTime.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                case "FISCALDOCUMENTDISCOUNTTOTAL_BR":
                    return Printing.InternalApplication.Services.Rounding.Round(fiscalDocument.TotalDiscountAmount, retailTransaction.StoreCurrencyCode, true);
                case "FISCALDOCUMENTESTABLISHMENTNAME_BR":
                    return fiscalDocument.FiscalEstablishmentName;
                case "FISCALDOCUMENTNUMBER_BR":
                    return fiscalDocument.FiscalDocumentNumber;
                case "FISCALDOCUMENTPROTOCOLMESSAGE_BR":
                    if (fiscalDocument.ContingencyMode == (int)IssueType.Regular)
                    {
                        return FiscalDocumentProtocolMessage;
                    }
                    return null;
                case "FISCALDOCUMENTSERIES_BR":
                    return fiscalDocument.FiscalDocumentSeries;
                case "FISCALDOCUMENTTAXTOTAL_BR":
                    var totalAproximateTaxAmount = fiscalDocument.Lines.Sum(x => x.ApproximateTaxAmount);
                    return Printing.InternalApplication.Services.Rounding.Round(totalAproximateTaxAmount,
                        retailTransaction.StoreCurrencyCode, true);
                case "FISCALDOCUMENTTOTALGOODSAMOUNT_BR":
                    return Printing.InternalApplication.Services.Rounding.Round(fiscalDocument.TotalGoodsAmount, retailTransaction.StoreCurrencyCode, true);
                case "IENUM_BR":
                    return fiscalDocument.FiscalEstablishmentIe;
            }

            return null;
        }

        private string GetCustomerAddress()
        {
            if (string.IsNullOrWhiteSpace(fiscalDocument.ThirdPartyCnpjCpf)
                && string.IsNullOrWhiteSpace(fiscalDocument.ThirdPartyForeignerId))
            {
                return null;
            }

            return string.Format(ThirdPartyAddressFormat, 
                fiscalDocument.ThirdPartyAddressStreet,
                !string.IsNullOrWhiteSpace(fiscalDocument.ThirdPartyAddressStreetNumber) ? string.Concat(", ", fiscalDocument.ThirdPartyAddressStreetNumber) : string.Empty,
                !string.IsNullOrWhiteSpace(fiscalDocument.ThirdPartyAddressCity) ? string.Concat("- ",fiscalDocument.ThirdPartyAddressCity) : string.Empty,
                fiscalDocument.ThirdPartyAddressState,
                !string.IsNullOrWhiteSpace(fiscalDocument.ThirdPartyAddressZipCode) ? string.Concat("CEP:", fiscalDocument.ThirdPartyAddressZipCode, " ") : string.Empty,
                fiscalDocument.ThirdPartyAddressCountry);
        }

        private void GetFiscalDocumentFromRetailTransaction(RetailTransaction retailTransaction)
        {
            if (fiscalDocument == null
                || fiscalDocument.TransactionId != retailTransaction.TransactionId
                || fiscalDocument.StoreId != retailTransaction.StoreId
                || fiscalDocument.TerminalId != retailTransaction.TerminalId
                || fiscalDocument.ChannelId != retailTransaction.ChannelId)
            {
                using (var sqlConnection = new SqlConnection(connectionString))
                {
                    using (var efdDbContext = new EfdDbContext(sqlConnection))
                    {
                        var fiscalDocumentData = new FiscalDocumentData(efdDbContext);

                        fiscalDocument = fiscalDocumentData.GetFiscalDocumentFromRetailTransaction(retailTransaction.StoreId,
                    retailTransaction.TerminalId, retailTransaction.TransactionId, retailTransaction.ChannelId);
                    }
                }
            }
        }
    }
}
