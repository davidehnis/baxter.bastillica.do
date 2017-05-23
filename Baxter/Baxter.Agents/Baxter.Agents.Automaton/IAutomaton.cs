using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace Baxter.Agents.Automaton
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name
    //       "IService1" in both code and config file together.
    [ServiceContract]
    public interface IAutomaton
    {
        [OperationContract]
        string GetData(int value);

        [OperationContract]
        CompositeType GetDataUsingDataContract(CompositeType composite);

        [OperationContract]
        void Post(IAutomatonObserver ob,
            string script, IEnumerable<object> data);

        [OperationContract]
        void PostFuture(IAutomatonObserver ob, string script, IEnumerable<object> data, DateTime runAt);
    }

    [ServiceContract]
    public interface IAutomatonObserver
    {
        [OperationContract]
        void Notify(object ev);

        [OperationContract]
        void Send(object data);
    }

    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    [DataContract]
    public class CompositeType
    {
        private bool boolValue = true;
        private string stringValue = "Hello ";

        [DataMember]
        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        [DataMember]
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }
    }
}