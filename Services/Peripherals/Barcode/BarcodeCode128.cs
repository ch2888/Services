/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/


namespace Microsoft.Dynamics.Retail.Pos.Services
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// Encapsulates the Barcode Code 128 string encoding
    /// </summary>
    public sealed class BarcodeCode128 : Barcode
    {        
        private const string FONT_NAME = "BC C128 Narrow";
        private const int FONT_SIZE = 36;

        private const int EscChar = 126;
        private const string EscCharStr = "~";
        private const string F1_Encode = "~f";

        private const string StartAOptimizer = "~g";
        private const string StartBOptimizer = "~h";
        private const string StartCOptimizer = "~i";

        private const string Shift2AOptimizer = "~e";
        private const string Shift2BOptimizer = "~d";
        private const string Shift2COptimizer = "~c";

        private const int CodeSetA = 1;
        private const int CodeSetB = 2;
        private const int CodeSetC = 3;

        private const int NextA = 1;
        private const int NextB = 2;

        private const int NotFound = -1;

        private const int MaxReturnStringSize = 250;

        public override string FontName
        {
            get { return FONT_NAME; }
        }

        public override int FontSize
        {
            get { return FONT_SIZE; }
        }

        public override string Encode(string text)
        {
            return Encode(text, false);
        }

        public string Encode(string text, bool useF1Code)
        {
            int stringLength = (text + '.').Length - 1; // handle trailing spaces so make sure len1 includes specified spaces
            bool cont = true;
            int tichr = 0;
            int codeSet = CodeSetB;
            int fnc4 = 1;
            int tcchr;
            int checksumTotal = 0;
            int val;
            int charVal;
            int charCount = 0;
            int returnInt;
            string optimizedText;
            string result = string.Empty;
            bool shift = false;
            int outputCount = 0;

            if (stringLength < 1)
            {
                return text;
            }

            // Check the character string and optimize the structure
            optimizedText = this.OptimizeCodeSets(text, stringLength, useF1Code);

            // Map the characters
            while (cont)
            {
                charVal = (int)optimizedText.ElementAtOrDefault(tichr);
                if ((charVal == EscChar) && ((int)optimizedText.ElementAtOrDefault(tichr + 1) != 0))   // Tilde character #126
                {
                    tichr++;
                    charVal = (int)optimizedText.ElementAtOrDefault(tichr);

                    // Make the val variable equal to the character after the /
                    if (charVal == EscChar)
                    {
                        val = 95; // if the character is an esckey then make the value a 95
                    }
                    else
                    {
                        val = (charVal < 0 || charVal > 105) ? NotFound : charVal + 1; // convert to index
                    }

                    if (val != NotFound)
                    {
                        result += this.Index2OutStr(val);
                    }
                    else
                    {
                        result += (char)63; // ?
                        result += (char)118; // v
                        result += (char)63; // ?
                    }

                    if (charCount == 0)
                    {
                        checksumTotal += val - 1;
                    }
                    else
                    {
                        checksumTotal += charCount * (val - 1);
                    }

                    charCount++;
                    outputCount += 3;

                    // handle FNC4
                    if (((charVal == 100) && (codeSet == CodeSetB)) || ((charVal == 101) && (codeSet == CodeSetA)))
                    {
                        if (fnc4 < 4)
                        {
                            fnc4++;
                        }
                        else
                        {
                            fnc4 = 1;
                        }
                    }
                    else
                    {
                        switch (charVal)
                        {
                            case 101: // e
                            case 103: // g
                                codeSet = CodeSetA;
                                break;
                            case 100: // d
                            case 104: // h
                                codeSet = CodeSetB;
                                break;
                            case 99: // c
                            case 105: // i                                 
                                codeSet = CodeSetC;
                                break;
                        }
                    }
                    tichr++;
                }
                else
                {
                    if ((codeSet == CodeSetC) && (charVal >= 48) && (charVal <= 57))
                    {
                        returnInt = this.CodeSetCSearch(optimizedText, tichr);
                        if (returnInt != -1)
                        {
                            result += this.Index2OutStr(returnInt);

                            checksumTotal += charCount * (returnInt - 1);
                            charCount++;
                            outputCount += 3;
                        }
                        else
                        {
                            result += (char)63; // ?
                            result += (char)99; // c
                            result += (char)63; // ?
                            outputCount += 3;
                        }
                        tichr += 2;
                    }
                    else
                    {
                        if (fnc4 > 1)
                        {
                            if (fnc4 == 4)
                            {
                                tcchr = (int)optimizedText.ElementAtOrDefault(tichr) + 128;
                            }
                            else
                            {
                                tcchr = (int)optimizedText.ElementAtOrDefault(tichr);
                            }

                            if (fnc4 == 2)
                            {
                                fnc4 = 1;
                            }
                            else
                            {
                                fnc4 = 3;
                            }
                        }
                        else
                        {
                            tcchr = (int)optimizedText.ElementAtOrDefault(tichr);
                        }

                        tichr++;
                        if (shift)
                        {
                            if (codeSet == CodeSetA)
                            {
                                returnInt = (tcchr <= 31 || tcchr > 127) ? NotFound : tcchr - 31; // is tcchr in codeset B?
                            }
                            else
                            {
                                returnInt = (tcchr < 0 || tcchr > 127) ? NotFound : (tcchr <= 31) ? tcchr + 65 : tcchr - 31; // is tcchr in codeset a
                            }
                            shift = false;
                        }
                        else
                        {
                            if (codeSet == CodeSetA)
                            {
                                returnInt = (tcchr < 0 || tcchr > 127) ? 96 : (tcchr <= 31) ? tcchr + 65 : tcchr - 31; // is tcchr in codeset a
                            }
                            else
                            {
                                returnInt = (tcchr <= 31 || tcchr > 127) ? NotFound : tcchr - 31; // is tcchr in codeset B?
                            }
                        }
                        if (returnInt != NotFound)
                        {
                            result += this.Index2OutStr(returnInt);

                            checksumTotal += charCount * (returnInt - 1);
                            charCount++;
                            outputCount += 3;
                        }
                        else
                        {
                            result += (char)63; // ?
                            result += (char)98; // b
                            result += (char)63; // ?
                            outputCount += 3;
                        }
                        if ((charVal == 98) && ((codeSet == CodeSetA) || (codeSet == CodeSetB)))
                        {
                            shift = true;
                        }
                    }
                }

                if ((int)optimizedText.ElementAtOrDefault(tichr) <= 1)
                {
                    cont = false;
                }
            }

            val = checksumTotal % 103;
            if ((codeSet == CodeSetA) && (val == 91))  // ESc
            {
                result += this.Index2OutStr(101); // shift to codeset b
                checksumTotal += charCount * (100);
                charCount++;

                result += this.Index2OutStr(74);
                checksumTotal += charCount * (73);
                charCount++;
                outputCount += 6;

                val = checksumTotal % 103;
            }

            result += this.Index2OutStr(1 + val);
            // add stop character
            result += "\')!1";  // num2char(39) + num2char(41) + num2char(33) + num2char(49)

            if (outputCount > MaxReturnStringSize - 7) // 3 start + 4 stop
            {
                throw (new Exception("String is too long for the barcode"));
            }

            return result;
        }

        private string OptimizeCodeSets(string text, int stringLength, bool useF1Code)
        {
            int codeset = CodeSetA;
            string returnString = string.Empty;           // return string
            int nchr;
            int nCodeSetC;             // Count of characters using CodesetC
            bool cont;
            int i = 0;
            int j;
            int k = 0;
            int charValue;          // ASCII value of current character
            int charValue2;         // ASCII value of next character
            int nextCodeSet;

            // check to see if control codes are already imbedding in input string
            charValue = (int)text[0];
            if (charValue == EscChar)
            {
                return text;
            }

            // Calculate the start code
            // If the length of the string is 2 and chr 0 and 1 are numeric then start code is c
            charValue2 = (int)text.ElementAtOrDefault(1);
            if (charValue2 == default(char))
            {
                return text;

            }
            if ((stringLength == 2) && ((charValue >= 48) && (charValue <= 57)) && ((charValue2 >= 48) && (charValue2 <= 57)))
            {
                returnString = StartCOptimizer;
                codeset = CodeSetC;      // set the codeset to c
            }
            else
            {
                // Check the start of the string to see if it starts with numbers
                nCodeSetC = this.CharInCodeSetCount(text, stringLength, 0, CodeSetC);

                // if string starts with more than 4 number it will be Codeset c
                if (nCodeSetC >= 4)
                {
                    // If ncodec is odd then the 1st character should be in Codeset B then go to Code c
                    if (nCodeSetC % 2 > 0)
                    {
                        returnString = StartBOptimizer;
                        codeset = CodeSetB;    // set the codeset to b
                    }
                    else
                    {
                        // if string starts with even number of numerics start with Code C.
                        returnString = StartCOptimizer;
                        codeset = CodeSetC;    // set the codeset to c
                    }
                }
                else
                {
                    // if string doesn't start with 4 numbers
                    cont = true;
                    i = 0;
                    // Find if the first character is Codeset A or B.
                    while (cont)
                    {
                        charValue = (int)text.ElementAtOrDefault(i);
                        if ((charValue >= 0) && (charValue <= 31) || charValue > 127) //0 - 31 is unique to a
                        {
                            cont = false;
                            returnString = StartAOptimizer;
                            codeset = CodeSetA;    // set the codeset to a
                        }
                        else
                        {
                            if ((charValue >= 96) && (charValue <= 127)) //96-127 is unique to b
                            {
                                cont = false;
                                returnString = StartBOptimizer;
                                codeset = CodeSetB;    // set the codeset to b
                            }
                        }

                        // Default to Codeset B if all characters are in both A & b
                        if (cont && (stringLength == i + 1))
                        {
                            cont = false;
                            returnString = StartBOptimizer;
                            codeset = CodeSetB;    // set the codeset to b
                        }
                        i++;

                    }
                }
            }

            // add support for fnc1 adding it right after the start code
            if (useF1Code)
            {
                returnString += F1_Encode;
            }

            // convert the message
            cont = true;    // whether or not to continue
            i = 0;          // index into input string

            while (cont)
            {
                j = this.CharInCodeSetCount(text, stringLength, i, codeset);
                if (j == 0)
                {
                    returnString += (char)127; // DEL
                    i++;
                }
                else
                {
                    for (k = j; k > 0; k--)
                    {
                        if (text.ElementAtOrDefault(i) == EscChar)
                        {
                            if (i + 2 <= text.Length)
                            {
                                returnString += text.Substring(i, 2);

                                i += 2;
                            }
                            else if (i + 1 <= text.Length)
                            {
                                returnString += text.Substring(i, 1);

                                i++;
                            }
                            k--;
                        }
                        else
                        {
                            returnString += text.Substring(i, 1);
                            i++;
                        }

                        if (i >= text.Length)
                        {
                            break;
                        }
                    }
                }

                if (i >= stringLength)  // end of string
                {
                    return returnString;
                }

                // process code change
                switch (codeset)
                {
                    case CodeSetA: // if currently Codeset a
                        charValue = (int)text.ElementAtOrDefault(i);
                        if ((charValue >= 48) && (charValue <= 57))
                        {
                            nchr = this.CharInCodeSetCount(text, stringLength, i, CodeSetC);
                            if (nchr >= 4)
                            {
                                if (nchr % 2 > 0)
                                {
                                    returnString += text.Substring(i, 1);
                                    i++;
                                }
                                returnString += Shift2COptimizer;
                                codeset = CodeSetC;
                            }
                            else
                            {
                                if (nchr > 0)
                                {
                                    returnString += text.Substring(i, nchr);
                                    i += nchr;
                                }
                                else
                                {
                                    returnString += (char)127; // DEL
                                }
                                i++;
                            }
                        }
                        else
                        {
                            if ((i <= stringLength) && (this.NextABChange(text, stringLength, i) == NextB) && (this.NextABChange(text, stringLength, i + 1) == NextA))
                            {
                                if (charValue == 172)
                                {
                                    returnString += (char)EscChar;
                                    returnString += (char)98; // b
                                    returnString += (char)95; // _
                                }
                                else
                                {
                                    returnString += (char)EscChar;
                                    returnString += (char)98; // b
                                    returnString += text.Substring(i, 1);
                                }
                                i++;
                            }
                            else
                            {
                                if (this.NextABChange(text, stringLength, i) == NextB)
                                {
                                    returnString += Shift2BOptimizer;
                                    codeset = CodeSetB;
                                }
                            }
                        }
                        break;
                    case CodeSetB: // if currently Codeset b
                        charValue = (int)text.ElementAtOrDefault(i);
                        if ((charValue >= 48) && (charValue <= 57))
                        {
                            nchr = this.CharInCodeSetCount(text, stringLength, i, CodeSetC);
                            if (nchr >= 4)
                            {
                                if (nchr % 2 > 0)
                                {
                                    returnString += text.Substring(i, 1);
                                    i++;
                                }
                                returnString += Shift2COptimizer;
                                codeset = CodeSetC;
                            }
                            else
                            {
                                if (nchr > 0)
                                {
                                    returnString += text.Substring(i, nchr);
                                    i += nchr;
                                }
                                else
                                {
                                    returnString += (char)127; // DEL
                                }
                                i++;
                            }
                        }
                        else
                        {
                            if ((i <= stringLength) && (this.NextABChange(text, stringLength, i) == NextA) && (this.NextABChange(text, stringLength, i + 1) == NextB))
                            {
                                if (charValue == 172) // ¼
                                {
                                    returnString += (char)EscChar;
                                    returnString += (char)98; // b
                                    returnString += (char)200; // ╚
                                }
                                else
                                {
                                    returnString += (char)EscChar;
                                    returnString += (char)98; // b
                                    returnString += text.Substring(i, 1);
                                }
                                i++;
                            }
                            else
                            {
                                if (this.NextABChange(text, stringLength, i) == NextA)
                                {
                                    returnString += Shift2AOptimizer;
                                    codeset = CodeSetA;
                                }
                            }
                        }
                        break;
                    case CodeSetC: // if currently Codeset c
                        nextCodeSet = this.NextABChange(text, stringLength, i);
                        switch (nextCodeSet)
                        {
                            case NextA:
                                returnString += Shift2AOptimizer;
                                codeset = CodeSetA;
                                break;
                            case NextB:
                                returnString += Shift2BOptimizer;
                                codeset = CodeSetB;
                                break;
                        }
                        break;
                }
            }

            return returnString;
        }

        private int CharInCodeSetCount(string text, int stringLength, int startPosition, int codeSet)
        {
            int retNum = 0;
            int nchr;
            int i;
            int charThisPos;
            bool anyF1 = false;
            int digitsBeforeF1 = 0;
            switch (codeSet)
            {
                case CodeSetA:
                    for (i = startPosition; i <= stringLength; i++)
                    {
                        charThisPos = (int)text.ElementAtOrDefault(i);
                        if (((charThisPos >= 0) && (charThisPos <= 47)) || ((charThisPos >= 58) && (charThisPos <= 95)) || (charThisPos > 127))
                        {
                            retNum++;
                        }
                        else
                        {
                            if ((charThisPos >= 48) && (charThisPos <= 57))
                            {
                                nchr = this.FollowingCharsBetween(text, stringLength, i, 48, 57);
                                if (nchr >= 4)
                                {
                                    i = stringLength + 1; // break the loop
                                }
                                else
                                {
                                    retNum += nchr;
                                    i += nchr - 1;
                                }
                            }
                            else
                            {
                                i = stringLength + 1; // break the loop
                            }
                        }
                    }
                    break;
                case CodeSetB:
                    for (i = startPosition; i <= stringLength; i++)
                    {
                        charThisPos = (int)text.ElementAtOrDefault(i);
                        if (((charThisPos >= 32) && (charThisPos <= 47)) || ((charThisPos >= 58) && (charThisPos <= 127)))
                        {
                            retNum++;
                        }
                        else
                        {
                            if ((charThisPos >= 48) && (charThisPos <= 57))
                            {
                                nchr = this.FollowingCharsBetween(text, stringLength, i, 48, 57);
                                if (nchr >= 4)
                                {
                                    i = stringLength + 1; // break the loop to force a switch to codesetc
                                    if (nchr % 2 == 1)
                                    {
                                        retNum += 1;
                                    }
                                }
                                else
                                {
                                    retNum += nchr;
                                    i += nchr - 1;
                                }
                            }
                            else
                            {
                                i = stringLength + 1; // break the loop
                            }
                        }
                    }
                    break;
                case CodeSetC:
                    for (i = startPosition; i <= stringLength; i++)
                    {
                        charThisPos = (int)text.ElementAtOrDefault(i);
                        if ((charThisPos >= 48) && (charThisPos <= 57))
                        {
                            retNum++;
                        }
                        else
                        {
                            /* if (i + 2 <= text.Length)
                            { */
                            if (retNum % 2 == 0 && text.Length >= i + 2 && text.Substring(i, 2) == F1_Encode) // equal number of digits followed by FNC1
                            {
                                if (!anyF1)
                                {
                                    digitsBeforeF1 = retNum;
                                }
                                retNum += 2;
                                i++;
                                anyF1 = true;
                            }
                            else
                            {
                                i = stringLength + 1; // break the loop
                            }
                            // }
                        }
                    }
                    if (anyF1 && (retNum % 2 != 0))
                    {
                        retNum = digitsBeforeF1;
                    }
                    break;
            }
            return retNum;

        }

        private int FollowingCharsBetween(string text, int stringLength, int startPosition, int minChar, int maxChar)
        {
            int retNum = 0;
            int i;
            int charValue;

            for (i = startPosition; i <= stringLength; i++)
            {
                charValue = (int)text.ElementAtOrDefault(i);
                if ((charValue < minChar) || (charValue > maxChar))
                {
                    return retNum;
                }
                retNum++;
            }
            return retNum;
        }

        private int NextABChange(string text, int stringLength, int startPosition)
        {
            int endReturn = 0;
            int charValue;
            int position;

            for (position = startPosition; position <= stringLength; position++)
            {
                charValue = (int)text.ElementAtOrDefault(position); ;
                if ((charValue >= 96) && (charValue <= 127))
                {  // only supported in codesetB
                    return NextB;
                }
                if ((charValue >= 1) && (charValue <= 31))
                { // only supported in codesetA
                    return NextA;
                }
                if (charValue == 172)
                {
                    return NextA;
                }
                if ((charValue >= 32) && (charValue <= 95))
                { // supported in both codesetA and codesetB, but prefer B as it encodes more human readable characters
                    endReturn = NextB;
                }
            }
            return endReturn;
        }

        private int CodeSetCSearch(string text, int startPosition)
        {
            int c1 = (int)text.ElementAtOrDefault(startPosition) - 48;
            int c2 = (int)text.ElementAtOrDefault(startPosition + 1) - 48;

            if (c1 < 0 || c1 > 9)
            {
                return NotFound;
            }

            if (c2 < 0 || c2 > 9)
            {
                return NotFound;
            }

            return 10 * c1 + c2 + 1;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1505: Avoid unmaintainable code", Justification = "The method describes characters used for the encoding process and can not be split.")]
        private string Index2OutStr(int index)
        {
            string ret = string.Empty;

            switch (index)
            {
                case 106:
                    ret += (char)37; // %
                    ret += (char)34; // "
                    ret += (char)42; // *
                    break;

                case 105:
                    ret += (char)37; // %
                    ret += (char)34; // "
                    ret += (char)36; // $
                    break;

                case 104:
                    ret += (char)37; // %
                    ret += (char)36; // $
                    ret += (char)34; // "
                    break;

                case 103:
                    ret += (char)45; // -
                    ret += (char)33; // ! 
                    ret += (char)41; // )
                    break;

                case 102:
                    ret += (char)41; // )
                    ret += (char)33; // ! 
                    ret += (char)45; // -
                    break;

                case 101:
                    ret += (char)33; // ! 
                    ret += (char)45; // -
                    ret += (char)41; // )
                    break;

                case 100:
                    ret += (char)33; // !
                    ret += (char)41; // )
                    ret += (char)45; // -
                    break;

                case 99:
                    ret += (char)45; // -
                    ret += (char)35; // #
                    ret += (char)33; // !
                    break;

                case 98:
                    ret += (char)45; // -
                    ret += (char)33; // !
                    ret += (char)35; // #
                    break;

                case 97:
                    ret += (char)33; // !
                    ret += (char)47; // /
                    ret += (char)33; // !
                    break;

                case 96:
                    ret += (char)33; // !
                    ret += (char)45; // -
                    ret += (char)35; // #
                    break;

                case 95:
                    ret += (char)35; // #
                    ret += (char)33; // !
                    ret += (char)45; // -
                    break;

                case 94:
                    ret += (char)33; // !
                    ret += (char)35; // #
                    ret += (char)45; // -
                    break;

                case 93:
                    ret += (char)33; // !
                    ret += (char)33; // !
                    ret += (char)47; // /
                    break;

                case 92:
                    ret += (char)45; // -
                    ret += (char)37; // %
                    ret += (char)37; // %
                    break;

                case 91:
                    ret += (char)37; // %
                    ret += (char)45; // -
                    ret += (char)37; // %
                    break;

                case 90:
                    ret += (char)37; // %
                    ret += (char)37; // %
                    ret += (char)45; // -
                    break;

                case 89:
                    ret += (char)46; // .
                    ret += (char)34; // "
                    ret += (char)33; // !
                    break;

                case 88:
                    ret += (char)46; // .
                    ret += (char)33; // !
                    ret += (char)34; // "
                    break;

                case 87:
                    ret += (char)45; // -
                    ret += (char)34; // "
                    ret += (char)34; // "
                    break;

                case 86:
                    ret += (char)34; // "
                    ret += (char)46; // .
                    ret += (char)33; // !
                    break;

                case 85:
                    ret += (char)34; // "
                    ret += (char)45; // -
                    ret += (char)34; // "
                    break;

                case 84:
                    ret += (char)33; // !
                    ret += (char)46; // .
                    ret += (char)34; // "
                    break;

                case 83:
                    ret += (char)34; // "
                    ret += (char)34; // "
                    ret += (char)45; // -
                    break;

                case 82:
                    ret += (char)34; // "
                    ret += (char)33; // !
                    ret += (char)46; // .
                    break;

                case 81:
                    ret += (char)33; // !
                    ret += (char)34; // "
                    ret += (char)46; // .
                    break;

                case 80:
                    ret += (char)35; // #
                    ret += (char)45; // -
                    ret += (char)33; // !
                    break;

                case 79:
                    ret += (char)40; // (
                    ret += (char)33; // !
                    ret += (char)34; // "
                    break;

                case 78:
                    ret += (char)45; // -
                    ret += (char)41; // )
                    ret += (char)33; // !
                    break;

                case 77:
                    ret += (char)38; // &
                    ret += (char)33; // !
                    ret += (char)36; // $
                    break;

                case 76:
                    ret += (char)40; // (
                    ret += (char)34; // "
                    ret += (char)33; // !
                    break;

                case 75:
                    ret += (char)36; // $
                    ret += (char)38; // &
                    ret += (char)33; // !
                    break;

                case 74:
                    ret += (char)36; // $
                    ret += (char)37; // %
                    ret += (char)34; // "
                    break;

                case 73:
                    ret += (char)34; // "
                    ret += (char)40; // (
                    ret += (char)33; // !
                    break;

                case 72:
                    ret += (char)34; // "
                    ret += (char)37; // %
                    ret += (char)36; // $
                    break;

                case 71:
                    ret += (char)33; // !
                    ret += (char)40; // (
                    ret += (char)34; // "
                    break;

                case 70:
                    ret += (char)33; // !
                    ret += (char)38; // &
                    ret += (char)36; // $
                    break;

                case 69:
                    ret += (char)36; // $
                    ret += (char)34; // "
                    ret += (char)37; // %
                    break;

                case 68:
                    ret += (char)36; // $
                    ret += (char)33; // !
                    ret += (char)38; // &
                    break;

                case 67:
                    ret += (char)34; // "
                    ret += (char)36; // $
                    ret += (char)37; // %
                    break;

                case 66:
                    ret += (char)34; // "
                    ret += (char)33; // !
                    ret += (char)40; // (
                    break;

                case 65:
                    ret += (char)33; // !
                    ret += (char)36; // $
                    ret += (char)38; // &
                    break;

                case 64:
                    ret += (char)33; // !
                    ret += (char)34; // "
                    ret += (char)40; // (
                    break;

                case 63:
                    ret += (char)47; // /
                    ret += (char)33; // !
                    ret += (char)33; // !
                    break;

                case 62:
                    ret += (char)38; // &
                    ret += (char)36; // $
                    ret += (char)33; // !
                    break;

                case 61:
                    ret += (char)41; // )
                    ret += (char)45; // -
                    ret += (char)33; // !
                    break;

                case 60:
                    ret += (char)43; // +
                    ret += (char)37; // %
                    ret += (char)33; // !
                    break;

                case 59:
                    ret += (char)41; // )
                    ret += (char)39; // '
                    ret += (char)33; // !
                    break;

                case 58:
                    ret += (char)41; // )
                    ret += (char)37; // %
                    ret += (char)35; // #
                    break;

                case 57:
                    ret += (char)43; // +
                    ret += (char)33; // !
                    ret += (char)37; // %
                    break;

                case 56:
                    ret += (char)41; // )
                    ret += (char)35; // #
                    ret += (char)37; // %
                    break;

                case 55:
                    ret += (char)41; // )
                    ret += (char)33; // !
                    ret += (char)39; // '
                    break;

                case 54:
                    ret += (char)37; // %
                    ret += (char)41; // )
                    ret += (char)41; // )
                    break;

                case 53:
                    ret += (char)37; // %
                    ret += (char)43; // +
                    ret += (char)33; // !
                    break;

                case 52:
                    ret += (char)37; // %
                    ret += (char)41; // )
                    ret += (char)35; // #
                    break;

                case 51:
                    ret += (char)39; // '
                    ret += (char)33; // !
                    ret += (char)41; // )
                    break;

                case 50:
                    ret += (char)37; // %
                    ret += (char)35; // #
                    ret += (char)41; // )
                    break;

                case 49:
                    ret += (char)41; // )
                    ret += (char)41; // )
                    ret += (char)37; // %
                    break;

                case 48:
                    ret += (char)35; // #
                    ret += (char)41; // )
                    ret += (char)37; // %
                    break;

                case 47:
                    ret += (char)33; // !
                    ret += (char)43; // +
                    ret += (char)37; // %
                    break;

                case 46:
                    ret += (char)33; // !
                    ret += (char)41; // )
                    ret += (char)39; // '
                    break;

                case 45:
                    ret += (char)35; // #
                    ret += (char)37; // %
                    ret += (char)41; // )
                    break;

                case 44:
                    ret += (char)33; // !
                    ret += (char)39; // '
                    ret += (char)41; // )
                    break;

                case 43:
                    ret += (char)33; // !
                    ret += (char)37; // %
                    ret += (char)43; // +
                    break;

                case 42:
                    ret += (char)39; // '
                    ret += (char)35; // #
                    ret += (char)33; // !
                    break;

                case 41:
                    ret += (char)39; // '
                    ret += (char)33; // !
                    ret += (char)35; // #
                    break;

                case 40:
                    ret += (char)37; // %
                    ret += (char)35; // #
                    ret += (char)35; // #
                    break;

                case 39:
                    ret += (char)35; // #
                    ret += (char)39; // '
                    ret += (char)33; // !
                    break;

                case 38:
                    ret += (char)35; // #
                    ret += (char)37; // %
                    ret += (char)35; // #
                    break;

                case 37:
                    ret += (char)33; // !
                    ret += (char)39; // '
                    ret += (char)35; // #
                    break;

                case 36:
                    ret += (char)35; // #
                    ret += (char)35; // #
                    ret += (char)37; // %
                    break;

                case 35:
                    ret += (char)35; // #
                    ret += (char)33; // !
                    ret += (char)39; // '
                    break;

                case 34:
                    ret += (char)33; // !
                    ret += (char)35; // #
                    ret += (char)39; // '
                    break;

                case 33:
                    ret += (char)39; // '
                    ret += (char)37; // %
                    ret += (char)37; // %
                    break;

                case 32:
                    ret += (char)37; // %
                    ret += (char)39; // '
                    ret += (char)37; // %
                    break;

                case 31:
                    ret += (char)37; // %
                    ret += (char)37; // %
                    ret += (char)39; // '
                    break;

                case 30:
                    ret += (char)42; // *
                    ret += (char)38; // &
                    ret += (char)33; // !
                    break;

                case 29:
                    ret += (char)42; // *
                    ret += (char)37; // %
                    ret += (char)34; // "
                    break;

                case 28:
                    ret += (char)41; // )
                    ret += (char)38; // &
                    ret += (char)34; // "
                    break;

                case 27:
                    ret += (char)42; // *
                    ret += (char)34; // "
                    ret += (char)37; // %
                    break;

                case 26:
                    ret += (char)42; // *
                    ret += (char)33; // !
                    ret += (char)38; // &
                    break;

                case 25:
                    ret += (char)41; // )
                    ret += (char)34; // "
                    ret += (char)38; // &
                    break;

                case 24:
                    ret += (char)41; // )
                    ret += (char)37; // %
                    ret += (char)41; // )
                    break;

                case 23:
                    ret += (char)38; // &
                    ret += (char)41; // )
                    ret += (char)34; // "
                    break;

                case 22:
                    ret += (char)37; // %
                    ret += (char)42; // *
                    ret += (char)34; // "
                    break;

                case 21:
                    ret += (char)38; // &
                    ret += (char)34; // "
                    ret += (char)41; // )
                    break;

                case 20:
                    ret += (char)38; // &
                    ret += (char)33; // !
                    ret += (char)42; // *
                    break;

                case 19:
                    ret += (char)38; // &
                    ret += (char)42; // *
                    ret += (char)33; // !
                    break;

                case 18:
                    ret += (char)34; // "
                    ret += (char)42; // *
                    ret += (char)37; // %
                    break;

                case 17:
                    ret += (char)34; // "
                    ret += (char)41; // )
                    ret += (char)38; // &
                    break;

                case 16:
                    ret += (char)33; // !
                    ret += (char)42; // *
                    ret += (char)38; // &
                    break;

                case 15:
                    ret += (char)34; // "
                    ret += (char)38; // &
                    ret += (char)41; // )
                    break;

                case 14:
                    ret += (char)34; // "
                    ret += (char)37; // %
                    ret += (char)42; // *
                    break;

                case 13:
                    ret += (char)33; // !
                    ret += (char)38; // &
                    ret += (char)42; // *
                    break;

                case 12:
                    ret += (char)39; // '
                    ret += (char)34; // "
                    ret += (char)34; // "
                    break;

                case 11:
                    ret += (char)38; // &
                    ret += (char)35; // #
                    ret += (char)34; // "
                    break;

                case 10:
                    ret += (char)38; // &
                    ret += (char)34; // "
                    ret += (char)35; // #
                    break;

                case 09:
                    ret += (char)35; // #
                    ret += (char)38; // &
                    ret += (char)34; // "
                    break;

                case 08:
                    ret += (char)34; // "
                    ret += (char)39; // '
                    ret += (char)34; // "
                    break;

                case 07:
                    ret += (char)34; // "
                    ret += (char)38; // &
                    ret += (char)35; // #
                    break;

                case 06:
                    ret += (char)35; // #
                    ret += (char)34; // "
                    ret += (char)38; // &
                    break;

                case 05:
                    ret += (char)34; // "
                    ret += (char)35; // #
                    ret += (char)38; // &
                    break;

                case 04:
                    ret += (char)34; // "
                    ret += (char)34; // "
                    ret += (char)39; // '
                    break;

                case 03:
                    ret += (char)38; // &
                    ret += (char)38; // &
                    ret += (char)37; // %
                    break;

                case 02:
                    ret += (char)38; // &
                    ret += (char)37; // %
                    ret += (char)38; // &
                    break;

                case 01:
                    ret += (char)37; // %
                    ret += (char)38; // &
                    ret += (char)38; // &
                    break;

                default:
                    ret = string.Empty;
                    break;
            }

            return ret;
        }
    }
}
