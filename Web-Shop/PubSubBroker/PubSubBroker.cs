using SoCreate.ServiceFabric.PubSub;
using System.Fabric;

namespace PubSubBroker
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class PubSubBroker : BrokerService
    {
        public PubSubBroker(StatefulServiceContext context)
           : base(context)
        {
            // optional: provide a logging callback
            ServiceEventSourceMessageCallback = message => ServiceEventSource.Current.ServiceMessage(context, message);
        }
    }
}
