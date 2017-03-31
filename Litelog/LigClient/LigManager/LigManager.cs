/***********************************************************************
 * Licensed to yourself if you like.....
 * 
 * Project    ： Lig - Litelog [LigClientManager]
 * Created by ： vivitue     
 * CreateDate ： 
 * ReviseData :  
 * References :  
 * 
 * Version    ： 1.0.0.0  
 * Description： 
 * ReviseDscpt:
 *              
 * *********************************************************************/
using System;
using System.ServiceModel;
using System.Collections.Generic;
using System.Text;
using Lig.vivitue.Contract.Services;
using Lig.vivitue.Contract.Data;

namespace Lig.vivitue.Client
{
    public class LigManager : IDisposable
    {
        #region Constructor
        private LigManager() { }
        #endregion

        #region Desconstructors
        ~LigManager()
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
            Console.WriteLine(this.CurrentTime + "Client was disposed! Clientself HashCode {0}", this.GetHashCode());
            this.isDisposed = true;
        }
        #endregion

        #region CommonInterfaces
        /// <summary>
        /// 
        /// </summary>
        public void ConnectServer()
        {
            try
            {
                this.callbackInstance = new LigAgentCallback();
                this.SubscribedEvents();
                //this.chnl = new ChannelFactory<ILigAgent>(SERVICENAME);
                this.chnl = new DuplexChannelFactory<ILigAgent>(callbackInstance,SERVICENAME);
                if (chnl.State == CommunicationState.Created)
                {
                    this.iLig = chnl.CreateChannel();
                    if (chnl.State == CommunicationState.Opened)
                    {
                        this.HashId = this.GetHashCode().ToString();
                        this.iLig.SayHelloToServer("LigManager Connecting : ClientHash : " + this.GetHashCode().ToString());
                        this.iLig.RegisterClient(this.GetType().Name);
                        this.connectStatus = true;
                    }
                }
            }
            catch (Exception ex)
            {
                this.chnl = null;
                this.callbackInstance.Release();
                this.callbackInstance = null;
                this.connectStatus = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void DisconnectServer()
        {
            try
            {
                if (this.chnl == null) return;
                if (this.chnl.State != CommunicationState.Closed)
                {
                    this.chnl.Close();
                }
                this.chnl = null;
                this.callbackInstance.Release();
                this.callbackInstance = null;
            }
            catch (Exception ex)
            {

            }
            finally
            {
                this.connectStatus = false;
                this.chnl = null;
            }
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
            
        }
        #endregion

        #region OnCallbacks
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private void OnNotifyMessage(string message)
        {
            Console.WriteLine(this.CurrentTime + "Message Received from LigServer!");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        private void OnNotifyOnline(LigArgs args)
        {
            this.PublicizedMessage(args);
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
                builder.AppendLine(CurrentTime + "Attention : Message received from LigServer!");
                builder.AppendLine("");
                builder.AppendLine(CurrentTime + "ClientID    : " + args.ClientID.ToString());
                builder.AppendLine(CurrentTime + "ClientName  : " + args.ClientName);
                builder.AppendLine(CurrentTime + "HashID      :" + args.HashID.ToString());
                builder.AppendLine(CurrentTime + "Message     :" + args.Message);
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
        private bool isDisposed = false;
        private string CurrentTime
        {
            get
            {
                return DateTime.Now.ToString() + " : ";
            }
        }
        public string HashId { get; set; }
        private ChannelFactory<ILigAgent> chnl = null;
        private const string SERVICENAME = "Lig.vivitue.Contract.Services.LigAgent";
        private bool connectStatus = false;
        public bool ConnectStatus
        {
            get { return connectStatus; }
        }
        private ILigAgent iLig = null;
        public ILigAgent ILig
        {
            get { return iLig; }
        }
        private static LigManager ligInstance = null;
        public static LigManager LigInstance
        {
            get
            {
                if (ligInstance == null)
                {
                    ligInstance = new LigManager();
                }
                return ligInstance;
            }
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
