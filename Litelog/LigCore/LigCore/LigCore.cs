
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


using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lig.vivitue.Core
{
    public class LigCore
    {
        #region Constructors
        public LigCore() { }
        static LigCore() { }
        #endregion

        #region PublicInterfaces

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool CheckOrCreateFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return false;
            bool isCreated = false;
            FileStream fstream = null;
            try
            {
                string dirName = Path.GetDirectoryName(fileName);
                if (!Directory.Exists(dirName))
                {
                    Directory.CreateDirectory(dirName);
                    fstream = new FileStream(fileName, FileMode.Create);
                    isCreated = true;

                }
                else
                {
                    if (!File.Exists(fileName))
                    {
                        fstream = new FileStream(fileName, FileMode.Create);
                        isCreated = true;
                    }
                }

            }
            catch (ExecutionEngineException eex)
            {
                //
            }
            catch (Exception ex)
            {

                //
            }
            finally
            {
                if (fstream != null)
                {
                    fstream.Close();
                    fstream = null;
                }
            }
            return isCreated;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public static string GetParentFolderFromCurrentFolder(string folderName)
        {
            string str = null;
            if (string.IsNullOrEmpty(folderName)) return null;
            try
            {
                string[] folderSplits = folderName.Split('\\');
                if (folderSplits.Length == 1) return folderName;
                for (int i = 0; i < folderSplits.Length - 1; i++)
                {
                    str += folderSplits[i] + "\\";
                }
                str = str.Remove(str.Length - 1);
            }
            catch (Exception e)
            {
                //Console.Error.WriteLine(CurstrTime + "Internal error! "+ e.ToString());
                return null;
            }
            return str;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="fullpath"></param>
        public void EmitFileMessage(string message, string fullpath, bool isAppending)
        {
            StreamWriter writer = null;
            try
            {
                writer = new StreamWriter(fullpath, isAppending);
                writer.Write(message);
                writer.Flush();
            }
            catch (Exception ex)
            {
                // ignore
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
                writer = null;
            }
        }
        #endregion

        #region PublicStaticInterfaces
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void LigInfo(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            string formatMsg = LigCore.LiggedTime + INFORPREFIX + message + Environment.NewLine;
            LigCore.EmitFileMessage(formatMsg);
            LigCore.LiggingDetails(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void LigDebug(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            string formatMsg = LigCore.LiggedTime + DEBUGPREFIX + message + Environment.NewLine;
            LigCore.EmitFileMessage(formatMsg);
            LigCore.LiggingDetails(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void LigError(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            string formatMsg = LigCore.LiggedTime + ERRORPREFIX + message + Environment.NewLine;
            LigCore.EmitFileMessage(formatMsg);
            LigCore.LiggingDetails(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void LigFatal(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            string formatMsg = LigCore.LiggedTime + WARNPREFIX + message + Environment.NewLine;
            LigCore.EmitFileMessage(formatMsg);
            LigCore.LiggingDetails(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public static void LigFatal(string message, Exception ex)
        {
            if (string.IsNullOrEmpty(message)) return;

            string formatMsg = LigCore.LiggedTime + WARNPREFIX + message + Environment.NewLine;
            LigCore.EmitFileMessage(formatMsg);
            if (ex != null)
            {
                LigCore.EmitFileMessage(LigCore.LiggedTime + ex.Message + Environment.NewLine);
            }
            LigCore.LiggingDetails(message);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void LigWarn(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            string msg = LigCore.LiggedTime + WARNPREFIX + message + Environment.NewLine;
            LigCore.EmitFileMessage(msg);
            LigCore.LiggingDetails(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private static void EmitFileMessage(string message)
        {
            LigCore.commonLock.WaitOne();
            StreamWriter writer = null;
            try
            {
                writer = new StreamWriter(LigCore.LigPath, LigCore.IsAppend);
                writer.Write(message);
                writer.Flush();
            }
            catch (Exception ex)
            {
                // ignore
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
                writer = null;
                LigCore.commonLock.ReleaseMutex();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private static void LiggingDetails(string message)
        {
            if (LigCore.IsLigDetails)
            {

            }
        }
        #endregion

        #region Static Fields & Properties
        private static object idlock = new object();
        private static int activeID = 0;
        public static int ActiveID
        {
            get
            {
                lock (LigCore.idlock)
                {
                    LigCore.activeID++;
                }
                return LigCore.activeID;
            }
        }

        private const string ERRORPREFIX = "- Error - ";
        private const string FATALPREFIX = "- Fatal - ";
        private const string WARNPREFIX = "- Warn - ";
        private const string INFORPREFIX = "- Info - ";
        private const string DEBUGPREFIX = "- Debug - ";

        private static Type dclringType = typeof(LigCore);
        private static Type DclringType
        {
            get { return LigCore.dclringType; }
        }

        private static string UniqueMutexName
        {
            get
            {
                return LigHelper.EncryptTxt(DclringType.GetType().Name);
            }
        }

        private static Mutex commonLock = new Mutex(false, UniqueMutexName);

        private static bool IsAppend
        {
            get { return true; }
        }
        public static bool IsLigDetails
        {
            get { return false; }
        }

        private static string LgExtentionName
        {
            get { return ".lg"; }
        }

        private static string DefaultLigPath
        {
            get { return @"C:\LigError\LigError.lg"; }
        }

        public static string LiggedTime
        {
            get { return DateTime.Now.ToString() + " : "; }
        }
        private static string ligPath = null;

        public static string LigPath
        {
            get
            {
                if (ligPath == null)
                {
                    try
                    {
                        // Set the default path
                        string tmppath = null;
                        string asmpath = Assembly.GetExecutingAssembly().Location;
                        string asmparentPath = Path.GetDirectoryName(asmpath);
                        string folder = LigCore.GetParentFolderFromCurrentFolder(asmparentPath);
                        string resultpath = Path.Combine(folder, "Ligs\\" + "Lig" + LigCore.LgExtentionName);
                        ligPath = resultpath;
                    }
                    catch (Exception ex)
                    {
                        // ignore
                        return LigCore.DefaultLigPath;
                    }
                }
                return ligPath;
            }
            set { ligPath = value; }
        }

        #endregion
    }
}
