
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
using System.ServiceModel;
using Lig.vivitue.Contract.Data;
namespace Lig.vivitue.Contract.Services
{
    [ServiceContract(CallbackContract = typeof(ILigAgentCallback))]
    public interface ILigAgent
    {
        [OperationContract]
        LigStatus RegisterClient(string name);

        [OperationContract]
        LigStatus UnregisterClient(int clientID);

        [OperationContract]
        LigStatus Subscribe(int clientID);

        [OperationContract]
        LigStatus Unsubscribe(int clientID);

        [OperationContract]
        void LigMessage(string message, LigLevel level);

        [OperationContract]
        void LigInfo(string message);

        [OperationContract]
        void LigDebug(string message);

        [OperationContract]
        void LigWarn(string message);

        [OperationContract]
        void LigError(string message);

        [OperationContract(Name = "LigFatal")]
        void LigFatal(string message);

        [OperationContract(Name = "LigFatalex")]
        void LigFatal(string message, Exception ex);

        [OperationContract]
        void SayHelloToServer(string message);

        [OperationContract]
        void Initialize(string ligPath);
    }
    public interface ILigAgentCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnNotifyMessage(string message);
        [OperationContract(IsOneWay = true)]
        void OnNotifyOnline(LigArgs args);
    }
}
