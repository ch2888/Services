/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/


using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.Dynamics.Retail.Pos.Services
{
    public static class PrinterHelper
    {
        /// <summary>
        /// List of string sequences that are rendered into images.
        /// </summary>
        public static IEnumerable<string> NonPrintableStringSequences
        {
            get { return new[] { Printer.barCodeRegEx, Printer.logoRegEx, Printer.qrCodeRegEx }; }
        }

        #region internal methods

        /// <summary>
        /// Get Image from image table by image id stored in the print text. 
        /// </summary>
        /// <param name="imageId">The id of the image</param>
        /// <returns>The Retail Image</returns>
        internal static Image GetRetailImage(int imageId)
        {
            var sqlCon = Peripherals.InternalApplication.Settings.Database.Connection;
            Image retailImage = null;
            try
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = sqlCon;
                    command.CommandText = "SELECT PICTUREID, PICTURE FROM dbo.RETAILIMAGES WHERE PICTUREID = @PICTUREID";
                    command.Parameters.Add("@PICTUREID", SqlDbType.Int).Value = imageId;

                    if (sqlCon.State != ConnectionState.Open)
                    {
                        sqlCon.Open();
                    }

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        using (var table = new DataTable())
                        {
                            adapter.Fill(table);

                            if (table.Rows.Count > 0 && table.Rows[0]["PICTURE"] != DBNull.Value)
                            {
                                var imageBytes = (byte[])table.Rows[0]["PICTURE"];
                                using (var memoryStream = new MemoryStream(imageBytes))
                                {
                                    retailImage = Image.FromStream(memoryStream);
                                }
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
            return retailImage;
        }

        /// <summary>
        /// Iterates the input string array looking for the index with the biggest valid string,
        /// Once it is found, the char width is multiplied by character length in hundredths of an inch.
        /// </summary>
        /// <param name="textToMeasure">The input string array</param>
        /// <param name="textFontCharWidth">The width in houndredths of an inch of the text font and size</param>
        /// <returns>The text width in hundredths of an inch</returns>
        /// <remarks> 
        /// The string input may contain lines with non-printable string sequences that may not be considered in 
        /// the calculation, those strings can be e.g. barcode strings that are meant to become images.
        /// </remarks> 
        internal static float GetTextWidthInHundredthsOfAnInch(string[] textToMeasure, float textFontCharWidth)
        {
            var textWidth = 0f;

            if (textToMeasure == null || textToMeasure.All(string.IsNullOrEmpty))
            {
                return 0;
            }

            foreach (var line in textToMeasure)
            {
                var cleanLine = RemoveNonPrintableStringSequences(line);

                var aux = RemoveTextMarkers(cleanLine).Length * textFontCharWidth;
                if (aux > textWidth)
                {
                    textWidth = aux;
                }
            }

            return textWidth;
        }

        /// <summary>
        /// Removes all text markers of the input string
        /// </summary>
        /// <param name="input">The input string</param>
        /// <returns>The input string without any text marker</returns>
        internal static string RemoveTextMarkers(string input)
        {
            var retVal = input;

            foreach (var marker in Printer.AllTextMarkers)
            {
                retVal = retVal.Replace(marker, string.Empty);
            }

            return retVal;
        }

        /// <summary>
        /// Split an input string into an array using the text markers and empty space as delimiters.
        /// It keeps the delimiters in the result array.
        /// </summary>
        /// <param name="line">The input string</param>
        /// <returns>The result array</returns>
        internal static string[] SplitWordsByMarkers(string line)
        {
            var delimiters = Printer.AllTextMarkers.ToArray().Concat(new String[] { " " });
            var regexPattern = BuildRegexPattern(delimiters);
            var splittedWords = Regex.Split(line, regexPattern);

            return splittedWords;
        }

        /// <summary>
        /// Checks if the input string starts with a text marker
        /// </summary>
        /// <param name="input">The input string</param>
        /// <returns>True if the input text starts with a marker; otherwise false</returns>
        internal static bool StartsWithMarker(string input)
        {
            return Printer.AllTextMarkers.Any(input.StartsWith);
        }

        /// <summary>
        /// Iterates the input array, for every environment new line sequence (\r\n),
        //  the current line will be split into another one in sequence.
        /// </summary>
        /// <param name="textToWrap">The input text to be wrapped</param>
        /// <param name="eolMarker">The text marker to be put at the end of each line</param>
        /// <returns>The string array wrapped by enviroment new line</returns>
        internal static string[] WrapLinesByEnviromentNewLine(
            string[] textToWrap,
            string eolMarker)
        {
            if (textToWrap == null) return null;

            var printTextList = new List<string>();
            var lastMarker = string.Empty;

            foreach (var line in textToWrap)
            {
                if (string.IsNullOrEmpty(RemoveTextMarkers(line).Trim()))
                {
                    printTextList.Add(line);
                }
                else
                {
                    lastMarker = GetFirstTextMarker(line);
                    var lineSplited = Regex.Split(RemoveMarkerAtLastPosition(line), Environment.NewLine);
                    foreach (var newLine in lineSplited)
                    {
                        var add = StartsWithMarker(newLine) ? newLine : lastMarker + newLine;
                        printTextList.Add(add + eolMarker);
                        lastMarker = GetLastTextMarker(add);
                    }
                }
            }

            return printTextList.ToArray();
        }

        /// <summary>
        /// Iterates the input array counting each word length, if the word does not fit
        /// on the current line, the line will break. If the word does not fit alone in one
        /// single line, the characters that fit will remain on the current line, and the rest
        /// will be sent to the next one.
        /// <param name="textToWrap">The input text to be wrapped</param>
        /// <param name="lineMaxWidth">The Width criteria to wrap the lines</param>
        /// <param name="eolMarker">The text marker to be put at the end of each line</param>
        /// <returns>The string array wrapped by page width</returns>
        /// </summary>
        /// <remarks> 
        /// The string input may contain lines with non-printable string sequences that may not be considered in 
        /// the calculation, those strings can be e.g. barcode strings that are meant to become images.
        /// </remarks> 
        internal static string[] WrapLinesByPageWidth(
            string[] textToWrap,
            int lineMaxWidth,
            string eolMarker)
        {
            if (textToWrap == null) return null;

            var printTextList = new List<string>();
            var lastMarker = string.Empty;

            foreach (var line in textToWrap)
            {
                if (NonPrintableStringSequences.Any(marker => Regex.Match(line, marker).Success))
                {
                    // If the line contains any invalid string sequence, it must not be wrapped.
                    printTextList.Add(line);
                    continue;
                }

                // If the line contains only spaces or it fits in a single line, just add it.
                if (RemoveTextMarkers(line).Length <= lineMaxWidth)
                {
                    printTextList.Add(line);
                    continue;
                }

                var currentLine = string.Empty;
                var lineWords = RemoveMarkerAtLastPosition(line).Split(' ');

                foreach (var lineWord in lineWords)
                {
                    var currentLineSize = RemoveTextMarkers(currentLine).Length;

                    // The word fits on the current line.
                    if (RemoveTextMarkers(lineWord).Length + currentLineSize <= lineMaxWidth)
                    {
                        currentLine += lineWord + ' ';
                        lastMarker = GetLastTextMarker(currentLine);
                    }
                    // The word doesn't fit on the current line, but it fits on a new line.
                    else if (RemoveTextMarkers(lineWord).Length <= lineMaxWidth)
                    {
                        lastMarker = GetLastTextMarker(currentLine);
                        printTextList.Add(currentLine.TrimEnd() + eolMarker);

                        currentLine = StartsWithMarker(lineWord) ? lineWord + ' ' : lastMarker + lineWord + ' ';
                    }
                    // If the word must be break into two or more lines.
                    else
                    {
                        if (currentLine.Length > 0)
                        {
                            printTextList.Add(currentLine.TrimEnd() + eolMarker);
                        }

                        var firstMarker = StartsWithMarker(lineWord)
                            ? lineWord.Substring(0, Printer.TextMarkerSize)
                            : lastMarker;

                        currentLine = RemoveTextMarkers(lineWord);

                        // Keep breaking word and adding lines.
                        while (currentLine.Length >= lineMaxWidth)
                        {
                            var currentPiece = firstMarker + currentLine.Substring(0, lineMaxWidth);
                            printTextList.Add(currentPiece + eolMarker);
                            currentLine = currentLine.Substring(lineMaxWidth,
                                currentLine.Length - lineMaxWidth);
                        }
                        if (currentLine.Length > 0)
                        {
                            currentLine = firstMarker + currentLine + ' ';
                        }
                    }
                }

                if (!string.IsNullOrEmpty(currentLine))
                {
                    printTextList.Add(currentLine.TrimEnd() + eolMarker);
                }
            }

            return printTextList.ToArray();
        }

        #endregion

        #region private methods

        private static string BuildRegexPattern(IEnumerable<string> delimiters)
        {
            var ret = string.Empty;

            if (delimiters == null || !delimiters.Any())
            {
                return string.Empty;
            }

            foreach (var delimiter in delimiters)
            {
                if (ret.Length > 0)
                {
                    ret += "|";
                }

                ret += @"(\" + delimiter + ")";
            }

            return ret;
        }

        private static bool EndsWithMarker(string text)
        {
            return Printer.AllTextMarkers.Any(text.EndsWith);
        }

        private static string GetFirstTextMarker(string subString)
        {
            var minIndex = subString.Length;
            var retVal = string.Empty;

            foreach (var textMarker in Printer.AllTextMarkers)
            {
                var ind = subString.IndexOf(textMarker);
                if (ind != -1 && ind < minIndex)
                {
                    retVal = textMarker;
                    minIndex = ind;
                }
            }

            return retVal;
        }

        private static string GetLastTextMarker(string subString)
        {
            var maxIndex = -1;
            var retVal = string.Empty;

            foreach (var textMarker in Printer.AllTextMarkers)
            {
                var ind = subString.LastIndexOf(textMarker);
                if (ind != -1 && ind > maxIndex)
                {
                    retVal = textMarker;
                    maxIndex = ind;
                }
            }

            return retVal;
        }

        private static string RemoveMarkerAtLastPosition(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return EndsWithMarker(text) ? text.Substring(0, text.Length - Printer.TextMarkerSize).TrimEnd() : text;
        }

        private static string RemoveNonPrintableStringSequences(string text)
        {
            var retVal = text;

            foreach (var nonPrintableStringSequence in NonPrintableStringSequences)
            {
                var match = Regex.Match(retVal, nonPrintableStringSequence, RegexOptions.Compiled | RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    var matchString = match.ToString();
                    retVal = retVal.Replace(matchString, string.Empty);
                }
            }
            return retVal;
        }

        #endregion
    }
}
