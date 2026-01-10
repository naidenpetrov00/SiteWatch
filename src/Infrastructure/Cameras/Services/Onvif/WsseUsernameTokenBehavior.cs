using System;
using System.Security;
using System.Security.Cryptography;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Xml;

namespace Infrastructure.Cameras.Services.Onvif;

public sealed class WsseUsernameTokenBehavior(string username, string password) : IEndpointBehavior
{
    public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        => clientRuntime.ClientMessageInspectors.Add(new Inspector(username, password));

    public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
    {
    }

    public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
    {
    }

    public void Validate(ServiceEndpoint endpoint)
    {
    }

    private sealed class Inspector(string u, string p) : IClientMessageInspector
    {
        public object BeforeSendRequest(ref Message request, System.ServiceModel.IClientChannel channel)
        {
            var created = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            var nonceBytes = RandomNumberGenerator.GetBytes(16);
            var nonceB64 = Convert.ToBase64String(nonceBytes);

            var createdBytes = Encoding.UTF8.GetBytes(created);
            var passBytes = Encoding.UTF8.GetBytes(p);

            var combined = new byte[nonceBytes.Length + createdBytes.Length + passBytes.Length];
            Buffer.BlockCopy(nonceBytes, 0, combined, 0, nonceBytes.Length);
            Buffer.BlockCopy(createdBytes, 0, combined, nonceBytes.Length, createdBytes.Length);
            Buffer.BlockCopy(passBytes, 0, combined, nonceBytes.Length + createdBytes.Length, passBytes.Length);

            var digestB64 = Convert.ToBase64String(SHA1.HashData(combined));

            var doc = new XmlDocument();
            doc.LoadXml($"""
                         <wsse:Security SOAP-ENV:mustUnderstand='1'
                          xmlns:wsse='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd'
                          xmlns:wsu='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd'
                          xmlns:SOAP-ENV='http://www.w3.org/2003/05/soap-envelope'>
                           <wsse:UsernameToken>
                             <wsse:Username>{SecurityElement.Escape(u)}</wsse:Username>
                             <wsse:Password Type='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordDigest'>{digestB64}</wsse:Password>
                             <wsse:Nonce EncodingType='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary'>{nonceB64}</wsse:Nonce>
                             <wsu:Created>{created}</wsu:Created>
                           </wsse:UsernameToken>
                         </wsse:Security>
                         """);

            request.Headers.Add(MessageHeader.CreateHeader(
                "Security",
                "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd",
                doc.DocumentElement!,
                mustUnderstand: true
            ));

            return null!;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
        }
    }
}