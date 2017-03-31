
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
using System.ServiceModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using L.vivitue;
using L.vivitue.Common;
using L.vivitue.Common.WCF;
using Lig.vivitue.Contract.Data;
using Lig.vivitue.Contract.Services;

namespace Lig.vivitue
{
    public class LiggerBase : ILiggerBase,IDisposable
    {
        #region Constructors
        private LiggerBase() { }
        public LiggerBase(string dirName,string fileName) 
        {
            if (string.IsNullOrEmpty(dirName) || string.IsNullOrEmpty(fileName)) throw new ArgumentNullException("None value!");
            this.filePath = this.GetLigPath(dirName, fileName);
            this.ConnectServer();
        }
        
        #endregion

        #region Desconstructors

        ~LiggerBase()
        {
            Dispose();
        }
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if (this.isDisposed) return;
            if (disposing)
            {
                //Release unmanaged resources!
            }
            this.UnsubscribedEvents();
            this.Release();
            Console.WriteLine(SysInfo.CurrentTime + "Client was disposed! Clientself ClientID {0}", this.ClientID);
            this.clientID = -2;
            this.isDisposed = true;
        }
        #endregion

        #region PrivateHelpers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dirName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string GetLigPath(string dirName, string fileName)
        {
            return this.CombinePath(SysInfo.LigsPath, (dirName + "\\" + fileName + ".lig"));
        }

        /// <summary>
        /// 
        /// </summary>
        private void Release()
        {
            this.UnsubscribeOnlineEvent(this.ClientID);

            if (this.LClient != null)
            {
                if (this.LClient.LinkStatus)
                {
                    this.LClient.DisconnectServer();
                }
                this.LClient = null;
            }
            
            if (this.callbackInstance != null)
            {
                this.callbackInstance.Release();
            }
            this.callbackInstance = null;
            this.iLig = null;
            this.connectStatus = false;
        }

        /// <summary>
        /// 
        /// </summary>
        private void ConnectServer()
        {
            int linkCnt = 0;
            Thread handle = new Thread(() =>
            {
                try
                {
                    if (this.ConnectStatus) return;

                    this.callbackInstance = new LigAgentCallback();
                    ILigAgentCallback icallback = this.callbackInstance;
                    InstanceContext context = new InstanceContext(icallback);
                    this.SubscribedEvents();

                    this.LClient = new LWCF<ILigAgent>(context,
                        WCFClientType.Duplex,
                        SERVICENAME,
                        RunTime.DicLFiles[FileID.LClient]);


                    this.LClient.ConnectServer();
                    if (this.LClient.LinkStatus)
                    {
                        this.iLig = this.LClient.GetInstance();
                        this.iLig.SayHelloToServer("LigManager Connecting : ClientHash : " + this.GetHashCode().ToString());
                        this.clientName = this.GetType().Name;
                        this.iLig.RegisterClient(this.clientName);
                        this.iLig.Initialize(this.filePath);
                        //this.connectStatus = true;
                    }

                }
                catch (Exception ex)
                {
                    this.Release();
                    Innerlig.Error(dclringType, "ConnectServer error", ex);
                }
            });
            handle.Start();
            while (!handle.IsAlive)
            {
                Thread.Sleep(100);
                linkCnt++;
                if (linkCnt > 10)
                {
                    Innerlig.Error(dclringType, "Inner fatal! LinkCnt =" + linkCnt.ToString(), null);
                    break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void DisconnectServer()
        {
            if (!this.ConnectStatus) return;
            this.Release();

        }

        #endregion

        #region CommonInterfaces

        /// <summary>
        /// 
        /// </summary>
        /// <param name="onlined"></param>
        /// <returns></returns>
        public bool SubscribeOnlineEvent(EventHandler onlined)
        {
            bool isSubscribed = false;
            if (this.onlineEvent != null)
            {
                Delegate[] items = this.onlineEvent.GetInvocationList();
                foreach (Delegate item in items)
                {
                    if (onlined == (EventHandler)item)
                    {
                        isSubscribed = true;
                        break;
                    }
                }
            }
            if (isSubscribed) return true;
            this.onlineEvent += onlined;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientID"></param>
        /// <returns></returns>
        public bool UnsubscribeOnlineEvent(int clientID)
        {
            if (this.clientID != clientID) return false;
            if (this.onlineEvent == null) return false;
            try
            {
                Delegate[] items = this.onlineEvent.GetInvocationList();
                foreach (Delegate item in items)
                {
                    this.onlineEvent -= (EventHandler)item;
                }

            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="level"></param>
        public void LigMessage(string message, LigLevel level)
        {
            if (!this.ConnectStatus) return;
            this.ILig.LigMessage(message,level);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void LigInfo(string message)
        {
            if (!this.ConnectStatus) return;
            this.ILig.LigInfo(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void LigDebug(string message)
        {
            if (!this.ConnectStatus) return;
            this.ILig.LigDebug(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void LigWarn(string message)
        {
            if (!this.ConnectStatus) return;
            this.ILig.LigWarn(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void LigError(string message)
        {
            if (!this.ConnectStatus) return;
            this.ILig.LigError(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void LigFatal(string message)
        {
            if (!this.ConnectStatus) return;
            this.ILig.LigFatal(message);
        }

        #endregion

        #region SubscribedEvents

        /// <summary>
        /// 
        /// </summary>
        private void SubscribedEvents()
        {
            this.callbackInstance.msgReceived += new MessageHandler(OnNotifyMessage);
            this.callbackInstance.onlined += new OnlineHandler(OnNotifyOnline);
        }

        /// <summary>
        /// 
        /// </summary>
        private void UnsubscribedEvents()
        {
            if (this.callbackInstance != null)
            {
                this.callbackInstance.Release();
            }
        }

        #endregion

        #region OnCallbacks
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private void OnNotifyMessage(string message)
        {
            LiggerEventArgs eventArgs = new LiggerEventArgs();
            eventArgs.Message = message;
            if (this.MessageRecv != null)
            {
                this.MessageRecv(this,eventArgs);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        private void OnNotifyOnline(LigArgs args)
        {
            LiggerEventArgs eventArgs = new LiggerEventArgs();
            eventArgs.ClientID = args.ClientID;
            eventArgs.ConnectStatus = args.ConnectStatus;
            eventArgs.Message = args.Message;
            this.clientID = args.ClientID;
            this.connectStatus = args.ConnectStatus;
            if (this.onlineEvent != null)
            {
                this.onlineEvent(this,eventArgs);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        private void PublicizedMessage(LigArgs args)
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine(Environment.NewLine);
                builder.AppendLine("-----------------------------------------------");
                builder.AppendLine(SysInfo.CurrentTime + "Attention : Message received from LigServer!");
                builder.AppendLine("");
                builder.AppendLine(SysInfo.CurrentTime + "ClientID    : " + args.ClientID.ToString());
                builder.AppendLine(SysInfo.CurrentTime + "ClientName  : " + args.ClientName);
                builder.AppendLine(SysInfo.CurrentTime + "HashID      :" + args.HashID.ToString());
                builder.AppendLine(SysInfo.CurrentTime + "Message     :" + args.Message);
                builder.AppendLine("-----------------------------------------------");
                Console.WriteLine(builder);
            }
            catch (Exception ex)
            {
                // ignore!
            }
        }
        #endregion

        #region Fields & Properties
        private static readonly Type dclringType=typeof(LiggerBase);
        private string filePath = null;
        private event EventHandler onlineEvent;
        private event EventHandler MessageRecv;
        private bool isDisposed = false;

        private string HashId { get; set; }
        private LWCF<ILigAgent> LClient=null;
        
        private const string SERVICENAME = "Lig.vivitue.Contract.Services.LigAgent";
        private bool connectStatus = false;
        public bool ConnectStatus
        {
            get { return connectStatus; }
        }
        private string clientName = null;
        public string ClientName
        {
            get { return clientName; }
        }
        private int clientID = -2;
        public int ClientID
        {
            get { return this.clientID; }
        }
        private ILigAgent iLig = null;
        private ILigAgent ILig
        {
            get { return iLig; }
        }

        private LigAgentCallback callbackInstance = null;

        private delegate void MessageHandler(string message);
        private delegate void OnlineHandler(LigArgs args);
        #endregion

        #region InnerClasses
        class LigAgentCallback : ILigAgentCallback
        {
            internal LigAgentCallback() { }
            internal event MessageHandler msgReceived;
            internal event OnlineHandler onlined;
            public void OnNotifyMessage(string message)
            {
                if (this.msgReceived != null)
                {
                    this.msgReceived(message);
                }
            }
            public void OnNotifyOnline(LigArgs args)
            {
                if (this.onlined != null)
                {
                    this.onlined(args);
                }
            }
            internal void Release()
            {
                if (this.msgReceived != null)
                {
                    Delegate[] items = this.msgReceived.GetInvocationList();
                    foreach (Delegate item in items)
                    {
                        this.msgReceived -= (MessageHandler)item;
                    }
                }
                if (this.onlined != null)
                {
                    Delegate[] items = this.onlined.GetInvocationList();
                    foreach (Delegate item in items)
                    {
                        this.onlined -= (OnlineHandler)item;
                    }
                }
            }
        }
        #endregion
    }
}
