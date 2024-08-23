using Mtd.Kiosk.RealTime.Entities;

namespace Mtd.Kiosk.Api.Models;

public class MessagingResponseModel
{
	public IReadOnlyCollection<SimpleGeneralMessage> GeneralMessages { get; set; }

	public MessagingResponseModel(GeneralMessage[] messageResult)
	{
		var generalMessages = new List<SimpleGeneralMessage>();
		foreach (var message in messageResult)
		{
			if (message.StopIds != null && message.StopIds.Length > 0)
			{
				foreach (var stopId in message.StopIds)
				{
					var simpleGeneralMessage = new SimpleGeneralMessage(stopId, message.Text, message.BlockRealtime);
					generalMessages.Add(simpleGeneralMessage);
				}
			}
		}

		GeneralMessages = generalMessages;
	}
}

public class SimpleGeneralMessage
{
	public string StopId { get; set; }
	public string Message { get; set; }

	public bool BlockRealtime { get; set; }

	public SimpleGeneralMessage(string stopId, string message, bool blockRealtime)
	{
		StopId = stopId;
		Message = message;
		BlockRealtime = blockRealtime;
	}
}

