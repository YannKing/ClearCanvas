#region License

// Copyright (c) 2013, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This file is part of the ClearCanvas RIS/PACS open source project.
//
// The ClearCanvas RIS/PACS open source project is free software: you can
// redistribute it and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// The ClearCanvas RIS/PACS open source project is distributed in the hope that it
// will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General
// Public License for more details.
//
// You should have received a copy of the GNU General Public License along with
// the ClearCanvas RIS/PACS open source project.  If not, see
// <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Globalization;

namespace ClearCanvas.Dicom.Utilities
{
    /// <summary>DateParser类根据Dicom分析正确格式化的日期。
    /// The DateParser class parses dates that are formatted correctly according to Dicom.
    /// 
    /// 我们使用TryParseExact函数来解析日期，因为它效率更高比ParseExact，因为它不会抛出异常。
    /// We use the TryParseExact function to parse the dates because it is far more efficient
    /// than ParseExact since it does not throw exceptions.
    /// 
    /// 请参阅http：blogs.msdn.com/ianhu/archive/2005/12/19/505702.aspx以获取良好的资料比较不同的Parse / Convert方法。
    /// See http://blogs.msdn.com/ianhu/archive/2005/12/19/505702.aspx for a good profile comparision of the different Parse/Convert methods.
    /// </summary>
    public static class DateParser
    {
        public const string DicomDateFormat = "yyyyMMdd";

        /// <summary>尝试根据接受的Dicom格式准确地解析日期字符串。如果格式无效，将*不*抛出异常。
        /// Attempts to parse the date string exactly, according to accepted Dicom format(s).
        /// Will *not* throw an exception if the format is invalid.
        /// </summary>
        /// <param name="dicomDate">dicom日期字符串
        /// the dicom date string</param>
        /// <returns>可以为空的DateTime
        /// a nullable DateTime</returns>
        public static DateTime? Parse(string dicomDate)
        {
            DateTime date;
            if (!Parse(dicomDate, out date))
                return null;

            return date;
        }

        /// <summary>尝试根据接受的Dicom格式准确地解析日期字符串。如果格式无效，将*不*抛出异常。
        /// Attempts to parse the date string exactly, according to accepted Dicom format(s).
        /// Will *not* throw an exception if the format is invalid.
        /// </summary>
        /// <param name="dicomDate">dicom日期字符串
        /// the dicom date string</param>
        /// <param name="date">将日期作为DateTime对象返回
        /// returns the date as a DateTime object</param>
        /// <returns>成功时为真，否则为假
        /// true on success, false otherwise</returns>
        public static bool Parse(string dicomDate, out DateTime date)
        {
            // This method is used in DicomAttribute Get/TryGet,
            // which allow leading/trailing spaces in the string
            // They are considered valid DICOM date/time.

            /*此方法用于DicomAttribute Get / TryGet,
             * 允许字符串中的前导/尾随空格
             * 它们被视为有效的DICOM日期/时间。 
             **/

            if (dicomDate != null)
                dicomDate = dicomDate.Trim();

            return DateTime.TryParseExact(dicomDate, DicomDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }

        /// <summary>将DateTime对象转换为DA字符串
        /// Convert a DateTime object into a DA string
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns>The DICOM formatted string</returns>
        public static string ToDicomString(DateTime datetime)
        {
            return datetime.ToString(DicomDateFormat, CultureInfo.InvariantCulture);
        }
    }
}