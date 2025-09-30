using MelonLoader;
using S1API.Entities;
using S1API.Entities.Schedule;
using S1API.Map;
using S1API.Money;
using S1API.Economy;
using S1API.Entities.NPCs.Northtown;
using S1API.GameTime;
using S1API.Growing;
using S1API.Map.Buildings;
using S1API.Products;
using S1API.Properties;
using UnityEngine;

namespace CustomNPCTest.NPCs
{
    /// <summary>
    /// An example S1API NPC that opts into a physical rig.
    /// Demonstrates movement and inventory usage.
    /// </summary>
    public sealed class ExamplePhysicalNPC1 : NPC
    {
        protected override bool IsPhysical => true;
        
        protected override void ConfigurePrefab(NPCPrefabBuilder builder)
        {
            MelonLogger.Msg("Configuring prefab for NPC 1");
            Vector3 posA = new Vector3(-28.060f, 1.065f, 62.070f);
            Vector3 spawnPos = new Vector3(-53.5701f, 1.065f, 67.7955f);
            builder.WithIdentity("example_physical_npc1", "Alex", "Test1")
                .WithAppearanceDefaults(av =>
                {
                    av.Gender = 0.0f;
                    av.Height = 1.0f;
                    av.Weight = 0.36f;
                    av.SkinColor = new Color32(150, 120, 95, 255);
                    av.EyeBallTint = Color.white;
                    av.PupilDilation = 0.66f;
                    av.EyebrowScale = 0.85f;
                    av.EyebrowThickness = 0.6f;
                    av.EyebrowRestingHeight = 0.1f;
                    av.EyebrowRestingAngle = 0.05f;
                    av.LeftEye = (0.5f, 0.5f);
                    av.RightEye = (0.5f, 0.5f);
                    av.HairColor = new Color(0.1f, 0.1f, 0.1f);
                    av.HairPath = "Avatar/Hair/Spiky/Spiky";
                    av.WithFaceLayer("Avatar/Layers/Face/Face_Agitated", Color.black);
                    av.WithFaceLayer("Avatar/Layers/Face/Freckles", Color.blue);
                    av.WithBodyLayer("Avatar/Layers/Top/T-Shirt", Color.red);
                    av.WithBodyLayer("Avatar/Layers/Bottom/Jeans", new Color(0.15f, 0.2f, 0.3f));
                    av.WithAccessoryLayer("Avatar/Accessories/Feet/Sneakers/Sneakers", Color.red);
                })
                .WithSpawnPosition(spawnPos)
                .EnsureCustomer()
                .WithCustomerDefaults(cd =>
                {
                    cd.WithSpending(minWeekly: 400f, maxWeekly: 1000f)
                        .WithOrdersPerWeek(1, 4)
                        .WithPreferredOrderDay(Day.Sunday)
                        .WithOrderTime(900)
                        .WithStandards(CustomerStandard.VeryLow)
                        .AllowDirectApproach(true)
                        .GuaranteeFirstSample(true)
                        .WithMutualRelationRequirement(minAt50: 2.5f, maxAt100: 4.0f)
                        .WithCallPoliceChance(0.15f)
                        .WithDependence(baseAddiction: 0.1f, dependenceMultiplier: 1.1f)
                        .WithAffinities(new[]
                        {
                            (DrugType.Marijuana, 0.45f), (DrugType.Cocaine, -0.2f)
                        })
                        // .WithPreferredPropertiesById("Munchies", "Energizing", "Cyclopean");
                        .WithPreferredProperties(Property.Munchies, Property.Energizing, Property.Cyclopean);
                })
                .WithRelationshipDefaults(r =>
                {
                    r.WithDelta(1.5f)
                        .SetUnlocked(false)
                        .SetUnlockType(NPCRelationship.UnlockType.DirectApproach)
                        // .WithConnectionsById("kyle_cooley", "ludwig_meyer", "austin_steiner")
                        .WithConnections(Get<KyleCooley>(), Get<LudwigMeyer>(), Get<AustinSteiner>());
                })
                .WithSchedule(plan =>
                {
                    plan.EnsureDealSignal()
                        .UseVendingMachine(900)
                        .WalkTo(posA, 925, faceDestinationDir: true)
                        .StayInBuilding(Building.Get<NorthApartments>(), 1100)
                        .LocationDialogue(posA, 1300)
                        .UseVendingMachine(1400)
                        .StayInBuilding(Building.Get<NorthApartments>(), 1425, 240);
                });
        }
        
        public ExamplePhysicalNPC1() : base(
            id: "example_physical_npc1",
            firstName: "Alex",
            lastName: "Test1",
            icon: null)
        {
        }

        protected override void OnCreated()
        {
            try
            {
                base.OnCreated();
                ApplyConsistentAppearance();
				Appearance.Build();
                
                SendTextMessage("Hello from physical NPC 1!");

                Dialogue.BuildAndSetDatabase(db => {
                    db.WithModuleEntry("Reactions", "GREETING", "Welcome.");
                });
                Dialogue.BuildAndRegisterContainer("AlexShop", c => {
                    c.AddNode("ENTRY", "Want some info for $100?", ch => {
                        ch.Add("PAY_FOR_INFO", "Pay $100", "INFO_NODE")
                            .Add("NO_THANKS", "No thanks", "EXIT");
                    });
                    c.AddNode("INFO_NODE", "Get scammed nerd.", ch => {
                        ch.Add("BYE", "Thanks", "EXIT");
                    });
                    c.AddNode("NOT_ENOUGH", "You don't have enough cash.", ch => {
                        ch.Add("BACK", "I'll come back.", "ENTRY");
                    });
                    c.AddNode("EXIT", "See you.");
                });

                Dialogue.OnChoiceSelected("PAY_FOR_INFO", () =>
                {
                    const float price = 100f;
                    var balance = Money.GetCashBalance();
                    if (balance >= price)
                    {
                        Money.ChangeCashBalance(-price, visualizeChange: true, playCashSound: true);
                        Dialogue.JumpTo("AlexShop", "INFO_NODE");
                    }
                    else
                    {
                        Dialogue.JumpTo("AlexShop", "NOT_ENOUGH");
                    }
                    
                });
                
                Dialogue.OnNodeDisplayed("INFO_NODE", () => {
                    // Ran when "Get scammed nerd." is shown
                });

                Dialogue.OnChoiceSelected("BYE", () =>
                {
                    Dialogue.StopOverride();
                    SendTextMessage("You got scammed");
                });

                Dialogue.UseContainerOnInteract("AlexShop");
                Aggressiveness = 3f;
                Region = Region.Northtown;

                // Customer.RequestProduct();
                
                Schedule.Enable();
                Schedule.InitializeActions();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"ExamplePhysicalNPC OnCreated failed: {ex.Message}");
                MelonLogger.Error($"StackTrace: {ex.StackTrace}");
            }
        }

		/// <summary>
		/// Applies a consistent appearance. Tweak the values below to your liking.
		/// </summary>
		private void ApplyConsistentAppearance()
		{
            // Core biometrics
            Appearance
                .Set<S1API.Entities.Appearances.CustomizationFields.Gender>(0.0f) // 0..1
                .Set<S1API.Entities.Appearances.CustomizationFields.Height>(1.0f)
                .Set<S1API.Entities.Appearances.CustomizationFields.Weight>(0.35f)
                .Set<S1API.Entities.Appearances.CustomizationFields.SkinColor>(new Color32(150, 120, 95, 255))
                .Set<S1API.Entities.Appearances.CustomizationFields.EyeBallTint>(Color.white)
                .Set<S1API.Entities.Appearances.CustomizationFields.PupilDilation>(0.66f)
                .Set<S1API.Entities.Appearances.CustomizationFields.EyebrowScale>(0.85f)
                .Set<S1API.Entities.Appearances.CustomizationFields.EyebrowThickness>(0.6f)
                .Set<S1API.Entities.Appearances.CustomizationFields.EyebrowRestingHeight>(0.1f)
                .Set<S1API.Entities.Appearances.CustomizationFields.EyebrowRestingAngle>(0.05f)
                .Set<S1API.Entities.Appearances.CustomizationFields.EyeLidRestingStateLeft>((0.5f, 0.5f))
                .Set<S1API.Entities.Appearances.CustomizationFields.EyeLidRestingStateRight>((0.5f, 0.5f))
                .Set<S1API.Entities.Appearances.CustomizationFields.HairColor>(new Color(0.1f, 0.1f, 0.1f))
                .Set<S1API.Entities.Appearances.CustomizationFields.HairStyle>("Avatar/Hair/Spiky/Spiky")
                .WithFaceLayer<S1API.Entities.Appearances.FaceLayerFields.Face>("Avatar/Layers/Face/Face_Agitated", Color.black)
                .WithFaceLayer<S1API.Entities.Appearances.FaceLayerFields.FacialHair>("Avatar/Layers/Face/Freckles", Color.blue)
                .WithBodyLayer<S1API.Entities.Appearances.BodyLayerFields.Shirts>("Avatar/Layers/Top/T-Shirt", Color.red)
                .WithBodyLayer<S1API.Entities.Appearances.BodyLayerFields.Pants>("Avatar/Layers/Bottom/Jeans", new Color(0.15f, 0.2f, 0.3f))
                .WithAccessoryLayer<S1API.Entities.Appearances.AccessoryFields.Feet>("Avatar/Accessories/Feet/Sneakers/Sneakers", Color.red);
        }
    }
}


