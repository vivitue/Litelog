
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
using Lig.vivitue.Contract.Data;

namespace Lig.vivitue
{
    public interface ILiggerBase
    {
        bool ConnectStatus { get; }
        int ClientID { get; }
        bool SubscribeOnlineEvent(EventHandler onlined);
        bool UnsubscribeOnlineEvent(int clientID);
        void LigMessage(string message, LigLevel level);
        void LigInfo(string message);
        void LigDebug(string message);
        void LigWarn(string message);
        void LigError(string message);
        void LigFatal(string message);
    }
}
