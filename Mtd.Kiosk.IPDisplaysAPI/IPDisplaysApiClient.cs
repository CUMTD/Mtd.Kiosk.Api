using IPDisplaysAPI;
using System.Xml;

namespace Mtd.Kiosk.IPDisplaysAPI
{
	public class IPDisplaysApiClient
	{
		public SignSvrSoapPortClient GetSoapClient()
		{
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
			var endpointAddress = new EndpointAddress(_uri);
			return new SignSvrSoapPortClient(binding, endpointAddress);
		}

	}
}
