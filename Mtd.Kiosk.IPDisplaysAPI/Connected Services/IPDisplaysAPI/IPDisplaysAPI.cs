using IPDisplaysAPI;
using System.Xml

namespace Mtd.Kiosk.Api.Connected_Services.IPDisplaysAPI
{
	public class IPDisplaysApiClient

	{
		public SignSvrSoapPortClient GetSoapClient(string ledIp)
		{
			var uri = new Uri($"http://{ledIp}/soap1.wsdl");

			var binding = new BasicHttpBinding
			{
				MaxBufferSize = int.MaxValue,
				ReaderQuotas = XmlDictionaryReaderQuotas.Max,
				MaxReceivedMessageSize = int.MaxValue,
				AllowCookies = true,
				CloseTimeout = _timeout,
				OpenTimeout = _timeout,
				ReceiveTimeout = _timeout,
				SendTimeout = _timeout
			};
			var endpointAddress = new EndpointAddress(uri);
			return new SignSvrSoapPortClient(binding, endpointAddress);
		}

	}
}
