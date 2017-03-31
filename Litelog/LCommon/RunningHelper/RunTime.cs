
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using L.vivitue.Common;
namespace L.vivitue
{
    public static class RunTime
    {
        #region Constructor
        static RunTime()
        {
            Initialized();
        }
        #endregion

        #region Private Helpers

        /// <summary>
        /// 
        /// </summary>
        private static void Initialized()
        {
            InitializedWCFConfig();
        }

        /// <summary>
        /// 
        /// </summary>
        private static void InitializedWCFConfig()
        {
            try
            {
                string lcientpath = SysInfo.CombinePath(SysInfo.LCommonPath, CONFIG + "\\" + LCLIENT);
                string lserverpath = SysInfo.CombinePath(SysInfo.LCommonPath, CONFIG + "\\" + LSERVER);
                dicLFiles.Add(FileID.LClient, lcientpath);
                dicLFiles.Add(FileID.LServer, lserverpath);
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region Fields & Properties
        private const string LSERVER = "LServer.config";
        private const string LCLIENT = "LClient.config";
        private const string CONFIG = "Config";
        private static Dictionary<FileID, string> dicLFiles = new Dictionary<FileID, string>();
        public static Dictionary<FileID, string> DicLFiles
        {
            get
            {
                return dicLFiles;
            }
        }
        #endregion
    }
}
