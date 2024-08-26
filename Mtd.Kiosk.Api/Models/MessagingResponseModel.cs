using Mtd.Kiosk.RealTime.Entities;

namespace Mtd.Kiosk.Api.Models;

/// <summary>
/// Model for the response to the general messaging endpoint.
/// </summary>
public class MessagingResponseModel
{
	/// <summary>
	/// The array of general messages.
	/// </summary>
	public IReadOnlyCollection<SimpleGeneralMessage> GeneralMessages { get; set; }

	/// <summary>
	/// Contructor that augments a GeneralMessage array to a MessagingResponseModel.
	/// </summary>
	/// <param name="messageResult"></param>
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

/// <summary>
/// A simplified version of a general message.
/// </summary>
public class SimpleGeneralMessage
{
	/// <summary>
	/// The stop id of the general message.
	/// </summary>
	public string StopId { get; set; }
	/// <summary>
	/// The message to display.
	/// </summary>
	public string Message { get; set; }

	/// <summary>
	/// Whether to block realtime updates when displaying the message.
	/// </summary>
	public bool BlockRealtime { get; set; }

	/// <summary>
	/// Constructor for SimpleGeneralMessage.
	/// </summary>
	/// <param name="stopId"></param>
	/// <param name="message"></param>
	/// <param name="blockRealtime"></param>
	public SimpleGeneralMessage(string stopId, string message, bool blockRealtime)
	{
		StopId = stopId;
		Message = message;
		BlockRealtime = blockRealtime;
	}
}

