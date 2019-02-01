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

using ClearCanvas.ImageViewer.Mathematics;
using ClearCanvas.ImageViewer.Volumes;

namespace ClearCanvas.ImageViewer.Volume.Mpr
{
	/// <summary>
	/// Defines the parameters for a particular slicing of a <see cref="Volume"/> object (i.e. plane boundaries, orientation, thickness, etc.)
	/// </summary>
	public interface IVolumeSlicerParams
	{
		/// <summary>
		/// Gets or sets a string describing the slicing parameters.
		/// </summary>
		string Description { get; set; }

        /// <summary>
        /// Gets or sets the rotation applied to the slicing plane as a 4x4 affine transform <see cref="Matrix"/>.
        /// 获取或设置作为4x4仿射变换应用于切片平面的旋转
        /// </summary>
        /// <remarks>
        /// <para>Implementations may choose to return a new object instance each time to ensure immutability if <see cref="IsReadOnly"/> is true.</para>
        /// </remarks>
        Matrix SlicingPlaneRotation { get; set; }

        /// <summary>
        /// Gets or sets the point, in patient coordinates, though which the slicing should begin.
        /// 获取或设置以患者坐标表示的点，尽管切片应该从该点开始。
        /// </summary>
        /// <remarks>
        /// <para>Implementations may choose to return a new object instance each time to ensure immutability if <see cref="IsReadOnly"/> is true.</para>
        /// </remarks>
        Vector3D SliceThroughPointPatient { get; set; }

        /// <summary>
        /// 获取或设置切片时使用的插值模式 <see cref="Volume"/>.
        /// </summary>
        VolumeSlicerInterpolationMode InterpolationMode { get; set; }

        /// <summary>
        /// Gets or sets the physical width of the slicing plane.
        ///获取或设置切片平面的物理宽度。
        /// </summary>
        float SliceExtentXMillimeters { get; set; }

        /// <summary>
        /// Gets or sets the physical height of the slicing plane.
        /// 获取或设置切片平面的物理高度。
        /// </summary>
        float SliceExtentYMillimeters { get; set; }

        /// <summary>
        ///获取或设置连续切片之间的物理间距（毫米）。
        /// </summary>
        float SliceSpacing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not these parameters are immutable.
        /// 获取或设置一个值，该值指示这些参数是否不可变。
        /// </summary>
        bool IsReadOnly { get; }
	}

	/// <summary>
	/// Enumerated values for specifying the interpolation mode used in slicing a <see cref="Volume"/>.
	/// </summary>
	public enum VolumeSlicerInterpolationMode
	{
		NearestNeighbor,
		Linear,
		Cubic
	}
}