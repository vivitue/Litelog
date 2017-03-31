
#region Apache License
//
// Copyright(c)2017 vivitue

// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at

//    http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

//define FILEINNERLIG
using System;
using System.Reflection;
using System.IO;
using System.Threading;
using System.Diagnostics;
using L.vivitue.Common;
namespace L.vivitue
{
    public class Innerlig
    {
        #region Constructors
        static Innerlig()
        {
            InitializedSourcer();
        }
        #endregion

        #region PrivateHelpers
        /// <summary>
        /// 
        /// </summary>
        private static void InitializedSourcer()
        {
            try
            {
#if !FILEINNERLIG
                Innerlig.ligsource = new TraceSource(dclringType.Name);
                ConsoleTraceListener listener = new ConsoleTraceListener();
                Innerlig.ligsource.Listeners.Add(listener);
#else
                // Lig to file
                StreamWriter writer = new StreamWriter(InnerligPath, true);
                writer.AutoFlush = true;
                Innerlig.ligsource.Listeners.Add(new TextWriterTraceListener(writer));
#endif
            }
            catch (Exception ex)
            {
                // ignore
            }
        }
        #endregion

        #region LigTracers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        public static void Info(Type type, string message)
        {
            Innerlig.ligMutex.WaitOne();
            try
            {
                ligsource.Switch = Innerlig.InfoSwitcher;
                ligsource.TraceEvent(TraceEventType.Information, (int)TraceEventType.Information,
                    SysInfo.CurrentTime + type.FullName + " - " + message);
            }
            catch (Exception e)
            {
                //ignore
            }
            finally
            {
                Innerlig.ligMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public static void Info(Type type, string message, Exception ex)
        {
            Innerlig.ligMutex.WaitOne();
            try
            {
                ligsource.Switch = Innerlig.InfoSwitcher;
                ligsource.TraceEvent(TraceEventType.Information, (int)TraceEventType.Information, 
                    SysInfo.CurrentTime + type.FullName + " - " + message);
                ligsource.TraceInformation(SysInfo.CurrentTime + type.FullName + " - " + message);
                if (ex != null) ligsource.TraceEvent(TraceEventType.Information, (int)TraceEventType.Information,
                    SysInfo.CurrentTime + ex.Message);
            }
            catch (Exception e)
            {
            }
            finally
            {
                Innerlig.ligMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        public static void Warn(Type type, string message)
        {
            Innerlig.ligMutex.WaitOne();
            try
            {
                ligsource.Switch = Innerlig.WarnSwitcher;
                ligsource.TraceEvent(
                    TraceEventType.Warning, (int)TraceEventType.Warning, 
                    SysInfo.CurrentTime + type.FullName + " - " + message);
            }
            catch (Exception e)
            {
            }
            finally
            {
                Innerlig.ligMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public static void Warn(Type type, string message, Exception ex)
        {
            Innerlig.ligMutex.WaitOne();
            try
            {
                ligsource.Switch = Innerlig.WarnSwitcher;
                ligsource.TraceEvent(
                    TraceEventType.Warning, (int)TraceEventType.Warning,
                    SysInfo.CurrentTime + type.FullName + " - " + message);

                if (ex != null) ligsource.TraceEvent(
                    TraceEventType.Warning, (int)TraceEventType.Warning,
                    SysInfo.CurrentTime + ex.Message);
            }
            catch (Exception e)
            {
            }
            finally
            {
                Innerlig.ligMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        public static void Error(Type type, string message)
        {
            Innerlig.ligMutex.WaitOne();
            try
            {
                ligsource.Switch = Innerlig.ErrorSwitcher;
                ligsource.TraceEvent(TraceEventType.Error, (int)TraceEventType.Error, 
                    SysInfo.CurrentTime + type.FullName + " - " + message);

            }
            catch (Exception e)
            {
            }
            finally
            {
                Innerlig.ligMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public static void Error(Type type, string message, Exception ex)
        {
            Innerlig.ligMutex.WaitOne();
            try
            {
                ligsource.Switch = Innerlig.ErrorSwitcher;
                ligsource.TraceEvent(TraceEventType.Error, (int)TraceEventType.Error, 
                    SysInfo.CurrentTime + type.FullName + " - " + message);
                if (ex != null) ligsource.TraceEvent(TraceEventType.Error, (int)TraceEventType.Error, SysInfo.CurrentTime + ex.Message);
            }
            catch (Exception e)
            {
                // ignore
            }
            finally
            {
                Innerlig.ligMutex.ReleaseMutex();
            }
        }
        #endregion

        #region Fields & Properties
        private static readonly Type dclringType = typeof(Innerlig);

        private static TraceSource ligsource = null;
        private static SourceSwitch warnSwitcher = null;
        private static SourceSwitch errorSwitcher = null;
        private static SourceSwitch infoSwitcher = null;
        private static SourceSwitch WarnSwitcher
        {
            get
            {
                if (Innerlig.warnSwitcher == null)
                {
                    Innerlig.warnSwitcher = new SourceSwitch("Warn");
                    Innerlig.warnSwitcher.Level = SourceLevels.Warning;
                }
                return Innerlig.warnSwitcher;
            }
        }
        private static SourceSwitch ErrorSwitcher
        {
            get
            {
                if (Innerlig.errorSwitcher == null)
                {
                    Innerlig.errorSwitcher = new SourceSwitch("Error");
                    Innerlig.errorSwitcher.Level = SourceLevels.Error;
                }
                return Innerlig.errorSwitcher;
            }
        }
        private static SourceSwitch InfoSwitcher
        {
            get
            {
                if (Innerlig.infoSwitcher == null)
                {
                    Innerlig.infoSwitcher = new SourceSwitch("Info");
                    Innerlig.infoSwitcher.Level = SourceLevels.Information;
                }
                return Innerlig.infoSwitcher;
            }
        }


        private static string innerligPath = null;
        public static string InnerligPath 
        {
            get
            {
                if (string.IsNullOrEmpty(Innerlig.innerligPath))
                {
                    return Innerlig.Defaultlig;
                }
                return innerligPath;
            }
            set { Innerlig.innerligPath = value; }
        }
        private static string Defaultlig
        {
            get { return @"C:\Innerlig\Innerlig.lig"; }
        }
        private static Mutex ligMutex = new Mutex(false, dclringType.FullName);
        #endregion
    }
}
