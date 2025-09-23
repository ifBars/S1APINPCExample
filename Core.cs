using CustomNPCTest.NPCs;
using CustomNPCTest.Utils;
using MelonLoader;
using S1API;
using S1API.Entities;
using S1API.Entities.NPCs.Northtown;
using S1API.Internal.Utils;
using UnityEngine;

[assembly: MelonInfo(typeof(CustomNPCTest.Core), Constants.MOD_NAME, Constants.MOD_VERSION, Constants.MOD_AUTHOR)]
[assembly: MelonGame(Constants.Game.GAME_STUDIO, Constants.Game.GAME_NAME)]

namespace CustomNPCTest
{
    public class Core : MelonMod
    {
        public static Core? Instance { get; private set; }

        public override void OnInitializeMelon()
        {
            Instance = this;
            MelonLogger.Msg("CustomNPCTest mod initialized");
        }

        public override void OnApplicationQuit()
        {
            Instance = null;
        }
    }
}