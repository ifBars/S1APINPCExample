using S1API.Entities;
using UnityEngine;

namespace CustomNPCTest.NPCs
{
    /// Minimal non-physical NPC used as the "unknown client" contact
    /// for Escalating Orders SMS conversations.
    public sealed class UnknownClientNpc : NPC
    {
        public static UnknownClientNpc? Instance { get; private set; }

        public override bool IsPhysical => false;

        protected override void ConfigurePrefab(NPCPrefabBuilder builder)
        {
            // Identity only; no schedule / customer / dealer
            builder.WithIdentity("eo_unknown_client", "Unknown", "Client")
                .WithIcon(null);
        }

        protected override void OnCreated()
        {
            base.OnCreated();
            Instance = this;
            SendTextMessage("Hello from unknown client!");
        }
    }
}