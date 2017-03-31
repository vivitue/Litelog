
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
using Lig.vivitue.Contract.Data;

namespace Lig.vivitue
{
    public class Ligger : ILigger,IDisposable
    {
        #region Constructors
        private Ligger() { }
        public Ligger(string dirName, string fileName)
        {
            this.iLigbase = new LiggerBase(dirName, fileName);
        }
        ~Ligger()
        {
            this.Dispose();
        }
        public void Dispose()
        {
            if (this.iLigbase != null)
            {
                (this.iLigbase as LiggerBase).Dispose();
            }
            this.iLigbase = null;
        }
        #endregion

        #region Ligger Interfaces
        public bool SubscribeOnlineEvent(EventHandler onlined)
        {
            return iLigbase.SubscribeOnlineEvent(onlined);
        }

        public bool UnsubscribeOnlineEvent(int clientID)
        {
            return iLigbase.UnsubscribeOnlineEvent(clientID);
        }
        public void LigMessage(string message, int level)
        {
            if (level < 0 || level > 5) return;
            iLigbase.LigMessage(message, (LigLevel)level);

        }
        public void LigInfo(string message)
        {
            iLigbase.LigInfo(message);
        }
        public void LigDebug(string message)
        {
            iLigbase.LigDebug(message);
        }
        public void LigWarn(string message)
        {
            iLigbase.LigWarn(message);
        }
        public void LigError(string message)
        {
            iLigbase.LigError(message);
        }
        public void LigFatal(string message)
        {
            iLigbase.LigFatal(message);
        }
        #endregion

        #region Fields & Properties
        private ILiggerBase iLigbase = null;
        public bool ConnectStatus
        {
            get
            {
                if (iLigbase == null) return false;
                return iLigbase.ConnectStatus;
            }
        }
        public int ClientID
        {
            get
            {
                if (iLigbase == null) return -2;
                return iLigbase.ClientID;
            }
        }
        #endregion

    }
}
