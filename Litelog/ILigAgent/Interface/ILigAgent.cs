/***********************************************************************
 * Licensed to yourself if you like.....
 * 
 * Project    ： Lig - Litelog
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
using Lig.vivitue.Contract.Data;
namespace Lig.vivitue.Contract.Services
{
    [ServiceContract]
    public interface ILigAgent
    {
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
}
