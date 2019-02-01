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
using System.Threading;

namespace ClearCanvas.Common.Utilities
{
    /// <summary>由<see cref ="Timer"/>对象使用的委托。
    /// A delegate for use by a <see cref="Timer"/> object.
    /// </summary>
    public delegate void TimerDelegate(object state);

    /// <summary>实现一个简单的计时器类，它将编组委托处理回到其上的线程此对象已分配（通常是主UI线程）。
    /// Implements a simple timer class that handles marshalling delegates back to the thread on which
    /// this object was allocated (usually the main UI thread).
    /// </summary>
    /// 
    /// <remarks>
    /// <B>必须</B>从UI线程中实例化此类，否则为异常可以在构造时抛出（除非线程有自定义<see cref ="SynchronizationContext"/>）。
    /// 这个类依赖于<see cref ="SynchronizationContext.Current"/>非空，以便进行编组。
    /// 此外，此类非常简单，可能不如其他计时器类准确。
    /// 
    /// This class <B>must</B> be instantiated from within a UI thread, otherwise an exception
    /// could be thrown upon construction (unless the thread has a custom <see cref="SynchronizationContext"/>).  
    /// This class relies on <see cref="SynchronizationContext.Current"/> being non-null in order to do the marshalling.
    /// Also, this class is very simple and may not be as accurate as other timer classes.
    /// </remarks>
    public sealed class Timer : IDisposable
    {
        private readonly SynchronizationContext _synchronizationContext;
        private readonly object _stateObject;
        private readonly TimerDelegate _elapsedDelegate;

        private System.Threading.Timer _timer;
        private volatile int _intervalMilliseconds;
        private int _processing;

        /// <summary>构造函数。</summary>
        /// <param name="elapsedDelegate">要在计时器上执行的委托。
        /// The delegate to execute on a timer.</param>
        public Timer(TimerDelegate elapsedDelegate) : this(elapsedDelegate, null)
        {
        }

        /// <summary>构造函数。</summary>
        /// <param name="elapsedDelegate">要在计时器上执行的委托。
        /// The delegate to execute on a timer.</param>
        /// <param name="stateObject">用户定义的状态对象。
        /// A user defined state object.</param>
        public Timer(TimerDelegate elapsedDelegate, object stateObject) : this(elapsedDelegate, stateObject, 1000)
        {
        }

        /// <summary>构造函数。</summary>
        /// <param name="elapsedDelegate">要在计时器上执行的委托。
        /// The delegate to execute on a timer.</param>
        /// <param name="stateObject">用户定义的状态对象。
        /// A user defined state object.</param>
        /// <param name="interval">定时器间隔。
        /// The timer interval.</param>
        public Timer(TimerDelegate elapsedDelegate, object stateObject, TimeSpan interval) : this(elapsedDelegate, stateObject, (int)interval.TotalMilliseconds)
        {
        }

        /// <summary>构造函数。</summary>
        /// <param name="elapsedDelegate">要在计时器上执行的委托。
        /// The delegate to execute on a timer.</param>
        /// <param name="stateObject">用户定义的状态对象。
        /// A user defined state object.</param>
        /// <param name="intervalMilliseconds">等待的时间，以毫秒为单位。
        /// The time to wait in milliseconds.</param>
        public Timer(TimerDelegate elapsedDelegate, object stateObject, int intervalMilliseconds)
        {
            _synchronizationContext = SynchronizationContext.Current;

            Platform.CheckForNullReference(_synchronizationContext, "SynchronizationContext.Current");
            Platform.CheckForNullReference(elapsedDelegate, "elapsedDelegate");

            _stateObject = stateObject;
            _elapsedDelegate = elapsedDelegate;

            _intervalMilliseconds = intervalMilliseconds;
        }

        /// <summary>获取计时器当前是否正在运行。
        /// Gets whether or not the timer is currently running.
        /// </summary>
        public bool Enabled
        {
            get { return _timer != null; }
        }

        /// <summary>设置计时器间隔（以毫秒为单位）。
        /// Sets the timer interval in milliseconds.
        /// </summary>
        /// <remarks>默认值为1000毫秒或1秒。
        /// The default value is 1000 milliseconds, or 1 second.
        /// </remarks>
        public int IntervalMilliseconds
        {
            get { return _intervalMilliseconds; }
            set { _intervalMilliseconds = value; }
        }

        /// <summary>启动计时器。
        /// Starts the timer.
        /// </summary>
        public void Start()
        {
            if (_timer == null)
                _timer = new System.Threading.Timer(OnTimer, _stateObject, _intervalMilliseconds, _intervalMilliseconds);
        }

        /// <summary>停止计时器。
        /// Stops the timer.
        /// </summary>
        public void Stop()
        {
            if (_timer == null) return;
            _timer.Dispose();
            _timer = null;
        }

        #region IDisposable Members

        /// <summary>执行<see cref ="IDisposable"/>模式。
        /// Implementation of the <see cref="IDisposable"/> pattern.
        /// </summary>
        public void Dispose()
        {
            try
            {
                Stop();
            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Error, e);
            }
        }

        #endregion 
        private void OnElapsed(object nothing)
        {
            try
            {
                if (Enabled)
                    _elapsedDelegate(_stateObject);
            }
            finally
            {
                //The next one can be posted.下一个可以发布。
                Interlocked.Exchange(ref _processing, 0);
            }
        }

        private void OnTimer(object nothing)
        {
            //A couple things:
            //1. System.Threading.Timer allows re-entrancy, so you can actually get
            //   multiple callbacks executing at once on different thread pool threads.
            //2. Depending on how long the "elapsed" callback takes, things can get pretty
            //   out of hand with the message pump filling up, so we actually delay
            //   posting again until the current user callback is done.

            /*
             * 几件事：
             * 1. System.Threading.Timer允许重入，所以你可以实际获得在不同的线程池线程上一次执行多个回调。
             * 2.根据“已过去”回调的持续时间，事情可能变得很漂亮,随着消息泵填满而失控，所以我们实际上是在拖延再次发布，直到当前用户回调完成。
             * */

            if (0 == Interlocked.Exchange(ref _processing, 1))
                _synchronizationContext.Post(OnElapsed, null);
        }
    }
}
