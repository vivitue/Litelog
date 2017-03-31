
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
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.ServiceModel;

using L.vivitue;
using Lig.vivitue.Core;
using Lig.vivitue.Contract.Data;
using Lig.vivitue.Contract.Services;
namespace Lig.vivitue.Contract.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class LigAgent : ILigAgent, IDisposable
    {
        #region Constructor
        public LigAgent() { }
        #endregion

        #region Deconstructors
        public void Dispose()
        {
            //NOP
        }

        private void Dispose(bool disposing)
        {
            //NOP
        }
        #endregion

        #region PrivateHelpers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnShowOffline(object sender, EventArgs e)
        {
            int hash = this.GetHashCode();
            try
            {
                this.RemoveClient(hash);
            }
            catch (Exception ex)
            {
                Innerlig.Error(dclringType, "RemoveClient failed!",ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private LigStatus AddClient(string name, out int clientID)
        {
            clientID = -1;
            int hash = this.GetHashCode();
            if (string.IsNullOrEmpty(name)) return LigStatus.InvalidClient;

            if (dicClientsContext.ContainsKey(hash)) return LigStatus.ClientExisted;

            clientID = LigCore.ActiveID;

            ClientContext context = new ClientContext();
            context.ClientID = clientID;
            context.ClientName = name;
            context.HashID = hash;
            context.IsRegistered = true;
            context.Context = OperationContext.Current;
            context.CallbackInstance = OperationContext.Current.GetCallbackChannel<ILigAgentCallback>();

            context.Context.Channel.Closed += new EventHandler(OnShowOffline);

            dicClientsContext.Add(hash, context);
            Console.WriteLine(LigCore.LiggedTime + "Client with hashCode [{0}] is online!", hash);
            Console.WriteLine(LigCore.LiggedTime + "Current Total Clients : {0}", dicClientsContext.Count);
            return LigStatus.Success;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hashValue"></param>
        /// <returns></returns>
        private LigStatus RemoveClient(int hashValue)
        {
            if (!dicClientsContext.ContainsKey(hashValue))
            {
                Console.WriteLine(LigCore.LiggedTime + "InnerError! Client doesn't existed! Remove failed!");
                return LigStatus.Failed;
            }
            Console.WriteLine(LigCore.LiggedTime + dicClientsContext[hashValue].ClientName
                + " with hashcode[{0}] was offline! This client's resources will be disposed!",
                dicClientsContext[hashValue].HashID);
            dicClientsContext.Remove(hashValue);
            return LigStatus.Success;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        private bool CheckOperationRight()
        {
            int hash = this.GetHashCode();
            if (hash <= 0) return false;
            if (!dicClientsContext.ContainsKey(hash)) return false;
            if (!dicClientsContext[hash].IsRegistered) return false;
            return true;
        }
        #endregion

        #region RegisterInterfaces
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public LigStatus RegisterClient(string name)
        {
            if (string.IsNullOrEmpty(name)) return LigStatus.InvalidClient;
            LigStatus status = LigStatus.Default;
            try
            {
                int clientID = -1;
                status = this.AddClient(name, out clientID);
                if (status != LigStatus.Success) return status;
                status = this.Subscribe(clientID);
                if (status != LigStatus.Success) return status;
            }
            catch (Exception ex)
            {
                status = LigStatus.Failed;
                Innerlig.Error(dclringType, "RegisterClient failed!", ex);
            }
            return status;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientID"></param>
        /// <returns></returns>
        public LigStatus UnregisterClient(int clientID)
        {
            int hash = this.GetHashCode();
            if (!dicClientsContext.ContainsKey(hash)) return LigStatus.ClientIsNotExisted;
            if (dicClientsContext[hash].ClientID != clientID) return LigStatus.InvalidClient;

            dicClientsContext.Remove(hash);
            Console.WriteLine(LigCore.LiggedTime + "Client with hashcode[{0}] is unregistered!", hash);
            return LigStatus.Success;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientID"></param>
        /// <returns></returns>
        public LigStatus Subscribe(int clientID)
        {
            int hash = this.GetHashCode();
            if (!dicClientsContext.ContainsKey(hash)) return LigStatus.ClientIsNotExisted;
            if (dicClientsContext[hash].IsSubscribed) return LigStatus.ClientHasbeenRegistered;
            if (clientID != dicClientsContext[hash].ClientID) return LigStatus.InvalidClient;
            int count = 0;
            LigStatus status = LigStatus.Success;
            dicClientsContext[hash].IsSubscribed = true;
            Thread handle = new Thread(() =>
                {
                    try
                    {
                        LigArgs args = new LigArgs();
                        args.ClientID = dicClientsContext[hash].ClientID;
                        args.ClientName = dicClientsContext[hash].ClientName;
                        args.HashID = dicClientsContext[hash].HashID;
                        args.ConnectStatus = true;
                        args.Message = args.ClientName + " client is online!";
                        dicClientsContext[hash].CallbackInstance.OnNotifyOnline(args);

                    }
                    catch (Exception ex)
                    {
                        Innerlig.Error(dclringType, "Subscribe failed!",ex);
                    }
                });
            handle.Start();
            while (!handle.IsAlive)
            {
                Thread.Sleep(100);
                count++;
                if (count > 50)
                {
                    status = LigStatus.Failed;
                    Innerlig.Warn(dclringType, "Inner Thread start waiting failed! Waiting Count = " + count.ToString());
                    break;
                }
            }
            return status;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientID"></param>
        /// <returns></returns>
        public LigStatus Unsubscribe(int clientID)
        {
            return LigStatus.Success;
        }
        #endregion

        #region LigInterfaces
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ligPath"></param>
        public void Initialize(string ligPath)
        {
            this.ligPath = ligPath;
            LigCore.CheckOrCreateFile(this.ligPath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="level"></param>
        public void LigMessage(string message, LigLevel level)
        {
            if (!this.CheckOperationRight()) return;
            string formatMsg = LigCore.LiggedTime + level.ToString() + " " + message;
            new LigCore().EmitFileMessage(formatMsg, this.LigPath, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void LigInfo(string message)
        {

            if (!this.CheckOperationRight()) return;
            LigCore.LigPath = this.LigPath;
            LigCore.LigInfo(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void LigDebug(string message)
        {
            if (!this.CheckOperationRight()) return;
            LigCore.LigPath = this.LigPath;
            LigCore.LigDebug(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void LigError(string message)
        {
            if (!this.CheckOperationRight()) return;
            LigCore.LigPath = this.LigPath;
            LigCore.LigError(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void LigFatal(string message)
        {
            if (!this.CheckOperationRight()) return;
            LigCore.LigPath = this.LigPath;
            LigCore.LigFatal(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public void LigFatal(string message, Exception ex)
        {
            if (!this.CheckOperationRight()) return;
            LigCore.LigPath = this.LigPath;
            LigCore.LigFatal(message, ex);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void LigWarn(string message)
        {
            if (!this.CheckOperationRight()) return;
            LigCore.LigPath = this.LigPath;
            LigCore.LigWarn(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void SayHelloToServer(string message)
        {
            Console.WriteLine(LigCore.LiggedTime + message);
            Console.WriteLine(LigCore.LiggedTime + "Client with hashCode [" + this.GetHashCode().ToString() + "] is connected to LigServer.");
        }
        #endregion

        #region Fields & Properties

        private static readonly Type dclringType=typeof(LigAgent);
        private string ligPath = null;
        private string LigPath
        {
            get { return ligPath; }
        }

        private static object clientlock = new object();
        private static Dictionary<int, ClientContext> dicClientsContext = new Dictionary<int, ClientContext>();
        private static Dictionary<int, ClientContext> DicClientsContext
        {
            get { return dicClientsContext; }
        }

        #endregion

        #region InnerClasses
        class ClientContext
        {
            internal ClientContext() { }
            internal bool IsRegistered { get; set; }
            internal bool IsSubscribed { get; set; }
            internal int ClientID { get; set; }
            internal int HashID { get; set; }
            internal string ClientName { get; set; }
            internal ILigAgentCallback CallbackInstance { get; set; }
            internal OperationContext Context { get; set; }
        }
        #endregion
    }
}
