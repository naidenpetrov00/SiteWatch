using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using Infrastructure.Cameras.Services.Onvif.Generated.Device;
using Infrastructure.Cameras.Services.Onvif.Generated.Media;
using Infrastructure.Cameras.Services.Onvif.Generated.Ptz;

namespace Infrastructure.Cameras.Services.Onvif;

public static class OnvifClientFactory
{
    private static CustomBinding CreateBinding()
    {
        var encoding = new TextMessageEncodingBindingElement(
            MessageVersion.Soap12WSAddressing10,
            Encoding.UTF8
        );

        var transport = new HttpTransportBindingElement
        {
            MaxReceivedMessageSize = 1024 * 1024
        };

        return new CustomBinding(encoding, transport);
    }

    public static DeviceClient Device(string url) => new(CreateBinding(), new EndpointAddress(url));
    public static MediaClient Media(string url) => new(CreateBinding(), new EndpointAddress(url));
    public static PTZClient Ptz(string url) => new(CreateBinding(), new EndpointAddress(url));
}