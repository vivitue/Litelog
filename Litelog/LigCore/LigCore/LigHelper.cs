
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

using System.Text;
using System.Runtime.InteropServices;

namespace Lig.vivitue.Core
{
    public static class LigHelper
    {
        #region Public Interfaces
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inTxt"></param>
        /// <returns></returns>
        public static string EncryptTxt(string inTxt)
        {
            if (string.IsNullOrEmpty(inTxt)) throw new ArgumentNullException("None inTxt!"); ;
            UnicodeEncoding utf8 = new UnicodeEncoding();
            byte[] bytes = utf8.GetBytes(inTxt);
            string base64str = Convert.ToBase64String(bytes);
            return base64str.ToUpper();
        }
        #endregion

    }
}
