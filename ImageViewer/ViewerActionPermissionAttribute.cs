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

using ClearCanvas.Common.Specifications;
using ClearCanvas.Desktop.Actions;

namespace ClearCanvas.ImageViewer
{
    /// <summary>将权限令牌与特定于查看者的操作相关联。
    /// Associates authority tokens with a viewer-specific action.
    /// </summary>
    /// <remarks>
    /// <para>指定的所有标记将与<see cref ="AuthorityTokens.ViewerVisible"/>进行AND运算，这是一个全局标记旨在限制对所有特定于查看器的功能的访问。
    /// All tokens specified will be ANDed with <see cref="AuthorityTokens.ViewerVisible"/>, which is a global token
    /// intended to limit access to all viewer-specific functionality.
    /// </para>
    /// <para>此属性通过<see cref="Action.SetPermissibility(ISpecification)"/>方法设置操作允许性。
    /// 如果将数组中的多个权限标记提供给属性的单个实例，那么将使用AND组合这些标记。
    /// 如果指定了此属性的多个实例，组合了与每个实例关联的标记使用OR逻辑。
    /// 这允许基于复杂布尔值构造权限规范的可能性权威令牌的组合。
    /// 
    /// This attribute sets the action permissibility via the <see cref="Action.SetPermissibility(ISpecification)"/> method.
    /// If multiple authority tokens are supplied in an array to a single instance of the attribute, those tokens will be combined using AND.  If
    /// multiple instances of this attribute are specified, the tokens associated with each instance are combined
    /// using OR logic.  This allows for the possibility of constructing a permission specification based on a complex boolean
    /// combination of authority tokens.
    /// </para>
    /// </remarks>
    public class ViewerActionPermissionAttribute : ActionPermissionAttribute
    {
        /// <summary>构造函数 - 指定的权限令牌将与指定的操作ID相关联。
        /// Constructor - the specified authority token will be associated with the specified action ID.
        /// </summary>
        public ViewerActionPermissionAttribute(string actionID, string authorityToken) : this(actionID, new string[] { authorityToken })
        {
        }

        /// <summary>构造函数 - 所有指定的标记将使用AND组合并与指定的操作ID相关联。
        /// Constructor - all of the specified tokens will combined using AND and associated with the specified action ID.
        /// </summary>
        public ViewerActionPermissionAttribute(string actionID, params string[] authorityTokens): base(actionID, CreateViewerTokens(authorityTokens))
        {
        }

        private static string[] CreateViewerTokens(string[] authorityTokens)
        {
            authorityTokens = authorityTokens ?? new string[0];
            string[] viewerTokens = new string[authorityTokens.Length + 1];

            viewerTokens[0] = AuthorityTokens.ViewerVisible;

            for (int i = 0; i < authorityTokens.Length; ++i)
                viewerTokens[i + 1] = authorityTokens[i];

            return viewerTokens;
        }
    }
}
