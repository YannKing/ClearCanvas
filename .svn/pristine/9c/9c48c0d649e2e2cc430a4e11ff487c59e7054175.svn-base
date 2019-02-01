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
using System.IO;

namespace ClearCanvas.Common.Utilities
{
    /// <summary>
    /// A helper class providing methods for processing files.
    /// </summary>
    public class FileProcessor
    {
        /// <summary>委托供<see cref =" FileProcessor.Process（string,string,ProcessFile,bool）"/>方法使用。</summary>
        /// <param name="filePath">要处理的文件的路径。</param>
        public delegate void ProcessFile(string filePath);

        /// <summary>
        /// Delegate for use by the <see cref="FileProcessor.Process(string,string,ProcessFileCancellable,bool)"/> method.
        /// </summary>
        /// <param name="filePath">The path to the file to be processed.</param>
        /// <param name="cancel">Gets whether or not the entire processing operation should be cancelled.</param>
        public delegate void ProcessFileCancellable(string filePath, out bool cancel);

        /// <summary>处理给定<paramref name ="path"/>上与指定的<paramref name ="searchPattern"/>匹配的所有文件。</summary>
        /// <remarks>输入<paramref name ="path"/>可以是文件或目录。</remarks>
        /// <param name="path">要处理的文件的根路径。The root path to the file(s) to be processed.</param>
        /// <param name="searchPattern">
        /// 要使用的搜索模式。值<b> null </b>或<b>""</b>表示所有文件都匹配。
        /// The search pattern to be used.  A value of <b>null</b> or <b>""</b> indicates that all files are a match.</param>
        /// <param name="proc">
        /// 调用每个匹配文件的方法。
        /// The method to call for each matching file.</param>
        /// <param name="recursive">
        /// 是否应该递归搜索<paramref name ="path"/>。
        /// Whether or not the <paramref name="path"/> should be searched recursively.</param>
        public static void Process(string path, string searchPattern, ProcessFile proc, bool recursive)
        {
            Process(path, searchPattern, delegate (string filePath, out bool cancel) { cancel = false; proc(filePath); }, recursive);
        }

        /// <summary>处理给定<paramref name ="path"/>上与指定的<paramref name ="searchPattern"/>匹配的所有文件。
        /// Processes all files on the given <paramref name="path"/> matching the specified <paramref name="searchPattern"/>.
        /// </summary>
        /// <remarks>输入<paramref name ="path"/>可以是文件或目录。
        /// The input <paramref name="path"/> can be a file or a directory.
        /// </remarks>
        /// <param name="path">要处理的文件的根路径。</param>
        /// <param name="searchPattern">要使用的搜索模式。值<b> null </b>或<b>“”</b>表示所有文件都匹配。</param>
        /// <param name="proc">调用每个匹配文件的方法。</param>
        /// <param name="recursive">是否应该递归搜索<paramref name ="path"/>。</param>
        /// <returns>如果该过程被取消，则为真;否则就错了</returns>
        public static bool Process(string path, string searchPattern, ProcessFileCancellable proc, bool recursive)
        {
            Platform.CheckForNullReference(path, "path");
            Platform.CheckForEmptyString(path, "path");
            Platform.CheckForNullReference(proc, "proc");

            bool cancel;

            // If the path is a directory, process its contents
            // 如果路径是目录，则处理其内容
            if (Directory.Exists(path))
            {
                ProcessDirectory(path, searchPattern, proc, recursive, out cancel);
            }
            // If the path is a file, just process the file
            // 如果路径是文件，则只处理文件
            else if (File.Exists(path))
            {
                proc(path, out cancel);
            }
            else
            {
                throw new FileNotFoundException(String.Format(SR.ExceptionPathDoesNotExist, path));
            }

            // TODO: from API perspective, more intuitive if this returns false when cancelled. Leave it as is for now.
            // TODO: 从API角度来看，如果在取消时返回false，则更直观。暂时保留原样。
            return cancel;
        }

        private static void ProcessDirectory(string path, string searchPattern, ProcessFileCancellable proc, bool recursive, out bool cancel)
        {
            cancel = false;

            // Process files in this directory 处理此目录中的文件
            string[] fileList;
            GetFiles(path, searchPattern, out fileList);

            if (fileList != null)
            {
                for (int i = 0; i < fileList.Length; i++)
                {
                    proc(fileList[i], out cancel);
                    if (cancel)
                        return;
                    fileList[i] = null;
                }
            }

            if (!recursive) return;

            // If recursive, then descend into lower directories and process those as well
            // 如果是递归的，那么下降到较低的目录并处理它们
            TraverseDirectories(path, searchPattern, proc, out cancel);
        }

        private static void GetFiles(string path, string searchPattern, out string[] fileList)
        {
            fileList = null;

            try
            {
                fileList = string.IsNullOrEmpty(searchPattern)
                    ? Directory.GetFiles(path)
                    : Directory.GetFiles(path, searchPattern);
            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Warn, e);
                throw;
            }
        }

        /// <summary>遍历目录</summary>
        /// <param name="path"></param>
        /// <param name="searchPattern"></param>
        /// <param name="proc"></param>
        /// <param name="cancel"></param>
        private static void TraverseDirectories(string path, string searchPattern, ProcessFileCancellable proc, out bool cancel)
        {
            string[] dirList;

            cancel = false;

            try
            {
                dirList = Directory.GetDirectories(path);
            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Warn, e);
                throw;
            }

            for (int i = 0; i < dirList.Length; i++)
            {
                ProcessDirectory(dirList[i], searchPattern, proc, true, out cancel);
                if (cancel)
                    break;
                dirList[i] = null;
            }


        }
    }
}
