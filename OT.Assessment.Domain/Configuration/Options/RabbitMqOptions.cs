﻿namespace OT.Assessment.Domain.Configuration.Options;

public class RabbitMqOptions
{
    public string Host { get; set; }
    public ushort Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string CasinoWagerRoutingKey { get; set; }
}