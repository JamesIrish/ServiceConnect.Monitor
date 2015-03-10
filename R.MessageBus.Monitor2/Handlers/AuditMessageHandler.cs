﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using R.MessageBus.Monitor.Interfaces;
using R.MessageBus.Monitor.Models;

namespace R.MessageBus.Monitor.Handlers
{
    public class AuditMessageHandler : IDisposable
    {
        private readonly IAuditRepository _auditRepository;
        private readonly IHubContext _hub;
        private readonly Timer _timer;
        private readonly IList<Audit> _audits = new List<Audit>();
        private readonly object _lock = new object();

        public AuditMessageHandler(IAuditRepository auditRepository, IHubContext hub)
        {
            _auditRepository = auditRepository;
            _hub = hub;
            var callback = new TimerCallback(SendAudits);
            _timer = new Timer(callback, null, 0, 2500);
        }

        public void Execute(string message, IDictionary<string, string> headers, string host)
        {
            lock (_lock)
            {
                var audit = new Audit
                {
                    Body = message,
                    DestinationAddress = headers["DestinationAddress"],
                    DestinationMachine = headers["DestinationMachine"],
                    FullTypeName = headers["FullTypeName"],
                    MessageId = headers["MessageId"],
                    MessageType = headers["MessageType"],
                    SourceAddress = headers["SourceAddress"],
                    SourceMachine = headers["SourceMachine"],
                    TypeName = headers["TypeName"],
                    ConsumerType = headers["ConsumerType"],
                    TimeProcessed = DateTime.ParseExact(headers["TimeProcessed"], "O", CultureInfo.InvariantCulture),
                    TimeReceived = DateTime.ParseExact(headers["TimeReceived"], "O", CultureInfo.InvariantCulture),
                    TimeSent = DateTime.ParseExact(headers["TimeSent"], "O", CultureInfo.InvariantCulture),
                    Language = headers["Language"],
                    CorrelationId = JsonConvert.DeserializeObject<Message>(message).CorrelationId,
                    Server = host,
                    Headers = headers
                };

                _auditRepository.InsertAudit(audit);
                _audits.Add(audit);
            }
        }

        private void SendAudits(object state)
        {
            lock (_lock)
            {
                if (_audits.Count > 0)
                {
                    _hub.Clients.All.Audits(_audits);
                    _audits.Clear();
                }
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}