
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;

namespace L.vivitue.Common
{
    public static class SysInfo
    {
        #region Win32
        [DllImport("kernel32.dll")]
        private static extern uint GetTickCount();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        public static int GetTickCount(this object graph)
        {
            return (int)SysInfo.GetTickCount();
        }
        #endregion

        #region PublicInterfaces

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="pathOne"></param>
        /// <param name="pathTwo"></param>
        /// <returns></returns>
        public static string CombinePath(this object graph, string pathOne, string pathTwo)
        {
            return SysInfo.CombinePath(pathOne, pathTwo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathOne"></param>
        /// <param name="pathTwo"></param>
        /// <returns></returns>
        public static string CombinePath(string pathOne, string pathTwo)
        {
            if (string.IsNullOrEmpty(pathOne) || string.IsNullOrEmpty(pathTwo)) return null;
            string combinepathInfo = null;
            try
            {
                combinepathInfo = Path.Combine(pathOne, pathTwo);
            }
            catch (Exception e)
            {
                combinepathInfo = null;
            }
            return combinepathInfo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public static string GetParentFolder(this object graph, string folderName)
        {
            return SysInfo.GetParentFolderFromCurrentFolder(folderName);
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
                return null;
            }
            return str;
        }
        #endregion

        #region Fields & Properties

        public static string CurrentTime
        {
            get
            {
                return DateTime.Now.ToString() + " : ";
            }
        }
        
        private static string lcommonpath = null;
        public static string LCommonPath
        {
            get
            {
                if (lcommonpath == null)
                {
                    try
                    {
                        string tmppath = Assembly.GetExecutingAssembly().Location;
                        string dirpath = Path.GetDirectoryName(tmppath);
                        string result = SysInfo.GetParentFolderFromCurrentFolder(dirpath);
                        lcommonpath = result;
                    }
                    catch (Exception ex)
                    {
                        lcommonpath = @"C:\vivitue";
                    }
                }
                return lcommonpath;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static string LigsPath
        {
            get
            {
                return SysInfo.CombinePath(LCommonPath, "Ligs");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static int TickCount
        {
            get
            {
                return (int)SysInfo.GetTickCount();
            }
        }
        #endregion
    }
}
