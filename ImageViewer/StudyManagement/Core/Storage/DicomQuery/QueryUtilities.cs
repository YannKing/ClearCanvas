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

using ClearCanvas.Dicom;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ClearCanvas.ImageViewer.StudyManagement.Core.Storage.DicomQuery
{
    internal static class QueryUtilities
    {
        //These are the VRs DICOM says can't be searched on with wildcards,
        //therefore any wildcard characters present in the criteria are literal.
        //这些是DICOM说不能用通配符搜索的VR，因此，标准中出现的任何通配符都是字面的。
        private static readonly string[] WildcardExcludedVRs = { "DA", "TM", "DT", "SL", "SS", "US", "UL", "FL", "FD", "OB", "OW", "UN", "AT", "DS", "IS", "AS", "UI" };

        internal static bool IsWildcardCriterionAllowed(DicomVr vr)
        {
            return WildcardExcludedVRs.All(excludedVr => excludedVr != vr.Name);
        }

        internal static bool IsWildcardCriterion(DicomVr vr, string criterion)
        {
            if (String.IsNullOrEmpty(criterion))
                return false;

            if (!IsWildcardCriterionAllowed(vr))
                return false;

            return criterion.Contains("*") || criterion.Contains("?");
        }

        internal static bool IsLike(string value, string criterion)
        {
            string test = criterion.Replace("*", ".*"); //zero or more characters 零个或多个字符
            test = test.Replace("?", "."); //single character 单个字符
            test = String.Format("^{0}", test); //match at beginning 比赛开始

            //DICOM says if we manage an object having no value, it's considered a match,
            //but it doesn't actually make sense, so we don't do it.
            //DICOM also says matching is case sensitive, but that's just silly.
            
            /*
             * DICOM说如果我们管理一个没有价值的对象，它被认为是匹配，
             * 但它实际上没有意义，所以我们不这样做。
             * DICOM还说匹配是区分大小写的，但这只是愚蠢的。             
             */
            return Regex.IsMatch(value, test, RegexOptions.IgnoreCase);
        }

        internal static bool AreEqual(string value, string criterion)
        {
            //DICOM says if we manage an object having no value, it's considered a match,
            //but it doesn't actually make sense, so we don't do it.
            //DICOM also says matching is case sensitive, but that's just silly.
            /*
             * DICOM说如果我们管理一个没有价值的对象，它被认为是匹配，
             * 但它实际上没有意义，所以我们不这样做。
             * DICOM还说匹配是区分大小写的，但这只是愚蠢的。
             */
            return 0 == string.Compare(value, criterion, StringComparison.InvariantCultureIgnoreCase);
        }

        internal static bool IsMultiValued(string value)
        {
            return !String.IsNullOrEmpty(value) && value.Contains(@"\");
        }
    }
}
