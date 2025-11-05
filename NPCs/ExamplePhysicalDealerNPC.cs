using MelonLoader;
using S1API.Entities;
using S1API.Entities.Schedule;
using S1API.Map;
using S1API.Map.ParkingLots;
using S1API.Money;
using S1API.Economy;
using S1API.Entities.NPCs.Northtown;
using S1API.GameTime;
using S1API.Growing;
using S1API.Map.Buildings;
using S1API.Products;
using S1API.Properties;
using S1API.Vehicles;
using UnityEngine;
using System.Linq;

namespace CustomNPCTest.NPCs
{
    /// <summary>
    /// An example S1API NPC that opts into dealer functionality.
    /// Demonstrates dealer configuration, customer assignment, and cash management.
    /// </summary>
    public sealed class ExamplePhysicalDealerNPC : NPC
    {
        public override bool IsPhysical => true;
        public override bool IsDealer => true;
        
        protected override void ConfigurePrefab(NPCPrefabBuilder builder)
        {
            var manorParking = ParkingLotRegistry.Get<ManorParking>();
            var northApartments = Building.Get<NorthApartments>();
            MelonLogger.Msg("Configuring prefab for Example Physical Dealer NPC");
            
            Vector3 posA = new Vector3(-27.6272f, 1.065f, 62.2025f);
            Vector3 spawnPos = new Vector3(-52.6123f, 1.065f, 68f);
            
            builder.WithIdentity("example_physical_dealer_npc", "Dealer", "Smith")
                .WithAppearanceDefaults(av =>
                {
                    av.Gender = 0.5f; // Neutral gender
                    av.Height = 1.0f;
                    av.Weight = 0.5f;
                    av.SkinColor = new Color32(120, 100, 80, 255);
                    av.EyeBallTint = Color.white;
                    av.PupilDilation = 0.8f;
                    av.EyebrowScale = 1.0f;
                    av.EyebrowThickness = 0.8f;
                    av.EyebrowRestingHeight = 0.0f;
                    av.EyebrowRestingAngle = 0.0f;
                    av.LeftEye = (0.6f, 0.6f);
                    av.RightEye = (0.6f, 0.6f);
                    av.HairColor = new Color(0.2f, 0.15f, 0.1f);
                    av.HairPath = "Avatar/Hair/Spiky/Spiky";
                    av.WithFaceLayer("Avatar/Layers/Face/Face_Agitated", Color.black);
                    av.WithBodyLayer("Avatar/Layers/Top/T-Shirt", new Color(0.2f, 0.2f, 0.2f));
                    av.WithBodyLayer("Avatar/Layers/Bottom/Jeans", new Color(0.1f, 0.1f, 0.1f));
                    av.WithAccessoryLayer("Avatar/Accessories/Feet/Sneakers/Sneakers", Color.black);
                })
                .WithSpawnPosition(spawnPos)
                .EnsureDealer()
                .WithDealerDefaults(dd =>
                {
                    dd.WithSigningFee(1000f) // Cost to recruit this dealer
                        .WithCut(0.15f) // Dealer keeps 15% of earnings
                        .WithDealerType(DealerType.PlayerDealer) // Works for the player
                        .WithHomeName("North Apartments") // Home building name
                        .AllowInsufficientQuality(false) // Won't sell below-quality items
                        .AllowExcessQuality(true) // Can sell above-quality items
                        .WithCompletedDealsVariable("dealer_completed_deals"); // Variable to track deals
                })
                .WithRelationshipDefaults(r =>
                {
                    r.WithDelta(2.0f) // Starting relationship
                        .SetUnlocked(false) // Start locked
                        .WithConnections(Get<KyleCooley>(), Get<LudwigMeyer>(), Get<AustinSteiner>())
                        .SetUnlockType(NPCRelationship.UnlockType.DirectApproach); // Can be unlocked via direct approach
                })
                .WithSchedule(plan =>
                {
                    plan.EnsureDealSignal() // Signal for handling deals
                        .WalkTo(posA, 900, faceDestinationDir: true)
                        .StayInBuilding(northApartments, 1100, 120) // Stay at home building
                        .LocationDialogue(posA, 1300)
                        .WalkTo(spawnPos, 1400)
                        .StayInBuilding(northApartments, 1425, 60);
                })
                .WithInventoryDefaults(inv =>
                {
                    // Startup items that will always be in inventory when spawned
                    inv.WithStartupItems("banana", "baseballbat")
                        // Random cash between $100 and $1000 for dealer operations
                        .WithRandomCash(min: 100, max: 1000)
                        // Preserve inventory across sleep cycles
                        .WithClearInventoryEachNight(false);
                });
        }
        
        public ExamplePhysicalDealerNPC() : base(
            id: "example_physical_dealer_npc",
            firstName: "Dealer",
            lastName: "Example",
            icon: null)
        {
        }

        protected override void OnCreated()
        {
            try
            {
                base.OnCreated();
                ApplyAppearance();
                Appearance.Build();
                
                SendTextMessage("Hello, I'm a dealer NPC! I can help you distribute products to customers.");

                // Subscribe to dealer events
                Dealer.OnRecruited(() =>
                {
                    MelonLogger.Msg($"Dealer {ID} has been recruited!");
                    SendTextMessage("I'm ready to work for you!");
                });

                Dealer.OnContractAccepted(() =>
                {
                    MelonLogger.Msg($"Dealer {ID} accepted a new contract!");
                });

                Dealer.OnRecommended(() =>
                {
                    MelonLogger.Msg($"Dealer {ID} has been recommended!");
                });

                Aggressiveness = 2f;
                Region = Region.Northtown;

                Schedule.Enable();
                Schedule.InitializeActions();
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"ExamplePhysicalDealerNPC OnCreated failed: {ex.Message}");
                MelonLogger.Error($"StackTrace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Applies an appearance. Tweak the values below to your liking.
        /// </summary>
        private void ApplyAppearance()
        {
            // Core biometrics
            Appearance
                .Set<S1API.Entities.Appearances.CustomizationFields.Gender>(0.5f) // 0..1
                .Set<S1API.Entities.Appearances.CustomizationFields.Height>(1.0f)
                .Set<S1API.Entities.Appearances.CustomizationFields.Weight>(0.5f)
                .Set<S1API.Entities.Appearances.CustomizationFields.SkinColor>(new Color32(120, 100, 80, 255))
                .Set<S1API.Entities.Appearances.CustomizationFields.EyeBallTint>(Color.white)
                .Set<S1API.Entities.Appearances.CustomizationFields.PupilDilation>(0.8f)
                .Set<S1API.Entities.Appearances.CustomizationFields.EyebrowScale>(1.0f)
                .Set<S1API.Entities.Appearances.CustomizationFields.EyebrowThickness>(0.8f)
                .Set<S1API.Entities.Appearances.CustomizationFields.EyebrowRestingHeight>(0.0f)
                .Set<S1API.Entities.Appearances.CustomizationFields.EyebrowRestingAngle>(0.0f)
                .Set<S1API.Entities.Appearances.CustomizationFields.EyeLidRestingStateLeft>((0.6f, 0.6f))
                .Set<S1API.Entities.Appearances.CustomizationFields.EyeLidRestingStateRight>((0.6f, 0.6f))
                .Set<S1API.Entities.Appearances.CustomizationFields.HairColor>(new Color(0.2f, 0.15f, 0.1f))
                .Set<S1API.Entities.Appearances.CustomizationFields.HairStyle>("Avatar/Hair/Spiky/Spiky")
                .WithFaceLayer<S1API.Entities.Appearances.FaceLayerFields.Face>("Avatar/Layers/Face/Face_Agitated", Color.black)
                .WithBodyLayer<S1API.Entities.Appearances.BodyLayerFields.Shirts>("Avatar/Layers/Top/T-Shirt", new Color(0.2f, 0.2f, 0.2f))
                .WithBodyLayer<S1API.Entities.Appearances.BodyLayerFields.Pants>("Avatar/Layers/Bottom/Jeans", new Color(0.1f, 0.1f, 0.1f))
                .WithAccessoryLayer<S1API.Entities.Appearances.AccessoryFields.Feet>("Avatar/Accessories/Feet/Sneakers/Sneakers", Color.black);
        }
    }
}

