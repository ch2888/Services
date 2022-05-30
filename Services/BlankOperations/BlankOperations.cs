/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/


using System.ComponentModel.Composition;
using System.Text;
using System.Windows.Forms;
using LSRetailPosis;
using LSRetailPosis.POSProcesses.Common;
using LSRetailPosis.Settings.FunctionalityProfiles;
using Microsoft.Dynamics.Retail.Pos.Contracts;
using Microsoft.Dynamics.Retail.Pos.Contracts.BusinessObjects;
using Microsoft.Dynamics.Retail.Pos.Contracts.DataEntity;
using Microsoft.Dynamics.Retail.Pos.Contracts.Services;
using System;
using LSRetailPosis.Transaction;
using LSRetailPosis.Transaction.Line.SaleItem;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Dynamics.Retail.Pos.SystemCore;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using LSRetailPosis.POSProcesses;
using Microsoft.Dynamics.Retail.Pos.Contracts.Triggers;
using UnifonicNextGen.Standard.Controllers;
using UnifonicNextGen.Standard.Models;
using UnifonicNextGen.Standard.Exceptions;
using UnifonicNextGen.Standard.Http.Response;
using SQLServerCrypto;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text.RegularExpressions;

using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;


namespace Microsoft.Dynamics.Retail.Pos.BlankOperations
{

    [Export(typeof(IBlankOperations))]
    public sealed class BlankOperations : IBlankOperations
    {
        private static readonly HttpClient client = new HttpClient();
        private string m_strTokenType;
        private string m_strAccessToken;
        private string m_strPN;
        private string m_strPN4Qitaf;
        private string m_strAmount;
        private string m_strResidueAmount4RewardUdt;
        private string m_strOTPValue;
        private string m_strOTPToken;
        private string m_strTransactionID;
        private string m_strTransactionDate;
        private string m_strTransactionType;
        private string m_strStoreId4POS;
        private string m_strStore4Alrajhi;
        private string m_strTansactionID4POS;
        private string m_strRequestID;
        private bool m_bSuccessOnReverse;
        private bool m_bSuccessOnRedeem;
        private bool m_bSuccessOnAuthCustomer;
        private bool m_bSuccessOnGenToken;
        private string m_strRetMessage;
        private string m_strErrCode;
        private int m_nResponseCode_QitafApi2;
        private string m_strRefRequestId;
        private string m_strRefRequestDate;
        private string m_strRewardAmount;
        private string m_strReductionAmount;
        private string m_strTransactionGUIDReward;
        private string m_strTransactionGUIDRedeem;
        private string m_strEncInvoice;

        [Import]
        public IApplication Application { get; set; }

        // Get all text through the Translation function in the ApplicationLocalizer
        // TextID's for BlankOperations are reserved at 50700 - 50999


        private const String strDllName = @"madaapi_v1_8_32.dll";

        #region IBlankOperations Members
        [DllImport(strDllName, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr api_RequestCOMTrxn(int port, int Rate, int x, int y, int z,
            byte[] inOutBuff, byte[] intval, int trnxType, byte[] panNo, byte[] purAmount, byte[] stanNo, byte[] dataTime,
            byte[] expDate, byte[] trxRrn, byte[] authCode, byte[] rspCode,
            byte[] terminalId, byte[] schemeId, byte[] merchantId, byte[] addtlAmount, byte[] ecrrefno, byte[] version,
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder outResp, byte[] outRespLen);


        [DllImport(strDllName, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr api_CommTest(int port, int Rate, int bParity, int bDataBits, int bStopBits, byte[] inReqBuff,
    byte[] inReqLen);

        [DllImport(strDllName, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr api_CheckStatus(int port, int Rate, int bParity, int bDataBits, int bStopBits, byte[] inReqBuff,
            byte[] inReqLen);
        //int api_CheckStatus(BYTE bPort, DWORD dwBaudRate, BYTE bParity, BYTE bDataBits,
        // BYTE bStopBits, unsigned char* inReqBuff, int* inReqLen)


        [DllImport(strDllName, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr api_TCPIPCheckStatus(byte[] bIPAddress, byte[] port, byte[] inReqBuff, byte[] inReqLen);
        // int api_TCPIPCheckStatus (BYTE *bIPAddress, BYTE *bPort, BYTE* inReqBuff, int *inReqLen)

        [DllImport(strDllName, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr api_RequestTCPIPTrxn(byte[] ip, byte[] port, byte[] inOutBuff, byte[] intval,
            int trnxType, byte[] panNo, byte[] purAmount, byte[] stanNo, byte[] dataTime, byte[] expDate, byte[] trxRrn, byte[] authCode,
            byte[] rspCode, byte[] terminalId, byte[] schemeId, byte[] merchantId, byte[] addtlAmount,
            byte[] ecrrefno, byte[] version, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder outResp, byte[] outRespLen);
        /*        int api_RequestTCPIPTrxn(BYTE* bIPAddress, BYTE* bPort, BYTE* inReqBuff, int * inReqLen, 
                    int txtype, BYTE* panNo, BYTE* purAmount, BYTE* stanNo, BYTE* dataTime,
                    BYTE* expDate, BYTE* trxRrn, BYTE* authCode, BYTE* rspCode, BYTE* terminalId, 
                    BYTE* schemeId, BYTE* merchantId, BYTE* addtlAmount, BYTE* ecrrefno, BYTE* version,
                    wchar_t* outresp, int* outRspLen)
                    */

        public int num = 0;
        public int code = 0;
        private string strCardNumber;// = "966560687581";//PN number
        private double fDiscountLimitPerYear;
        private double fDiscountPercentage;
        private double fSumDiscount4Year;
        private int ndiscount30;
        private int ndiscount34;
        private int ndiscount8;
        private int ndiscount9;
        private int nelsediscount;

        private int test_CommTest(int port)
        {
            int ret = 0;
            string TextString;

            TextString = "14!";
            byte[] inOutBuff2 = System.Text.Encoding.ASCII.GetBytes(TextString);

            byte[] intval2 = new byte[1];
            intval2[0] = (byte)inOutBuff2.Length;

            var Result = api_CommTest(port, 38400, 0, 8, 0, inOutBuff2, intval2);
            ret = (int)Result.ToInt64();
            //api_CommTest(4, 38400, 0, 8, 0, inOutBuff2, intval2);
            //Trans_richTextBox.AppendText("CommTest Result: " + ret.ToString());
            //ret = Result.ToInt32();
            return ret;
        }


        private int api_comstatus(int port)
        {
            int ret = 0;
            string TextString;

            TextString = "04!";
            byte[] inOutBuff2 = System.Text.Encoding.ASCII.GetBytes(TextString);

            byte[] intval2 = new byte[1];
            intval2[0] = (byte)inOutBuff2.Length;

            var Result = api_CheckStatus(port, 38400, 0, 8, 0, inOutBuff2, intval2);
            ret = (int)Result.ToInt64();
            //api_CommTest(4, 38400, 0, 8, 0, inOutBuff2, intval2);
            //Trans_richTextBox.AppendText("ComStatus Result: " + ret.ToString());
            return ret;
        }
        private int do_TransactionByCOMM(int nPort, String strAmount, out string strSchemeId, out int nRespondCode, out bool bApproved)
        {
            string TextString;
            byte[] intval = new byte[1];
            byte[] panNo = new byte[23];
            byte[] purAmount = new byte[13];
            byte[] stanNo = new byte[7];
            byte[] dataTime = new byte[13];
            byte[] expDate = new byte[5];
            byte[] trxRrn = new byte[13];
            byte[] authCode = new byte[7];
            byte[] rspCode = new byte[4];
            byte[] terminalId = new byte[17];
            byte[] schemeId = new byte[3];
            byte[] merchantId = new byte[16];
            byte[] addtlAmount = new byte[13];
            byte[] ecrrefno = new byte[17];
            byte[] version = new byte[10];
            byte[] outRespLen = new byte[1];
            StringBuilder outResp = new StringBuilder(15000);

            int trnxType = 0;
            TextString = strAmount + ";1;1!";
            byte[] inOutBuff = System.Text.Encoding.ASCII.GetBytes(TextString);
            intval[0] = (byte)inOutBuff.Length;

            var Result = api_RequestCOMTrxn(nPort, 38400, 0, 8, 0, inOutBuff, intval, trnxType, panNo, purAmount, stanNo, dataTime,
                expDate, trxRrn, authCode, rspCode, terminalId, schemeId, merchantId, addtlAmount, ecrrefno, version,
                outResp, outRespLen);
            int ret = Result.ToInt32();

            if (ret != 0)
            {
                nRespondCode = -10000;
                strSchemeId = " ";
                bApproved = false;
                return ret;
            }
            string result = System.Text.Encoding.UTF8.GetString(panNo);
            //Trans_richTextBox.AppendText("PAN NO: " + result);

            string strRspCode = System.Text.Encoding.UTF8.GetString(rspCode);
            nRespondCode = Int32.Parse(strRspCode);
            bApproved = showMessageForm_SAMA(nRespondCode);

            strSchemeId = System.Text.Encoding.Default.GetString(schemeId);
            int i = strSchemeId.IndexOf('\0');
            if (i >= 0) strSchemeId = strSchemeId.Substring(0, i);
            //Trans_richTextBox.AppendText("\nCARD ID: " + result);


            switch (strSchemeId)
            {
                case "VC":
                    strSchemeId = "2";
                    break;
                case "DM":
                    strSchemeId = "5";
                    break;
                case "MC":
                    strSchemeId = "7";
                    break;
                case "AX":
                    strSchemeId = "8";
                    break;
                case "P1":
                    strSchemeId = "9";
                    break;
                case "GN":
                    strSchemeId = "18";
                    break;
                case "UP":
                    strSchemeId = "19";
                    break;
                default:
                    break;
            }


            return ret;

            //MessageBox.Show(System.Text.Encoding.UTF8.GetString(schemeId));
        }


        private int do_TransactionByIP(String strAmount)
        {
            int ret;
            string TextString;
            string ip;
            string port;
            byte[] intval = new byte[3];
            byte[] panNo = new byte[23];
            byte[] purAmount = System.Text.Encoding.ASCII.GetBytes(strAmount);// new byte[13];
            byte[] stanNo = new byte[7];
            byte[] dataTime = new byte[13];
            byte[] expDate = new byte[5];
            byte[] trxRrn = new byte[13];
            byte[] authCode = new byte[7];
            byte[] rspCode = new byte[4];
            byte[] terminalId = new byte[17];
            byte[] schemeId = new byte[3];
            byte[] merchantId = new byte[16];
            byte[] addtlAmount = new byte[13];
            byte[] ecrrefno = new byte[17];
            byte[] version = new byte[10];
            byte[] outRespLen = new byte[1];
            StringBuilder outResp = new StringBuilder(15000);

            int trnxType = 0;
            TextString = "50;0;1!";
            ip = "192.168.43.171";
            port = "255";
            byte[] inOutBuff = System.Text.Encoding.ASCII.GetBytes(TextString);
            byte[] b_ip = System.Text.Encoding.ASCII.GetBytes(ip);
            byte[] b_port = System.Text.Encoding.ASCII.GetBytes(port);
            intval[0] = (byte)inOutBuff.Length;

            var Result = api_RequestTCPIPTrxn(b_ip, b_port, inOutBuff, intval, trnxType, panNo, purAmount, stanNo, dataTime,
                expDate, trxRrn, authCode, rspCode, terminalId, schemeId, merchantId, addtlAmount, ecrrefno, version, outResp, outRespLen);
            MessageBox.Show(Result.ToString());

            string result = System.Text.Encoding.UTF8.GetString(panNo);
            //Trans_richTextBox.AppendText("PAN NO: " + result);

            result = System.Text.Encoding.UTF8.GetString(schemeId);
            //Trans_richTextBox.AppendText("\nCARD ID: " + result);



            MessageBox.Show(System.Text.Encoding.UTF8.GetString(schemeId));
            ret = Result.ToInt32();
            return ret;
        }

        private int checkTerminalStatus_byIP()
        {
            int ret;
            string TextString;
            string ip;
            string port;
            byte[] intval = new byte[5];
            TextString = "50;0;1!";
            ip = "192.168.43.171";
            port = "255";
            byte[] inOutBuff = System.Text.Encoding.ASCII.GetBytes(TextString);
            byte[] b_ip = System.Text.Encoding.ASCII.GetBytes(ip);
            byte[] b_port = System.Text.Encoding.ASCII.GetBytes(port);
            intval[0] = (byte)inOutBuff.Length;
            var Result = api_TCPIPCheckStatus(b_ip, b_port, inOutBuff, intval);
            ret = Result.ToInt32();

            MessageBox.Show(Result.ToString());

            return ret;
        }
        private int checkTerminalStatus_byCOM(int nPort)
        {
            int ret;
            string TextString;
            byte[] intval = new byte[4];
            TextString = "04!";
            byte[] inOutBuff = System.Text.Encoding.ASCII.GetBytes(TextString);
            intval[0] = (byte)inOutBuff.Length;

            var Result = api_CheckStatus(nPort, 38400, 0, 8, 0, inOutBuff, intval);
            ret = Result.ToInt32();

            MessageBox.Show(Result.ToString());

            return ret;
        }

        private void showMessageForm_Api(int nApiRet)
        {
            StringBuilder comments2 = new StringBuilder(128);
            switch (nApiRet)
            {
                case -1: comments2.AppendFormat("-1:Library Failed  "); break;
                case -2: comments2.AppendFormat("-2:No Response Receivedلم يتلقى رد  "); break;
                case -3: comments2.AppendFormat("-3:Not able to open port المنفذ غير صحيح  "); break;
                case -4: comments2.AppendFormat("-4:Acknowledgement Failed فشل الإقرار  "); break;
                case 1: comments2.AppendFormat("1:Terminal TMS not Loaded "); break;
                case 2: comments2.AppendFormat("2:Blocked Cardالبطاقة محظورة  "); break;
                case 3: comments2.AppendFormat("3:No Active Application Foundالبرنامج غير نشط  "); break;
                case 4: comments2.AppendFormat("4 :Card Read Error غير قادر على قراءة الكارت"); break;
                case 5: comments2.AppendFormat("5:Insert Card Onlyادخل البطاقة  "); break;
                case 6: comments2.AppendFormat("6:Maximum Amount Limit Exceededتم تجاوز الحد الأقصى للمبلغ  "); break;
                case 7: comments2.AppendFormat("6:PIN Quitادخل الرقم السري  "); break;
                case 8: comments2.AppendFormat("8: User Cancelled or Timeout تم إلغاء العملية أو إنقضى الوقت"); break;
                case 9: comments2.AppendFormat("9:Data Errorخطأ في البيانات  "); break;
                case 10: comments2.AppendFormat("10:Card Scheme Not Supportedالبطاقة غير معتمدة  "); break;
                case 11: comments2.AppendFormat("11:Terminal Busyالشبكة مشغولة  "); break;
                case 12: comments2.AppendFormat("12:Paper Outنفذ الورق   "); break;
                case 13: comments2.AppendFormat("13:No Reconciliation Record foundلايوجد سجل تسوية   "); break;
                case 14: comments2.AppendFormat("14:Transaction Cancelledتم الغاء العملية  "); break;
                case 15: comments2.AppendFormat("15:De-SAF Processing "); break;
                case 16: comments2.AppendFormat("16:Transaction Not Allowedالتحويل غير مسموح   "); break;
                case 17: comments2.AppendFormat("17:Reconciliation Failedفشلت العملية   "); break;


                default: comments2.AppendFormat("Unknown Issue."); break;
            }
            comments2.AppendLine();
            using (LSRetailPosis.POSProcesses.frmMessage dialog = new LSRetailPosis.POSProcesses.frmMessage(comments2.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Stop))
            {
                LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog);
            }
        }

        private bool showMessageForm_Qitaf(string strMsgPrefix, int nRetCode)
        {
            bool nSuccess = true;
            StringBuilder comments2 = new StringBuilder(128);
            comments2.AppendFormat(strMsgPrefix);
            switch (nRetCode)
            {
                case 000: return nSuccess;// comments2.AppendFormat("Approved  ");break;
                case 100: comments2.AppendFormat("100:Do not honor "); break;
                case 101: comments2.AppendFormat("101:Expired card البطاقة منتهية الصلاحية   "); break;
                case 1: comments2.AppendFormat("1:Unhandled Exception استثناء غير معالج	"); break;
                case 9: comments2.AppendFormat("9:Invalid Msisdn msisdn غير صالح	"); break;
                case 10: comments2.AppendFormat("10:Invalid Language Code رمز الغة غير صالح	"); break;
                case 801: comments2.AppendFormat("801:Reward Transaction is Pending For Posting معاملة المكافأة معلقة للترحيل	"); break;
                case 802: comments2.AppendFormat("802:Reward Transaction is Posted تم نشر معاملة المكافأة	"); break;
                case 803: comments2.AppendFormat("803:Reward Transaction is Rejected تم رفض معاملة المكافأة	"); break;
                case 804: comments2.AppendFormat("804:Reward Transaction is Cancelled تم إلغاء معاملة المكافأة	"); break;
                case 905: comments2.AppendFormat("905:Batch Is Null الدفعة لاغية	"); break;
                case 906: comments2.AppendFormat("906:Reward Trnx Not Found لم يتم العثور على مكافأة 	"); break;
                case 907: comments2.AppendFormat("907:Validation Error : RefRequestDate خطأ في التحقق من الصحة: ​​تعويضات	"); break;
                case 908: comments2.AppendFormat("908:Validation Error : RefRequestId خطأ في التحقق من الصحة: ​​Refrequestid	"); break;
                case 909: comments2.AppendFormat("909:Can Not Void Reward Old Date لا يمكن إبطال تاريخ المكافأة القديم	"); break;
                case 910: comments2.AppendFormat("910:Loyalty Service Exception استثناء خدمة الولاء	"); break;
                case 911: comments2.AppendFormat("911:Loyalty Service Database Exception استثناء قاعدة بيانات خدمة الولاء	"); break;
                case 912: comments2.AppendFormat("912:Loyalty Service Started Successfully بدأت خدمة الولاء بنجاح	"); break;
                case 914: comments2.AppendFormat("914:Customer Not Found العميل غير موجود	"); break;
                case 915: comments2.AppendFormat("915:Can Not Refund Reward Before Batch Closed لا يمكن استرداد المكافأة قبل إغلاق الدفعة	"); break;
                case 916: comments2.AppendFormat("916:Customer already exists العملاء موجود بالفعل	"); break;
                case 917: comments2.AppendFormat("917:Customer not found العميل غير موجود	"); break;
                case 918: comments2.AppendFormat("918:Invalid password رمز مرور خاطئ	"); break;
                case 919: comments2.AppendFormat("919:Register not found سجل غير موجود	"); break;
                case 920: comments2.AppendFormat("920:Register and Account already related التسجيل والحساب مرتبطان بالفعل	"); break;
                case 921: comments2.AppendFormat("921:Register related to another account تسجيل مرتبط بحساب آخر	"); break;
                case 922: comments2.AppendFormat("922:Register Blocked تسجيل محظور	"); break;
                case 923: comments2.AppendFormat("923:Mail address in use عنوان البريد قيد الاستخدام	"); break;
                case 924: comments2.AppendFormat("924:Card not available البطاقة غير متوفرة	"); break;
                case 925: comments2.AppendFormat("925:Register is not active التسجيل غير نشط	"); break;
                case 926: comments2.AppendFormat("926:Customer is not active العميل غير نشط	"); break;
                case 927: comments2.AppendFormat("927:Invalid activation code رمز التفعيل غير صالح	"); break;
                case 928: comments2.AppendFormat("928:Customer not activated العميل غير مفعل	"); break;
                case 929: comments2.AppendFormat("929:Customer is active العميل نشط	"); break;
                case 930: comments2.AppendFormat("930:Card not created لم يتم إنشاء البطاقة	"); break;
                case 931: comments2.AppendFormat("931:Too many rows affected الكثير من الصفوف المتضررة	"); break;
                case 932: comments2.AppendFormat("932:Invalid query parameters معلمات الاستعلام غير صالحة	"); break;
                case 933: comments2.AppendFormat("933:Invalid affected row count عدد الصف المتأثر غير صالح	"); break;
                case 934: comments2.AppendFormat("934:Cache not loaded مخبأ غير محمل	"); break;
                case 935: comments2.AppendFormat("935:Card already exist البطاقة موجودة بالفعل	"); break;
                case 936: comments2.AppendFormat("936:Undefined register segment مقطع تسجيل غير محدد	"); break;
                case 937: comments2.AppendFormat("937:Account not found الحساب غير موجود	"); break;
                case 938: comments2.AppendFormat("938:Reward detail not found or updated تفاصيل المكافأة غير موجودة أو محدثة	"); break;
                case 939: comments2.AppendFormat("939:Register Balance not found or updated تسجيل الرصيد غير موجود أو محدث	"); break;
                case 940: comments2.AppendFormat("940:Register issuer balance not found or updated سجل رصيد المصدر غير موجود أو محدث	"); break;
                case 941: comments2.AppendFormat("941:Reward info not found معلومات المكافأة غير موجودة	"); break;
                case 942: comments2.AppendFormat("942:Lookup item(s) not found لم يتم العثور على عنصر (عناصر) البحث	"); break;
                case 1001: comments2.AppendFormat("1001:Validation Error : RequestId خطأ في التحقق من الصحة:	"); break;
                case 1002: comments2.AppendFormat("1002:Validation Error : RequestDate خطأ في التحقق من الصحة: ​​requestdate	"); break;
                case 1003: comments2.AppendFormat("1003:Validation Error : BranchId خطأ في التحقق من الصحة:	"); break;
                case 1004: comments2.AppendFormat("1004:Validation Error : TerminalId خطأ في التحقق من الصحة: ​​TerminalID	"); break;
                case 1005: comments2.AppendFormat("1005:Not Enrolled لم يدرج اسمه	"); break;
                case 1006: comments2.AppendFormat("1006:Existing request : OTP used طلب موجود: OTP المستخدمة	"); break;
                case 1007: comments2.AppendFormat("1007:الرمز المدخل غير صحيح"); break;
                case 1008: comments2.AppendFormat("1008:Resend إعادة إرسال	"); break;
                case 1009: comments2.AppendFormat("1009:Ratio Error نسبة خطأ	"); break;
                case 1010: comments2.AppendFormat("1010:Limit Error حد الخطأ	"); break;
                case 1011: comments2.AppendFormat("1011:Min Max Amount Error الحد الأدنى للخطأ في المبلغ	"); break;
                case 1012: comments2.AppendFormat("1012:تم الوصول للحد الأعلى الشهري"); break;
                case 1013: comments2.AppendFormat("1013:Dealer Not Found تاجر غير موجود	"); break;
                case 1014: comments2.AppendFormat("1014:لا يوجد رصيد كاف لإتمام العملية"); break;
                case 1015: comments2.AppendFormat("1015:تم استخدام الرمز مسبقاً"); break;
                case 1016: comments2.AppendFormat("1016:Terminal and Branch Not Match المحطة والفرع غير متطابقين	"); break;
                case 1017: comments2.AppendFormat("1017:Land Line Customer Not Allowed الخط الأرضي العميل غير مسموح به	"); break;
                case 1018: comments2.AppendFormat("1018:العملية غير موجودة"); break;
                case 1019: comments2.AppendFormat("1019:العميل غير مشترك في برنامج قطاف. للاشتراك أرسل الرمز 201 برسالة نصية إلى 900"); break;
                case 1020: comments2.AppendFormat("1020:Reward Point Must Be Greater Than Zero يجب أن تكون نقطة المكافآت أكبر من الصفر	"); break;
                case 1021: comments2.AppendFormat("لا يمكن إلغاء العملية"); break;
                case 1022: comments2.AppendFormat("1022:NonSTC Not Allowed nonstc غير مسموح بها	"); break;
                case 1030: comments2.AppendFormat("1030:تم إرسال الرمز للعميل"); break;
                case 1040: comments2.AppendFormat("1040:تم إلغاء العملية مسبقا "); break;
                case 1041: comments2.AppendFormat("1041:Refund Period Has Expired انتهت فترة الاسترداد	"); break;
                case 1042: comments2.AppendFormat("1042:Msisdn Does Not Match With Original Transaction msisdn لا يتطابق مع المعاملة الأصلية	"); break;
                case 1044: comments2.AppendFormat("1044:Reduction Amount Must Be Positive يجب أن يكون مبلغ التخفيض موجبًا	"); break;
                case 1045: comments2.AppendFormat("1045:Reward Is Already Cancelled تم إلغاء المكافأة بالفعل	"); break;
                case 1046: comments2.AppendFormat("1046:EBU Customers Can Not Make Rewarding لا يمكن لعملاء EBU تقديم المكافآت	"); break;
                case 1047: comments2.AppendFormat("1047:Earn Transaction Already Succeeded كسب المعاملة نجحت بالفعل	"); break;
                case 1048: comments2.AppendFormat("1048:Earn Update Transaction Already Succeeded كسب تحديث المعاملة نجحت بالفعل	"); break;
                case 1049: comments2.AppendFormat("1049:Earn Reversal Transaction Already Succeeded كسب المعاملة الانعكاس نجحت بالفعل	"); break;
                case 1050: comments2.AppendFormat("1050:توجد عملية سابقة بنفس الرقم    "); break;
                case 1051: comments2.AppendFormat("1051:Earn Rejected - Duplicate Request Id كسب مرفوض - معرف طلب مكررة	"); break;
                case 1052: comments2.AppendFormat("1052:Earn Update Rejected - Duplicate RequestId كسب تحديث مرفوض - مكررة	"); break;
                case 1053: comments2.AppendFormat("1053:Earn Reversal Rejected - Duplicate RequestId كسب الانعكاس المرفوض - مكررة	"); break;
                case 1070: comments2.AppendFormat("1070:Redemption Not Allowed For This Customer الاسترداد غير مسموح به لهذا العميل	"); break;
                case 1100: comments2.AppendFormat("1100:LoyaltyLookupitemsnotfound loyaltylookupitemsnotfound.	"); break;
                case 1101: comments2.AppendFormat("1101:LoyaltyUnabletoinsertLoyaltyTrnx loyaltynabletoinsertloyaltytrnx.	"); break;
                case 1109: comments2.AppendFormat("1109:تم إرسال العملية من مصدر غير مصرح له بتنفيذ العمليات"); break;
                case 1200: comments2.AppendFormat("1200:ConfigurationObject is empty ConfignationObject فارغ	"); break;
                case 1201: comments2.AppendFormat("1201:Configuration file not found ملف التكوين غير موجود	"); break;
                case 1202: comments2.AppendFormat("1202:File header error خطأ رأس الملف	"); break;
                case 1203: comments2.AppendFormat("1203:File footer error ملف تذييل الملفات	"); break;
                case 1204: comments2.AppendFormat("1204:File name error خطأ اسم الملف	"); break;
                case 1205: comments2.AppendFormat("1205:File has already been processed تم بالفعل معالجة الملف	"); break;
                case 1300: comments2.AppendFormat("1300:Gift Card Not Found لم يتم العثور على بطاقة الهدية	"); break;
                case 1301: comments2.AppendFormat("1301:Invalid initial amount المبلغ الأولية غير صالح	"); break;
                case 1302: comments2.AppendFormat("1302:Invalid verification number رقم التحقق غير صالح	"); break;
                case 1303: comments2.AppendFormat("1303:Invalid CVN cvn غير صالح	"); break;
                case 1304: comments2.AppendFormat("1304:Gift card is passive بطاقة الهدايا سلبية	"); break;
                case 1305: comments2.AppendFormat("1305:Not enough balance لا يوجد توازن كافٍ	"); break;
                case 1306: comments2.AppendFormat("1306:Invalid volume type نوع حجم غير صالح	"); break;
                case 1307: comments2.AppendFormat("1307:Invalid volume type code رمز نوع وحدة التخزين غير صالح	"); break;
                case 1308: comments2.AppendFormat("1308:Invalid application id معرف التطبيق غير صالح	"); break;
                case 1309: comments2.AppendFormat("1309:Gift card not found for application لم يتم العثور على بطاقة الهدية للتطبيق	"); break;
                case 1310: comments2.AppendFormat("1310:Gift Constants not found لم يتم العثور على ثوابت الهدايا	"); break;
                case 1311: comments2.AppendFormat("1311:Sale Resend بيع RESEND.	"); break;
                case 1312: comments2.AppendFormat("1312:Sale Trnx Is not Found لم يتم العثور على بيع Trnx	"); break;
                case 1313: comments2.AppendFormat("1313:Sale Trnx is processing بيع TRNX المعالجة	"); break;
                case 1314: comments2.AppendFormat("1314:Reload Trnx Resended إعادة تحميل TRNX	"); break;
                case 1315: comments2.AppendFormat("1315:Reload Reversal Trnx Resended إعادة تحميل الانعكاس Trnx	"); break;
                case 1316: comments2.AppendFormat("1316:Sale Trnx Resended إعادة بيع Trnx	"); break;
                case 1317: comments2.AppendFormat("1317:GiftCard Merchant not matching التاجر GiftCard غير مطابق	"); break;
                case 1318: comments2.AppendFormat("1318:GiftCard Merchant is not defined لا يتم تعريف تاجر GiftCard	"); break;
                case 1319: comments2.AppendFormat("1319:SaleRefundInquiry Trnx Resended Salerefundinquiry Trnx	"); break;
                case 1320: comments2.AppendFormat("1320:Gift Card Already Reloaded بطاقة هدية إعادة تحميل بالفعل	"); break;
                case 1750: comments2.AppendFormat("1750:GSM ID Type is not Valid for Redemption نوع معرف GSM غير صالح للاسترداد	"); break;
                case 1751: comments2.AppendFormat("1751:LL ID Type is not Valid for Redemption LL معرف نوع غير صالح للاسترداد	"); break;
                case 1752: comments2.AppendFormat("1752:ID Type is invalid for Prepaid Redemption نوع المعرف غير صالح للفداء المدفوع مسبقا	"); break;
                case 1753: comments2.AppendFormat("1753:GSM CustomerType, SubType is invalid for Redemption GSM CustomerTtype، النوع الفرعي غير صالح للاسترداد	"); break;
                case 1754: comments2.AppendFormat("1754:LL CustomerType, SubType is invalid for Redemption LL Customertype، النوع الفرعي غير صالح للاسترداد	"); break;
                case 1755: comments2.AppendFormat("1755:Redemption Cannot be Done for Blacklisted Customer لا يمكن الاسترداد للعملاء المدرجين في القائمة السوداء	"); break;
                case 1760: comments2.AppendFormat("1760:Insufficient Member Balance رصيد عضو غير كاف	"); break;
                case 1763: comments2.AppendFormat("1763:Exceed the Monthly External Redemption count تجاوز عدد عمليات الاسترداد الخارجية الشهرية	"); break;
                case 1764: comments2.AppendFormat("1764:Exceed the Monthly External Redemption count تجاوز عدد عمليات الاسترداد الخارجية الشهرية	"); break;
                case 1765: comments2.AppendFormat("1765:Total Points Required is '' AND Total points available is” مجموع النقاط المطلوبة هو والنقاط الإجمالية المتاحة هي "); break;
                case 1801: comments2.AppendFormat("1801:Invalid TimeIntervalCriteria وقت غير صالح	"); break;
                case 1802: comments2.AppendFormat("1802:Invalid PurchaseCriteria الشراء غير صالح	"); break;
                case 1803: comments2.AppendFormat("1803:Invalid FrequencyCriteria تردد غير صالح	"); break;
                case 1804: comments2.AppendFormat("1804:Invalid OwnerCriteria مالك المالك غير صالح	"); break;
                case 1805: comments2.AppendFormat("1805:Invalid LocationCriteria موقع محام غير صالح	"); break;
                case 1806: comments2.AppendFormat("1806:AllowMultipleReward is false السماح هو خطأ	"); break;
                case 1807: comments2.AppendFormat("1807:Minute based amount fraud detected الدقيقة المبلغ المستنى بالاحتيال الكشف	"); break;
                case 1808: comments2.AppendFormat("1808:Minute based count fraud detected الدقيقة تم اكتشاف الاحتيال	"); break;
                case 1809: comments2.AppendFormat("1809:Hour based amount fraud detected ساعة المستندة إلى الاحتيال	"); break;
                case 1810: comments2.AppendFormat("1810:Hour based count fraud detected ساعة تعتمد على الاحتيال	"); break;
                case 1811: comments2.AppendFormat("1811:Day based amount fraud detected الكمية القائمة على النهار كشف الاحتيال	"); break;
                case 1812: comments2.AppendFormat("1812:Day based count fraud detected يوم التعتمد على الاحتيال الكشف	"); break;
                case 1813: comments2.AppendFormat("1813:Week based amount fraud detected الاسبوع القائم على الاحتيال المكتشف	"); break;
                case 1814: comments2.AppendFormat("1814:Week based count fraud detected الأسبوع القائم على الاحتيال الكشف عن الاحتيال	"); break;
                case 1815: comments2.AppendFormat("1815:Month based amount fraud detected الشهر القائم على الاحتيال المكتشف	"); break;
                case 1816: comments2.AppendFormat("1816:Month based count fraud detected الشهر القائم على الاحتيال الكشف	"); break;
                case 1817: comments2.AppendFormat("1817:Year based amount fraud detected الكمية القائمة على السنة المكتشفة	"); break;
                case 1818: comments2.AppendFormat("1818:Year based count fraud detected سنة التعتمد على الاحتيال الكشف	"); break;
                case 1819: comments2.AppendFormat("1819:Fraud Rewarding set to false الاحتيال المكافئ مجموعة كاذبة	"); break;
                case 1820: comments2.AppendFormat("1820:Transactional amount fraud detected اكتشاف كمية المعاملات	"); break;
                case 1821: comments2.AppendFormat("1821:Transactional count fraud detected عملية الاحتيال عدد المعاملات	"); break;
                case 1822: comments2.AppendFormat("1822:Reward re send إعادة إرسال المكافأة	"); break;
                case 1823: comments2.AppendFormat("1823:Invalid Dynamic Reward Criteria معايير مكافأة ديناميكية غير صالحة	"); break;
                case 1824: comments2.AppendFormat("1824:Invalid Amount مبلغ غير صحيح	"); break;
                case 1825: comments2.AppendFormat("1825:Segment not related الجزء غير مرتبط	"); break;
                case 1829: comments2.AppendFormat("1829:Zero Value Not Rewarded القيمة الصفرية لا تكافأ	"); break;
                case 1844: comments2.AppendFormat("1844:Invalid DC EDUCATION_ID التعليم DC غير صالح_	"); break;
                case 1845: comments2.AppendFormat("1845:Invalid DC GENDER bender dc غير صالح	"); break;
                case 1846: comments2.AppendFormat("1846:Invalid DC MARTIAL_STATUS DC Martial_Status غير صالح	"); break;
                case 1847: comments2.AppendFormat("1847:Invalid DC NATIONALITY جنسية dc غير صالحة	"); break;
                case 1849: comments2.AppendFormat("1849:Invalid DC RESIDENCE_IN_COUNTRY dc residence_in_country غير صالح	"); break;
                case 1910: comments2.AppendFormat("1910:No Limit Defined لا يوجد حد محدد	"); break;
                case 1913: comments2.AppendFormat("1913:Prepaid Customer Not Allowed غير مسموح لعملاء الدفع المسبق	"); break;
                case 1925: comments2.AppendFormat("1925:Corporate Montly Limit Exceeded تم تجاوز حد Monty Corpory	"); break;
                case 2000: comments2.AppendFormat("2000:Received Pos Request Data بيانات طلب نقاط البيع المستلمة	"); break;
                case 2001: comments2.AppendFormat("2001:Serialized Pos Request Data طلب بيانات POS التسلسلي	"); break;
                case 2002: comments2.AppendFormat("2002:Pos Channel Exception استثناء قناة POS.	"); break;
                case 2003: comments2.AppendFormat("2003:Validation Pos Request Error التحقق من صحة خطأ طلب POS	"); break;
                case 2004: comments2.AppendFormat("2004:Validation Missing Object Field Error التحقق من صحة خطأ في مفقود كائن	"); break;
                case 2005: comments2.AppendFormat("2005:Validation Format Error خطأ تنسيق التحقق من الصحة	"); break;
                case 2006: comments2.AppendFormat("2006:POS Unknown operation نقاط البيع غير معروف	"); break;
                case 2310: comments2.AppendFormat("2310:GetCustomerDetails STC error (STC returned 0001) GetCustomerDetails STC خطأ (STC عاد 0001)	"); break;
                case 2311: comments2.AppendFormat("2311:GetCustomerDetails integration error (such as timeout) GetCustomerDetails خطأ تكامل (مثل المهلة)	"); break;
                case 2501: comments2.AppendFormat("2501:Invalid System Node ID معرف عقدة النظام غير صالح	"); break;
                case 2502: comments2.AppendFormat("2502:Invalid Device ID معرف الجهاز غير صالح	"); break;
                case 2503: comments2.AppendFormat("2503:Invalid Device Batch ID معرف دفعة الجهاز غير صالح	"); break;
                case 2504: comments2.AppendFormat("2504:Invalid User Device Info معلومات جهاز المستخدم غير صالحة	"); break;
                case 2505: comments2.AppendFormat("2505:Batch Close Trnx Resended دفعة إغلاق TRNX	"); break;
                case 2506: comments2.AppendFormat("2506:Batch Close Conflict دفعة إغلاق الصراع	"); break;
                case 2507: comments2.AppendFormat("2507:Ref Transaction Not Found المعاملة المرجع غير موجودة	"); break;
                case 2508: comments2.AppendFormat("2508:Reward Void Trnx Resended مكافأة الفراغ Trnx	"); break;
                case 2509: comments2.AppendFormat("2509:Batch ID Not Found معرف الدفعات غير موجود	"); break;
                case 2510: comments2.AppendFormat("2510:Insufficient Balance رصيد غير كاف	"); break;
                case 2511: comments2.AppendFormat("2511:Gift info not found. معلومات الهدية غير موجودة.	"); break;
                case 2512: comments2.AppendFormat("2512:Redemption Trnx Resended استرداد Trnx	"); break;
                case 2513: comments2.AppendFormat("2513:Redemption Void Trnx Resended استرداد الفراغ Trnx	"); break;
                case 2514: comments2.AppendFormat("2514:Redemption Trnx not found الفداء trnx غير موجود	"); break;
                case 2515: comments2.AppendFormat("2515:Stan in use ستان في الاستخدام	"); break;
                case 2516: comments2.AppendFormat("2516:Batch already closed دفعة مغلق بالفعل	"); break;
                case 2517: comments2.AppendFormat("2517:Batch not closed دفعة غير مغلق	"); break;
                case 2518: comments2.AppendFormat("2518:Redemption details not found تفاصيل الاسترداد غير موجود	"); break;
                case 2519: comments2.AppendFormat("2519:Invalid amount for gift كمية غير صالحة للهدايا	"); break;
                case 2520: comments2.AppendFormat("2520:Gift Order Resended طلب هدية	"); break;
                case 2521: comments2.AppendFormat("2521:Invalid Redemption Refund Inquiry ID استرداد استرداد الاسترداد غير صالح	"); break;
                case 2522: comments2.AppendFormat("2522:Operation not allowed العملية غير مسموح بها	"); break;
                case 2523: comments2.AppendFormat("2523:Redemption Refund Trnx Resended استرداد استرداد TRNX	"); break;
                case 2524: comments2.AppendFormat("2524:Transaction status is not success حالة المعاملة ليست ناجحة	"); break;
                case 2525: comments2.AppendFormat("2525:Invalid Reward Refund Inquiry ID مكافأة استرداد مكافأة غير صالحة	"); break;
                case 2526: comments2.AppendFormat("2526:Reward Refund Trnx Resended إعادة استرداد المكافأة Trnx	"); break;
                case 2527: comments2.AppendFormat("2527:Invalid parameters معلمات غير صالحة	"); break;
                case 2530: comments2.AppendFormat("2530:Register Expired سجل انتهاء الصلاحية	"); break;
                case 2531: comments2.AppendFormat("2531:Batch Closed دفعة مغلق	"); break;
                case 2533: comments2.AppendFormat("2533:Currency Code Invalid كود العملة غير صالح	"); break;
                case 2537: comments2.AppendFormat("2537:Loyalty Daily exchange rate not found سعر الصرف اليومي الولاء غير موجود	"); break;
                case 2538: comments2.AppendFormat("2538:Loyalty Money point exchange rate no found لم يتم العثور على سعر صرف نقاط الولاء	"); break;
                case 2566: comments2.AppendFormat("2566:Undefined Merchant التاجر غير محدد	"); break;
                case 2582: comments2.AppendFormat("2582:Invalid Currency عملة غير صالحة	"); break;
                case 2594: comments2.AppendFormat("2594:Invalid Node Id معرف العقدة غير صالح	"); break;
                case 2595: comments2.AppendFormat("2595:OTP Not Found OTP غير موجود	"); break;
                case 2596: comments2.AppendFormat("2596:OTP Expired OTP انتهت	"); break;
                case 2813: comments2.AppendFormat("2813:Application Id Is Null معرف التطبيق فارغ	"); break;
                case 3001: comments2.AppendFormat("3001:Invalid Ticket Information معلومات تذكرة غير صالحة	"); break;
                case 3002: comments2.AppendFormat("3002:User Not Found لم يتم العثور على المستخدم	"); break;
                case 3003: comments2.AppendFormat("3003:User Info Not Updated معلومات المستخدم غير محدثة	"); break;
                case 3004: comments2.AppendFormat("3004:User Info History Not Updated معلومات معلومات المستخدم غير محدثة	"); break;
                case 3005: comments2.AppendFormat("3005:User Device Type Not Inserted نوع جهاز المستخدم غير مدرج	"); break;
                case 3006: comments2.AppendFormat("3006:User Locked تم حظر المستخدم	"); break;
                case 3007: comments2.AppendFormat("3007:User Device Not Inserted لم يتم إدراج جهاز المستخدم	"); break;
                case 3008: comments2.AppendFormat("3008:PIN must be at least 4 digits يجب أن يتكون رقم التعريف الشخصي من 4 أرقام على الأقل	"); break;
                case 3009: comments2.AppendFormat("3009:Application Not Found التطبيق غير موجود	"); break;
                case 3010: comments2.AppendFormat("3010:Loyalty Application Definition Not Allowed تعريف تطبيق الولاء غير مسموح به	"); break;
                case 3011: comments2.AppendFormat("3011:Discount Program Already Exist برنامج الخصم موجود بالفعل	"); break;
                case 3100: comments2.AppendFormat("3100:Column Count Error خطأ عدد العمود	"); break;
                case 3101: comments2.AppendFormat("3101:LFA Member length error (Max 6 char) خطأ طول عضو LFA (بحد أقصى 6 أحرف)	"); break;
                case 3102: comments2.AppendFormat("3102:Outlet length error (Max 4 char) خطأ طول المخرج (بحد أقصى 4 أحرف)	"); break;
                case 3103: comments2.AppendFormat("3103:Title length error (Max 4 char) خطأ طول العنوان (بحد أقصى 4 أحرف)	"); break;
                case 3104: comments2.AppendFormat("3104:First Name length error (Max 30 char) خطأ في طول الاسم الأول (بحد أقصى 30 حرفًا)	"); break;
                case 3105: comments2.AppendFormat("3105:Last Name length error (Max 30 char) خطأ في طول الاسم الأخير (بحد أقصى 30 حرفًا)	"); break;
                case 3106: comments2.AppendFormat("3106:Middle Name length error (Max 30 char) خطأ في طول الاسم الأوسط (بحد أقصى 30 حرفًا)	"); break;
                case 3107: comments2.AppendFormat("3107:Mobile Number length error (Max 20 char) خطأ في طول رقم الهاتف المحمول (بحد أقصى 20 حرفًا)	"); break;
                case 3108: comments2.AppendFormat("3108:Email length error (Max 80 char) خطأ في طول البريد الإلكتروني (بحد أقصى 80 حرفًا)	"); break;
                case 3109: comments2.AppendFormat("3109:Gender length error (Max 2 char) خطأ طول الجنس (بحد أقصى 2 حرف)	"); break;
                case 3110: comments2.AppendFormat("3110:Marital Status length error (Max 1 number) خطأ في طول الحالة الاجتماعية (رقم واحد كحد أقصى)	"); break;
                case 3111: comments2.AppendFormat("3111:Birth Date length error (Max 10 char) خطأ طول تاريخ الميلاد (بحد أقصى 10 أحرف)	"); break;
                case 3112: comments2.AppendFormat("3112:Address Line 1 length error (Max 50 char) خطأ طول سطر العنوان 1 (بحد أقصى 50 حرفًا)	"); break;
                case 3113: comments2.AppendFormat("3113:Address Line 2 length error (Max 50 char) خطأ طول سطر العنوان 2 (بحد أقصى 50 حرفًا)	"); break;
                case 3114: comments2.AppendFormat("3114:City Id length error (Max 6 number) خطأ في طول معرف المدينة (بحد أقصى 6 عدد)	"); break;
                case 3115: comments2.AppendFormat("3115:State Id length error (Max 4 number) خطأ طول معرف الحالة (الحد الأقصى 4 عدد)	"); break;
                case 3116: comments2.AppendFormat("3116:Country length error (Max 3 number) خطأ طول البلد (الحد الأقصى 3 رقم)	"); break;
                case 3117: comments2.AppendFormat("3117:Home Telephone length error (Max 20 number) خطأ في طول هاتف المنزل (بحد أقصى 20 رقمًا)	"); break;
                case 3118: comments2.AppendFormat("3118:Profession length error (Max 2 number) خطأ طول المهنة (الحد الأقصى 2 رقم)	"); break;
                case 3119: comments2.AppendFormat("3119:Position length error (Max 50char) خطأ طول الموضع (بحد أقصى 50char)	"); break;
                case 3120: comments2.AppendFormat("3120:Employment Status length error (Max 2 number) خطأ طول حالة التوظيف (الحد الأقصى 2 رقم)	"); break;
                case 3122: comments2.AppendFormat("3122:Card Activity Alerts length error (Max 1 number) خطأ طول تنبيهات نشاط البطاقة (رقم واحد كحد أقصى)	"); break;
                case 3123: comments2.AppendFormat("3123:Loyalty Program Code length error (Max 4 char) خطأ في طول رمز برنامج الولاء (بحد أقصى 4 أحرف)	"); break;
                case 3130: comments2.AppendFormat("3130:File not contain enought data (lower than 3 rows) لا يحتوي الملف على بيانات كافية (أقل من 3 صفوف)	"); break;
                case 3131: comments2.AppendFormat("3131:Header Format Error خطأ تنسيق الرأس	"); break;
                case 3132: comments2.AppendFormat("3132:Footer Format Error خطأ تنسيق التذييل	"); break;
                case 3133: comments2.AppendFormat("3133:Regex file not found ملف regex غير موجود	"); break;
                case 3134: comments2.AppendFormat("3134:Regex File Format Error خطأ تنسيق ملف Regex	"); break;
                case 3301: comments2.AppendFormat("3301:Authentication Ticket Insertion is Failed فشل إدخال تذكرة المصادقة	"); break;
                case 4001: comments2.AppendFormat("4001:Foreign CardID not found الكركيد الأجنبي غير موجود	"); break;
                case 5001: comments2.AppendFormat("5001:Merchant Register Segment Not Set سجل التسجيل التاجر غير محدد	"); break;
                case 5002: comments2.AppendFormat("5002:AccountGroupID not found accountgroupid غير موجود	"); break;
                case 5003: comments2.AppendFormat("5003:Merchant Multiplier not found merchant مضاعف غير موجود	"); break;
                case 5004: comments2.AppendFormat("5004:PartnerShip Multiplier not found مضاعف الشراكة غير موجودة	"); break;
                case 5078: comments2.AppendFormat("5078:Account not related to a segment الحساب لا يرتبط بشريحة	"); break;
                case 5080: comments2.AppendFormat("5080:Acoount already exist. الحساب موجود بالفعل	"); break;
                case 7000: comments2.AppendFormat("7000:Register Not Related To Application تسجيل لا علاقة له بالتطبيق	"); break;
                case 8000: comments2.AppendFormat("8000:CaptchaError كلمة التحقق خطأ	"); break;
                case 8001: comments2.AppendFormat("8001:Resource Problem مشكلة الموارد	"); break;
                case 9001: comments2.AppendFormat("9001:New Account Group Id Get Error حساب مجموعة حساب جديد الحصول على خطأ	"); break;
                case 9002: comments2.AppendFormat("9002:Invalid RegisterType تسجيل غير صالح	"); break;
                case 9003: comments2.AppendFormat("9003:User already defined تم تعريف المستخدم بالفعل	"); break;
                case 9007: comments2.AppendFormat("9007:Register serial already exists تسجيل المسلسل موجود بالفعل	"); break;
                case 9008: comments2.AppendFormat("9008:Register status is unknown حالة التسجيل غير معروفة	"); break;
                case 9009: comments2.AppendFormat("9009:Account related to active register حساب مرتبط بالسجل النشط	"); break;
                case 9010: comments2.AppendFormat("9010:Invalid DC CITY مدينة DC غير صالحة	"); break;
                case 9011: comments2.AppendFormat("9011:Invalid DC STATE حالة DC غير صالحة	"); break;
                case 9012: comments2.AppendFormat("9012:Invalid DC PROFESSION مهنة DC غير صالحة	"); break;
                case 9013: comments2.AppendFormat("9013:Invalid DC EMPLOYMENT STATUS حالة توظيف DC غير صالحة	"); break;
                case 9014: comments2.AppendFormat("9014:Invalid DC TITLE عنوان DC غير صالح	"); break;
                case 9015: comments2.AppendFormat("9015:Invalid DC PREFERRED LANGUAGE CODE رمز اللغة المفضل DC غير صالح	"); break;
                case 9016: comments2.AppendFormat("9016:Invalid DC NOTIFICATION MEDIA وسائل الإعلام الإعلامية DC غير صالحة	"); break;
                case 9080: comments2.AppendFormat("9080:CUSTOMER_NUMBER not unique in the file customer_number غير فريد في الملف	"); break;
                case 9132: comments2.AppendFormat("9132:REGISTER_NO not unique in the file register_no غير فريد في الملف	"); break;
                case 9134: comments2.AppendFormat("9134:SMS REJECTED الرسائل القصيرة مرفوضة	"); break;
                case 9135: comments2.AppendFormat("9135:SMS EXPIRED انتهت صلاحية الرسائل القصيرة	"); break;
                case 9136: comments2.AppendFormat("9136:Assignable card not found بطاقة واحدة غير موجودة	"); break;
                case 9137: comments2.AppendFormat("9137:Application Id is required معرف التطبيق مطلوب	"); break;
                case 9138: comments2.AppendFormat("9138:Clear register is required مطلوب تسجيل واضح	"); break;
                case 9139: comments2.AppendFormat("9139:Title is required العنوان مطلوب	"); break;
                case 9140: comments2.AppendFormat("9140:Gender is required النوع الاجتماعي مطلوب	"); break;
                case 9141: comments2.AppendFormat("9141:Home Address 1 is required العنوان الرئيسية 1 مطلوب	"); break;
                case 9142: comments2.AppendFormat("9142:Martial status is required الوضع العسكري مطلوب	"); break;
                case 9143: comments2.AppendFormat("9143:Employment status is required حالة التوظيف مطلوبة	"); break;
                case 9144: comments2.AppendFormat("9144:Mobile phone number prefix must be 3 digit يجب أن يكون بادئة رقم الهاتف المحمول 3 أرقام	"); break;
                case 9145: comments2.AppendFormat("9145:Mobile phone number is required رقم الهاتف المحمول مطلوب	"); break;
                case 9146: comments2.AppendFormat("9146:City is required المدينة مطلوبة	"); break;
                case 9147: comments2.AppendFormat("9147:Country is required الدولة مطلوبة	"); break;
                case 9148: comments2.AppendFormat("9148:Card activity alerts is required مطلوب تنبيهات نشاط البطاقة	"); break;
                case 9149: comments2.AppendFormat("9149:Receive marketing notifications is required تلقي إشعارات التسويق مطلوبة	"); break;
                case 9150: comments2.AppendFormat("9150:Delivery location is required مطلوب موقع التسليم	"); break;
                case 9151: comments2.AppendFormat("9151:Profession is required المهنة مطلوبة	"); break;
                case 9152: comments2.AppendFormat("9152:Mobile phone number must be 0-17 digit يجب أن يتكون رقم الهاتف المحمول من 0 إلى 17 رقمًا	"); break;
                case 9153: comments2.AppendFormat("9153:Birthday is required عيد الميلاد مطلوب	"); break;
                case 9154: comments2.AppendFormat("9154:New mobile phone number prefix is required مطلوب بادئة رقم الهاتف المحمول الجديد	"); break;
                case 9155: comments2.AppendFormat("9155:New mobile phone number is required مطلوب رقم الهاتف المحمول الجديد	"); break;
                case 9156: comments2.AppendFormat("9156:Message text is required نص الرسالة مطلوب	"); break;
                case 9157: comments2.AppendFormat("9157:Activation Code must be 4 digit يجب أن يكون رمز التنشيط 4 أرقام	"); break;
                case 9158: comments2.AppendFormat("9158:Volume type code is required مطلوب كود نوع وحدة التخزين	"); break;
                case 9159: comments2.AppendFormat("9159:Volume type id is required مطلوب معرف نوع وحدة التخزين	"); break;
                case 9160: comments2.AppendFormat("9160:Node id is required معرف العقدة مطلوب	"); break;
                case 9161: comments2.AppendFormat("9161:Clear secure info is required معلومات واضحة آمنة مطلوبة	"); break;
                case 9162: comments2.AppendFormat("9162:Clear secure info must be 4 digit يجب أن تتكون المعلومات الآمنة المحوَّلة من 4 أرقام	"); break;
                case 9163: comments2.AppendFormat("9163:New clear secure info is required مطلوب معلومات آمنة واضحة جديدة	"); break;
                case 9164: comments2.AppendFormat("9164:New Clear secure info must be 4 digit يجب أن تكون معلومات آمنة واضحة جديدة 4 أرقام	"); break;
                case 9165: comments2.AppendFormat("9165:Property value is required قيمة الممتلكات مطلوبة	"); break;
                case 9166: comments2.AppendFormat("9166:Range Definition is used before يتم استخدام تعريف النطاق من قبل	"); break;
                case 9167: comments2.AppendFormat("9167:Range Definition Expire Date is old تاريخ التعريف تاريخ انتهاء الصلاحية هو القديم	"); break;
                case 9172: comments2.AppendFormat("9172:AB Node type property not found لم يتم العثور على خاصية نوع عقدة AB	"); break;
                case 9173: comments2.AppendFormat("9173:Application insert error خطأ في إدراج التطبيق	"); break;
                case 9174: comments2.AppendFormat("9174:Sms Expired (Enrollment) انتهت صلاحية الرسائل القصيرة (التسجيل)	"); break;
                case 9175: comments2.AppendFormat("9175:Msisdn is not uniqune msisdn ليس من الناحية	"); break;
                case 9176: comments2.AppendFormat("9176:Delivery Location Code is not unique رمز موقع التسليم ليس فريدا	"); break;
                case 9178: comments2.AppendFormat("9178:Loyalty Terms Conditions is not checked لا يتم التحقق من شروط الولاء	"); break;
                case 9179: comments2.AppendFormat("9179:Max Rewarding Amount Exceeded تجاوز الحد الأقصى لمكافأة	"); break;


            }
            comments2.AppendLine();
            using (LSRetailPosis.POSProcesses.frmMessage dialog = new LSRetailPosis.POSProcesses.frmMessage(comments2.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Stop))
            {
                LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog);
            }
            nSuccess = false;
            return nSuccess;
        }


        private bool showMessageForm_Alrahji(string strMsgPrefix, int nRetCode)
        {
            bool nSuccess = true;
            StringBuilder comments2 = new StringBuilder(128);
            comments2.AppendFormat(strMsgPrefix);
            switch (nRetCode)
            {
                case 000: return nSuccess;// comments2.AppendFormat("Approved  ");break;
                case 101: comments2.AppendFormat("101: Validation Failed  فشل فى التحقق"); break;
                case 206: comments2.AppendFormat("206: Store is invalid  رقم الفرع خطأ"); break;
                case 209: comments2.AppendFormat("209: User does not exist  العميل ليس لديه حساب مكافأة "); break;
                case 205: comments2.AppendFormat("205: This member balance is lower than the minimum points balance required رصيد العميل أقل من الحد الأدني لإستخدام نقاط مكافأة"); break;
                case 204: comments2.AppendFormat("204: This member balance is insufficient for redemptions 204: رصيد هذا العضو غير كافٍ لعمليات الاسترداد"); break;
                case 301: comments2.AppendFormat("301:Invalid OTP  خطأ بالرقم السرى"); break;
                case 302: comments2.AppendFormat("302: Invalid OTP. OTP was resent, please check it again تم إرسال رمز التحقق مرة أخري فضلا المراجعة"); break;
                case 303: comments2.AppendFormat("303: OTP is expired  وقت كود التفعيل إنتهي"); break;
                case 304: comments2.AppendFormat("304: Transaction Failed {Reasons} فشلت الحركة "); break;
                case 500: comments2.AppendFormat("500: General API Error خطأ عام فى الإتصال بنظام مكافأة "); break;
                case 401: comments2.AppendFormat("401: Time exceeded, transaction cannot be reversed إنتهي الوقت ولا يمكن الإسترجاع "); break;
            }
            comments2.AppendLine();
            using (LSRetailPosis.POSProcesses.frmMessage dialog = new LSRetailPosis.POSProcesses.frmMessage(comments2.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Stop))
            {
                LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog);
            }
            nSuccess = false;
            return nSuccess;
        }

        private bool showMessageForm_SAMA(int nRetCode)
        {
            bool nSuccess = true;
            StringBuilder comments2 = new StringBuilder(128);
            switch (nRetCode)
            {
                case 000: return nSuccess;// comments2.AppendFormat("Approved  ");break;
                case 001: return nSuccess;// comments2.AppendFormat("Honor with identification  ");break;
                case 003: return nSuccess;// comments2.AppendFormat("Approved (VIP)  ");break;
                case 007: return nSuccess;// comments2.AppendFormat("Approved, update ICC (To be used when a response includes an issuer script. This code is used for SPAN only.)  ");break;
                case 087: return nSuccess;// comments2.AppendFormat("Offline Approved (Chip only)  ");break;
                case 089: return nSuccess;// comments2.AppendFormat("Unable to go On-line. Off-line approved (Chip only)  ");break;
                case 400: return nSuccess;// comments2.AppendFormat("Accepted  ");break;
                case 100: comments2.AppendFormat("100:Do not honor "); break;
                case 101: comments2.AppendFormat("101:Expired card البطاقة منتهية الصلاحية   "); break;
                case 102: comments2.AppendFormat("102:Suspected fraud (To be used when ARQC validation fails البطاقة مشتبه بها )  "); break;
                case 103: comments2.AppendFormat("103:Card acceptor contact acquirer  "); break;
                case 104: comments2.AppendFormat("104:Restricted card  "); break;
                case 105: comments2.AppendFormat("105:Card acceptor call acquirer’s security department  "); break;
                case 106: comments2.AppendFormat("106:Allowable PIN tries exceeded تم تجاوز عدد المحاولات المسموح بها   "); break;
                case 107: comments2.AppendFormat("107:Refer to card issuer الرجوع الى مصدر البطاقة   "); break;
                case 108: comments2.AppendFormat("108:Refer to card issuer’s special conditions لرجوع الى البنك   "); break;
                case 109: comments2.AppendFormat("109:Invalid merchant التاجر غير صالح   "); break;
                case 110: comments2.AppendFormat("110:Invalid amount المبلغ غير صالح   "); break;
                case 111: comments2.AppendFormat("111:Invalid card number رقم البطاقة غير صالح   "); break;
                case 112: comments2.AppendFormat("112:PIN data required بيانات الرقم السري مطلوبة   "); break;
                case 114: comments2.AppendFormat("114:No account of type requested لايوجد حساب من هذا النوع   "); break;
                case 115: comments2.AppendFormat("115:Requested function not supported المهمة غير مدعومة "); break;
                case 116: comments2.AppendFormat("116:Not sufficient funds لايوجد رصيد "); break;
                case 117: comments2.AppendFormat("117:Incorrect PIN الرقم السري غير صحيح   "); break;
                case 118: comments2.AppendFormat("118:No card record لايوجد سجل للبطاقة   "); break;
                case 119: comments2.AppendFormat("119:Transaction not permitted to cardholder العملية غير مسموحة   "); break;
                case 120: comments2.AppendFormat("120:Transaction not permitted to terminal العملية غير مسموحة في الفرع   "); break;
                case 121: comments2.AppendFormat("121:Exceeds withdrawal amount limit يتجاوز الحد المسموح به للسحب   "); break;
                case 122: comments2.AppendFormat("122:Security violation اختراق الامن   "); break;
                case 123: comments2.AppendFormat("123:Exceeds withdrawal frequency limit يتجاوز حد تردد السحب   "); break;
                case 125: comments2.AppendFormat("125:Card not effective البطاقة لاتعمل   "); break;
                case 126: comments2.AppendFormat("126:Invalid PIN block الرقم السري محظور  "); break;
                case 127: comments2.AppendFormat("127:PIN length error خطأ في طول الرقم السري   "); break;
                case 128: comments2.AppendFormat("128:PIN key synch error خطأ في مزامنة الرقم السري   "); break;
                case 129: comments2.AppendFormat("129:Suspected counterfeit card البطاقة مشتبه بتزويرها   "); break;
                case 182: comments2.AppendFormat("182:Invalid date (Visa 80) التاريخ منتهي للفيزا   "); break;
                case 183: comments2.AppendFormat("183:Cryptographic error found in PIN or CVV (Visa 81) عثور على خطأ في تشفير الرقم السري "); break;
                case 184: comments2.AppendFormat("184:Incorrect CVV (Visa 82) غير صحيح الرمز  "); break;
                case 185: comments2.AppendFormat("185:Unable to verify PIN (Visa 83) تعثر التحقق من الرقم السري "); break;
                case 188: comments2.AppendFormat("188:Offline declined مرفوضه غير متصل    "); break;
                case 190: comments2.AppendFormat("190:Unable to go online – Offline declined تعذر الاتصال بالانترنت   "); break;
                case 200: comments2.AppendFormat("200:Do not honor "); break;
                case 201: comments2.AppendFormat("201:Expired card البطاقة منتهية الصلاحية   "); break;
                case 202: comments2.AppendFormat("202:Suspected fraud (To be used when ARQC validation fails) مشتبه في الاحتيال   "); break;
                case 203: comments2.AppendFormat("203:Card acceptor contact acquirer  "); break;
                case 204: comments2.AppendFormat("204:Restricted card البطاقة مقيدة   "); break;
                case 205: comments2.AppendFormat("205:Card acceptor call acquirer’s security department  "); break;
                case 206: comments2.AppendFormat("206:Allowable PIN tries exceeded تم تجاوز عدد محاولات ادخال الرقم السري "); break;
                case 207: comments2.AppendFormat("207:Special conditions شروط خاصة "); break;
                case 208: comments2.AppendFormat("208:Lost card البطاقة مفقودة "); break;
                case 209: comments2.AppendFormat("209:Stolen card البطاقة مسروقة "); break;
                case 210: comments2.AppendFormat("210:Suspected counterfeit card البطاقة مشتبه بتزويرها "); break;
                case 902: comments2.AppendFormat("902:Invalid transaction العملية غير صالحة "); break;
                case 903: comments2.AppendFormat("903:Re-enter transaction اعد ادخال العملية	 "); break;
                case 904: comments2.AppendFormat("904:Format error خطأ في التنسيق   "); break;
                case 906: comments2.AppendFormat("906:Cutover in process تحويل في العملية  "); break;
                case 907: comments2.AppendFormat("907:Card issuer or switch inoperative  "); break;
                case 908: comments2.AppendFormat("908:Transaction destination cannot be found for routing مصدر البطاقة معطل "); break;
                case 909: comments2.AppendFormat("909:System malfunction خلل في النظام "); break;
                case 910: comments2.AppendFormat("910:Card issuer signed off مصدر البطاقة ملغي "); break;
                case 911: comments2.AppendFormat("911:Card issuer timed out انتهت مهلة مصدار البطاقة "); break;
                case 912: comments2.AppendFormat("912:Card issuer unavailableغير متوفر مصدر البطاقة   "); break;
                case 913: comments2.AppendFormat("913:Duplicate transmission انتقال متكرر "); break;
                case 914: comments2.AppendFormat("914:Not able to trace back to original transaction غير قادر على تتبع العودة الى المعاملة الاصلية   "); break;
                case 915: comments2.AppendFormat("915:Reconciliation cutover or checkpoint error خطأ في نقطة التفتيش  "); break;
                case 916: comments2.AppendFormat("916:MAC incorrect (permissible in 1644)  "); break;
                case 917: comments2.AppendFormat("917:MAC key sync  "); break;
                case 918: comments2.AppendFormat("918:No communication keys available for use لاتوجد مفاتيح متاحه للاستخدام "); break;
                case 919: comments2.AppendFormat("919:Encryption key sync error خطأ مزامنة مفتاح التشفير "); break;
                case 920: comments2.AppendFormat("920:Security software/hardware error – try again خطأ في برنامج الامان حاول مره اخرى   "); break;
                case 921: comments2.AppendFormat("921:Security software/hardware error – no action خطأ في الاجهزة لايوجد اي اجراء   "); break;
                case 922: comments2.AppendFormat("922:Message number out of sequence رقم الرسالة خارج التسلسل "); break;
                case 923: comments2.AppendFormat("923:Request in progress الطلب قيد التقدم   "); break;
                case 940: comments2.AppendFormat("940:Unknown terminal خارج الخدمة "); break;
                case 942: comments2.AppendFormat("942:Invalid Reconciliation Date تاريخ التسوية غير صالح   "); break;


            }
            comments2.AppendLine();
            using (LSRetailPosis.POSProcesses.frmMessage dialog = new LSRetailPosis.POSProcesses.frmMessage(comments2.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Stop))
            {
                LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog);
            }
            nSuccess = false;
            return nSuccess;
        }


        /// <summary>
        /// Displays an alert message according operation id passed.
        /// </summary>
        /// <param name="operationInfo"></param>
        /// <param name="posTransaction"></param>        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Grandfather")]
        public void BlankOperation(IBlankOperationInfo operationInfo, IPosTransaction posTransaction)
        {

            // This country check can be removed when customizing the BlankOperations service.
            /* if (Functions.CountryRegion == SupportedCountryRegion.BR ||
                 Functions.CountryRegion == SupportedCountryRegion.HU ||
                 Functions.CountryRegion == SupportedCountryRegion.RU)
             {
                 if (Application.Services.Peripherals.FiscalPrinter.FiscalPrinterEnabled())
                 {
                     Application.Services.Peripherals.FiscalPrinter.BlankOperations(operationInfo, posTransaction);
                 }
                 return;
             }*/
            RetailTransaction myTransaction1;
            //if(posTransaction.TransactionType != LSRetailPosis.Transaction.PosTransaction.TypeOfTransaction.Internal)
            {
                // myTransaction1 = (RetailTransaction)posTransaction;
            }
            Boolean tst = true;
            switch (operationInfo.OperationId)
            {

                case "SetSalesManAll":

                    String Employeeid, EmployeeName;
                    Employeeid = "";
                    EmployeeName = "";


                    if (((object)posTransaction).GetType() == typeof(RetailTransaction))
                    {
                        if (((RetailTransaction)posTransaction).SaleItems.Count > 0)
                        {
                            using (LSRetailPosis.POSProcesses.frmSalesPerson dialog = new LSRetailPosis.POSProcesses.frmSalesPerson())
                            {
                                LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog);
                                Employeeid = dialog.SelectedEmployeeId;
                                EmployeeName = dialog.SelectEmployeeName;
                            }
                        }

                        RetailTransaction myTransaction = (RetailTransaction)posTransaction;
                        if (myTransaction.SaleItems.Count > 0)
                        {

                            LinkedList<SaleLineItem> Saleslines = myTransaction.SaleItems;
                            LinkedList<SaleLineItem>.Enumerator enumtr = Saleslines.GetEnumerator();
                            while (enumtr.MoveNext())
                            {
                                enumtr.Current.SalesPersonId = Employeeid;
                                enumtr.Current.SalespersonName = EmployeeName;

                            }
                        }

                    }
                    break;



                case "SetSalesManDiff":

                    Employeeid = "";
                    EmployeeName = "";
                    string[] num1;
                    int num;
                    using (LSRetailPosis.POSProcesses.frmInputNumpad dialog1 = new LSRetailPosis.POSProcesses.frmInputNumpad())
                    {
                        LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog1);

                        num1 = dialog1.InputText.Split(',');
                    }

                    if (((object)posTransaction).GetType() == typeof(RetailTransaction))
                    {
                        if (((RetailTransaction)posTransaction).SaleItems.Count > 0)
                        {
                            using (LSRetailPosis.POSProcesses.frmSalesPerson dialog = new LSRetailPosis.POSProcesses.frmSalesPerson())
                            {
                                LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog);
                                Employeeid = dialog.SelectedEmployeeId;
                                EmployeeName = dialog.SelectEmployeeName;
                            }
                        }

                        RetailTransaction myTransaction = (RetailTransaction)posTransaction;
                        /*
                        num = int.Parse(num1[1]);
                        SaleLineItem saleItem = myTransaction.GetItem(num);
                        saleItem.SalesPersonId = Employeeid;
                        saleItem.SalespersonName = EmployeeName;
                         * */

                        int x = 0;
                        while (x < num1.Length)
                        {
                            num = int.Parse(num1[x]);
                            SaleLineItem saleItem = myTransaction.GetItem(num);
                            saleItem.SalesPersonId = Employeeid;
                            saleItem.SalespersonName = EmployeeName;
                            x++;
                        }


                    }
                    break;
                case "PayCardnew":  // payment case 1 
                    tst = true;
                    Microsoft.Win32.RegistryKey key;
                    key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Card");
                    key.SetValue("Type", operationInfo.Parameter);
                    key.Close();
                    myTransaction1 = (RetailTransaction)posTransaction;
                    if (myTransaction1.SaleItems.Count > 0)
                    {

                        LinkedList<SaleLineItem> Saleslines = myTransaction1.SaleItems;
                        LinkedList<SaleLineItem>.Enumerator enumtr = Saleslines.GetEnumerator();
                        while (enumtr.MoveNext())
                        {
                            if (enumtr.Current.SalesPersonId == string.Empty || enumtr.Current.SalesPersonId == null)
                            {
                                MessageBox.Show("Please select Salesman من فضلك إدخل رقم البائع ");
                                tst = false;
                                break;
                            }

                        }
                    }
                    if (tst == true)
                    {
                        SaleLineItem aItem1 = myTransaction1.SaleItems.First();//!!!!QitafRewardUpdate
                        bool bSuccess = processQitafRewardUpdateIfAvailable(aItem1.ReturnTransId, aItem1.NetAmountWithTax);


                        Application.RunOperation(PosisOperations.PayCard, null);
                        m_strRewardAmount = ((int)((LSRetailPosis.Transaction.RetailTransaction)posTransaction).NetAmountWithTax).ToString();
                        process_Qitaf_RewardPoint(posTransaction.TransactionId);
                        genShelf(posTransaction.TransactionId, posTransaction.StoreId, false);
                        //  sendLinkAfterPayment(posTransaction.ReceiptId); // send the balancae to qitaf in cas
                    }


                    break;

                case "Reversel": // payment case 
                    myTransaction1 = (RetailTransaction)posTransaction;
                    if (myTransaction1.SaleItems.Count > 0)
                    {

                        LinkedList<SaleLineItem> Saleslines = myTransaction1.SaleItems;
                        LinkedList<SaleLineItem>.Enumerator enumtr = Saleslines.GetEnumerator();
                        while (enumtr.MoveNext())
                        {
                            if (enumtr.Current.SalesPersonId == string.Empty || enumtr.Current.SalesPersonId == null)
                            {
                                MessageBox.Show("Please select Salesman من فضلك إدخل رقم البائع ");
                                tst = false;
                                break;
                            }

                        }
                        if (tst == false)
                        {

                            Employeeid = "";
                            EmployeeName = "";


                            if (((object)posTransaction).GetType() == typeof(RetailTransaction))
                            {
                                if (((RetailTransaction)posTransaction).SaleItems.Count > 0)
                                {
                                    using (LSRetailPosis.POSProcesses.frmSalesPerson dialog = new LSRetailPosis.POSProcesses.frmSalesPerson())
                                    {
                                        LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog);
                                        Employeeid = dialog.SelectedEmployeeId;
                                        EmployeeName = dialog.SelectEmployeeName;
                                    }
                                }

                                RetailTransaction myTransaction = (RetailTransaction)posTransaction;
                                if (myTransaction.SaleItems.Count > 0)
                                {

                                    LinkedList<SaleLineItem> Saleslines1 = myTransaction.SaleItems;
                                    LinkedList<SaleLineItem>.Enumerator enumtr1 = Saleslines.GetEnumerator();
                                    while (enumtr1.MoveNext())
                                    {
                                        enumtr1.Current.SalesPersonId = Employeeid;
                                        enumtr1.Current.SalespersonName = EmployeeName;

                                    }
                                }

                            }
                            break;
                        }
                    }

                    if (m_bSuccessOnRedeem)
                    {
                        callapi_ReverseTrans();
                        if (m_bSuccessOnReverse)
                        {
                            PayCash pay = new PayCash(false, "1");

                            pay.OperationID = PosisOperations.PayCash; // choose ure payment method
                            pay.OperationInfo = new OperationInfo();
                            pay.OperationInfo.NumpadValue = "-" + m_strAmount;
                            pay.POSTransaction = (PosTransaction)posTransaction;
                            pay.Amount = decimal.Parse("-" + m_strAmount);
                            pay.RunOperation();

                            MessageBox.Show("Reverse Blue Redepmtion has been completed." + m_strRetMessage);
                            break;
                        }
                        else
                        {
                            // MessageBox.Show("Reverse Blue Redepmtion has been failed." + m_strRetMessage);
                            break;
                        }
                    }
                    else
                    {
                        MessageBox.Show("There is not any pending Redeem transaction to be reversed.");
                    }
                    break;

                case "alrajhi": // payment case 
                    myTransaction1 = (RetailTransaction)posTransaction;
                    if (myTransaction1.SaleItems.Count > 0)
                    {

                        LinkedList<SaleLineItem> Saleslines = myTransaction1.SaleItems;
                        LinkedList<SaleLineItem>.Enumerator enumtr = Saleslines.GetEnumerator();
                        while (enumtr.MoveNext())
                        {
                            if (enumtr.Current.SalesPersonId == string.Empty || enumtr.Current.SalesPersonId == null)
                            {
                                MessageBox.Show("Please select Salesman من فضلك إدخل رقم البائع ");
                                tst = false;
                                break;
                            }

                        }
                        if (tst == false)
                        {

                            Employeeid = "";
                            EmployeeName = "";


                            if (((object)posTransaction).GetType() == typeof(RetailTransaction))
                            {
                                if (((RetailTransaction)posTransaction).SaleItems.Count > 0)
                                {
                                    using (LSRetailPosis.POSProcesses.frmSalesPerson dialog = new LSRetailPosis.POSProcesses.frmSalesPerson())
                                    {
                                        LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog);
                                        Employeeid = dialog.SelectedEmployeeId;
                                        EmployeeName = dialog.SelectEmployeeName;
                                    }
                                }

                                RetailTransaction myTransaction = (RetailTransaction)posTransaction;
                                if (myTransaction.SaleItems.Count > 0)
                                {

                                    LinkedList<SaleLineItem> Saleslines1 = myTransaction.SaleItems;
                                    LinkedList<SaleLineItem>.Enumerator enumtr1 = Saleslines.GetEnumerator();
                                    while (enumtr1.MoveNext())
                                    {
                                        enumtr1.Current.SalesPersonId = Employeeid;
                                        enumtr1.Current.SalespersonName = EmployeeName;

                                    }
                                }

                            }
                            break;
                        }
                    }

                    if (myTransaction1.LoyaltyItem.LoyaltyCardNumber.Length > 0)
                    {
                        MessageBox.Show("Alrajhi Can't be process because Loyalty Card is applied.");
                        break;
                    }

                    //!!POINTS
                    // form take phone number for customer 
                    // if not register in rajhi error msg not resgister 
                    // if register   open form enter otp and amount 
                    //otp token that you need to take from Authorization to Redemption API  Otptoken from the response

                    // make undo button to reverse id 
                    // after done enter amount to pos 
                    // save transaction date - id - amount = transactionod = store  - [transactionIDinrajhi] - [transactionTypeinrajhi] in table sql
                    // [ax].[Retailalrajhitrans]
                    string strAuthToken = callapi_genAuthTk();
                    if (m_bSuccessOnGenToken == false)
                    {
                        break;
                    }
                    // input Phone number
                    m_strPN = "+002060454";//966002060454
                    using (Microsoft.Dynamics.Retail.Pos.BlankOperations.Dialog.frmPNCode dialogPN = new Microsoft.Dynamics.Retail.Pos.BlankOperations.Dialog.frmPNCode())
                    {
                        LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialogPN);
                        m_strPN = "+" + dialogPN.strPN;
                        if (m_strPN.ToString().Length < 7)
                        {
                            MessageBox.Show("Invalid phone number.");
                            break;
                        }
                    }

                    m_strAmount = "";
                    callapi_authCustomer();
                    m_strTansactionID4POS = ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).TransactionId;
                    m_strStoreId4POS = ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).StoreId;
                    if (m_bSuccessOnAuthCustomer)
                    {
                        on_RedeemCustomerAmount();
                    }
                    else
                    {
                        //MessageBox.Show("Authorize Blu Customer has been failed:" + m_strRetMessage);
                        if (m_strErrCode == "210")
                        {
                            //"message": "Last generated OTP is still valid",
                            //"errorCode": "210"
                            on_RedeemCustomerAmount();
                        }
                        else
                        {
                            break;
                        }
                    }




                    if (tst == true)
                    {
                        if (m_bSuccessOnRedeem)
                        {
                            PayCash pay = new PayCash(false, "24");

                            pay.OperationID = PosisOperations.PayCash; // choose ure payment method
                            pay.OperationInfo = new OperationInfo();
                            pay.OperationInfo.NumpadValue = m_strAmount;
                            pay.POSTransaction = (PosTransaction)posTransaction;
                            pay.Amount = decimal.Parse(m_strAmount);
                            pay.RunOperation();
                            decimal paid = pay.Amount;
                            if (((LSRetailPosis.Transaction.RetailTransaction)posTransaction).NetAmountWithTax ==
                                ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).Payment +
                                paid)
                            {
                                genShelf(posTransaction.TransactionId, posTransaction.StoreId, false);
                            }
                            {
                                string strTranId = posTransaction.TransactionId;
                                updateTransTbl4ReturnQty(strTranId);

                                /*  m_strRewardAmount = ((int)((LSRetailPosis.Transaction.RetailTransaction)posTransaction).NetAmountWithTax).ToString();
                                  process_Qitaf_RewardPoint();*/

                            }

                        }

                    }


                    break;
                case "Qitaf": // pay Qitaf 
                    myTransaction1 = (RetailTransaction)posTransaction;
                    if (myTransaction1.SaleItems.Count > 0)
                    {

                        LinkedList<SaleLineItem> Saleslines = myTransaction1.SaleItems;
                        LinkedList<SaleLineItem>.Enumerator enumtr = Saleslines.GetEnumerator();
                        while (enumtr.MoveNext())
                        {
                            if (enumtr.Current.SalesPersonId == string.Empty || enumtr.Current.SalesPersonId == null)
                            {
                                MessageBox.Show("Please select Salesman من فضلك إدخل رقم البائع ");
                                tst = false;
                                break;
                            }

                        }
                        if (tst == false)
                        {

                            Employeeid = "";
                            EmployeeName = "";


                            if (((object)posTransaction).GetType() == typeof(RetailTransaction))
                            {
                                if (((RetailTransaction)posTransaction).SaleItems.Count > 0)
                                {
                                    using (LSRetailPosis.POSProcesses.frmSalesPerson dialog = new LSRetailPosis.POSProcesses.frmSalesPerson())
                                    {
                                        LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog);
                                        Employeeid = dialog.SelectedEmployeeId;
                                        EmployeeName = dialog.SelectEmployeeName;
                                    }
                                }

                                RetailTransaction myTransaction = (RetailTransaction)posTransaction;
                                if (myTransaction.SaleItems.Count > 0)
                                {

                                    LinkedList<SaleLineItem> Saleslines1 = myTransaction.SaleItems;
                                    LinkedList<SaleLineItem>.Enumerator enumtr1 = Saleslines.GetEnumerator();
                                    while (enumtr1.MoveNext())
                                    {
                                        enumtr1.Current.SalesPersonId = Employeeid;
                                        enumtr1.Current.SalespersonName = EmployeeName;

                                    }
                                }

                            }
                            break;
                        }
                    }

                    if (tst == false)
                    {
                        break;
                    }

                    if (myTransaction1.LoyaltyItem.LoyaltyCardNumber.Length > 0)
                    {
                        MessageBox.Show("Qitaf Can't be process because Loyalty Card is applied. لا يمكن إستخدام قطاف فى حالة إضافة حساب ريف ستار ");
                        break;
                    }
                    using (Microsoft.Dynamics.Retail.Pos.BlankOperations.Dialog.frmPNCode dialog1 = new Microsoft.Dynamics.Retail.Pos.BlankOperations.Dialog.frmPNCode())
                    {
                        LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog1);
                        code = dialog1.Code;

                        if (code.ToString().Length > 7)
                        {
                            // if he enter, send SMS
                            m_strPN = dialog1.strPN.ToString();

                        }
                        else
                        {
                            MessageBox.Show("Pay Qitaf can't be processed without phone number. Please Try again.  ");
                            break;
                        }

                    }

                    int nRespCode1 = call_Qitaf_1_GenOTP();
                    if (nRespCode1 != 0)
                    {
                        break;
                    }

                    m_strTansactionID4POS = ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).TransactionId;
                    m_strStoreId4POS = ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).StoreId;
                    // input Phone number
                    int nRespCode2 = on_QitafRedeemPoint();
                    if (nRespCode2 > 0)
                    {
                        break;
                    }
                    if (nRespCode2 < 0)
                    {
                        break;
                    }

                    //if (tst == true)
                    {
                        // pay by Qitaf 
                        PayCash pay = new PayCash(false, "1");
                        pay.OperationID = PosisOperations.PayCash; // choose ure payment method
                        pay.OperationInfo = new OperationInfo();
                        pay.OperationInfo.NumpadValue = m_strAmount;
                        pay.POSTransaction = (PosTransaction)posTransaction;
                        pay.Amount = decimal.Parse(m_strAmount);
                        pay.RunOperation();
                        decimal paid = pay.Amount;
                        /*if (((LSRetailPosis.Transaction.RetailTransaction)posTransaction).NetAmountWithTax ==
                            ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).Payment +
                            paid)*/
                        {
                            // run update query here? yes OK!
                            string strTranId = posTransaction.TransactionId;
                            updateTransTbl4ReturnQty(strTranId);
                            /*m_strRewardAmount = ((int)((LSRetailPosis.Transaction.RetailTransaction)posTransaction).NetAmountWithTax).ToString();
                            process_Qitaf_RewardPoint();*/
                        }
                        MessageBox.Show("SAR  تم دفع مبلغ من حساب قطاف  بنجاح بقيمة " + m_strAmount);
                    }


                    break;

                case "updatepoint":
                    myTransaction1 = (RetailTransaction)posTransaction;
                    if (myTransaction1.SaleItems.Count > 0)
                    {

                        LinkedList<SaleLineItem> Saleslines = myTransaction1.SaleItems;
                        LinkedList<SaleLineItem>.Enumerator enumtr = Saleslines.GetEnumerator();
                        while (enumtr.MoveNext())
                        {
                            if (enumtr.Current.SalesPersonId == string.Empty || enumtr.Current.SalesPersonId == null)
                            {
                                MessageBox.Show("Please select Salesman من فضلك إدخل رقم البائع ");
                                tst = false;
                                break;
                            }

                        }
                        if (tst == false)
                        {

                            Employeeid = "";
                            EmployeeName = "";


                            if (((object)posTransaction).GetType() == typeof(RetailTransaction))
                            {
                                if (((RetailTransaction)posTransaction).SaleItems.Count > 0)
                                {
                                    using (LSRetailPosis.POSProcesses.frmSalesPerson dialog = new LSRetailPosis.POSProcesses.frmSalesPerson())
                                    {
                                        LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog);
                                        Employeeid = dialog.SelectedEmployeeId;
                                        EmployeeName = dialog.SelectEmployeeName;
                                    }
                                }

                                RetailTransaction myTransaction = (RetailTransaction)posTransaction;
                                if (myTransaction.SaleItems.Count > 0)
                                {

                                    LinkedList<SaleLineItem> Saleslines1 = myTransaction.SaleItems;
                                    LinkedList<SaleLineItem>.Enumerator enumtr1 = Saleslines.GetEnumerator();
                                    while (enumtr1.MoveNext())
                                    {
                                        enumtr1.Current.SalesPersonId = Employeeid;
                                        enumtr1.Current.SalespersonName = EmployeeName;

                                    }
                                }

                            }
                            break;
                        }
                    }

                    if (tst == false)
                    {
                        break;
                    }

                    if (tst)
                    {
                        SaleLineItem aItem1 = myTransaction1.SaleItems.First();
                        bool bSuccess = processQitafRewardUpdateIfAvailable(aItem1.ReturnTransId, aItem1.NetAmountWithTax);
                        if (bSuccess)
                        {
                            /*
                            PayCash pay = new PayCash(false, "1");

                            pay.OperationID = PosisOperations.PayCash; // choose ure payment method
                            pay.OperationInfo = new OperationInfo();
                            pay.OperationInfo.NumpadValue = "-" + m_strReductionAmount;
                            pay.POSTransaction = (PosTransaction)posTransaction;
                            pay.Amount = decimal.Parse("-" + m_strAmount);
                            pay.RunOperation();*/

                            //MessageBox.Show("Qitaf Reward Update has been completed successfully.");

                        }

                        {
                            // there is no previous transaction for RewardUpdate
                            Application.RunOperation(PosisOperations.PayCash, 1);
                            break;
                        }

                    }

                    break;

                case "reversonreturn":
                    // test case
                    // PN:050 661 7981
                    // Pay qitaf, "Show Journal"-> "Return Transacton" ?
                    // Select and return item 

                    // -- process step
                    // 1. get receipt id, tranId, get qitaf tranid, get qitaf amount, reverseQitafAmount
                    // 1.. then need to create new transaction for reverQitaf?
                    // 2. return residue amount by cash
                    // 2.. then need to create new transaction for refund cash?



                    myTransaction1 = (RetailTransaction)posTransaction;

                    if (myTransaction1.SaleItems.Count > 0)
                    {
                        LinkedList<SaleLineItem> Saleslines = myTransaction1.SaleItems;
                        LinkedList<SaleLineItem>.Enumerator enumtr = Saleslines.GetEnumerator();
                        while (enumtr.MoveNext())
                        {
                            if (enumtr.Current.SalesPersonId == string.Empty || enumtr.Current.SalesPersonId == null)
                            {
                                MessageBox.Show("Please select Salesman من فضلك إدخل رقم البائع ");
                                tst = false;
                                break;
                            }

                        }
                        if (tst == false)
                        {

                            Employeeid = "";
                            EmployeeName = "";


                            if (((object)posTransaction).GetType() == typeof(RetailTransaction))
                            {
                                if (((RetailTransaction)posTransaction).SaleItems.Count > 0)
                                {
                                    using (LSRetailPosis.POSProcesses.frmSalesPerson dialog = new LSRetailPosis.POSProcesses.frmSalesPerson())
                                    {
                                        LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog);
                                        Employeeid = dialog.SelectedEmployeeId;
                                        EmployeeName = dialog.SelectEmployeeName;
                                    }
                                }

                                RetailTransaction myTransaction = (RetailTransaction)posTransaction;
                                if (myTransaction.SaleItems.Count > 0)
                                {

                                    LinkedList<SaleLineItem> Saleslines1 = myTransaction.SaleItems;
                                    LinkedList<SaleLineItem>.Enumerator enumtr1 = Saleslines.GetEnumerator();
                                    while (enumtr1.MoveNext())
                                    {
                                        enumtr1.Current.SalesPersonId = Employeeid;
                                        enumtr1.Current.SalespersonName = EmployeeName;

                                    }
                                }

                            }
                            break;
                        }
                    }
                    if (myTransaction1.LoyaltyItem.LoyaltyCardNumber != null && myTransaction1.LoyaltyItem.LoyaltyCardNumber.Length > 0)
                    {
                        MessageBox.Show("Qitaf Can't be process because Loyalty Card is applied. لا يمكن إستخدام قطاف فى حالة إضافة حساب ريف ستار ");
                        break;
                    }

                    m_nResponseCode_QitafApi2 = 0;
                    // Init reverse params.
                    /*
                     --select * from ax.RetailQitaftrans
                    select netamount,netamountincltax,price,transactionid,receiptid,returntransactionid from testdatabase.ax.RETAILTRANSACTIONSALESTRANS
                    --select * from testdatabase.ax.RETAILTRANSACTIONSALESTRANS
                    where TRANSDATE = '03-26-2022'
                     */

                    m_strTransactionGUIDRedeem = "";
                    m_strAmount = "";
                    m_strPN = "";// @"506617981";
                    m_strRefRequestDate = "";//transDATATIME for Reward
                    m_strRefRequestId = "";//tranGUIDReward for Reward
                    m_strTansactionID4POS = ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).TransactionId;
                    SaleLineItem aItem = myTransaction1.SaleItems.First();
                    String strRetInvoiceId = aItem.ReturnInvoiceId;
                    m_strTansactionID4POS = aItem.ReturnTransId;

                    bool bRet = prepareFields4ReverseQitaf(m_strTansactionID4POS);
                    if (m_strRefRequestId.Length < 4)
                    {
                        m_strRefRequestId = m_strTransactionGUIDRedeem;
                    }
                    if (m_nResponseCode_QitafApi2 == 0 && bRet)
                    {
                        int nResCode3 = call_Qitaf_3_ReversePoint();
                        if (nResCode3 == 0)
                        {
                            PayCash pay = new PayCash(false, "1");

                            pay.OperationID = PosisOperations.PayCash; // choose ure payment method
                            pay.OperationInfo = new OperationInfo();
                            pay.OperationInfo.NumpadValue = "-" + m_strAmount;
                            pay.POSTransaction = (PosTransaction)posTransaction;
                            pay.Amount = decimal.Parse("-" + m_strAmount);
                            pay.RunOperation();

                            MessageBox.Show("تم إسترجاع نقاط قطاف لحساب العميل مرة أخري");
                            break;
                        }
                        else
                        {
                            //MessageBox.Show("Qitaf Reverse Point has been failed." + m_strRetMessage);
                            break;
                        }
                    }
                    else
                    {
                        MessageBox.Show(".لا يوجد نقطا مدفوعة لإسترجاعها ");
                    }
                    break;

                case "reserveQitaf": // Qitaf 
                    myTransaction1 = (RetailTransaction)posTransaction;
                    if (myTransaction1.SaleItems.Count > 0)
                    {

                        LinkedList<SaleLineItem> Saleslines = myTransaction1.SaleItems;
                        LinkedList<SaleLineItem>.Enumerator enumtr = Saleslines.GetEnumerator();
                        while (enumtr.MoveNext())
                        {
                            if (enumtr.Current.SalesPersonId == string.Empty || enumtr.Current.SalesPersonId == null)
                            {
                                MessageBox.Show("Please select Salesman من فضلك إدخل رقم البائع ");
                                tst = false;
                                break;
                            }

                        }
                        if (tst == false)
                        {

                            Employeeid = "";
                            EmployeeName = "";


                            if (((object)posTransaction).GetType() == typeof(RetailTransaction))
                            {
                                if (((RetailTransaction)posTransaction).SaleItems.Count > 0)
                                {
                                    using (LSRetailPosis.POSProcesses.frmSalesPerson dialog = new LSRetailPosis.POSProcesses.frmSalesPerson())
                                    {
                                        LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog);
                                        Employeeid = dialog.SelectedEmployeeId;
                                        EmployeeName = dialog.SelectEmployeeName;
                                    }
                                }

                                RetailTransaction myTransaction = (RetailTransaction)posTransaction;
                                if (myTransaction.SaleItems.Count > 0)
                                {

                                    LinkedList<SaleLineItem> Saleslines1 = myTransaction.SaleItems;
                                    LinkedList<SaleLineItem>.Enumerator enumtr1 = Saleslines.GetEnumerator();
                                    while (enumtr1.MoveNext())
                                    {
                                        enumtr1.Current.SalesPersonId = Employeeid;
                                        enumtr1.Current.SalespersonName = EmployeeName;

                                    }
                                }

                            }
                            break;
                        }
                    }
                    if (myTransaction1.LoyaltyItem.LoyaltyCardNumber.Length > 0)
                    {
                        MessageBox.Show("Qitaf Can't be process because Loyalty Card is applied. لا يمكن إستخدام قطاف فى حالة إضافة حساب ريف ستار ");
                        break;
                    }

                    if (m_nResponseCode_QitafApi2 == 0)
                    {
                        int nResCode3 = call_Qitaf_3_ReversePoint();
                        if (nResCode3 == 0)
                        {
                            PayCash pay = new PayCash(false, "1");

                            pay.OperationID = PosisOperations.PayCash; // choose ure payment method
                            pay.OperationInfo = new OperationInfo();
                            pay.OperationInfo.NumpadValue = "-" + m_strAmount;
                            pay.POSTransaction = (PosTransaction)posTransaction;
                            pay.Amount = decimal.Parse("-" + m_strAmount);
                            pay.RunOperation();

                            MessageBox.Show("تم إسترجاع نقاط قطاف لحساب العميل مرة أخري");
                            break;
                        }
                        else
                        {
                            //MessageBox.Show("Qitaf Reverse Point has been failed." + m_strRetMessage);
                            break;
                        }
                    }
                    else
                    {
                        MessageBox.Show(".لا يوجد نقطا مدفوعة لإسترجاعها ");
                    }
                    break;

                case "collectQitaf": // Qitaf 
                    myTransaction1 = (RetailTransaction)posTransaction;
                    if (myTransaction1.LoyaltyItem.LoyaltyCardNumber.Length > 0)
                    {
                        MessageBox.Show("Qitaf Can't be process because Loyalty Card is applied. لا يمكن إستخدام قطاف فى حالة إضافة حساب ريف ستار ");
                        break;
                    }
                    m_strRewardAmount = ((int)((LSRetailPosis.Transaction.RetailTransaction)posTransaction).NetAmountWithTax).ToString();
                    m_strPN4Qitaf = "";
                    m_strTansactionID4POS = ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).TransactionId;
                    m_strStoreId4POS = ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).StoreId;

                    // input Phone number
                    using (Microsoft.Dynamics.Retail.Pos.BlankOperations.Dialog.frmPNCode dialog1 = new Microsoft.Dynamics.Retail.Pos.BlankOperations.Dialog.frmPNCode())
                    {
                        LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog1);
                        code = dialog1.Code;

                        if (code.ToString().Length > 7)
                        {
                            // if he enter, send SMS
                            m_strPN4Qitaf = dialog1.strPN.ToString();
                            double fMount = ((double)((LSRetailPosis.Transaction.RetailTransaction)posTransaction).NetAmountWithTax) * 0.05;// Convert.ToDouble(m_strRewardAmount) / 20;
                            string strAmount = fMount.ToString("N");
                            string strMessage = "تم حفظ رقم الجوال " + m_strPN4Qitaf + "لإضافة نقاط إلى قطاف بقيمة " + strAmount + "نقطة";
                            MessageBox.Show(strMessage);
                            break;
                        }
                        else
                        {
                            // if he skip enter, no SMS
                            // don't send link
                            MessageBox.Show("Collect Qitaf can't be processed without phone number. Please Try again.\n الرجاء إعادة المحاولة لا يمكن إضافة مشتريات العميل لقطاف بدون إدخال رقم الجوال ");
                            break;
                        }

                    }






                case "PayLoyaltyNew": // payment case 2
                    myTransaction1 = (RetailTransaction)posTransaction;

                    if (((LSRetailPosis.Transaction.RetailTransaction)posTransaction).Payment >=
                            ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).TaxAmount)
                    {


                        if (myTransaction1.SaleItems.Count > 0)
                        {

                            LinkedList<SaleLineItem> Saleslines = myTransaction1.SaleItems;
                            LinkedList<SaleLineItem>.Enumerator enumtr = Saleslines.GetEnumerator();
                            while (enumtr.MoveNext())
                            {
                                if (enumtr.Current.SalesPersonId == string.Empty || enumtr.Current.SalesPersonId == null)
                                {
                                    MessageBox.Show("Please select Salesman من فضلك إدخل رقم البائع ");
                                    tst = false;
                                    break;
                                }

                            }
                            if (tst == false)
                            {

                                Employeeid = "";
                                EmployeeName = "";


                                if (((object)posTransaction).GetType() == typeof(RetailTransaction))
                                {
                                    if (((RetailTransaction)posTransaction).SaleItems.Count > 0)
                                    {
                                        using (LSRetailPosis.POSProcesses.frmSalesPerson dialog = new LSRetailPosis.POSProcesses.frmSalesPerson())
                                        {
                                            LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog);
                                            Employeeid = dialog.SelectedEmployeeId;
                                            EmployeeName = dialog.SelectEmployeeName;
                                        }
                                    }

                                    RetailTransaction myTransaction = (RetailTransaction)posTransaction;
                                    if (myTransaction.SaleItems.Count > 0)
                                    {

                                        LinkedList<SaleLineItem> Saleslines1 = myTransaction.SaleItems;
                                        LinkedList<SaleLineItem>.Enumerator enumtr1 = Saleslines.GetEnumerator();
                                        while (enumtr1.MoveNext())
                                        {
                                            enumtr1.Current.SalesPersonId = Employeeid;
                                            enumtr1.Current.SalespersonName = EmployeeName;

                                        }
                                    }

                                }
                                break;
                            }
                        }
                        if (tst == true)
                        {
                            Application.RunOperation(PosisOperations.PayLoyalty, 23);
                            genShelf(posTransaction.TransactionId, posTransaction.StoreId, false);

                        }

                    }
                    else
                    {
                        MessageBox.Show("المبلغ المدفوع أقل من الضريبة فضلا تحصيل الضريبة من العميل  ");
                    }
                    break;
                case "CashNew": // payment case 3 
                    myTransaction1 = (RetailTransaction)posTransaction;
                    if (myTransaction1.SaleItems.Count > 0)
                    {

                        LinkedList<SaleLineItem> Saleslines = myTransaction1.SaleItems;
                        LinkedList<SaleLineItem>.Enumerator enumtr = Saleslines.GetEnumerator();
                        while (enumtr.MoveNext())
                        {
                            if (enumtr.Current.SalesPersonId == string.Empty || enumtr.Current.SalesPersonId == null)
                            {
                                MessageBox.Show("Please select Salesman من فضلك إدخل رقم البائع ");
                                tst = false;
                                break;
                            }

                        }
                        if (tst == false)
                        {

                            Employeeid = "";
                            EmployeeName = "";


                            if (((object)posTransaction).GetType() == typeof(RetailTransaction))
                            {
                                if (((RetailTransaction)posTransaction).SaleItems.Count > 0)
                                {
                                    using (LSRetailPosis.POSProcesses.frmSalesPerson dialog = new LSRetailPosis.POSProcesses.frmSalesPerson())
                                    {
                                        LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog);
                                        Employeeid = dialog.SelectedEmployeeId;
                                        EmployeeName = dialog.SelectEmployeeName;
                                    }
                                }

                                RetailTransaction myTransaction = (RetailTransaction)posTransaction;
                                if (myTransaction.SaleItems.Count > 0)
                                {

                                    LinkedList<SaleLineItem> Saleslines1 = myTransaction.SaleItems;
                                    LinkedList<SaleLineItem>.Enumerator enumtr1 = Saleslines.GetEnumerator();
                                    while (enumtr1.MoveNext())
                                    {
                                        enumtr1.Current.SalesPersonId = Employeeid;
                                        enumtr1.Current.SalespersonName = EmployeeName;

                                    }
                                }

                            }
                            break;
                        }
                    }

                    if (tst == true)
                    {
                        string strRegkey2 = "regAmount";
                        RegistryKey myKey2 = Registry.CurrentUser.OpenSubKey(strRegkey2, true);
                        if (myKey2 != null)
                        {
                            myKey2.SetValue(strRegkey2, "00", RegistryValueKind.String);
                            myKey2.Close();
                        }

                        SaleLineItem aItem1 = myTransaction1.SaleItems.First();//!!!!QitafRewardUpdate
                        bool bSuccess = processQitafRewardUpdateIfAvailable(aItem1.ReturnTransId, aItem1.NetAmountWithTax);

                        Application.RunOperation(PosisOperations.PayCash, 1);
                        string strScanInfo = "00";
                        RegistryKey myKey = Registry.CurrentUser.OpenSubKey(strRegkey2, true);
                        if (myKey != null)
                        {
                            strScanInfo = myKey.GetValue(strRegkey2).ToString();
                            myKey.Close();
                        }
                        decimal paid = Decimal.Parse(strScanInfo);
                        if (((LSRetailPosis.Transaction.RetailTransaction)posTransaction).NetAmountWithTax ==
                            ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).Payment +
                            paid)
                        {
                            m_strRewardAmount = ((int)((LSRetailPosis.Transaction.RetailTransaction)posTransaction).NetAmountWithTax).ToString();
                            process_Qitaf_RewardPoint(posTransaction.TransactionId);
                            genShelf(posTransaction.TransactionId, posTransaction.StoreId, false);
                        }

                    }

                    if (((LSRetailPosis.Transaction.RetailTransaction)posTransaction).NetAmountWithTax ==
                            ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).Payment)
                    {// or if(((LSRetailPosis.Transaction.RetailTransaction)posTransaction).TransSalePmtDiff == 0)
                        Console.WriteLine("Amount:" +
                            ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).NetAmountWithTax +
                            ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).TaxAmount +
                            ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).Payment +
                            ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).NetAmountWithTaxAndCharges +
                            ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).NetAmountWithNoTax +
                            ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).TransSalePmtDiff
                            );
                    }

                    break;

                case "tester":
                    {

                        string storeid = posTransaction.StoreId;
                        using (Microsoft.Dynamics.Retail.Pos.BlankOperations.Dialog.frmTester dlgTester = new Microsoft.Dynamics.Retail.Pos.BlankOperations.Dialog.frmTester(storeid))
                        {
                            //dlgTester.m_strStorenumber = storeid;
                            LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dlgTester);
                            //string strRet = dlgTester..Code.ToString();
                            Console.WriteLine("Tester DONE");
                        }

                    }

                    break;


                case "CashQuickNew": // payment case 4
                    tst = true;
                    myTransaction1 = (RetailTransaction)posTransaction;
                    if (myTransaction1.SaleItems.Count > 0)
                    {

                        LinkedList<SaleLineItem> Saleslines = myTransaction1.SaleItems;
                        LinkedList<SaleLineItem>.Enumerator enumtr = Saleslines.GetEnumerator();
                        while (enumtr.MoveNext())
                        {
                            if (enumtr.Current.SalesPersonId == string.Empty || enumtr.Current.SalesPersonId == null)
                            {
                                MessageBox.Show("Please select Salesman من فضلك إدخل رقم البائع");
                                tst = false;
                                break;
                            }

                        }
                    }
                    if (tst == true)
                    {
                        SaleLineItem aItem1 = myTransaction1.SaleItems.First();//!!!!QitafRewardUpdate
                        bool bSuccess = processQitafRewardUpdateIfAvailable(aItem1.ReturnTransId, aItem1.NetAmountWithTax);

                        Application.RunOperation(PosisOperations.PayCashQuick, 1);
                        m_strRewardAmount = ((int)((LSRetailPosis.Transaction.RetailTransaction)posTransaction).NetAmountWithTax).ToString();
                        process_Qitaf_RewardPoint(posTransaction.TransactionId);
                        genShelf(posTransaction.TransactionId, posTransaction.StoreId, false);

                    }


                    break;


                //-----------------------------------------------------
                // worker discount from mohamed abd Elnabi 
                case "workerdiscount":
                    Employeeid = "";
                    EmployeeName = "";
                    using (LSRetailPosis.POSProcesses.frmSalesPerson dialog = new LSRetailPosis.POSProcesses.frmSalesPerson())
                    {
                        LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog);
                        Employeeid = dialog.SelectedEmployeeId;
                        EmployeeName = dialog.SelectEmployeeName;

                        decimal nAmount =
                            ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).NetAmountWithTax -
                            ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).Payment;
                        //int discountpercent;
                        //discountpercent = 70;

                        // check if worker id is existing on parameter table
                        bool bExist = true;
                        fDiscountPercentage = 0;
                        bExist = IsRegisteredWorkerid(Employeeid);
                        nAmount = nAmount * ((decimal)fDiscountPercentage / 100);
                        string strAmount = nAmount.ToString("0.00");
                        if (bExist == true)
                        {
                            double fAmountDiscount = Convert.ToDouble(nAmount.ToString());
                            fSumDiscount4Year = getSumDiscount4Year(Employeeid); // check if discount_amount + discounttake by worker > Retailworkerdiscountparameter.amount
                            if (fDiscountLimitPerYear >= fSumDiscount4Year + fAmountDiscount)
                            {
                                bool bCorrectSMS =
                                SendSMS(strCardNumber, decimal.Parse(strAmount), 1);
                                if (bCorrectSMS)
                                {
                                    // Process payment for discount 50%
                                    ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).SetTotalDiscAmount(nAmount);
                                    decimal dDiscountPecent = (decimal)fDiscountPercentage;
                                    ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).SetTotalDiscPercent(dDiscountPecent);
                                    decimal dTotDiscount = ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).TotalDiscount;
                                    Application.RunOperation(PosisOperations.TotalDiscountAmount, nAmount);
                                    // insert new transaction into the table : ax.Retailworkerdiscounttrans with new payment info
                                    Insert_workerDiscountTrans(
                                        ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).TransactionId,
                                        Employeeid, strCardNumber, strAmount,
                                        ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).StoreId,
                                        ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).ReceiptId);
                                }
                                else
                                {
                                    // invalid sms => will show message from SMS lib function
                                    MessageBox.Show("Invalid Code رمز التأكيد خطأ ");
                                }
                            }
                            else
                            {// Discount is not available
                                // errror message : "You've crossed the limit for discount"
                                string strMsg = "You've crossed the limit for discount  لقد تجازوت حد الخصم المسموح به  .\n";
                                strMsg += "Your limit  حد الخصم السنوى : " + fDiscountLimitPerYear.ToString("F") + "\n";
                                strMsg += "and you used لقد إستخدمت    :" + fSumDiscount4Year.ToString("F") + "\n";
                                strMsg += "remaing is متبقي  : " + (fDiscountLimitPerYear - fSumDiscount4Year).ToString("F") + "\n";
                                MessageBox.Show(strMsg + "لقد تجازت الحد الأعلى المسموح لك به خلال العام "); // need to add his limit 
                            }
                        }
                        else
                        {// workerid is not existing in the worker table
                            MessageBox.Show("Unregistered worker الرقم المدخل غير مسجل بالنظام ولا يوجد له صلاحية خصم ");
                        }


                    }


                    break;

                case "workernotinlist":

                    // we hide this form and show form same vervcation code to take number manually  only  the number from form == Employeeid
                    // we stop show this form   and copy vervection code form and numebr from vecation code == em id and all same 

                    Employeeid = "";
                    EmployeeName = "";

                    using (LSRetailPosis.POSProcesses.frmInputNumpad dialog1 = new LSRetailPosis.POSProcesses.frmInputNumpad())
                    {
                        dialog1.PromptText = "من فضلك أدخل الرقم الوظيفي ، أول الجوال المسجل بالنظام ";

                        LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog1);

                        Employeeid = dialog1.InputText.ToString();

                        decimal nAmount =
                            ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).NetAmountWithTax -
                            ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).Payment;

                        // check if worker id is existing on parameter table
                        bool bExist = true;
                        fDiscountPercentage = 0;
                        bExist = IsRegisteredWorkerid(Employeeid);
                        nAmount = nAmount * ((decimal)fDiscountPercentage / 100);
                        string strAmount = nAmount.ToString("0.00");
                        if (bExist == true)
                        {
                            double fAmountDiscount = Convert.ToDouble(nAmount.ToString());
                            fSumDiscount4Year = getSumDiscount4Year(Employeeid); // check if discount_amount + discounttake by worker > Retailworkerdiscountparameter.amount
                            if (fDiscountLimitPerYear >= fSumDiscount4Year + fAmountDiscount)
                            {
                                bool bCorrectSMS =
                                SendSMS(strCardNumber, decimal.Parse(strAmount), 1);
                                if (bCorrectSMS)
                                {
                                    // Process payment for discount 50%
                                    ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).SetTotalDiscAmount(nAmount);
                                    decimal dDiscountPecent = (decimal)fDiscountPercentage;
                                    ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).SetTotalDiscPercent(dDiscountPecent);
                                    decimal dTotDiscount = ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).TotalDiscount;
                                    Application.RunOperation(PosisOperations.TotalDiscountAmount, nAmount);
                                    // insert new transaction into the table : ax.Retailworkerdiscounttrans with new payment info
                                    Insert_workerDiscountTrans(
                                        ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).TransactionId,
                                        Employeeid, strCardNumber, strAmount,
                                        ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).StoreId,
                                        ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).ReceiptId);
                                }
                                else
                                {
                                    // invalid sms => will show message from SMS lib function
                                    MessageBox.Show("Invalid Code رمز التأكيد خطأ ");
                                }
                            }
                            else
                            {// Discount is not available
                                // errror message : "You've crossed the limit for discount"
                                string strMsg = "You've crossed the limit for discount  لقد تجازوت حد الخصم المسموح به  .\n";
                                strMsg += "Your limit  حد الخصم السنوى : " + fDiscountLimitPerYear.ToString("F") + "\n";
                                strMsg += "and you used لقد إستخدمت    :" + fSumDiscount4Year.ToString("F") + "\n";
                                strMsg += "remaing is متبقي  : " + (fDiscountLimitPerYear - fSumDiscount4Year).ToString("F") + "\n";
                                MessageBox.Show(strMsg + "لقد تجازت الحد الأعلى المسموح لك به خلال العام "); // need to add his limit 
                            }
                        }
                        else
                        {// workerid is not existing in the worker table
                            MessageBox.Show("Unregistered worker الرقم المدخل غير مسجل بالنظام ولا يوجد له صلاحية خصم ");
                        }


                    }


                    break;



                case "Ownerdiscount":

                    // we hide this form and show form same vervcation code to take number manually  only  the number from form == Employeeid
                    // we stop show this form   and copy vervection code form and numebr from vecation code == em id and all same 

                    Employeeid = "";
                    EmployeeName = "";

                    using (LSRetailPosis.POSProcesses.frmInputNumpad dialog1 = new LSRetailPosis.POSProcesses.frmInputNumpad())
                    {
                        dialog1.PromptText = "من فضلك أدخل الرقم الوظيفي ، أول الجوال المسجل بالنظام ";

                        LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog1);

                        Employeeid = dialog1.InputText.ToString();

                        decimal nAmount =
                            ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).NetAmountWithTax -
                            ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).Payment;

                        // check if worker id is existing on parameter table
                        bool bExist = true;
                        fDiscountPercentage = 0;
                        bExist = IsRegisteredWorkerAndOwner(Employeeid);
                        if (bExist == false)
                        {
                            // workerid is not existing in the worker table
                            MessageBox.Show("Unregistered worker الرقم المدخل غير مسجل بالنظام ولا يوجد له صلاحية خصم ");
                            break;
                        }
                        nAmount = nAmount * ((decimal)fDiscountPercentage / 100);
                        LSRetailPosis.Transaction.RetailTransaction curTransaction = ((LSRetailPosis.Transaction.RetailTransaction)posTransaction);
                        decimal amountSum = 0;
                        for (int i = 0; i < curTransaction.SaleItems.Count; i++)
                        {
                            LSRetailPosis.Transaction.Line.SaleItem.SaleLineItem item1 = curTransaction.SaleItems.ElementAt(i);
                            if (item1.ItemId.StartsWith("30"))
                            {
                                amountSum += item1.Price * (ndiscount30) / 100 * item1.Quantity; // this 2 take form filed discount30


                            }
                            else if (item1.ItemId.StartsWith("34"))
                            {
                                amountSum += item1.Price * (ndiscount34) / 100 * item1.Quantity; // this 35 take from discount34

                            }
                            else if (item1.ItemId.StartsWith("8"))
                            {
                                amountSum += item1.Price * (ndiscount8) / 100 * item1.Quantity;  // this 35 take from   [discount8] from database 

                            }
                            else if (item1.ItemId.StartsWith("9"))
                            {
                                amountSum += item1.Price * (ndiscount9) / 100 * item1.Quantity; // this 35 take from   [discount9] from database

                            }
                            else
                            {
                                // this 0 take from   [elsediscount] from database
                                amountSum += item1.Price * (nelsediscount) / 100 * item1.Quantity;
                            }

                        }
                        nAmount = amountSum;
                        string strAmount = nAmount.ToString("0.00");
                        {
                            double fAmountDiscount = Convert.ToDouble(nAmount.ToString());
                            fSumDiscount4Year = getSumDiscount4Year(Employeeid); // check if discount_amount + discounttake by worker > Retailworkerdiscountparameter.amount
                            if (fDiscountLimitPerYear >= fSumDiscount4Year + fAmountDiscount)
                            {
                                bool bCorrectSMS =
                                SendSMS(strCardNumber, decimal.Parse(strAmount), 1);
                                if (bCorrectSMS)
                                {
                                    // Process payment for discount 50%
                                    fDiscountPercentage = 0;//!! Ignore normal discount for owner
                                    decimal dDiscountPecent = (decimal)fDiscountPercentage;
                                    ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).SetTotalDiscPercent(dDiscountPecent);
                                    decimal dTotDiscount = ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).TotalDiscount;
                                    ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).SetTotalDiscAmount(nAmount);
                                    Application.RunOperation(PosisOperations.TotalDiscountAmount, nAmount);
                                    //Application.RunOperation(PosisOperations.LineDiscountAmount, nAmount);
                                    //Application.RunOperation(PosisOperations.CalculateFullDiscounts, nAmount); 
                                    // insert new transaction into the table : ax.Retailworkerdiscounttrans with new payment info
                                    Insert_workerDiscountTrans(
                                        ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).TransactionId,
                                        Employeeid, strCardNumber, strAmount,
                                        ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).StoreId,
                                        ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).ReceiptId);
                                }
                                else
                                {
                                    // invalid sms => will show message from SMS lib function
                                    MessageBox.Show("Invalid Code رمز التأكيد خطأ ");
                                }
                            }
                            else
                            {// Discount is not available
                                // errror message : "You've crossed the limit for discount"
                                string strMsg = "You've crossed the limit for discount  لقد تجازوت حد الخصم المسموح به  .\n";
                                strMsg += "Your limit  حد الخصم السنوى : " + fDiscountLimitPerYear.ToString("F") + "\n";
                                strMsg += "and you used لقد إستخدمت    :" + fSumDiscount4Year.ToString("F") + "\n";
                                strMsg += "remaing is متبقي  : " + (fDiscountLimitPerYear - fSumDiscount4Year).ToString("F") + "\n";
                                MessageBox.Show(strMsg + "لقد تجازت الحد الأعلى المسموح لك به خلال العام "); // need to add his limit 
                            }
                        }



                    }


                    break;







                case "99": // payment case 5
                    {
                        myTransaction1 = (RetailTransaction)posTransaction;

                        if (myTransaction1.SaleItems.Count > 0)
                        {

                            LinkedList<SaleLineItem> Saleslines = myTransaction1.SaleItems;
                            LinkedList<SaleLineItem>.Enumerator enumtr = Saleslines.GetEnumerator();
                            while (enumtr.MoveNext())
                            {
                                if (enumtr.Current.SalesPersonId == string.Empty || enumtr.Current.SalesPersonId == null)
                                {
                                    MessageBox.Show("Please select Salesman من فضلك إدخل رقم البائع ");
                                    tst = false;
                                    break;
                                }

                            }
                            if (tst == false)
                            {

                                Employeeid = "";
                                EmployeeName = "";


                                if (((object)posTransaction).GetType() == typeof(RetailTransaction))
                                {
                                    if (((RetailTransaction)posTransaction).SaleItems.Count > 0)
                                    {
                                        using (LSRetailPosis.POSProcesses.frmSalesPerson dialog = new LSRetailPosis.POSProcesses.frmSalesPerson())
                                        {
                                            LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog);
                                            Employeeid = dialog.SelectedEmployeeId;
                                            EmployeeName = dialog.SelectEmployeeName;
                                        }
                                    }

                                    RetailTransaction myTransaction = (RetailTransaction)posTransaction;
                                    if (myTransaction.SaleItems.Count > 0)
                                    {

                                        LinkedList<SaleLineItem> Saleslines1 = myTransaction.SaleItems;
                                        LinkedList<SaleLineItem>.Enumerator enumtr1 = Saleslines.GetEnumerator();
                                        while (enumtr1.MoveNext())
                                        {
                                            enumtr1.Current.SalesPersonId = Employeeid;
                                            enumtr1.Current.SalespersonName = EmployeeName;

                                        }
                                    }

                                }
                                break;
                            }
                        }

                        // select Port from user input afz
                        int nPort = 254;


                        //!! call device  on port 255 ;   api_CheckStatus
                        int nStatusOfPort = test_CommTest(nPort);//api_CommTest()
                        decimal nAmount =
                            ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).NetAmountWithTax -
                            ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).Payment;
                        nAmount = decimal.Multiply(nAmount, 100);
                        int nCost = (int)nAmount;
                        String strAmount = nCost.ToString();
                        string PaymentId1 = "1"; // need to change this string To schemeId value 




                        if (nStatusOfPort == 0)
                        {
                            //nStatus = api_comstatus(nPort);
                            int nRespondCode;
                            bool isApproved;
                            int nRetTrans = do_TransactionByCOMM(nPort, strAmount, out PaymentId1, out nRespondCode, out isApproved);

                            if (nRetTrans == 0)
                            {
                                if (isApproved)
                                {
                                    SaleLineItem aItem1 = myTransaction1.SaleItems.First();//!!!!QitafRewardUpdate
                                    bool bSuccess = processQitafRewardUpdateIfAvailable(aItem1.ReturnTransId, aItem1.NetAmountWithTax);

                                    var application = PosApplication.Instance as IApplication;
                                    application.RunOperation(PosisOperations.PayCashQuick, PaymentId1, posTransaction);
                                    // sendLinkAfterPayment(posTransaction.ReceiptId); 
                                    m_strRewardAmount = ((int)((LSRetailPosis.Transaction.RetailTransaction)posTransaction).NetAmountWithTax).ToString();
                                    process_Qitaf_RewardPoint(posTransaction.TransactionId);
                                    genShelf(posTransaction.TransactionId, posTransaction.StoreId, false);
                                }
                            }
                            else
                            {
                                showMessageForm_Api(nRetTrans);
                                break;
                            }
                        }
                        else
                        {
                            showMessageForm_Api(nStatusOfPort);
                            break;
                        }

                    }



                    break;
                //-----------------------------------------------------
                case "992": // payment case 6
                    {
                        myTransaction1 = (RetailTransaction)posTransaction;

                        if (myTransaction1.SaleItems.Count > 0)
                        {

                            LinkedList<SaleLineItem> Saleslines = myTransaction1.SaleItems;
                            LinkedList<SaleLineItem>.Enumerator enumtr = Saleslines.GetEnumerator();
                            while (enumtr.MoveNext())
                            {
                                if (enumtr.Current.SalesPersonId == string.Empty || enumtr.Current.SalesPersonId == null)
                                {
                                    MessageBox.Show("Please select Salesman من فضلك إدخل رقم البائع ");
                                    tst = false;
                                    break;
                                }

                            }
                            if (tst == false)
                            {

                                Employeeid = "";
                                EmployeeName = "";


                                if (((object)posTransaction).GetType() == typeof(RetailTransaction))
                                {
                                    if (((RetailTransaction)posTransaction).SaleItems.Count > 0)
                                    {
                                        using (LSRetailPosis.POSProcesses.frmSalesPerson dialog = new LSRetailPosis.POSProcesses.frmSalesPerson())
                                        {
                                            LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog);
                                            Employeeid = dialog.SelectedEmployeeId;
                                            EmployeeName = dialog.SelectEmployeeName;
                                        }
                                    }

                                    RetailTransaction myTransaction = (RetailTransaction)posTransaction;
                                    if (myTransaction.SaleItems.Count > 0)
                                    {

                                        LinkedList<SaleLineItem> Saleslines1 = myTransaction.SaleItems;
                                        LinkedList<SaleLineItem>.Enumerator enumtr1 = Saleslines.GetEnumerator();
                                        while (enumtr1.MoveNext())
                                        {
                                            enumtr1.Current.SalesPersonId = Employeeid;
                                            enumtr1.Current.SalespersonName = EmployeeName;

                                        }
                                    }

                                }
                                break;
                            }
                        }

                        // select Port from user input afz
                        int nPort = 255;


                        //!! call device  on port 255 ;   api_CheckStatus
                        int nStatusOfPort = test_CommTest(nPort);//api_CommTest()
                        decimal nAmount =
                            ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).NetAmountWithTax -
                            ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).Payment;
                        nAmount = decimal.Multiply(nAmount, 100);
                        int nCost = (int)nAmount;
                        String strAmount = nCost.ToString();
                        string PaymentId1 = "1"; // need to change this string To schemeId value 




                        if (nStatusOfPort == 0)
                        {
                            //nStatus = api_comstatus(nPort);
                            int nRespondCode;
                            bool isApproved;
                            int nRetTrans = do_TransactionByCOMM(nPort, strAmount, out PaymentId1, out nRespondCode, out isApproved);

                            if (nRetTrans == 0)
                            {
                                if (isApproved)
                                {
                                    SaleLineItem aItem1 = myTransaction1.SaleItems.First();//!!!!QitafRewardUpdate
                                    bool bSuccess = processQitafRewardUpdateIfAvailable(aItem1.ReturnTransId, aItem1.NetAmountWithTax);


                                    var application = PosApplication.Instance as IApplication;
                                    application.RunOperation(PosisOperations.PayCashQuick, PaymentId1, posTransaction);
                                    // sendLinkAfterPayment(posTransaction.ReceiptId); 
                                    m_strRewardAmount = ((int)((LSRetailPosis.Transaction.RetailTransaction)posTransaction).NetAmountWithTax).ToString();
                                    process_Qitaf_RewardPoint(posTransaction.TransactionId);
                                    genShelf(posTransaction.TransactionId, posTransaction.StoreId, false);
                                }
                            }
                            else
                            {
                                showMessageForm_Api(nRetTrans);
                                break;
                            }
                        }
                        else
                        {
                            showMessageForm_Api(nStatusOfPort);
                            break;
                        }

                    }
                    break;

                //-----------------------------------------------------

                case "990": // payment case 7


                    myTransaction1 = (RetailTransaction)posTransaction;
                    if (myTransaction1.SaleItems.Count > 0)
                    {

                        LinkedList<SaleLineItem> Saleslines = myTransaction1.SaleItems;
                        LinkedList<SaleLineItem>.Enumerator enumtr = Saleslines.GetEnumerator();
                        while (enumtr.MoveNext())
                        {
                            if (enumtr.Current.SalesPersonId == string.Empty || enumtr.Current.SalesPersonId == null)
                            {
                                MessageBox.Show("Please select Salesman من فضلك إدخل رقم البائع ");
                                tst = false;
                                break;
                            }

                        }
                        if (tst == false)
                        {

                            Employeeid = "";
                            EmployeeName = "";


                            if (((object)posTransaction).GetType() == typeof(RetailTransaction))
                            {
                                if (((RetailTransaction)posTransaction).SaleItems.Count > 0)
                                {
                                    using (LSRetailPosis.POSProcesses.frmSalesPerson dialog = new LSRetailPosis.POSProcesses.frmSalesPerson())
                                    {
                                        LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog);
                                        Employeeid = dialog.SelectedEmployeeId;
                                        EmployeeName = dialog.SelectEmployeeName;
                                    }
                                }

                                RetailTransaction myTransaction = (RetailTransaction)posTransaction;
                                if (myTransaction.SaleItems.Count > 0)
                                {

                                    LinkedList<SaleLineItem> Saleslines1 = myTransaction.SaleItems;
                                    LinkedList<SaleLineItem>.Enumerator enumtr1 = Saleslines.GetEnumerator();
                                    while (enumtr1.MoveNext())
                                    {
                                        enumtr1.Current.SalesPersonId = Employeeid;
                                        enumtr1.Current.SalespersonName = EmployeeName;

                                    }
                                }

                            }
                            break;
                        }
                    }

                    int iRet2 = -1;
                    SpanapiWrapper.SpanInteg spanInteg = new SpanapiWrapper.SpanInteg();
                    iRet2 = spanInteg.CheckExistance("255"); // Post Number


                    /*     if (iRet2 == 0  )
                         {
                             StringBuilder comments2 = new StringBuilder(128);
                             comments2.AppendFormat("The device is busy or not connected to the correct port  جهاز الشبكة مشغول أو غير متصل بالبورت الصحيح  ");
                             comments2.AppendLine();
                             using (LSRetailPosis.POSProcesses.frmMessage dialog = new LSRetailPosis.POSProcesses.frmMessage(comments2.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Stop))
                             {
                                 LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog);
                             }
                             break;

                           }*/

                    if (iRet2 == 1)
                    {
                        StringBuilder comments2 = new StringBuilder(128);
                        comments2.AppendFormat(" 1:Failed to load Spanapi.dll or failed to seek the address of the function called تحقق من وجود ملفات التشغيل بشكل صحيح   ");
                        comments2.AppendLine();
                        using (LSRetailPosis.POSProcesses.frmMessage dialog = new LSRetailPosis.POSProcesses.frmMessage(comments2.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Stop))
                        {
                            LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog);
                        }
                        break;
                    }


                    if (iRet2 == 2)
                    {
                        StringBuilder comments2 = new StringBuilder(128);
                        comments2.AppendFormat("2:The terminal is not responding to requests transmitted by the host, تحقق من إتصال الماكينة والكيبل و حالة الماكينة جاهز  ");
                        comments2.AppendLine();
                        using (LSRetailPosis.POSProcesses.frmMessage dialog = new LSRetailPosis.POSProcesses.frmMessage(comments2.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Stop))
                        {
                            LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog);
                        }
                        break;

                    }

                    if (iRet2 == 3)
                    {
                        StringBuilder comments2 = new StringBuilder(128);
                        comments2.AppendFormat("3:The serial port assigned for communicating is not present or is being used by another process. يمكنك تغيير المنفذ التسلسلي بتعديل ملف spanapi.cnf الموجود في مجلد Windows الخاص بك  ");
                        comments2.AppendLine();
                        using (LSRetailPosis.POSProcesses.frmMessage dialog = new LSRetailPosis.POSProcesses.frmMessage(comments2.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Stop))
                        {
                            LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog);
                        }
                        break;

                    }


                    if (iRet2 == 4)
                    {
                        StringBuilder comments2 = new StringBuilder(128);
                        comments2.AppendFormat("4:An unexpected error has occurred and the task requested has been cancelled.   إذا إستمرت المشكلة تواصل مع قسم الدعم الفنى  ");
                        comments2.AppendLine();
                        using (LSRetailPosis.POSProcesses.frmMessage dialog = new LSRetailPosis.POSProcesses.frmMessage(comments2.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Stop))
                        {
                            LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog);
                        }
                        break;

                    }

                    if (iRet2 == 6)
                    {
                        StringBuilder comments2 = new StringBuilder(128);
                        comments2.AppendFormat("6:An unexpected error has occurred and the task requestedAn error returned if there is no response from the terminal during the transaction execution.   has been cancelled. إذا إستمرت المشكلة تواصل مع قسم الدعم الفنى    ");
                        comments2.AppendLine();
                        using (LSRetailPosis.POSProcesses.frmMessage dialog = new LSRetailPosis.POSProcesses.frmMessage(comments2.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Stop))
                        {
                            LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog);
                        }
                        break;

                    }



                    //LSRetailPosis.Transaction.RetailTransaction myTransaction = (LSRetailPosis.Transaction.RetailTransaction)posTransaction;
                    StringBuilder StrXmlResult = new StringBuilder(10 * 1024);
                    int nXmlResultLen = new int();




                    decimal Amount = ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).NetAmountWithTax - ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).Payment;
                    string StoreId = ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).StoreId;
                    string TerminalId = ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).TerminalId;
                    string DATAAREAID = LSRetailPosis.Settings.ApplicationSettings.Database.DATAAREAID;
                    //int iRet = spanInteg.PerformEcrTrx("4", "SAlE", " ", Amount.ToString(), "",
                    //                                    ref StrCardNumber, ref StrRRN, ref StrStatusCode, ref StrResponseCode, ref StrAuthCode, ref StrCardSchemes, ref StrDate, ref StrTime, ref StrAid, ref StrStan, ref StrTerminalID);




                    int iRet = spanInteg.PerformMadaEcrTrx("255", "SALE", " ", string.Format("{0:0.}", Amount * 100), "", ref StrXmlResult, ref nXmlResultLen);

                    ///// get xml values

                    //2 line for test values//        
                    //string StrXmlResultstring = "<?xml version=\"1.0\" encoding=\"windows-1256\" standalone=\"no\" ?><madaTransactionResult><Retailer RetailerNameEng=\"Jarir Bookstore\" RetailerNameArb=\"ãßÊÈÉ ÌÑíÑ\" address_eng_1=\"Al-Olaya, Riyadh\" address_eng_2=\"\" address_arb_1=\"ÇáÚáíÇ¡ÇáÑíÇÖ\" address_arb_2=\"ÑíÇÖ\" download_phone=\"92 0000 089\" /><Performance StartDateTime=\"05042018191715\" EndDateTime=\"05042018191716\" /><BankId>SABB</BankId><MerchantID>650999000911   </MerchantID><TerminalID>1234567812121250</TerminalID><MCC>5411</MCC><STAN>000126</STAN><Version>3.009</Version><RRN>809516000126</RRN><CardScheme ID=\"VC\" Arabic=\"ÝíÒÇ\" English=\"Visa\" /><ApplicationLabel Arabic=\"ÝíÒÇ\" English=\"VISA CREDIT\" /><PAN>476173******0010</PAN><CardExpiryDate>1210</CardExpiryDate><TransactionType Arabic=\"ÔÑÇÁ\" English=\"PURCHASE\"/><Amounts ArabicCurrency=\"ÑíÇá\"  EnglishCurrency=\"SAR\"><Amount ArabicName=\"ãÈáÛ ÇáÔÑÇÁ\" EnglishName=\"PURCHASE AMOUNT\" >30.00</Amount></Amounts><Result Arabic=\"ãÑÝæÖÉ\" English=\"DECLINED\" ResponseMessageArabic=\"ÇáÑÕíÏ áÇ íÓãÍ\"  ResponseMessageEnglish=\"Not sufficient funds\"/><CardHolderName>VISA ACQUIRER TEST CARD 14</CardHolderName><EMV_Tags><PosEntryMode>DIPPED</PosEntryMode><ResponseCode>116</ResponseCode><TerminalStatusCode>01</TerminalStatusCode><TVR>0240000040</TVR><TSI>F800</TSI><CVR>1E0300</CVR><ACI>00</ACI><AC>BB0B802EEDE0E11F</AC></EMV_Tags><Campaign><QrCodeData></QrCodeData><CampaignText></CampaignText></Campaign></madaTransactionResult>";
                    //string StrXmlResultstring = "<?xml version=\"1.0\" encoding=\"windows-1256\" standalone=\"no\" ?><madaTransactionResult><Retailer RetailerNameEng=\"ALMAJED FOR OUD\" RetailerNameArb=\"ÇáãÇÌÏ ááÚæÏ\" address_eng_1=\"Alrawdhah Dist\" address_eng_2=\"\" address_arb_1=\"Íí ÇáÑæÖÉ\" address_arb_2=\"\" download_phone=\"\" /><Performance StartDateTime=\"02062020125942\" EndDateTime=\"02062020125944\" /><BankId>BSHB</BankId><MerchantID>505300000090   </MerchantID><TerminalID>8112144209516186</TerminalID><MCC>5977</MCC><STAN>002052</STAN><Version>4.410</Version><RRN>015409002052</RRN><CardScheme ID=\"AX\" Arabic=\"ÇãÑíßÇä ÇßÓÈÑíÓ\" English=\"AMERICAN EXPRESS\" /><ApplicationLabel Arabic=\"ÇãÑíßÇä ÇßÓÈÑÓ\" English=\"AMEX\" /><PAN>376655*****5622</PAN><CardExpiryDate>0823</CardExpiryDate><TransactionType Arabic=\"ÔÑÇÁ\" English=\"PURCHASE\"/><Amounts ArabicCurrency=\"ÑíÇá\"  EnglishCurrency=\"SAR\"><Amount ArabicName=\"ãÈáÛ ÇáÔÑÇÁ\" EnglishName=\"PURCHASE AMOUNT\" >3948.00</Amount></Amounts><Result Arabic=\"ãÞÈæáÉ\" English=\"APPROVED\"/><CardholderVerification Arabic=\"Êã ÇáÊÍÞÞ ãä ÇáÑÞã ÇáÓÑí ááÚãíá\" English=\"CARDHOLDER PIN VERIFIED\"/><ApprovalCode Arabic = \"ÑãÒ ÇáãæÇÝÞÉ\" English=\"APPROVAL CODE\">778000</ApprovalCode><EMV_Tags><PosEntryMode>CONTACTLESS</PosEntryMode><ResponseCode>000</ResponseCode><TerminalStatusCode>02</TerminalStatusCode><AID>A000000025010402</AID><TVR>0000048000</TVR><TSI>E800</TSI><CVR>420300</CVR><ACI>80</ACI><AC>8404BA3A56856B8D</AC><KID>04</KID></EMV_Tags><Campaign><QrCodeData></QrCodeData><CampaignText></CampaignText></Campaign></madaTransactionResult>";
                    //StrXmlResult = new StringBuilder(StrXmlResultstring);


                    //end test
                    string RRN = "";
                    string ResultEnglish = "";
                    string ResponseMessageEnglish = "";
                    string ApprovalCode = "";
                    string PAN = "";
                    string CardSchemeID = "";
                    string CardSchemeEnglish = "";
                    string ResponseCode = "";
                    string CardholderVerification = "";
                    string CardExpiryDate = "";
                    string StartDateTime = "";
                    string PaymentId = "1";
                    string TerminalStatusCode = "";

                    if (StrXmlResult.ToString() != "")
                    {
                        using (XmlReader xmlReader = XmlReader.Create(new StringReader(StrXmlResult.ToString())))
                        {
                            xmlReader.MoveToContent();
                            while (xmlReader.Read())
                            {
                                if (xmlReader.NodeType == XmlNodeType.Element)
                                {
                                    if (xmlReader.Name == "RRN")
                                    {
                                        XElement el = XNode.ReadFrom(xmlReader) as XElement;
                                        if (el != null)
                                        {
                                            RRN = el.Value;
                                        }
                                    }
                                    if (xmlReader.Name == "Result")
                                    {
                                        XElement el2 = XNode.ReadFrom(xmlReader) as XElement;
                                        foreach (var attribute in el2.Attributes())
                                        {
                                            if (attribute.Name.ToString() == "English")
                                            {
                                                ResultEnglish = attribute.Value;
                                            }
                                            if (attribute.Name.ToString() == "ResponseMessageEnglish")
                                            {
                                                ResponseMessageEnglish = attribute.Value;
                                            }

                                        }

                                    }
                                    //
                                    if (xmlReader.Name == "CardScheme")
                                    {
                                        XElement el6 = XNode.ReadFrom(xmlReader) as XElement;
                                        foreach (var attribute in el6.Attributes())
                                        {
                                            if (attribute.Name.ToString() == "ID")
                                            {
                                                CardSchemeID = attribute.Value;

                                                switch (CardSchemeID)
                                                {
                                                    case "VC":
                                                        PaymentId = "2";
                                                        break;
                                                    case "DM":
                                                        PaymentId = "5";
                                                        break;
                                                    case "MC":
                                                        PaymentId = "7";
                                                        break;
                                                    case "AX":
                                                        PaymentId = "8";
                                                        break;
                                                    case "P1":
                                                        PaymentId = "9";
                                                        break;
                                                    case "GN":
                                                        PaymentId = "18";
                                                        break;
                                                    case "UP":
                                                        PaymentId = "19";
                                                        break;
                                                    default:
                                                        break;
                                                }

                                            }
                                            if (attribute.Name.ToString() == "English")
                                            {
                                                CardSchemeEnglish = attribute.Value;
                                            }
                                        }
                                    }
                                    if (xmlReader.Name == "Performance")
                                    {
                                        XElement el7 = XNode.ReadFrom(xmlReader) as XElement;
                                        foreach (var attribute in el7.Attributes())
                                        {
                                            if (attribute.Name.ToString() == "StartDateTime")
                                            {
                                                StartDateTime = attribute.Value;
                                            }
                                        }
                                    }
                                    //
                                    if (xmlReader.Name == "PAN")
                                    {
                                        XElement el3 = XNode.ReadFrom(xmlReader) as XElement;
                                        if (el3 != null)
                                        {
                                            PAN = el3.Value;
                                        }
                                    }
                                    if (xmlReader.Name == "ApprovalCode")
                                    {
                                        XElement el4 = XNode.ReadFrom(xmlReader) as XElement;
                                        if (el4 != null)
                                        {
                                            ApprovalCode = el4.Value;
                                        }

                                    }
                                    if (xmlReader.Name == "ResponseCode")
                                    {
                                        XElement el5 = XNode.ReadFrom(xmlReader) as XElement;
                                        if (el5 != null)
                                        {
                                            ResponseCode = el5.Value;
                                        }
                                    }

                                    if (xmlReader.Name == "CardholderVerification")
                                    {
                                        XElement el100 = XNode.ReadFrom(xmlReader) as XElement;

                                        foreach (var attribute in el100.Attributes())
                                        {
                                            if (attribute.Name.ToString() == "English")
                                            {
                                                CardholderVerification = attribute.Value;
                                            }
                                        }
                                        // fkn code 
                                        if (xmlReader.Name == "ApprovalCode")
                                        {
                                            XElement el4 = XNode.ReadFrom(xmlReader) as XElement;
                                            if (el4 != null)
                                            {
                                                ApprovalCode = el4.Value;
                                            }

                                        }
                                        //
                                    }
                                    if (xmlReader.Name == "CardExpiryDate")
                                    {
                                        XElement el20 = XNode.ReadFrom(xmlReader) as XElement;
                                        if (el20 != null)
                                        {
                                            CardExpiryDate = el20.Value;
                                        }
                                    }


                                    if (xmlReader.Name == "TerminalStatusCode")
                                    {
                                        XElement el22 = XNode.ReadFrom(xmlReader) as XElement;
                                        if (el22 != null)
                                        {
                                            TerminalStatusCode = el22.Value;
                                        }
                                    }
                                }
                            }



                        }

                    }
                    /////





                    StringBuilder comments = new StringBuilder(128);
                    // Old Comment and Status // 
                    //comments.AppendFormat(" Connection : " + DispResult(iRet2) );
                    //comments.AppendLine();
                    //comments.AppendFormat("Net Amount : " + Amount.ToString("G29"));
                    //comments.AppendLine();
                    //comments.AppendFormat("Result > RRN :" + RRN + " - Status :" + ResultEnglish );
                    //comments.AppendLine();
                    //comments.AppendFormat("Message :" + ResponseMessageEnglish);
                    //comments.AppendLine();
                    if (TerminalStatusCode == "00")
                    {
                        //MessageBox.Show("Approved Transaction العملية مقبولة ");
                        /*comments.AppendFormat("Approved Transaction العملية مقبولة ");
                        comments.AppendLine();
                        using (LSRetailPosis.POSProcesses.frmMessage dialog = new LSRetailPosis.POSProcesses.frmMessage(comments.ToString(), MessageBoxButtons.OK, MessageBoxIcon.None))
                        {
                            ImageList ImageList1 = new ImageList();
                            dialog.ShowIcon = false;
                            LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog);
                        }*/
                    }
                    else
                    {
                        if (TerminalStatusCode == "01")
                        {
                            comments.AppendFormat("01: تم رفض المعاملة لأسباب مختلفة (بما في ذلك رقم التعريف الشخصي غير صالح) ، ولم يتم الخصم من الحساب.");
                        }
                        else if (TerminalStatusCode == "02")
                        {
                            comments.AppendFormat("02 :  تم رفض المعاملة ، ولم يتم الخصم من الحساب");
                        }

                        else if (TerminalStatusCode == "11")
                        {
                            comments.AppendFormat("11 :  تم الإلغاء بواسطة المستخدم ، لم يتم الخصم من الحساب. ");
                        }

                        else if (TerminalStatusCode == "12")
                        {
                            comments.AppendFormat("12 :  لم تكتمل بسبب فشل الاتصال أو لسبب آخر ، لم يتم الخصم من الحساب.");
                        }

                        else if (TerminalStatusCode == "13")
                        {
                            comments.AppendFormat("13 :  البطاقة غير مدعومة");
                        }
                        else if (TerminalStatusCode == "14")
                        {
                            comments.AppendFormat("14 :  العملية غير مسموح بها ");
                        }
                        else if (TerminalStatusCode == "15")
                        {
                            comments.AppendFormat("15 :  بطاقة منتهية الصلاحية");
                        }
                        else if (TerminalStatusCode == "16")
                        {
                            comments.AppendFormat("16 :  لا يوجد اتصال خط الهاتف غير متصل");
                        }
                        else if (TerminalStatusCode == "17")
                        {
                            comments.AppendFormat("17 :  البطاقة غير مقبولة");
                        }
                        else if (TerminalStatusCode == "86")
                        {
                            comments.AppendFormat("86 :  بطاقة رد أموال غير صحيحة");

                        }
                        else if (TerminalStatusCode == "87")
                        {
                            comments.AppendFormat("87 :  تم تأمين رقم التعريف الشخصي ");

                        }


                        else if (TerminalStatusCode == "88")
                        {
                            comments.AppendFormat("88 :إنتهى الوقت لإدخال الرقم السرى");
                        }

                        else if (TerminalStatusCode == "89")
                        {
                            comments.AppendFormat("89 :  الخدمة غير مقبولة. ");
                        }

                        else if (TerminalStatusCode == "90")
                        {
                            comments.AppendFormat("90 :  تمت إزالة البطاقة بعد إدخال الرقم السرى");
                        }

                        else if (TerminalStatusCode == "91")
                        {
                            comments.AppendFormat("91 :  تمت إزالة البطاقة قبل إدخال الرقم السرى");
                        }
                        else if (TerminalStatusCode == "92")
                        {
                            comments.AppendFormat("92 :  تمت إزالة البطاقة بعد إرسال الطلب (فشل التحقق من ARQC). ");
                        }
                        else if (TerminalStatusCode == "93")
                        {
                            comments.AppendFormat("93 :  Card Timeout. ");
                        }
                        else if (TerminalStatusCode == "94")
                        {
                            comments.AppendFormat("16 :  الرقم السرى غير صحيح ");
                        }
                        else if (TerminalStatusCode == "95")
                        {
                            comments.AppendFormat("95 :  المبلغ غير صحيح ");
                        }
                        else if (TerminalStatusCode == "96")
                        {
                            comments.AppendFormat("96 :  بطاقة غير صالحة (بطاقة محظورة)");

                        }
                        else if (TerminalStatusCode == "97")
                        {
                            comments.AppendFormat("97 :  تم حظر تطبيق EMV ");

                        }

                        else if (TerminalStatusCode == "98")
                        {
                            comments.AppendFormat("98 :  تم رفض المعاملة بواسطة البطاقة ");

                        }
                        else if (TerminalStatusCode == "99")
                        {
                            comments.AppendFormat("99 :  لم يتم اكتشاف بطاقة EMV بواسطة نقطة البيع (لم يتم إدخال البطاقة في نقطة البيع) ");

                        }




                        else
                        {
                            //MessageBox.Show("Faild Transaction العملية مرفوضة ");
                            comments.AppendFormat("Faild Transaction العملية مرفوضة ");

                        }

                        comments.AppendLine();
                        using (LSRetailPosis.POSProcesses.frmMessage dialog = new LSRetailPosis.POSProcesses.frmMessage(comments.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Stop))
                        {
                            LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog);
                        }
                    }



                    //// log vars  //
                    string _transactionId = ((LSRetailPosis.Transaction.RetailTransaction)posTransaction).TransactionId;
                    string _note = comments.ToString();
                    string _inputstr = "Port No : 250 , Amount : Net Amount With Tax(" + Amount.ToString("G29") + ")";
                    string _resultXML = StrXmlResult.ToString();
                    string _connStatus = DispResult(iRet2);



                    // close Transaction //ss
                    if (TerminalStatusCode == "00")
                    {
                        //((LSRetailPosis.Transaction.RetailTransaction)posTransaction).
                        SaleLineItem aItem1 = myTransaction1.SaleItems.First();//!!!!QitafRewardUpdate
                        bool bSuccess = processQitafRewardUpdateIfAvailable(aItem1.ReturnTransId, aItem1.NetAmountWithTax);


                        var application = PosApplication.Instance as IApplication;

                        application.RunOperation(PosisOperations.PayCashQuick, PaymentId, posTransaction); //1- is ID payment method, transaction - your transaction object
                                                                                                           // sendLinkAfterPayment(posTransaction.ReceiptId); ;// sendLinkAfterPayment(posTransaction.ReceiptId); 
                        m_strRewardAmount = ((int)((LSRetailPosis.Transaction.RetailTransaction)posTransaction).NetAmountWithTax).ToString();
                        process_Qitaf_RewardPoint(posTransaction.TransactionId);
                    }
                    //UpdateLog  
                    InsertTransLog(_transactionId, _resultXML, Amount, TerminalStatusCode, PAN, ApprovalCode, CardSchemeEnglish, CardExpiryDate, CardholderVerification);
                    genShelf(posTransaction.TransactionId, posTransaction.StoreId, false);
                    operationInfo.OperationHandled = true;
                    break;


                default:
                    StringBuilder comment = new StringBuilder(128);
                    comment.AppendFormat(ApplicationLocalizer.Language.Translate(50700), operationInfo.OperationId);
                    comment.AppendLine();
                    comment.AppendFormat(ApplicationLocalizer.Language.Translate(50701), operationInfo.Parameter);
                    comment.AppendLine();
                    comment.Append(ApplicationLocalizer.Language.Translate(50702));

                    using (LSRetailPosis.POSProcesses.frmMessage dialog = new LSRetailPosis.POSProcesses.frmMessage(comment.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error))
                    {
                        LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog);
                    }
                    break;
            }
            // Set this property to true when your operation is handled
            operationInfo.OperationHandled = true;

            // Other examples:

            // Add an item to the transaction
            // Application.RunOperation(PosisOperations.ItemSale, "<ItemId>");

            // Logoff
            // Application.RunOperation(PosisOperations.LogOff, null);
        }


        #endregion
        public string DispResult(int iRet)
        {
            string TxtStatus = "";
            switch (iRet)
            {
                case (int)SpanapiWrapper.ReturnCode.API_FAILED:
                    TxtStatus = "4 API_FAILED";
                    break;
                case (int)SpanapiWrapper.ReturnCode.API_LIBRARY_FAILED:
                    TxtStatus = "1 API_LIBRARY_FAILED";
                    break;
                case (int)SpanapiWrapper.ReturnCode.API_NO_RESPONSE:
                    TxtStatus = "2 API_NO_RESPONSE";
                    break;
                case (int)SpanapiWrapper.ReturnCode.API_PORT_OPEN_FAILED:
                    TxtStatus = "3 API_PORT_OPEN_FAILED";
                    break;
                case (int)SpanapiWrapper.ReturnCode.API_TIMEOUT:
                    TxtStatus = "5 API_TIMEOUT";
                    break;
                case (int)SpanapiWrapper.ReturnCode.API_OK:
                    TxtStatus = "0 API_OK";
                    break;
            }
            return TxtStatus;
        }

        public void InsertTransLog(string _transactionId, string _resultXML, decimal _amount, string _RRN, string _PAN, string _ApprovalCode,
                                   string _CardSchemeEnglish, string _CardExpiryDate, string _CardholderVerification)
        {

            System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection();

            //sqlConn.ConnectionString = "Data Source=DESKTOP-U5RH05T;Initial Catalog=test;Persist Security Info=True;Integrated Security=true;";
            sqlConn.ConnectionString = LSRetailPosis.Settings.ApplicationSettings.Database.LocalConnectionString;
            string query = "INSERT INTO ax.RetailPaymentTerminalLog (TransactionId, CARDLOG,Amount, RRN , PAN,ApprovalCode, CardSchemeEnglish, CardExpiryDate, CardholderVerification,TRANSDATATIME)";
            query += " VALUES (@transactionId, @CARDLOG,@Amount, @RRN, @PAN, @ApprovalCode, @CardSchemeEnglish, @CardExpiryDate , @CardholderVerification,@TRANSDATATIME)";

            SqlDataAdapter da = new SqlDataAdapter();
            da.InsertCommand = new SqlCommand(query, sqlConn);



            da.InsertCommand.Parameters.Add("@transactionId", SqlDbType.VarChar).Value = _transactionId;
            da.InsertCommand.Parameters.Add("@CARDLOG", SqlDbType.VarChar).Value = _resultXML;
            da.InsertCommand.Parameters.Add("@Amount", SqlDbType.Decimal).Value = _amount;
            da.InsertCommand.Parameters.Add("@PAN", SqlDbType.VarChar).Value = _PAN;
            da.InsertCommand.Parameters.Add("@RRN", SqlDbType.VarChar).Value = _RRN;
            da.InsertCommand.Parameters.Add("@ApprovalCode", SqlDbType.VarChar).Value = _ApprovalCode;
            da.InsertCommand.Parameters.Add("@CardSchemeEnglish", SqlDbType.VarChar).Value = _CardSchemeEnglish;
            da.InsertCommand.Parameters.Add("@CardholderVerification", SqlDbType.VarChar).Value = _CardholderVerification;
            da.InsertCommand.Parameters.Add("@CardExpiryDate", SqlDbType.VarChar).Value = _CardExpiryDate;
            da.InsertCommand.Parameters.Add("@TRANSDATATIME", SqlDbType.DateTime).Value = DateTime.Now;


            sqlConn.Open();
            da.InsertCommand.ExecuteNonQuery();
            sqlConn.Close();

        }

        public void Insert_workerDiscountTrans(string strTransId, string strWorkerid, string strPN, string strDiscountAmount, string strStoreId,
                   string strReceiptNum)
        {
            System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection();
            sqlConn.ConnectionString = LSRetailPosis.Settings.ApplicationSettings.Database.LocalConnectionString;
            string query = "INSERT INTO ax.Retailworkerdiscounttrans (TransactionId, workerid, phonenumber, discountamount, storeid, receiptnumber, TRANSDATATIME,discpercentage)";
            query += " VALUES (@transactionId, @workerid,@phonenumber, @discountamount, @storeid, @receiptnumber, @TRANSDATATIME,@discpercentage)";

            SqlDataAdapter da = new SqlDataAdapter();
            da.InsertCommand = new SqlCommand(query, sqlConn);



            da.InsertCommand.Parameters.Add("@transactionId", SqlDbType.VarChar).Value = strTransId;
            da.InsertCommand.Parameters.Add("@workerid", SqlDbType.VarChar).Value = strWorkerid;
            da.InsertCommand.Parameters.Add("@phonenumber", SqlDbType.Decimal).Value = strPN;
            da.InsertCommand.Parameters.Add("@discountamount", SqlDbType.VarChar).Value = strDiscountAmount;
            da.InsertCommand.Parameters.Add("@storeid", SqlDbType.VarChar).Value = strStoreId;
            da.InsertCommand.Parameters.Add("@receiptnumber", SqlDbType.VarChar).Value = strReceiptNum;
            da.InsertCommand.Parameters.Add("@TRANSDATATIME", SqlDbType.DateTime).Value = DateTime.Now;
            da.InsertCommand.Parameters.Add("@discpercentage", SqlDbType.VarChar).Value = fDiscountPercentage.ToString();


            sqlConn.Open();
            da.InsertCommand.ExecuteNonQuery();
            sqlConn.Close();

        }

        public Boolean processQitafRewardUpdateIfAvailable(string strTranId, decimal price4Process)
        {
            if (price4Process >= 0)
            { // supported for only refund process. - valeues only.
                return false;
            }
            m_strTansactionID4POS = strTranId;
            Boolean bRet = false;
            fDiscountLimitPerYear = -1;
            System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection();
            connection.ConnectionString = LSRetailPosis.Settings.ApplicationSettings.Database.LocalConnectionString;
            try
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "SELECT TRANSDATATIME,transactionid,phonenumber,Amount,StoreID,transactionGUIDRedeem" +
                        ",transactionGUIDReward,Reserved,RequestDate FROM ax.RetailQitaftrans where transactionid=@TranId";
                    command.Parameters.Add("@TranId", SqlDbType.NVarChar).Value = strTranId;

                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }

                    using (System.Data.SqlClient.SqlDataAdapter adapter = new System.Data.SqlClient.SqlDataAdapter(command))
                    {
                        using (System.Data.DataTable table = new System.Data.DataTable())
                        {
                            adapter.Fill(table);

                            if (table.Rows.Count > 0)
                            {
                                if (table.Rows[0]["transactionGUIDRedeem"] != DBNull.Value)
                                {
                                    m_strTransactionGUIDRedeem = table.Rows[0]["transactionGUIDRedeem"].ToString();
                                }
                                if (table.Rows[0]["phonenumber"] != DBNull.Value)
                                {
                                    m_strPN = table.Rows[0]["phonenumber"].ToString();
                                    m_strPN4Qitaf = m_strPN;
                                }
                                if (table.Rows[0]["RequestDate"] != DBNull.Value)
                                {
                                    m_strRefRequestDate = table.Rows[0]["RequestDate"].ToString();
                                }
                                if (table.Rows[0]["transactionGUIDReward"] != DBNull.Value)
                                {
                                    m_strRefRequestId = table.Rows[0]["transactionGUIDReward"].ToString();
                                }
                                if (table.Rows[0]["Amount"] != DBNull.Value)
                                {
                                    m_strAmount = (table.Rows[0]["Amount"].ToString());
                                }
                                if (table.Rows[0]["Reserved"] != DBNull.Value)
                                {
                                    m_strResidueAmount4RewardUdt = (table.Rows[0]["Reserved"].ToString());
                                }
                                else
                                {
                                    m_strResidueAmount4RewardUdt = (table.Rows[0]["Amount"].ToString());
                                }

                                bRet = true;
                            }
                        }
                    }
                }
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

            if (decimal.Parse(m_strResidueAmount4RewardUdt) <= 0)
            {
                return false;
            }
            if (bRet == false)
            {
                return false;
            }
            decimal reductionAmount = price4Process * -1;
            reductionAmount = decimal.Parse(m_strResidueAmount4RewardUdt) + price4Process;
            m_strReductionAmount = ((int)reductionAmount).ToString();
            int nResCode5 = call_Qitaf_5_RewardUpdate();
            if (nResCode5 == 0)
            {
                MessageBox.Show("Qitaf Reward Update has been completed successfully.");
                return true;
            }

            return false;
        }

        public Boolean prepareFields4ReverseQitaf(string strTranId)
        {
            Boolean bRet = false;
            fDiscountLimitPerYear = -1;
            System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection();
            connection.ConnectionString = LSRetailPosis.Settings.ApplicationSettings.Database.LocalConnectionString;
            try
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "SELECT TRANSDATATIME,transactionid,phonenumber,Amount,StoreID,transactionGUIDRedeem" +
                        ",transactionGUIDReward,Reserved,RequestDate FROM ax.RetailQitaftrans where transactionid=@TranId";
                    command.Parameters.Add("@TranId", SqlDbType.NVarChar).Value = strTranId;

                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }

                    using (System.Data.SqlClient.SqlDataAdapter adapter = new System.Data.SqlClient.SqlDataAdapter(command))
                    {
                        using (System.Data.DataTable table = new System.Data.DataTable())
                        {
                            adapter.Fill(table);

                            if (table.Rows.Count > 0)
                            {
                                if (table.Rows[0]["transactionGUIDRedeem"] != DBNull.Value)
                                {
                                    m_strTransactionGUIDRedeem = table.Rows[0]["transactionGUIDRedeem"].ToString();
                                }
                                if (table.Rows[0]["phonenumber"] != DBNull.Value)
                                {
                                    m_strPN = table.Rows[0]["phonenumber"].ToString();
                                }
                                if (table.Rows[0]["RequestDate"] != DBNull.Value)
                                {
                                    m_strRefRequestDate = table.Rows[0]["RequestDate"].ToString();
                                }
                                if (table.Rows[0]["transactionGUIDReward"] != DBNull.Value)
                                {
                                    m_strRefRequestId = table.Rows[0]["transactionGUIDReward"].ToString();
                                }
                                if (table.Rows[0]["Amount"] != DBNull.Value)
                                {
                                    m_strAmount = (table.Rows[0]["Amount"].ToString());
                                }
                                bRet = true;
                            }
                        }
                    }
                }
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

            return bRet;
        }

        public Boolean IsRegisteredWorkerid(string strWorkerId)
        {
            Boolean bRet = false;
            fDiscountLimitPerYear = -1;
            System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection();
            connection.ConnectionString = LSRetailPosis.Settings.ApplicationSettings.Database.LocalConnectionString;
            try
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "SELECT phonenumber,AmountPeryear,discpercentage FROM     ax.Retailworkerdiscountparameter where workerid=@WORKERID AND " +
                        "YEAR(startdate) = YEAR(GETDATE())";// TRANSDATATIME -> startdate /
                    // "SELECT * FROM dbo.RETAILFORMLAYOUT WHERE FORMLAYOUTID = @FORMID";
                    command.Parameters.Add("@WORKERID", SqlDbType.NVarChar).Value = strWorkerId;

                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }

                    using (System.Data.SqlClient.SqlDataAdapter adapter = new System.Data.SqlClient.SqlDataAdapter(command))
                    {
                        using (System.Data.DataTable table = new System.Data.DataTable())
                        {
                            adapter.Fill(table);

                            if (table.Rows.Count > 0)
                            {
                                if (table.Rows[0]["phonenumber"] != DBNull.Value)
                                {
                                    strCardNumber = table.Rows[0]["phonenumber"].ToString();
                                }
                                if (table.Rows[0]["AmountPeryear"] != DBNull.Value)
                                {
                                    string strAmountPerYear = table.Rows[0]["AmountPeryear"].ToString();
                                    fDiscountLimitPerYear = Convert.ToDouble(strAmountPerYear);
                                }
                                if (table.Rows[0]["discpercentage"] != DBNull.Value)
                                {
                                    fDiscountPercentage = Convert.ToDouble(table.Rows[0]["discpercentage"].ToString());
                                }
                                bRet = true;
                            }
                        }
                    }
                }
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

            return bRet;
        }

        public Boolean IsRegisteredWorkerAndOwner(string strWorkerId)
        {
            Boolean bRet = false;
            fDiscountLimitPerYear = -1;
            System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection();
            connection.ConnectionString = LSRetailPosis.Settings.ApplicationSettings.Database.LocalConnectionString;
            try
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "SELECT phonenumber,AmountPeryear,discpercentage, discount30, discount34,discount8,discount9,elsediscount FROM     ax.Retailworkerdiscountparameter where workerid=@WORKERID AND isowner=1 AND " +
                        "YEAR(startdate) = YEAR(GETDATE())";// TRANSDATATIME -> startdate /
                    // "SELECT * FROM dbo.RETAILFORMLAYOUT WHERE FORMLAYOUTID = @FORMID";
                    command.Parameters.Add("@WORKERID", SqlDbType.NVarChar).Value = strWorkerId;

                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }

                    using (System.Data.SqlClient.SqlDataAdapter adapter = new System.Data.SqlClient.SqlDataAdapter(command))
                    {
                        using (System.Data.DataTable table = new System.Data.DataTable())
                        {
                            adapter.Fill(table);

                            if (table.Rows.Count > 0)
                            {
                                if (table.Rows[0]["phonenumber"] != DBNull.Value)
                                {
                                    strCardNumber = table.Rows[0]["phonenumber"].ToString();
                                }
                                if (table.Rows[0]["AmountPeryear"] != DBNull.Value)
                                {
                                    string strAmountPerYear = table.Rows[0]["AmountPeryear"].ToString();
                                    fDiscountLimitPerYear = Convert.ToDouble(strAmountPerYear);
                                }
                                if (table.Rows[0]["discpercentage"] != DBNull.Value)
                                {
                                    fDiscountPercentage = Convert.ToDouble(table.Rows[0]["discpercentage"].ToString());
                                }
                                if (table.Rows[0]["discount30"] != DBNull.Value)
                                {
                                    ndiscount30 = Convert.ToInt32(table.Rows[0]["discount30"].ToString());
                                }
                                if (table.Rows[0]["discount34"] != DBNull.Value)
                                {
                                    ndiscount34 = Convert.ToInt32(table.Rows[0]["discount34"].ToString());
                                }
                                if (table.Rows[0]["discount8"] != DBNull.Value)
                                {
                                    ndiscount8 = Convert.ToInt32(table.Rows[0]["discount8"].ToString());
                                }
                                if (table.Rows[0]["discount9"] != DBNull.Value)
                                {
                                    ndiscount9 = Convert.ToInt32(table.Rows[0]["discount9"].ToString());
                                }
                                if (table.Rows[0]["elsediscount"] != DBNull.Value)
                                {
                                    nelsediscount = Convert.ToInt32(table.Rows[0]["elsediscount"].ToString());
                                }
                                bRet = true;
                            }
                        }
                    }
                }
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

            return bRet;
        }

        public double getSumDiscount4Year(string strWorkerId)
        {
            fSumDiscount4Year = 0;
            System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection();
            connection.ConnectionString = LSRetailPosis.Settings.ApplicationSettings.Database.LocalConnectionString;
            try
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "SELECT SUM(discountamount) AS Expr1 FROM ax.Retailworkerdiscounttrans" +
                        " WHERE(workerid = @WORKERID) AND (YEAR(TRANSDATATIME) = YEAR(GETDATE()))";

                    //"SELECT workerid FROM ax.Retailworkerdiscounttrans " +
                    //"WHERE  workerid = @WORKERID AND " +
                    //"((SELECT Amountperyear FROM ax.Retailworkerdiscountparameter AS Retailworkerdiscountparameter_1 " +
                    //"WHERE(workerid = @WORKERID) AND(YEAR(TRANSDATATIME) = YEAR(GETDATE()))) - " +
                    //"(SELECT SUM(discountamount) AS Expr1 " +
                    //"FROM ax.Retailworkerdiscounttrans WHERE(workerid = @WORKERID) AND(YEAR(TRANSDATATIME) = YEAR(GETDATE()))) > @ORDERPRICE)";
                    // "SELECT * FROM dbo.RETAILFORMLAYOUT WHERE FORMLAYOUTID = @FORMID";
                    command.Parameters.Add("@WORKERID", SqlDbType.NVarChar).Value = strWorkerId;
                    // command.Parameters.Add("@ORDERPRICE", SqlDbType.NVarChar).Value = strAmount;

                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }

                    using (System.Data.SqlClient.SqlDataAdapter adapter = new System.Data.SqlClient.SqlDataAdapter(command))
                    {
                        using (System.Data.DataTable table = new System.Data.DataTable())
                        {
                            adapter.Fill(table);

                            if (table.Rows.Count > 0)
                            {
                                if (table.Rows[0]["Expr1"] != DBNull.Value)
                                {
                                    string strSum = table.Rows[0]["Expr1"].ToString();
                                    fSumDiscount4Year = Convert.ToDouble(strSum);
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

            return fSumDiscount4Year;
        }

        public void sendLinkAfterPayment1(string strReceiptId)
        {
            if (strReceiptId == null)
            {
                return;
            }
            if (strReceiptId.Length < 2)
            {
                return;
            }
            // generate Link
            string strLink = "";
            {
                string strEncInvoice;// = "01000000C9061D89A44DABE14430785BCCFA7BEA4D9109A4F7485C31B3FD1FDB2802F894";
                var passphrase = "key";
                //var encrypted = SQLServerCryptoMethod.EncryptByPassPhrase(@passphrase, strReceiptId);
                var encrypted = SQLServerCryptoMethod.ToBase36(ulong.Parse(strReceiptId));
                System.Console.WriteLine(encrypted.ToString().ToUpper());
                strEncInvoice = encrypted.ToString();// encrypted.ToString().ToUpper().Substring(2);
                saveEncInvoice2TableByTranId(strEncInvoice, strReceiptId);
                Console.WriteLine(strEncInvoice);
                strLink = "http://einvoice.khaltaa.com/ReportServer?%2Fzakat&receipt2=" +
                    strEncInvoice + "&rs:Command=Render&rc:Toolbar=false&rs:Format=PDF";


            }


            // input Phone number
            using (Microsoft.Dynamics.Retail.Pos.BlankOperations.Dialog.frmPNCode dialog1 = new Microsoft.Dynamics.Retail.Pos.BlankOperations.Dialog.frmPNCode())
            {
                LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog1);
                code = dialog1.Code;

                if (code.ToString().Length > 7)
                {
                    // if he enter, send SMS
                    SendLink2SMS(dialog1.strPN.ToString(), strLink);
                }
                else
                {
                    // if he skip enter, no SMS
                    // don't send link
                }

            }

            // after payment done after this point 

            // show enter customer number
            //dialog1.PromptText = "show enter customer number ";

            //LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog1);

            // if he enter 
            // send message 
            // dear customer you can found the receipt on 

            // if he skip enter, no SMS


            //http://einvoice.khaltaa.com/ReportServer?%2Fzakat&receipt2=" + strEncInvoice + "&rs:Command=Render&rc:Toolbar=false&rs:Format=PDF" + ">";
            // the code work after all payment cases
        }

        public void SendLink2SMS(string CardNumber, string strLink)
        {
            WrapperController wrapper = new WrapperController();
            string appsid = "fTXbaUqGeUWz8py67d5gqtpkQuRl6F";
            string msg = "عزيزي العميل يمكنك الحصول على الفاتورة من خلال الرابط:" + strLink;
            //long to = 966540803793L; // Maged number
            long to = long.Parse(CardNumber);
            string sender = "REEFSTAR";
            bool? baseEncode = false;
            string encoding = "GSM7";

            try
            {

                HttpStringResponse result = wrapper.CreateSendMessageAsyncICore(appsid, msg, to, sender, baseEncode, encoding).Result;

            }
            catch (APIException e) { };



        }


        public Boolean SendSMS(string CardNumber, decimal Amount, int limit = 1)
        {
            Boolean ret = false;
            if (limit == 1)
            {

                Random rnd = new Random();
                num = rnd.Next(1001, 9999);
                code = 0;
                WrapperController wrapper = new WrapperController();
                string appsid = "fTXbaUqGeUWz8py67d5gqtpkQuRl6F";
                string msg = "شكرا لكونك أحد شركاء النجاح رمز التحقق هو:" + num.ToString();
                //long to = 966540803793L; // Maged number
                long to = long.Parse(CardNumber);
                string sender = "REEFSTAR";
                bool? baseEncode = false;
                string encoding = "GSM7";

                try
                {

                    HttpStringResponse result = wrapper.CreateSendMessageAsyncICore(appsid, msg, to, sender, baseEncode, encoding).Result;

                }
                catch (APIException e) { };
            }


            using (Microsoft.Dynamics.Retail.Pos.BlankOperations.Dialog.frmSMSCode dialog1 = new Microsoft.Dynamics.Retail.Pos.BlankOperations.Dialog.frmSMSCode())
            {

                LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog1);
                code = dialog1.Code;

                if (num == code)
                {
                    ret = true;
                }
                else if (code != 0)
                {
                    if (limit < 3)
                    {
                        limit++;
                        MessageBox.Show("Invalid Code رمز التأكيد غير صحيح ");
                        ret = this.SendSMS(CardNumber, Amount, limit);
                    }
                    else if (limit == 3)
                    {
                        MessageBox.Show("Invalid Code رمز التأكيد غير صحيح ");
                        ret = false;
                    }

                }
                else
                {
                    ret = false;
                }

            }

            return ret;

        }

        public void callapi_ReverseTrans()
        {
            m_strErrCode = " ";
            string url = "https://dpw.alrajhibank.com.sa:9443/api-factory/prod/blu-loyalty/1.0.0/redemption-transaction-reversal";
            // old     "https://gwt.alrajhibank.com.sa:9443/api-factory/sit/blu-loyalty/1.0.0/redemption-transaction-reversal";
            try
            {
                HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
                webrequest.Method = "POST";

                webrequest.ContentType = "application/json";
                webrequest.Headers["merchantToken"] = m_strTokenType + " Reef@2022";//webrequest.Headers.Add("merchantToken", "Bearer Khalta@2021");
                webrequest.Headers["Authorization"] = m_strTokenType + " " + m_strAccessToken; //webrequest.Headers.Add("Authorization", "Bearer " + m_strAccessToken);
                string postData = "{\"transactionID\":" + Uri.EscapeDataString(m_strTransactionID) + "}";



                var data = Encoding.Default.GetBytes(postData);
                webrequest.ContentLength = data.Length;
                var newStream = webrequest.GetRequestStream();
                newStream.Write(data, 0, data.Length);
                newStream.Close();

                //webrequest.Headers.Add("merchantToken", "Bearer Khalta@2021");
                HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();
                Encoding enc = System.Text.Encoding.GetEncoding("utf-8");
                StreamReader responseStream = new StreamReader(webresponse.GetResponseStream(), enc);
                string result = string.Empty;
                result = responseStream.ReadToEnd();
                webresponse.Close();
                Console.WriteLine(result);
                JObject jObject = JObject.Parse(result);
                dynamic dataRet = jObject;
                Console.WriteLine(dataRet.message);//=="Success"
                Console.WriteLine(dataRet.requestID);//==m_strRequestID
                m_bSuccessOnReverse = false;
                if (dataRet.message == "Success")
                {
                    m_bSuccessOnReverse = true;
                }
            }
            catch (WebException wex)
            {
                m_bSuccessOnReverse = false;
                m_strRetMessage = wex.Message.ToString();
                Console.Write(wex.ToString());
                using (WebResponse response = wex.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        Console.WriteLine(text);
                        JObject jObject = JObject.Parse(text);
                        dynamic dataRet = jObject;
                        m_strRetMessage = dataRet.message + " ErrorCode:" + dataRet.errorCode;
                        m_strErrCode = dataRet.errorCode;
                        showMessageForm_Alrahji(" ", int.Parse(m_strErrCode));
                    }
                }
            }

        }

        public void callapi_RedeemCustomerAmount()
        {
            m_strErrCode = " ";
            string url = "https://dpw.alrajhibank.com.sa:9443/api-factory/prod/blu-loyalty/1.0.0/otp-validation";
            // old  "https://gwt.alrajhibank.com.sa:9443/api-factory/sit/blu-loyalty/1.0.0/otp-validation";
            try
            {
                HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
                webrequest.Method = "POST";

                webrequest.ContentType = "application/json";
                webrequest.Headers["merchantToken"] = m_strTokenType + " Reef@2022";//webrequest.Headers.Add("merchantToken", "Bearer Khalta@2021");
                webrequest.Headers["Authorization"] = m_strTokenType + " " + m_strAccessToken; //webrequest.Headers.Add("Authorization", "Bearer " + m_strAccessToken);
                var data = @"{
                    ""OTPValue"": """ + m_strOTPValue + @""",
                    ""OTPToken"": """ + m_strOTPToken + @""",
                    ""amount"": """ + m_strAmount + @""",
                    ""language"": ""ar""
                }";

                using (var streamWriter = new StreamWriter(webrequest.GetRequestStream()))
                {
                    streamWriter.Write(data);
                }

                //webrequest.Headers.Add("merchantToken", "Bearer Khalta@2021");
                HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();
                Encoding enc = System.Text.Encoding.GetEncoding("utf-8");
                StreamReader responseStream = new StreamReader(webresponse.GetResponseStream(), enc);
                string result = string.Empty;
                result = responseStream.ReadToEnd();
                webresponse.Close();
                Console.WriteLine(result);
                JObject jObject = JObject.Parse(result);
                dynamic dataRet = jObject;
                m_strRequestID = dataRet.requestID;
                m_strTransactionID = dataRet.transactionID;
                m_strTransactionDate = dataRet.transactionDate;
                m_strTransactionType = dataRet.transactionType;
                m_strStore4Alrajhi = dataRet.merchant;
                m_strRetMessage = dataRet.message;
                if (m_strRetMessage == "Success")
                {
                    m_bSuccessOnRedeem = true;
                    Insert_AlrahjiBankTrans();
                    MessageBox.Show("Redeem Blu Customer has been processed successfully.");
                }
                else
                {
                    m_bSuccessOnRedeem = false;
                }
            }
            catch (WebException wex)
            {
                m_bSuccessOnRedeem = false;
                m_strRetMessage = wex.Message.ToString();
                Console.Write(wex.ToString());
                using (WebResponse response = wex.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        Console.WriteLine(text);
                        JObject jObject = JObject.Parse(text);
                        dynamic dataRet = jObject;
                        m_strRetMessage = dataRet.message + " ErrorCode:" + dataRet.errorCode;
                        m_strErrCode = dataRet.errorCode;
                        showMessageForm_Alrahji(" ", int.Parse(m_strErrCode));
                    }
                }
            }
        }

        public void on_RedeemCustomerAmount()
        {
            using (Microsoft.Dynamics.Retail.Pos.BlankOperations.Dialog.frmOTPCode dialogOTP = new Microsoft.Dynamics.Retail.Pos.BlankOperations.Dialog.frmOTPCode())
            {
                LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialogOTP);
                m_strOTPValue = dialogOTP.strOTPCode;
                if (m_strOTPValue.Length == 4)
                {
                    using (Microsoft.Dynamics.Retail.Pos.BlankOperations.Dialog.frmAmountCode dialogAmount = new Microsoft.Dynamics.Retail.Pos.BlankOperations.Dialog.frmAmountCode())
                    {
                        LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialogAmount);
                        m_strAmount = dialogAmount.Code.ToString();
                        if (m_strAmount.ToString().Length < 1)
                        {
                            MessageBox.Show("Invalid input.");
                            return;
                        }
                    }
                    callapi_RedeemCustomerAmount();
                    if (m_bSuccessOnRedeem == false)
                    {
                        //MessageBox.Show("Redeem Blu Customer has been failed." + m_strRetMessage);
                        if (m_strErrCode == "301")
                        {
                            on_RedeemCustomerAmount();
                            return;
                        }
                        return;
                    }
                }
                else
                {
                    //MessageBox.Show("Invalid OTP code:" + m_strRetMessage);
                    if (m_strErrCode == "301")
                    {
                        on_RedeemCustomerAmount();
                        return;
                    }
                    return;
                }
            }
        }


        public void callapi_authCustomer()
        {
            m_strErrCode = " ";
            string url = "https://dpw.alrajhibank.com.sa:9443/api-factory/prod/blu-loyalty/1.0.0/customer-authorization";
            // old "https://gwt.alrajhibank.com.sa:9443/api-factory/sit/blu-loyalty/1.0.0/customer-authorization";
            try
            {
                HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
                webrequest.Method = "POST";
                webrequest.ContentType = "application/json";
                webrequest.Headers["merchantToken"] = m_strTokenType + " Reef@2022 ";//webrequest.Headers.Add("merchantToken", "Bearer Khalta@2021");
                webrequest.Headers["Authorization"] = m_strTokenType + " " + m_strAccessToken; //webrequest.Headers.Add("Authorization", "Bearer " + m_strAccessToken);
                var data = @"{
                    ""mobile"": """ + m_strPN + @""",
                    ""amount"": """",
                    ""currency"": ""SAR"",
                    ""lang"": ""ar"",
                    ""mappingPosID"": ""3""
                }";
                using (var streamWriter = new StreamWriter(webrequest.GetRequestStream()))
                {
                    streamWriter.Write(data);
                }

                HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();
                Encoding enc = System.Text.Encoding.GetEncoding("utf-8");
                StreamReader responseStream = new StreamReader(webresponse.GetResponseStream(), enc);
                string result = string.Empty;
                result = responseStream.ReadToEnd();
                webresponse.Close();
                Console.WriteLine(result);
                JObject jObject = JObject.Parse(result);
                dynamic dataRet = jObject;
                Console.WriteLine(dataRet.otp.otp_token);
                m_strOTPToken = dataRet.otp.otp_token;
                m_strRetMessage = dataRet.message;
                if (m_strRetMessage == "OK")
                {
                    m_bSuccessOnAuthCustomer = true;
                }
                else
                {
                    m_bSuccessOnAuthCustomer = false;
                }
            }
            catch (WebException wex)
            {
                m_bSuccessOnAuthCustomer = false;
                m_strRetMessage = wex.Message.ToString();
                Console.Write(wex.ToString());
                using (WebResponse response = wex.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        Console.WriteLine(text);
                        JObject jObject = JObject.Parse(text);
                        dynamic dataRet = jObject;
                        m_strRetMessage = dataRet.message + " ErrorCode:" + dataRet.errorCode;
                        m_strErrCode = dataRet.errorCode;
                        showMessageForm_Alrahji(" ", int.Parse(m_strErrCode));
                    }
                }
            }

        }

        public int on_QitafRedeemPoint()
        {
            using (Microsoft.Dynamics.Retail.Pos.BlankOperations.Dialog.frmOTPCode dialogOTP = new Microsoft.Dynamics.Retail.Pos.BlankOperations.Dialog.frmOTPCode())
            {
                LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialogOTP);
                m_strOTPValue = dialogOTP.strOTPCode;
                if (m_strOTPValue.Length == 4)
                {
                    using (Microsoft.Dynamics.Retail.Pos.BlankOperations.Dialog.frmAmountCode dialogAmount = new Microsoft.Dynamics.Retail.Pos.BlankOperations.Dialog.frmAmountCode())
                    {
                        LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialogAmount);
                        m_strAmount = dialogAmount.Code.ToString();
                        if (m_strAmount.ToString().Length < 1)
                        {
                            MessageBox.Show("Invalid input.");
                            return -100;
                        }
                    }
                    m_nResponseCode_QitafApi2 = -1;
                    int nResponseCode2 = call_Qitaf_2_RedeemPoint(m_strAmount, m_strOTPValue);
                    m_nResponseCode_QitafApi2 = nResponseCode2;
                    return nResponseCode2;
                }
                else
                {
                    MessageBox.Show("Invalid OTP code");
                    //on_QitafRedeemPoint();
                    return -10;
                }
            }
        }

        public int call_Qitaf_5_RewardUpdate()
        {
            Cursor.Current = Cursors.WaitCursor; AutoClosingMessageBox.Show("Please wait for Qitaf respone for few seconds.", "Processing for Qitaf", 2000);
            var _url = "http://78.93.37.230:9799/RedemptionLiteIntegrationServiceBasicHttpEndPoint"; // here ?? 
            var _action = "http://tempuri.org/IRedemptionLiteIntegrationService/RewardUpdate";
            try
            {
                XmlDocument soapEnvelopeXml = CreateSoapEnvelope5();
                HttpWebRequest webRequest = CreateWebRequest(_url, _action);
                InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);

                // begin async call to web request.
                IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);

                // suspend this thread until call is complete. You might want to
                // do something usefull here like update your UI.
                asyncResult.AsyncWaitHandle.WaitOne();

                // get the response from the completed web request.
                string soapResult;
                using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult))
                {
                    using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                    {
                        soapResult = rd.ReadToEnd();
                    }
                    Console.Write(soapResult);
                }

                Regex regex = new Regex("<a:ResponseCode>(.*?)</a:ResponseCode>");
                var regMatch = regex.Match(soapResult);
                string strResponseCode = regMatch.Groups[1].ToString();
                int nResponseCode = Int32.Parse(strResponseCode);
                if (nResponseCode == 0)
                {
                    Update_QitafTrans4RewardUpdate();
                }
                Cursor.Current = Cursors.Default;
                if (nResponseCode > 0)
                {
                    showMessageForm_Qitaf("Qitaf Reward Update has been failed.", nResponseCode);
                }
                return nResponseCode;
            }
            catch (Exception e)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show("Error on Qitaf Reward Update API request:" + e.ToString());
                return -1;
            }

        }

        public int call_Qitaf_4_RewardPoint()
        {
            Cursor.Current = Cursors.WaitCursor; AutoClosingMessageBox.Show("Please wait for Qitaf respone for few seconds.", "Processing for Qitaf", 2000);
            var _url = "http://78.93.37.230:9799/RedemptionLiteIntegrationServiceBasicHttpEndPoint"; // here ?? 
            var _action = "http://tempuri.org/IRedemptionLiteIntegrationService/RewardPoint";
            try
            {
                XmlDocument soapEnvelopeXml = CreateSoapEnvelope4();
                HttpWebRequest webRequest = CreateWebRequest(_url, _action);
                InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);

                // begin async call to web request.
                IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);

                // suspend this thread until call is complete. You might want to
                // do something usefull here like update your UI.
                asyncResult.AsyncWaitHandle.WaitOne();

                // get the response from the completed web request.
                string soapResult;
                using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult))
                {
                    using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                    {
                        soapResult = rd.ReadToEnd();
                    }
                    Console.Write(soapResult);
                }

                Regex regex = new Regex("<a:ResponseCode>(.*?)</a:ResponseCode>");
                var regMatch = regex.Match(soapResult);
                string strResponseCode = regMatch.Groups[1].ToString();
                int nResponseCode = Int32.Parse(strResponseCode);
                if (nResponseCode == 0)
                {
                    m_strPN = m_strPN4Qitaf;
                    Insert_QitafTrans();
                }
                Cursor.Current = Cursors.Default;
                if (nResponseCode > 0)
                {
                    showMessageForm_Qitaf("Qitaf Reward Point has been failed.", nResponseCode);
                }
                return nResponseCode;
            }
            catch (Exception e)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show("Error on Qitaf Reward Point API request:" + e.ToString());
                return -1;
            }

        }

        public int call_Qitaf_3_ReversePoint()
        {
            Cursor.Current = Cursors.WaitCursor; AutoClosingMessageBox.Show("Please wait for Qitaf respone for few seconds.", "Processing for Qitaf", 2000);
            string strGuid = Guid.NewGuid().ToString();
            var _url = "http://78.93.37.230:9799/RedemptionLiteIntegrationServiceBasicHttpEndPoint";
            var _action = "http://tempuri.org/IRedemptionLiteIntegrationService/ReverseQitafPointRedemption";
            try
            {
                XmlDocument soapEnvelopeXml = CreateSoapEnvelope3();
                HttpWebRequest webRequest = CreateWebRequest(_url, _action);
                InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);

                // begin async call to web request.
                IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);

                // suspend this thread until call is complete. You might want to
                // do something usefull here like update your UI.
                asyncResult.AsyncWaitHandle.WaitOne();

                // get the response from the completed web request.
                string soapResult;
                using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult))
                {
                    using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                    {
                        soapResult = rd.ReadToEnd();
                    }
                    Console.Write(soapResult);
                }

                Regex regex = new Regex("<a:ResponseCode>(.*?)</a:ResponseCode>");
                var regMatch = regex.Match(soapResult);
                string strResponseCode = regMatch.Groups[1].ToString();
                int nResponseCode = Int32.Parse(strResponseCode);
                if (nResponseCode == 0)
                {
                    Update_QitafTrans4Reverse();
                }
                Cursor.Current = Cursors.Default;
                if (nResponseCode > 0)
                {
                    showMessageForm_Qitaf("Qitaf Reverse Point has been failed.", nResponseCode);
                }
                return nResponseCode;
            }
            catch (Exception e)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show("Error on Qitaf Reverse Point API request:" + e.ToString());
                return -1;
            }

        }


        public int call_Qitaf_2_RedeemPoint(string strAmount, string strPin)
        {
            Cursor.Current = Cursors.WaitCursor; AutoClosingMessageBox.Show("Please wait for Qitaf respone for few seconds.", "Processing for Qitaf", 2000);
            m_strAmount = strAmount;
            var _url = "http://78.93.37.230:9799/RedemptionLiteIntegrationServiceBasicHttpEndPoint";
            var _action = "http://tempuri.org/IRedemptionLiteIntegrationService/RedeemQitafPoints";
            try
            {
                XmlDocument soapEnvelopeXml = CreateSoapEnvelope2(strAmount, strPin);
                HttpWebRequest webRequest = CreateWebRequest(_url, _action);
                InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);

                // begin async call to web request.
                IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);

                // suspend this thread until call is complete. You might want to
                // do something usefull here like update your UI.
                asyncResult.AsyncWaitHandle.WaitOne();

                // get the response from the completed web request.
                string soapResult;
                using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult))
                {
                    using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                    {
                        soapResult = rd.ReadToEnd();
                    }
                    Console.Write(soapResult);
                }

                Regex regex = new Regex("<a:ResponseCode>(.*?)</a:ResponseCode>");
                var regMatch = regex.Match(soapResult);
                string strResponseCode = regMatch.Groups[1].ToString();
                int nResponseCode = Int32.Parse(strResponseCode);
                if (nResponseCode == 0)
                {
                    Insert_QitafTrans();
                }
                Cursor.Current = Cursors.Default;
                if (nResponseCode > 0)
                {
                    showMessageForm_Qitaf("Qitaf Redeem Point has been failed.", nResponseCode);
                }
                return nResponseCode;
            }
            catch (Exception e)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show("Error on Qitaf Redeem Point API request:" + e.ToString());
                return -1;
            }


        }

        public int call_Qitaf_1_GenOTP()
        {
            Cursor.Current = Cursors.WaitCursor; AutoClosingMessageBox.Show("Please wait for Qitaf respone for few seconds.", "Processing for Qitaf", 4000);
            string strGuid = Guid.NewGuid().ToString();
            var _url = "http://78.93.37.230:9799/RedemptionLiteIntegrationServiceBasicHttpEndPoint";
            var _action = "http://tempuri.org/IRedemptionLiteIntegrationService/GenerateOTP";
            try
            {
                XmlDocument soapEnvelopeXml = CreateSoapEnvelope1();
                HttpWebRequest webRequest = CreateWebRequest(_url, _action);
                InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);

                // begin async call to web request.
                IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);

                // suspend this thread until call is complete. You might want to
                // do something usefull here like update your UI.
                asyncResult.AsyncWaitHandle.WaitOne();

                // get the response from the completed web request.
                string soapResult;
                using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult))
                {
                    using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                    {
                        soapResult = rd.ReadToEnd();
                    }
                    Console.Write(soapResult);
                }

                Regex regex = new Regex("<a:ResponseCode>(.*?)</a:ResponseCode>");
                var regMatch = regex.Match(soapResult);
                string strResponseCode = regMatch.Groups[1].ToString();
                int nResponseCode = Int32.Parse(strResponseCode);
                Cursor.Current = Cursors.Default;
                if (nResponseCode > 0)
                {
                    showMessageForm_Qitaf("", nResponseCode);
                    // showMessageForm_Qitaf("Qitaf Generate OTP has been failed.", nResponseCode);
                }
                return nResponseCode;
            }
            catch (Exception e)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show("Error on Qitaf Generate OTP API request:" + e.ToString());
                return -1;
            }

        }

        private static HttpWebRequest CreateWebRequest(string url, string action)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Headers.Add("SOAPAction", action);
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }

        private XmlDocument CreateSoapEnvelope5()
        {
            string strReqGuid = Guid.NewGuid().ToString();
            m_strTransactionGUIDReward = strReqGuid;
            m_strTransactionGUIDRedeem = " ";
            string strBranchId = "15930001";//11980000
            string strMSIDSN = m_strPN4Qitaf;
            //m_strPN = strMSIDSN;
            string strReqDate = DateTime.Now.ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToString("HH:mm:ss");
            string strTermId = strBranchId;
            string strLangCode = "en-US";

            XmlDocument soapEnvelopeDocument = new XmlDocument();
            string strXml =
                @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" 
                    xmlns:tem = ""http://tempuri.org/""
                    xmlns:red = ""http://schemas.datacontract.org/2004/07/Redemption.Lite.Integration.Service.Interface"">
                    <soapenv:Header/>
                    <soapenv:Body>
                      <tem:RewardUpdate>
                       <tem:request>
                        <red:BranchId>" + strBranchId + @"</red:BranchId>
                        <red:Language>" + strLangCode + @"</red:Language>
                        <red:MSISDN>" + strMSIDSN + @"</red:MSISDN>
                        <red:RequestDateTime>" + strReqDate + @"</red:RequestDateTime>
                        <red:RequestId>" + strReqGuid + @"</red:RequestId>
                        <red:TerminalId>" + strTermId + @"</red:TerminalId>
                        <red:ReductionAmount>" + m_strReductionAmount + @"</red:ReductionAmount>
                        <red:RefRequestDateTime>" + m_strRefRequestDate + @"</red:RefRequestDateTime>
                        <red:RefRequestId>" + m_strRefRequestId + @"</red:RefRequestId>
                       </tem:request>
                      </tem:RewardUpdate>
                    </soapenv:Body>
                    </soapenv:Envelope>";
            soapEnvelopeDocument.LoadXml(strXml);
            //                        < red:RefRequestDateTime > 2022 - 05 - 11T23: 49:59 </ red:RefRequestDateTime >
            //                        < red:RefRequestId > 95416d57 - 9937 - 47bd - a625 - 95adafec2735 </ red:RefRequestId >

            return soapEnvelopeDocument;
        }

        private XmlDocument CreateSoapEnvelope4()
        {
            string strReqGuid = Guid.NewGuid().ToString();
            m_strTransactionGUIDReward = strReqGuid;
            m_strTransactionGUIDRedeem = " ";
            string strBranchId = "15930001";//11980000
            string strMSIDSN = m_strPN4Qitaf;
            //m_strPN = strMSIDSN;
            string strReqDate = DateTime.Now.ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToString("HH:mm:ss");
            string strTermId = strBranchId;
            string strLangCode = "en-US";

            XmlDocument soapEnvelopeDocument = new XmlDocument();
            string strXml =
                @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" 
                    xmlns:tem = ""http://tempuri.org/""
                    xmlns:red = ""http://schemas.datacontract.org/2004/07/Redemption.Lite.Integration.Service.Interface"">
                    <soapenv:Header/>
                    <soapenv:Body>
                      <tem:RewardPoint>
                       <tem:request>
                        <red:BranchId>" + strBranchId + @"</red:BranchId>
                        <red:Language>" + strLangCode + @"</red:Language>
                        <red:MSISDN>" + strMSIDSN + @"</red:MSISDN>
                        <red:RequestDateTime>" + strReqDate + @"</red:RequestDateTime>
                        <red:RequestId>" + strReqGuid + @"</red:RequestId>
                        <red:TerminalId>" + strTermId + @"</red:TerminalId>
                        <red:Amount>" + m_strRewardAmount + @"</red:Amount>
                       </tem:request>
                      </tem:RewardPoint>
                    </soapenv:Body>
                    </soapenv:Envelope>";
            soapEnvelopeDocument.LoadXml(strXml);

            return soapEnvelopeDocument;
        }

        private XmlDocument CreateSoapEnvelope3()
        {
            string strReqGuid = Guid.NewGuid().ToString();
            string strBranchId = "15930001";
            string strTermId = strBranchId;
            string strMSIDSN = m_strPN;// @"506617981";
            //m_strRefRequestDate
            //m_strRefRequestId
            string strReqDate = DateTime.Now.ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToString("HH:mm:ss");
            string strLangCode = "en-US";

            XmlDocument soapEnvelopeDocument = new XmlDocument();
            string strXml =
                @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" 
                    xmlns:tem = ""http://tempuri.org/""
                    xmlns:red = ""http://schemas.datacontract.org/2004/07/Redemption.Lite.Integration.Service.Interface"">
                    <soapenv:Header/>
                    <soapenv:Body>
                      <tem:ReverseQitafPointRedemption>
                       <tem:request>
                        <red:BranchId>" + strBranchId + @"</red:BranchId>
                        <red:MSISDN>" + strMSIDSN + @"</red:MSISDN>
                        <red:RequestDate>" + strReqDate + @"</red:RequestDate>
                        <red:RequestId>" + strReqGuid + @"</red:RequestId>
                        <red:TerminalId>" + strTermId + @"</red:TerminalId>
                        <red:RefRequestDate>" + m_strRefRequestDate + @"</red:RefRequestDate>
                        <red:RefRequestId>" + m_strRefRequestId + @"</red:RefRequestId>
                       </tem:request>
                      </tem:ReverseQitafPointRedemption>
                    </soapenv:Body>
                    </soapenv:Envelope>";
            soapEnvelopeDocument.LoadXml(strXml);

            return soapEnvelopeDocument;
        }

        private XmlDocument CreateSoapEnvelope2(string strAmount, string strPin)
        {
            string strReqGuid = Guid.NewGuid().ToString();
            m_strTransactionGUIDRedeem = strReqGuid;
            m_strTransactionGUIDReward = " ";
            m_strRefRequestId = strReqGuid;
            string strBranchId = "15930001";
            string strMSIDSN = m_strPN;// @"506617981";
            m_strPN = strMSIDSN;
            string strReqDate = DateTime.Now.ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToString("HH:mm:ss");
            m_strRefRequestDate = strReqDate;
            string strTermId = "15930001";//]strBranchId;
            string strLangCode = "en-US";

            XmlDocument soapEnvelopeDocument = new XmlDocument();
            string strXml =
                @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" 
                    xmlns:tem = ""http://tempuri.org/""
                    xmlns:red = ""http://schemas.datacontract.org/2004/07/Redemption.Lite.Integration.Service.Interface"">
                    <soapenv:Header/>
                    <soapenv:Body>
                      <tem:RedeemQitafPoints>
                       <tem:request>
                        <red:BranchId>" + strBranchId + @"</red:BranchId>
                        <red:MSISDN>" + strMSIDSN + @"</red:MSISDN>
                        <red:RequestDate>" + strReqDate + @"</red:RequestDate>
                        <red:RequestId>" + strReqGuid + @"</red:RequestId>
                        <red:TerminalId>" + strTermId + @"</red:TerminalId>
                        <red:Amount>" + strAmount + @"</red:Amount>
                        <red:LanguageCode>" + strLangCode + @"</red:LanguageCode>
                        <red:Pin>" + strPin + @"</red:Pin>
                       </tem:request>
                      </tem:RedeemQitafPoints>
                    </soapenv:Body>
                    </soapenv:Envelope>";
            soapEnvelopeDocument.LoadXml(strXml);

            return soapEnvelopeDocument;
        }

        private XmlDocument CreateSoapEnvelope1()
        {
            string strReqGuid = Guid.NewGuid().ToString();
            string strBranchId = "15930001";
            string strMSIDSN = m_strPN;// @"506617981";//
            string strReqDate = DateTime.Now.ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToString("HH:mm:ss");
            string strTermId = "15930001";
            string strLangCode = "en-US";

            XmlDocument soapEnvelopeDocument = new XmlDocument();
            string strXml =
                @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" 
                    xmlns:tem = ""http://tempuri.org/""
                    xmlns:red = ""http://schemas.datacontract.org/2004/07/Redemption.Lite.Integration.Service.Interface"">
                    <soapenv:Header/>
                    <soapenv:Body>
                      <tem:GenerateOTP>
                       <tem:request>
                        <red:BranchId>" + strBranchId + @"</red:BranchId>
                        <red:MSISDN>" + strMSIDSN + @"</red:MSISDN>
                        <red:RequestDate>" + strReqDate + @"</red:RequestDate>
                        <red:RequestId>" + strReqGuid + @"</red:RequestId>
                        <red:TerminalId>" + strTermId + @"</red:TerminalId>
                        <red:LanguageCode>" + strLangCode + @"</red:LanguageCode>
                       </tem:request>
                      </tem:GenerateOTP>
                    </soapenv:Body>
                    </soapenv:Envelope>";
            soapEnvelopeDocument.LoadXml(strXml);

            return soapEnvelopeDocument;
        }

        private static void InsertSoapEnvelopeIntoWebRequest(XmlDocument soapEnvelopeXml, HttpWebRequest webRequest)
        {
            using (Stream stream = webRequest.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }
        }

        public string callapi_genAuthTk()
        {
            m_strErrCode = " ";
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                | SecurityProtocolType.Tls11
                | SecurityProtocolType.Tls12
                | SecurityProtocolType.Ssl3;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            try
            {
                string url = "https://dpw.alrajhibank.com.sa:9443/api-factory/prod/loyalty-redemption/oauth2/token";
                //old api:   "https://gwt.alrajhibank.com.sa:9443/api-factory/sit/loyalty-redemption/oauth2/token";

                HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
                webrequest.Method = "POST";

                webrequest.ContentType = "application/x-www-form-urlencoded";
                var postData = "grant_type=" + Uri.EscapeDataString("client_credentials");
                postData += "&scope=" + Uri.EscapeDataString("customer-authorization otp-validation redemption-transaction-reversal redemption-transactions");
                postData += "&client_id=" + Uri.EscapeDataString("7bbc3efaf563b5997ed2bb6b8f55f2b4");
                postData += "&client_secret=" + Uri.EscapeDataString("b276a587621e15bbe57569c95cb40a12");

                var data = Encoding.Default.GetBytes(postData);
                webrequest.ContentLength = data.Length;
                var newStream = webrequest.GetRequestStream();
                newStream.Write(data, 0, data.Length);
                newStream.Close();

                //webrequest.Headers.Add("merchantToken", "Bearer Khalta@2021");
                HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();
                Encoding enc = System.Text.Encoding.GetEncoding("utf-8");
                StreamReader responseStream = new StreamReader(webresponse.GetResponseStream(), enc);
                string result = string.Empty;
                result = responseStream.ReadToEnd();
                webresponse.Close();
                Console.WriteLine(result);
                JObject jObject = JObject.Parse(result);
                dynamic dataRet = jObject;
                Console.WriteLine(dataRet.access_token);
                m_strAccessToken = dataRet.access_token;
                m_strTokenType = dataRet.token_type;
                m_bSuccessOnGenToken = true;
                return dataRet.access_token;
            }
            catch (WebException wex)
            {
                m_bSuccessOnGenToken = false;
                m_strRetMessage = wex.Message.ToString();
                Console.Write(wex.ToString());
                using (WebResponse response = wex.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        Console.WriteLine(text);
                        JObject jObject = JObject.Parse(text);
                        dynamic dataRet = jObject;
                        m_strRetMessage = dataRet.message + " ErrorCode:" + dataRet.errorCode;
                        m_strErrCode = dataRet.errorCode;
                        showMessageForm_Alrahji(" ", int.Parse(m_strErrCode));
                        return "";
                    }
                }
            }

        }

        public void updateTransTbl4ReturnQty(string strTranId)
        {
            return;
            System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection();
            sqlConn.ConnectionString = LSRetailPosis.Settings.ApplicationSettings.Database.LocalConnectionString;
            string query = "UPDATE ax.retailtransactionsalestrans SET RETURNQTY = -999 WHERE TRANSACTIONID=@transid";

            SqlDataAdapter da = new SqlDataAdapter();
            da.UpdateCommand = new SqlCommand(query, sqlConn);

            da.UpdateCommand.Parameters.Add("@transid", SqlDbType.VarChar).Value = strTranId;

            sqlConn.Open();
            da.UpdateCommand.ExecuteNonQuery();
            sqlConn.Close();

        }

        public void genEncstrFromId(string strReceiptId)
        {
            char c = strReceiptId.FirstOrDefault();

            string strIn = strReceiptId.Replace("-", "");
            if (c == 'R')
            {
                strIn = strReceiptId.Substring(1);
            }
            var encrypted = SQLServerCryptoMethod.ToBase36(ulong.Parse(strIn));
            System.Console.WriteLine(encrypted.ToString().ToUpper());
            m_strEncInvoice = encrypted.ToString();// encrypted.ToString().ToUpper().Substring(2);
        }

        public void genShelf(string strReceiptId, string strStoreId, bool bSendSMS)
        {
            if (strReceiptId == null)
            {
                //return;
                strReceiptId = "9999";
            }
            if (strReceiptId.Length < 2)
            {
                //return;
                strReceiptId = "9999";
            }
            // generate Link
            string strLink = "";
            {
                var passphrase = "key";
                //var encrypted = SQLServerCryptoMethod.EncryptByPassPhrase(@passphrase, strReceiptId);
                genEncstrFromId(strReceiptId);/*
                var encrypted = SQLServerCryptoMethod.ToBase36(ulong.Parse(strReceiptId));
                System.Console.WriteLine(encrypted.ToString().ToUpper());
                m_strEncInvoice = encrypted.ToString();// encrypted.ToString().ToUpper().Substring(2);*/
                Insert_QRShelf(strReceiptId, m_strEncInvoice, strStoreId);
                saveEncInvoice2TableByTranId(m_strEncInvoice, strReceiptId);
            }
        }


        public void saveEncInvoice2TableByTranId(string strEncInvoice, string strReceiptId)
        {
            System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection();
            sqlConn.ConnectionString = LSRetailPosis.Settings.ApplicationSettings.Database.LocalConnectionString;
            string query = "UPDATE ax.retailtransactionsalestrans SET shelf = @encInvoice WHERE transactionid =@receiptnum";

            SqlDataAdapter da = new SqlDataAdapter();
            da.UpdateCommand = new SqlCommand(query, sqlConn);

            da.UpdateCommand.Parameters.Add("@receiptnum", SqlDbType.VarChar).Value = strReceiptId;
            da.UpdateCommand.Parameters.Add("@encInvoice", SqlDbType.VarChar).Value = strEncInvoice;


            sqlConn.Open();
            da.UpdateCommand.ExecuteNonQuery();
            sqlConn.Close();
        }

        public void Insert_QRShelf(string strTrans, string strShelf, string strStoreId)
        {
            System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection();
            sqlConn.ConnectionString = LSRetailPosis.Settings.ApplicationSettings.Database.LocalConnectionString;
            string query = "INSERT INTO ax.QRSHELF (TRANSDATATIME,transactionid, SHELF, StoreID)";
            query += " VALUES (@TRANSDATATIME,@transactionid,@SHELF,@StoreID)";

            SqlDataAdapter da = new SqlDataAdapter();
            da.InsertCommand = new SqlCommand(query, sqlConn);

            da.InsertCommand.Parameters.Add("@TRANSDATATIME", SqlDbType.DateTime).Value = DateTime.Now;
            da.InsertCommand.Parameters.Add("@transactionid", SqlDbType.VarChar).Value = strTrans;
            da.InsertCommand.Parameters.Add("@SHELF", SqlDbType.VarChar).Value = strShelf;
            da.InsertCommand.Parameters.Add("@StoreID", SqlDbType.VarChar).Value = strStoreId;

            sqlConn.Open();
            da.InsertCommand.ExecuteNonQuery();
            sqlConn.Close();

        }

        public void Insert_AlrahjiBankTrans()
        {
            System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection();
            sqlConn.ConnectionString = LSRetailPosis.Settings.ApplicationSettings.Database.LocalConnectionString;
            string query = "INSERT INTO ax.RetailAlrajhitrans (TRANSDATATIME,transactionid, phonenumber, store, StoreID, Amount, transactionIDinrajhi,transactionTypeinrajhi)";
            query += " VALUES (@TRANSDATATIME,@transactionid,@phonenumber, @store, @StoreID, @Amount, @transactionIDinrajhi, @transactionTypeinrajhi)";

            SqlDataAdapter da = new SqlDataAdapter();
            da.InsertCommand = new SqlCommand(query, sqlConn);



            da.InsertCommand.Parameters.Add("@TRANSDATATIME", SqlDbType.DateTime).Value = m_strTransactionDate;
            da.InsertCommand.Parameters.Add("@transactionid", SqlDbType.VarChar).Value = m_strTansactionID4POS;
            da.InsertCommand.Parameters.Add("@phonenumber", SqlDbType.Decimal).Value = m_strPN;
            da.InsertCommand.Parameters.Add("@store", SqlDbType.VarChar).Value = m_strStore4Alrajhi;
            da.InsertCommand.Parameters.Add("@StoreID", SqlDbType.VarChar).Value = m_strStoreId4POS;
            da.InsertCommand.Parameters.Add("@Amount", SqlDbType.VarChar).Value = m_strAmount;
            da.InsertCommand.Parameters.Add("@transactionIDinrajhi", SqlDbType.VarChar).Value = m_strTransactionID;
            da.InsertCommand.Parameters.Add("@transactionTypeinrajhi", SqlDbType.VarChar).Value = m_strTransactionType;


            sqlConn.Open();
            da.InsertCommand.ExecuteNonQuery();
            sqlConn.Close();

        }

        public void Update_QitafTrans4Reverse()
        {
            System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection();
            sqlConn.ConnectionString = LSRetailPosis.Settings.ApplicationSettings.Database.LocalConnectionString;
            string query = "UPDATE ax.RetailQitaftrans SET Reserved = @Reserved WHERE transactionGUIDRedeem=@transactionGUIDRedeem";

            SqlDataAdapter da = new SqlDataAdapter();
            da.UpdateCommand = new SqlCommand(query, sqlConn);

            da.UpdateCommand.Parameters.Add("@Reserved", SqlDbType.VarChar).Value = "Reversed";
            da.UpdateCommand.Parameters.Add("@transactionGUIDRedeem", SqlDbType.VarChar).Value = m_strTransactionGUIDRedeem;

            sqlConn.Open();
            da.UpdateCommand.ExecuteNonQuery();
            sqlConn.Close();
        }

        public void Update_QitafTrans4RewardUpdate()
        {
            System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection();
            sqlConn.ConnectionString = LSRetailPosis.Settings.ApplicationSettings.Database.LocalConnectionString;
            string query = "UPDATE ax.RetailQitaftrans SET Reserved = @Reserved WHERE transactionGUIDReward=@transactionGUIDReward";

            SqlDataAdapter da = new SqlDataAdapter();
            da.UpdateCommand = new SqlCommand(query, sqlConn);
            string strReserved = "";
            decimal restAmount;
            if (decimal.Parse(m_strResidueAmount4RewardUdt) == 0)
            {
                restAmount = decimal.Parse(m_strAmount);
            }
            else
            {
                restAmount = decimal.Parse(m_strResidueAmount4RewardUdt);
            }
            restAmount = restAmount - decimal.Parse(m_strReductionAmount);
            strReserved = restAmount.ToString();


            da.UpdateCommand.Parameters.Add("@Reserved", SqlDbType.VarChar).Value = strReserved;
            da.UpdateCommand.Parameters.Add("@transactionGUIDReward", SqlDbType.VarChar).Value = m_strTransactionGUIDReward;

            sqlConn.Open();
            da.UpdateCommand.ExecuteNonQuery();
            sqlConn.Close();
        }

        public void Insert_QitafTrans()
        {
            System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection();
            sqlConn.ConnectionString = LSRetailPosis.Settings.ApplicationSettings.Database.LocalConnectionString;
            string query = "INSERT INTO ax.RetailQitaftrans (TRANSDATATIME,transactionid, phonenumber, StoreID, Amount,transactionGUIDReward,transactionGUIDRedeem,RequestDate)";
            query += " VALUES (@TRANSDATATIME,@transactionid,@phonenumber, @StoreID, @Amount, @transactionGUIDReward,@transactionGUIDRedeem,@RequestDate)";

            SqlDataAdapter da = new SqlDataAdapter();
            da.InsertCommand = new SqlCommand(query, sqlConn);



            da.InsertCommand.Parameters.Add("@TRANSDATATIME", SqlDbType.DateTime).Value = DateTime.Now;
            da.InsertCommand.Parameters.Add("@transactionid", SqlDbType.VarChar).Value = m_strTansactionID4POS;
            da.InsertCommand.Parameters.Add("@phonenumber", SqlDbType.Decimal).Value = Convert.ToDecimal(m_strPN);
            da.InsertCommand.Parameters.Add("@StoreID", SqlDbType.VarChar).Value = m_strStoreId4POS;
            da.InsertCommand.Parameters.Add("@Amount", SqlDbType.VarChar).Value = m_strTransactionGUIDRedeem.Length > 5 ? m_strAmount : m_strRewardAmount;
            da.InsertCommand.Parameters.Add("@transactionGUIDReward", SqlDbType.VarChar).Value = m_strTransactionGUIDReward;
            da.InsertCommand.Parameters.Add("@transactionGUIDRedeem", SqlDbType.VarChar).Value = m_strTransactionGUIDRedeem;
            if (m_strRefRequestDate == "" || m_strRefRequestDate == null || m_strRefRequestDate.Length < 5)
            {
                m_strRefRequestDate = DateTime.Now.ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToString("HH:mm:ss");
            }
            da.InsertCommand.Parameters.Add("@RequestDate", SqlDbType.VarChar).Value = m_strRefRequestDate;


            sqlConn.Open();
            da.InsertCommand.ExecuteNonQuery();
            sqlConn.Close();

        }

        public void process_Qitaf_RewardPoint(string strTransId)
        {
            m_strTansactionID4POS = strTransId;
            if (m_strPN4Qitaf == null)
            {
                return;
            }
            if (m_strPN4Qitaf.Length == 0)
            {
                return;
            }
            int nRespCode4 = call_Qitaf_4_RewardPoint();
            if (nRespCode4 == 0)
            {
                //   SendLink2SMS(m_strPN4Qitaf, "Qitaf Reward Point has been completed successfully. Amount:" + m_strRewardAmount);
                // after process RewardPointer, collected Qitaf info should be deleted.
                // because it is available only one transaction. 2022.5.19 confirmed together!!!!OK?
                // and need to touch "collect Qitaf" before call "rewardupdate"?
                m_strPN4Qitaf = ""; // disabled now. it should keep for long for next phase.
                MessageBox.Show(" تم إرسال مبلغ المشتريات لحساب العميل بقطاف");
            }
            else
            {
                //MessageBox.Show("Qitaf Reward Point has been failed. Response Code:" + nRespCode4.ToString());
            }

        }

        public void processBankAPI()
        {

            using (Microsoft.Dynamics.Retail.Pos.BlankOperations.Dialog.frmPNCode dialog1 = new Microsoft.Dynamics.Retail.Pos.BlankOperations.Dialog.frmPNCode())
            {
                LSRetailPosis.POSProcesses.POSFormsManager.ShowPOSForm(dialog1);
                code = dialog1.Code;

                if (code.ToString().Length >= 9)
                {
                    // step1.1 : call "Gen OAuthToken"
                    string strAuthToken = callapi_genAuthTk();
                    callapi_authCustomer();


                }

                else
                {
                    // if he skip enter, no SMS
                    // don't send link
                }

            }
        }

    }
}