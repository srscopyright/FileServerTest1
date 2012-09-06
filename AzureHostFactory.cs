// =====COPYRIGHT=====
// github http copyright text
// =====COPYRIGHT=====
using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using Srs.WebPlatform.Common;

namespace Srs.WebPlatform.WebServices.Printer
{
    /// <summary>
    /// Used to create the service host within Windows Azure using the endpoints that all SRS services should
    /// have available.
    /// A custom host ensures consistency across web services and guarantees a certain level of availability
    /// to SRS customers.
    /// The only difference between this host factory and the CustomHostFactory in the SRS WP is that this one
    /// includes the UseRequestHeadersForMetadataAddressBehavior behavior to enable generating a service
    /// reference or a WSDL from a load-balanced WCF service hosted in the DevFabric or in Windows Azure.
    /// </summary>
    public class AzureHostFactory : CustomHostFactoryV1
    {
        /// <summary>
        /// Creates a ServiceHost for a specified type of service with a specific base address.
        /// </summary>
        /// <param name="serviceType">Specifies the type of service to host.</param>
        /// <param name="baseAddresses">The Array of type Uri that contains the base addresses for the service hosted.</param>
        /// <returns>A ServiceHost for the type of service specified with a specific base address.</returns>
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            ServiceHost customServiceHost = base.CreateServiceHost(serviceType, baseAddresses);
            
            UseRequestHeadersForMetadataAddressBehavior loadBalancedBehavior =
                customServiceHost.Description.Behaviors.Find<UseRequestHeadersForMetadataAddressBehavior>();
            if (null == loadBalancedBehavior)
            {
                loadBalancedBehavior = new UseRequestHeadersForMetadataAddressBehavior();
                loadBalancedBehavior.DefaultPortsByScheme.Add("http", 80);
                loadBalancedBehavior.DefaultPortsByScheme.Add("https", 443);
                customServiceHost.Description.Behaviors.Add(loadBalancedBehavior);
            }

            return customServiceHost;
        }
    }
}
