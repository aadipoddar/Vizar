﻿namespace Vizar.Shared.Services;

public interface INotificationService
{
	public Task ShowLocalNotification(int id, string title, string subTitle, string description);
}
