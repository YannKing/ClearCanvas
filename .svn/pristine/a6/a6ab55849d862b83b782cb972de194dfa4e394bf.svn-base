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

using ClearCanvas.Common.Utilities;
using NewLife.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
//using System.Security.Cryptography;
using System.Text;

namespace ClearCanvas.Common
{
    /// <summary>封装用于从磁盘加载插件和插件元数据的逻辑。</summary>
    /// <remarks>此类由框架在内部使用，不用于应用程序。</remarks>
    internal class PluginLoader
    {
        private class LoadPluginResult
        {
            public LoadPluginResult(bool isPlugin, Assembly assembly, PluginInfo pluginInfo)
            {
                IsPlugin = isPlugin;
                Assembly = assembly;
                PluginInfo = pluginInfo;
            }

            public readonly bool IsPlugin;
            public readonly Assembly Assembly;
            public readonly PluginInfo PluginInfo;
        }

        private readonly string _primaryCacheFile;
        private readonly string[] _alternateCacheFiles;
        private readonly string _pluginDir;

        internal PluginLoader(string pluginDir, string primaryCacheFile, string[] alternateCacheFiles)
        {
            _pluginDir = pluginDir;
            _primaryCacheFile = primaryCacheFile;
            _alternateCacheFiles = alternateCacheFiles;
        }

        /// <summary>插件程序集加载到内存时发生。</summary>
        internal event EventHandler<PluginLoadedEventArgs> PluginLoaded;

        /// <summary>加载插件元数据，而不必加载插件程序集。</summary>
        /// <remarks>调用此方法可能会将插件程序集加载到内存中，但前提是无法找到缓存的元数据。</remarks>
        internal List<PluginInfo> LoadPluginInfo()
        {
            // build list of candidate plugin files
            // Note: the reason for the "group by" operation is that some non-plugin files
            // may have the same file name (located in different sub-folders) - e.g. localization satellite assemblies in ASP.NET,
            // and we need to eliminate duplicates prior to building the pluginPathLookup dictionary below.
            //构建候选插件文件列表
            //注意：“分组依据”操作的原因是一些非插件文件可以具有相同的文件名（位于不同的子文件夹中）
            //- 例如ASP.NET中的本地化附属程序集，我们需要在构建下面的pluginPathLookup字典之前消除重复项。
            var pluginCandidates = (from p in ListPluginCandidateFiles()
                                    group p by Path.GetFileNameWithoutExtension(p)
                                    into g
                                    select g.First())
             .ToList();

            // debug dump of candidates 调试候选人的转储 
            foreach (var pluginCandidate in pluginCandidates.OrderBy(n => n))
            {
                Platform.Log(LogLevel.Debug, "Found plugin candidate file: {0}", pluginCandidate);
                XTrace.WriteLine($"找到插件候选文件:{pluginCandidate}");
            }

            // establish assembly load resolver 建立装配负载解析器
            var pluginPathLookup = BuildAssemblyMap(pluginCandidates);
            AssemblyRef.SetResolver(name => LoadPlugin(pluginPathLookup[name], false).Assembly);

            // see if we can load the meta-data from a cache 看看我们是否可以从缓存中加载元数据
            var checkSum = ComputeCheckSum(pluginCandidates);
            List<PluginInfo> pluginInfos;
            if (!TryLoadCachedMetadata(new[] { _primaryCacheFile }.Concat(_alternateCacheFiles), checkSum, out pluginInfos))
            {
                // No cached meta-data, so we need to load the plugins and build the meta-data from scratch.
                // 没有缓存的元数据，因此我们需要加载插件并从头开始构建元数据。
                LoadPluginFiles(pluginCandidates, true, out pluginInfos);
                SaveCachedMetadata(pluginInfos, checkSum);
            }

            return pluginInfos;
        }
        /// <summary>
        /// 列表插件候选文件
        /// Yann 2018年12月24日 11:40:50 </summary>
        /// <returns></returns>
        private List<string> ListPluginCandidateFiles()
        {
            var plugins = new List<string>();
            FileProcessor.Process(_pluginDir, "*.dll", plugins.Add, true);
            return plugins;
        }

        private void LoadPluginFiles(IEnumerable<string> pluginCandidates, bool processMetadata, out List<PluginInfo> pluginInfos)
        {
            EventsHelper.Fire(PluginLoaded, this, new PluginLoadedEventArgs(SR.MessageFindingPlugins, null));

            var loadResults = pluginCandidates.Select(pc => LoadPlugin(pc, processMetadata)).ToList();

            pluginInfos = processMetadata ? loadResults.Where(r => r.IsPlugin).Select(r => r.PluginInfo).ToList() : null;
        }

        private static bool TryLoadCachedMetadata(IEnumerable<string> cacheFilePaths, byte[] checkSum, out List<PluginInfo> pluginInfos)
        {
            cacheFilePaths = cacheFilePaths.Where(File.Exists).ToList();
            if (cacheFilePaths.Any())
            {
                try
                {
                    var pluginInfoCache = PluginInfoCache.Read(cacheFilePaths.First());
                    if (pluginInfoCache.CheckSum.SequenceEqual(checkSum))
                    {
                        pluginInfos = pluginInfoCache.Plugins;
                        return true;
                    }
                }
                catch (Exception)
                {
                    // not a big deal, it just means the cache isn't accessible right now
                    // (maybe another app domain or process is writing to it?)
                    // and we need to build meta-data from the binaries
                    Platform.Log(LogLevel.Debug, "Failed to read plugin metadata cache.");

                    //这不是什么大不了的事，只是意味着现在无法访问缓存
                    //（也许是另一个应用程序域或进程写入它？）
                    //我们需要从二进制文件构建元数据
                    XTrace.WriteLine("无法读取插件元数据缓存。");
                }
            }
            pluginInfos = null;
            return false;
        }

        private void SaveCachedMetadata(List<PluginInfo> pluginInfos, byte[] checkSum)
        {
            try
            {
                var pluginInfoCache = new PluginInfoCache(pluginInfos, checkSum);
                pluginInfoCache.Write(_primaryCacheFile);
            }
            catch (Exception)
            {
                // not a big deal, it just means the cache won't be updated this time around
                Platform.Log(LogLevel.Debug, "Failed to write plugin metadata cache.");

                //这不是什么大不了的事，只是意味着这次不会更新缓存
                XTrace.WriteLine("无法编写插件元数据缓存。");
            }
        }

        private LoadPluginResult LoadPlugin(string path, bool processMetadata)
        {
            try
            {
                // load assembly 装载组件
                var asm = Assembly.LoadFrom(path);

                // is it a plugin?? 它是一个插件吗？
                var pluginAttr = (PluginAttribute)asm.GetCustomAttributes(typeof(PluginAttribute), false).FirstOrDefault();
                if (pluginAttr == null)
                    return new LoadPluginResult(false, asm, null);

                var fileName = Path.GetFileName(path);

                Platform.Log(LogLevel.Debug, "Loaded plugin {0}", fileName);
                XTrace.WriteLine($"加载插件：{ fileName}");

                var e = new PluginLoadedEventArgs(string.Format(SR.FormatLoadedPlugin, fileName), asm);
                EventsHelper.Fire(PluginLoaded, this, e);

                // do not create a PluginInfo unless explicitly asked for, because it is expensive
                // 除非明确要求，否则不要创建PluginInfo，因为它很昂贵
                var pluginInfo = processMetadata ? new PluginInfo(asm, pluginAttr.Name, pluginAttr.Description, pluginAttr.Icon) : null;
                return new LoadPluginResult(true, asm, pluginInfo);
            }
            catch (BadImageFormatException e)
            {
                // unmanaged DLL in the plugin directory 插件目录中的非托管DLL
                Platform.Log(LogLevel.Debug, SR.LogFoundUnmanagedDLL, e.FileName);
                XTrace.WriteLine($"{SR.LogFoundUnmanagedDLL} 发现非托管DLL：{e.FileName}");
            }
            catch (ReflectionTypeLoadException e)
            {
                // this exception usually means one of the dependencies is missing 
                // 此异常通常意味着缺少其中一个依赖项 
                Platform.Log(LogLevel.Error, SR.LogFailedToProcessPluginAssembly, Path.GetFileName(path));
                XTrace.WriteLine($"{SR.LogFailedToProcessPluginAssembly}：{Path.GetFileName(path)}");

                // log a detail message for each missing dependency
                //记录每个缺少的依赖项的详细消息
                foreach (var loaderException in e.LoaderExceptions)
                {
                    // just log the message, don't need the full stack trace 
                    //只记录消息，不需要完整的堆栈跟踪
                    Platform.Log(LogLevel.Error, loaderException.Message);

                    XTrace.WriteLine($"{ loaderException.Message}");
                }
            }
            catch (FileNotFoundException e)
            {
                Platform.Log(LogLevel.Error, e, "File not found while loading plugin: {0}", path);

                XTrace.WriteLine($"加载插件时找不到文件：{path}");
            }
            catch (Exception e)
            {
                // there was a problem processing this assembly
                // 处理此程序集时出现问题
                Platform.Log(LogLevel.Error, e, SR.LogFailedToProcessPluginAssembly, path);
                XTrace.WriteLine($"处理此程序集时出现问题：{SR.LogFailedToProcessPluginAssembly} " +
                                 $"{Environment.NewLine}=>{path}");
            }

            return new LoadPluginResult(false, null, null);
        }

        private static byte[] ComputeCheckSum(IEnumerable<string> pluginCandidatePaths)
        {
            // include config files in the check sum
            // 在校验和中包含配置文件
            var configFiles = Directory.EnumerateFiles(Platform.InstallDirectory, "*.config", SearchOption.TopDirectoryOnly);
            var orderedFiles = configFiles.Concat(pluginCandidatePaths).Select(f => new FileInfo(f)).OrderBy(fi => fi.FullName);

            // generate a checksum based on the name, create time, and last write time of each file
            // 根据每个文件的名称，创建时间和上次写入时间生成校验和
            using (var byteStream = new MemoryStream())
            using (var hash = new SHA256CryptoServiceProvider2())
            {
                foreach (var fi in orderedFiles)
                {
                    var name = Encoding.Unicode.GetBytes(fi.Name);
                    byteStream.Write(name, 0, name.Length);

                    var createTime = BitConverter.GetBytes(fi.CreationTimeUtc.Ticks);
                    byteStream.Write(createTime, 0, createTime.Length);

                    var writeTime = BitConverter.GetBytes(fi.LastWriteTimeUtc.Ticks);
                    byteStream.Write(writeTime, 0, createTime.Length);
                }
                return hash.ComputeHash(byteStream.GetBuffer());
            }
        }

        /// <summary>返回加载插件的程序集字典 key为程序集名称，value为程序集位置 
        /// 
        /// Yann 2018年12月24日 14:44:03 
        /// </summary>
        /// <param name="filePaths"></param>
        /// <returns></returns>
        private static Dictionary<string, string> BuildAssemblyMap(IEnumerable<string> filePaths)
        {
            var result = new Dictionary<string, string>();
            foreach (var p in filePaths)
            {
                XTrace.WriteLine($"方法：BuildAssemblyMap  ");
                XTrace.WriteLine($" {p}");
                try
                {
                    var name = AssemblyName.GetAssemblyName(p).Name;
                    result.Add(name, p);
                }
                catch (Exception)
                {
                    // not an assembly  
                    Platform.Log(LogLevel.Debug, "The file at {0} does not seem to be an assembly.", p);
                    XTrace.WriteLine($"{p}处的文件似乎不是程序集。");
                }
            }
            return result;
        }
    }
}