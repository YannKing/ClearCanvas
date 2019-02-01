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
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace ClearCanvas.Desktop.View.WinForms
{
    /// <summary>This class handles displaying and dismissing a splash screen in an application.
    /// 此类处理在应用程序中显示和关闭启动画面。</summary>
    public class SplashScreenManager
    {
        /// <summary>启动画面。</summary>
        private static SplashScreen _splashScreen = null;

        //需要一个单独的线程来显示启动画面，以便主线程可以继续加载应用程序
        private static Thread _displayThread = null; //线程共享资源
        private static Mutex mutex = new Mutex();

        // Two timers are used to ensure that various forms (the splash screen and _formToActivate) are
        // updated and accessed from the threads they were created in (to avoid cross-threading exceptions)
        //两个计时器用于确保各种形式（启动画面和formToActivate）从创建它们的线程更新和访问（以避免跨线程异常）
        private static System.Windows.Forms.Timer _displayTimer = null;
        private static System.Windows.Forms.Timer _dismissTimer = null;

        // Members used in updating the contents of the splash screen (thread-shared resources)
        //用于更新启动画面内容的成员（线程共享资源）
        private static string _status = string.Empty;
        private static string _licenseText = string.Empty;
        private static List<Assembly> _assemblies = new List<Assembly>();

        private static bool _updateLicenseText = false;

        // Members used in closing the splash sceen (thread-shared resources)
        //用于关闭启动画面的成员（线程共享资源）
        private static bool _closing = false;
        private static int _closeStartTime = 0;
        private static Form _formToActivate = null;
        private static bool _stopDisplayTimer = false;
        private static bool _stopDismissTimer = false;

        // 常量
        private const double OpacityDelta = 0.20; // 20%
        private const int CloseDelay = 20;   // 0.02 seconds
        private const int TimerInterval = 50;  // 50 ms

        /// <summary>开始一个新的后台线程，立即创建和显示新的启动画面。
        /// 该线程还处理淡入淡出屏幕的淡入淡出和更新其内容。
        /// 
        /// Begins a new background thread that immediately creates and displays and new splash screen.
        /// The thread also handles fading the splash screen in and out, and updating its contents.
        /// </summary>
        public static void DisplaySplashScreen()
        {
            System.Windows.Forms.Application.EnableVisualStyles();

            // Shared resource access follows 
            //随后是共享资源访问
            mutex.WaitOne();

            // Make sure it's only launched once
            //确保它只发布一次
            if (_displayThread != null)
                return;

            mutex.ReleaseMutex();

            _displayThread = new Thread(new ThreadStart(CreateSplashScreen));
            _displayThread.IsBackground = true;
            _displayThread.SetApartmentState(ApartmentState.STA);
            _displayThread.Start();
        }

        /// <summary>开始关闭闪屏的过程，这通常涉及轻微延迟然后淡出。
        /// Begins the process of closing the splash screen, which typically involves a slight 
        /// delay followed by a fade out.
        /// </summary>
        /// <param name="formToActivate">The new form to activate once the splash screen is closed.[关闭启动画面后激活的新表单。]</param>
        public static bool DismissSplashScreen(Form formToActivate)
        {
            // Shared resource access follows //随后是共享资源访问
            mutex.WaitOne();

            // Make sure it's already launched and not being dismissed
            //确保它已经启动并且没有被解雇
            if (_displayThread == null || _closing)
            {
                mutex.ReleaseMutex();
                return false;
            }

            // Flag the splash screen for closing
            //标记启动画面以关闭
            _closing = true;
            _closeStartTime = Environment.TickCount;

            // Store the form until we can activate it properly in the dismiss timer thread
            //存储表单，直到我们可以在关闭计时器线程中正确激活它
            _formToActivate = formToActivate;

            // Flag the license info for updating //标记许可证信息以进行更新
            //_updateLicenseInfo = true;

            mutex.ReleaseMutex();

            // Initialize the dismiss timer //初始化关闭计时器
            _dismissTimer = new System.Windows.Forms.Timer();
            _dismissTimer.Tick += new EventHandler(OnDismissTimer);
            _dismissTimer.Interval = 50;

            _dismissTimer.Start();

            return true;
        }

        /// <summary>设置“状态”文本以在初始屏幕中显示。通常使用此文本表示某种加载进度。
        /// Set the 'status' text to display in the splash screen.  This text is generally used 
        /// to indicate some sort of loading progress.
        /// </summary>
        /// <param name="status">The new 'status' text to display on the splash screen.
        /// 要在初始屏幕上显示的新“状态”文本。
        /// </param>
        public static void SetStatus(string status)
        {
            // Shared resource access follows //随后是共享资源访问
            mutex.WaitOne();

            // Store the status temporarily until we can set it properly in the timer thread
            //暂时存储状态，直到我们可以在计时器线程中正确设置它
            _status = status;

            mutex.ReleaseMutex();
        }

        /// <summary>设置许可证文本以在初始屏幕中显示。
        /// Set the license text to display in the splash screen.
        /// </summary>
        /// <param name="licenseText">The new license text to display on the splash screen.
        /// 要在启动屏幕上显示的新许可证文本。</param>
        public static void SetLicenseText(string licenseText)
        {
            // Shared resource access follows  
            //随后是共享资源访问
            mutex.WaitOne();

            // Store the license text temporarily until we can set it properly in the timer thread
            //暂时存储许可证文本，直到我们可以在计时器线程中正确设置它
            _licenseText = licenseText;
            _updateLicenseText = true;

            mutex.ReleaseMutex();
        }

        public static void AddAssemblyIcon(Assembly pluginAssembly)
        {
            // Shared resource access follows
            //随后是共享资源访问
            mutex.WaitOne();

            // Store the assembly temporarily until we can add it properly in the timer thread
            //临时存储组件，直到我们可以在计时器线程中正确添加它
            if (pluginAssembly != null)
                _assemblies.Add(pluginAssembly);

            mutex.ReleaseMutex();
        }

        /// <summary>显示线程的入口点，用于创建初始屏幕窗体并开始播放计时器处理淡入淡出和更新它。
        /// 使用计时器使通过创建它的同一个线程访问的启动屏幕表单。这样可以避免跨线程表单异常。 
        /// 
        /// An entry point for the display thread that creates a splash screen form and begins a 
        /// timer to handle fading it in and out and updating it.  A timer is used so that the 
        /// splash screen form accessed through the same thread it was created in.  This avoids 
        /// cross-threading form exceptions.
        ///</summary>
        private static void CreateSplashScreen()
        {
            // Shared resource access follows
            mutex.WaitOne();

            // Create the splash screen
            //创建启动画面
            _splashScreen = new SplashScreen();

            // Reset the 'stop timer' flags
            //重置'停止计时器'标志
            _stopDismissTimer = false;
            _stopDisplayTimer = false;

            mutex.ReleaseMutex();

            // Initialize the display timer 
            //初始化显示计时器
            _displayTimer = new System.Windows.Forms.Timer();
            _displayTimer.Tick += new EventHandler(OnDisplayTimer);
            _displayTimer.Interval = TimerInterval;

            _displayTimer.Start();

            System.Windows.Forms.Application.Run(_splashScreen);
        }

        /// <summary>处理更新启动画面内容的显示计时器事件处理程序，淡入淡出，在被解雇时清理它，并发出解雇信号计时器结束。
        /// The display timer event handler that handles updating the contents of the splash screen, 
        /// fading it in and out, cleaning it up when it's been dismissed, and signalling the dismiss
        /// timer to end.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnDisplayTimer(object sender, EventArgs e)
        {
            // Shared resource access follows
            mutex.WaitOne();

            if (_splashScreen != null)
            {
                // Send the splash screen to the back if there's a licensing error
                //if (Sentinelle.Aegis.Model.AegisSessionManager.LicenseError)
                // _splashScreen.SendToBack();

                // Update the status
                _splashScreen.UpdateStatusText(_status);

                // Update the license
                if (_updateLicenseText)
                {
                    _splashScreen.UpdateLicenseText(_licenseText);
                    _updateLicenseText = false;
                }

                // Update the icons
                while (_assemblies.Count > 0)
                {
                    _splashScreen.AddAssemblyIcon(_assemblies[0]);
                    _assemblies.RemoveAt(0);
                }

                if (_closing)
                {
                    // Wait a fixed amount of time before actually closing the splash screen
                    //在实际关闭启动画面之前等待一段固定的时间
                    int timeElapsedSinceClose = Environment.TickCount - _closeStartTime;
                    if (timeElapsedSinceClose >= CloseDelay)
                    {
                        // Fade out, if necessary
                        //如有必要，淡出
                        if (_splashScreen.Opacity > 0)
                            _splashScreen.UpdateOpacity(_splashScreen.Opacity - OpacityDelta);
                        else
                        {
                            // We've faded out - flag the dismiss timer to stop
                            //我们已经淡出 - 标志着解雇计时器停止
                            _stopDismissTimer = true;
                        }
                    }
                }
                else
                {
                    // Fade in, if necessary
                    //如有必要，请淡入
                    if (_splashScreen.Opacity < 1)
                        _splashScreen.UpdateOpacity(_splashScreen.Opacity + OpacityDelta);
                }
            }

            // Check to see if it's time to stop the display timer
            //检查是否有时间停止显示计时器
            if (_stopDisplayTimer)
            {
                // Stop the timer
                _displayTimer.Stop();
                _displayTimer = null;

                // Close the splash screen
                //关闭启动画面
                if (_splashScreen != null)
                {
                    _splashScreen.Close();
                    _splashScreen.Dispose();
                    _splashScreen = null;
                }

                // Destroy the thread
                //销毁线程
                _displayThread = null;
            }

            mutex.ReleaseMutex();
        }

        /// <summary>dismiss计时器事件处理程序，用于处理激活要获得焦点的表单一旦启动屏幕关闭并发出显示计时器结束信号。
        /// 
        /// The dismiss timer event handler that handles activating the form that's to take focus 
        /// once the splash screen is closed and signalling the display timer to end.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnDismissTimer(object sender, EventArgs e)
        {
            // Shared resource access follows
            mutex.WaitOne();

            // Check to see if it's time to stop the dismiss timer
            //检查是否有时间停止解雇计时器
            if (_stopDismissTimer)
            {
                // Stop the timer
                _dismissTimer.Stop();
                _dismissTimer = null;

                // Once the splash screen has been closed, activate the new form (if any) and kill the dismiss timer
                //关闭启动画面后，激活新窗体（如果有）并终止关闭计时器
                if (_formToActivate != null)
                    _formToActivate.Activate();

                // Flag the display timer to stop
                //将显示计时器标记为停止
                _stopDisplayTimer = true;
            }

            mutex.ReleaseMutex();
        }
    }
}
