﻿namespace Mtd.Kiosk.Api.Models;

/// <summary>
/// Model for updating a ticket.
/// </summary>
public class UpdatedTicketModel
{
	/// <summary>
	/// The body of the ticket, in markdown format.
	/// </summary>
	public required string MarkdownBody { get; set; }
}
