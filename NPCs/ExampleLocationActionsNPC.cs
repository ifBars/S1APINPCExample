using System;
using MelonLoader;
using S1API.Entities;
using S1API.Entities.Equippables;
using S1API.Entities.NPCs.Northtown;
using S1API.Entities.Schedule;
using S1API.Map;
using S1API.Map.Buildings;
using UnityEngine;

namespace CustomNPCTest.NPCs
{
    /// <summary>
    /// Test NPC that exercises all location-based arrive actions with their new parameters:
    /// SmokeBreak, Graffiti (with region), Drinking (with drink path), HoldItem (with item path).
    /// </summary>
    public sealed class ExampleLocationActionsNPC : NPC
    {
        public override bool IsPhysical => true;

        protected override void ConfigurePrefab(NPCPrefabBuilder builder)
        {
            var northApartments = Building.Get<NorthApartments>();
            Vector3 spawnPos = new Vector3(-53.57f, 1.065f, 67.8f);

            // Spots for each action type (Northtown area)
            Vector3 smokeSpot = new Vector3(-28.06f, 1.065f, 62.07f);
            Vector3 phoneSpot = new Vector3(-35f, 1.065f, 58f);
            Vector3 coffeeSpot = new Vector3(-42f, 1.065f, 65f);
            Vector3 graffitiSpot = new Vector3(-50f, 1.065f, 55f);
            Vector3 flashlightSpot = new Vector3(-55f, 1.065f, 70f);
            Vector3 beerSpot = new Vector3(-60f, 1.065f, 62f);

            builder.WithIdentity("example_location_actions_npc", "Sam", "Actions")
                .WithAppearanceDefaults(av =>
                {
                    av.Gender = 0.0f;
                    av.Height = 1.0f;
                    av.Weight = 0.4f;
                    var skinColor = new Color32(140, 110, 90, 255);
                    av.SkinColor = skinColor;
                    av.LeftEyeLidColor = av.SkinColor;
                    av.RightEyeLidColor = av.SkinColor;
                    av.EyeBallTint = Color.white;
                    av.PupilDilation = 0.7f;
                    av.EyebrowScale = 0.9f;
                    av.EyebrowThickness = 0.55f;
                    av.EyebrowRestingHeight = 0.12f;
                    av.EyebrowRestingAngle = 0.03f;
                    av.LeftEye = (0.5f, 0.5f);
                    av.RightEye = (0.5f, 0.5f);
                    av.HairColor = new Color(0.15f, 0.12f, 0.08f);
                    av.HairPath = "Avatar/Hair/Spiky/Spiky";
                    av.WithFaceLayer("Avatar/Layers/Face/Face_Agitated", Color.black);
                    av.WithBodyLayer("Avatar/Layers/Top/T-Shirt", Color.green);
                    av.WithBodyLayer("Avatar/Layers/Bottom/Jeans", new Color(0.2f, 0.25f, 0.35f));
                    av.WithAccessoryLayer("Avatar/Accessories/Feet/Sneakers/Sneakers", Color.black);
                    av.WithImpostor("Mac");
                })
                .WithSpawnPosition(spawnPos)
                .EnsureSmokeBreak(debugMode: true)
                .EnsureGraffiti()
                .EnsureDrinking()
                .EnsureItemHolding()
                .WithSchedule(plan =>
                {
                    plan.EnsureDealSignal()
                        // 8:00 - Smoke break (no extra params)
                        .LocationBased(smokeSpot, 800, 30)
                            .Within(1.5f)
                            .Named("MorningSmoke")
                            .OnArriveSmokeBreak()
                        // 9:30 - Hold phone (explicit item)
                        .LocationBased(phoneSpot, 930, 30)
                            .WithItem(EquippablePath.Phone_Lowered)
                            .OnArriveHoldItem()
                        // 11:00 - Drink coffee (explicit drink)
                        .LocationBased(coffeeSpot, 1100, 30)
                            .WithDrink(EquippablePath.Coffee)
                            .OnArriveDrinking()
                        // 14:00 - Graffiti (pick surface in Northtown region)
                        .LocationBased(graffitiSpot, 1400, 45)
                            .WithSpraySurfaceInRegion(Region.Northtown)
                            .Named("AfternoonGraffiti")
                            .OnArriveGraffiti()
                        // 16:00 - Hold flashlight (different item)
                        .LocationBased(flashlightSpot, 1600, 30)
                            .WithItem(EquippablePath.Flashlight)
                            .OnArriveHoldItem()
                        // 18:00 - Drink beer (different drink)
                        .LocationBased(beerSpot, 1800, 30)
                            .WithDrink(EquippablePath.Beer)
                            .OnArriveDrinking();
                })
                .WithInventoryDefaults(inv =>
                {
                    inv.WithStartupItems("banana", "cuke")
                        .WithRandomCash(min: 100, max: 500)
                        .WithClearInventoryEachNight(false);
                });
        }

        protected override void OnCreated()
        {
            try
            {
                base.OnCreated();
                Appearance.Build();

                SendTextMessage("ExampleLocationActionsNPC loaded - testing SmokeBreak, Graffiti, Drinking, HoldItem with params!");

                Aggressiveness = 2f;
                Region = Region.Northtown;

                Schedule.Enable();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"ExampleLocationActionsNPC OnCreated failed: {ex.Message}");
                MelonLogger.Error($"StackTrace: {ex.StackTrace}");
            }
        }
    }
}
