
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
using System.ServiceModel;
using System.Runtime.Serialization;

using L.vivitue;
using L.vivitue.Common;

namespace L.vivitue.Common.WCF
{
    public class LWCF<T>
    {
        #region Constructors

        public LWCF(string serviceClass)
        {
            this.serviceClass = serviceClass;
            this.wcfType = WCFClientType.Default;
            this.callbackInstance = null;
            this.configPath = null;
        }

        public LWCF(string serviceClass, string configPath)
        {
            this.serviceClass = serviceClass;
            this.wcfType = WCFClientType.Single;
            this.callbackInstance = null;
            this.configPath = configPath;
        }

        public LWCF(InstanceContext instance, WCFClientType wcfType,string serviceClass, string configPath)
        {
            this.callbackInstance = instance;
            this.wcfType = wcfType;
            this.serviceClass = serviceClass;
            this.configPath = configPath;
            
        }

        #endregion Constructor

        #region PrivateMethods

        /// <summary>
        /// Create LChannelFactory
        /// </summary>
        /// <returns></returns>
        private LChannelFactory<T> CreateLChannelFactory()
        {
            return new LChannelFactory<T>(this.serviceClass, this.configPath);
        }

        /// <summary>
        /// Create DuplexChannel - Need set ServiceContract the right status!
        /// </summary>
        /// <returns></returns>
        private LDuplex<T> CreateDuplexChannel()
        {
            // Attention 
            LDuplex<T>.ServiceInfo = this.serviceClass;
            LDuplex<T>.ConfigInfo = this.configPath;

            return new LDuplex<T>(this.callbackInstance,this.serviceClass,this.configPath);
        }

        /// <summary>
        /// Create ChannelFactory - Only for default!
        /// </summary>
        /// <returns></returns>
        private ChannelFactory<T> CreateComchannel()
        {
            return new ChannelFactory<T>(this.serviceClass);
        }

        /// <summary>
        /// Get channel instance according WCFClientType
        /// </summary>
        /// <param name="type">WCFClientType</param>
        /// <returns></returns>
        private ChannelFactory<T> GetChannelInstance(WCFClientType type)
        {
            WCFClientType wtype = type;
            ChannelFactory<T> channel = null;
            try
            {
                switch (wtype)
                {
                    case WCFClientType.Single:
                        channel = CreateLChannelFactory();
                        break;
                    case WCFClientType.Duplex:
                        channel = CreateDuplexChannel();
                        break;
                    case WCFClientType.Default:
                        channel = CreateComchannel();
                        break;
                    default:
                        channel = CreateComchannel();
                        break;
                }
            }
            catch (Exception ex)
            {
                channel = null;
                Innerlig.Error(dclringType, "GetChannelInstance failed!", ex);
            }
            return channel;
        }

        /// <summary>
        /// check using params! -
        /// </summary>
        private void CheckValidityAndThrowException()
        {
            
            if (string.IsNullOrEmpty(ServiceClassName))
            {
                throw new Exception("Error : ServiceClassName has the none value!");
            }

            if (this.wcfType == WCFClientType.Duplex)
            {
                if (this.callbackInstance == null)
                {
                    throw new Exception("Error : Callbackinstance has the none value!");
                }
            }

        }
        #endregion

        #region PublicMethods

        /// <summary>
        /// 
        /// </summary>
        public void ConnectServer()
        {
            // Check config and service fullname!

            CheckValidityAndThrowException();

            // Clear last time record
            this.linkElapsedTime = 0;
            int beginTime = SysInfo.TickCount;

            if (!linkStatus)
            {
                try
                {
                    channelFactory = GetChannelInstance(this.wcfType);
                    if (channelFactory.State == CommunicationState.Created)
                    {
                        interfaceInstance = channelFactory.CreateChannel();
                        if (channelFactory.State == CommunicationState.Opened)
                        {
                            linkStatus = true;
                        }
                        else
                        {
                            linkStatus = false;
                        }
                    }
                }
                catch (Exception e) 
                {
                    linkStatus = false;
                    throw new Exception(e.Message);
                }
            }
            // Get link time
            this.linkElapsedTime = SysInfo.TickCount - beginTime;
        }

        /// <summary>
        /// 
        /// </summary>
        public void DisconnectServer()
        {
            // Clear last time record
            this.unLinkElapsedTime = 0;
            int beginTime = this.GetTickCount();

            if (linkStatus)
            {
                try
                {
                    if (channelFactory.State != CommunicationState.Closed)
                    {
                        channelFactory.Close();
                        channelFactory = null;
                        linkStatus = false;
                    }
                    if (callbackInstance != null)
                    {
                        if (callbackInstance.State != CommunicationState.Closed)
                        {
                            callbackInstance.Close();
                        }
                        callbackInstance = null;
                    }
                }
                catch (Exception ex)
                {
                    Innerlig.Error(dclringType, "DisconnectServer failed!", ex);
                }
                linkStatus = false;
            }
            // GetUnlinkTime 
            this.unLinkElapsedTime = this.GetTickCount() - beginTime;
        }

        /// <summary>
        /// Get client instance
        /// </summary>
        /// <returns></returns>
        public T GetInstance()
        {
            return interfaceInstance;
        }

        #endregion

        #region Fields & Properties
        private readonly Type dclringType=typeof(T);
        private string serviceClass = null;
        private string configPath = null;
        private WCFClientType wcfType = WCFClientType.Default;

        private ChannelFactory<T> channelFactory = null;
        private InstanceContext callbackInstance = null;

        /// <summary>
        /// Interface instance
        /// </summary>
        private T interfaceInstance = default(T);

        private bool linkStatus = false;

        private int linkElapsedTime = 0;
        public int LinkElapsedTime
        {
            get { return linkElapsedTime; }
        }
        private int unLinkElapsedTime = 0;
        public int UnLinkElapsedTime
        {
            get { return unLinkElapsedTime; }
        }

        /// <summary>
        /// Service full class name
        /// </summary>
        public string ServiceClassName
        {
            get { return serviceClass; }
        }

        /// <summary>
        /// User config for WCF Client
        /// </summary>
        public string ConfigPath
        {
            get { return configPath; }
        }

        /// <summary>
        /// Link status of current WCF server & client
        /// </summary>
        public bool LinkStatus
        {
            get { return linkStatus; }
        }

        /// <summary>
        /// Callback instance of client
        /// </summary>
        public InstanceContext CallbackInstance
        {
            get { return CallbackInstance; }
        }

        /// <summary>
        /// ChannelFactory type
        /// </summary>
        public WCFClientType WcfType
        {
            get { return wcfType; }
        }
        #endregion Fields
    }

    public enum WCFClientType
    {
        Default = 0,
        Single = 1,
        Duplex = 2
    }
}