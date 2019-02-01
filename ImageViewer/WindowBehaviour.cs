#region License

// Copyright (c) 2014, ClearCanvas Inc.
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

namespace ClearCanvas.ImageViewer
{
    /// <summary>指定<see cref ="ImageViewerComponent"/>的窗口启动选项。
    /// Specifies window launch options for the <see cref="ImageViewerComponent"/>.
    /// </summary>
    public enum WindowBehaviour
    {
        /// <summary>与<see cref ="Single"/>相同。
        /// Same as <see cref="Single"/> currently.
        /// </summary>
        Auto,

        /// <summary>指定应该启动<see cref ="ImageViewerComponent"/>在单个（例如活动的）桌面窗口中。
        /// Specifies that the <see cref="ImageViewerComponent"/> should be launched in a single (e.g. active) desktop window.
        /// </summary>
        Single,

        /// <summary>指定应该启动<see cref ="ImageViewerComponent"/>在单独的桌面窗口中。
        /// Specifies that the <see cref="ImageViewerComponent"/> should be launched in a separate desktop window.
        /// </summary>
        Separate
    }
}