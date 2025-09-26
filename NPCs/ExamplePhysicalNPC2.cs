using MelonLoader;
using S1API.Entities;
using S1API.Entities.Schedule;
using S1API.Map;
using S1API.Money;
using S1API.Economy;
using S1API.Entities.NPCs.Northtown;
using S1API.GameTime;
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
    public sealed class ExamplePhysicalNPC2 : NPC
    {
        protected override bool IsPhysical => true;
        
        protected override void ConfigurePrefab(NPCPrefabBuilder builder)
        {
            Vector3 posA = new Vector3(-64.6576f, 1.065f, 51.3718f);
            Vector3 spawnPos = new Vector3(-40.324f, 1.065f, 66.1782f);
            builder.WithSpawnPosition(spawnPos)
                .EnsureCustomer()
                .WithCustomerDefaults(cd =>
                {
                    cd.WithSpending(minWeekly: 500f, maxWeekly: 1500f)
                        .WithOrdersPerWeek(2, 4)
                        .WithPreferredOrderDay(Day.Saturday)
                        .WithOrderTime(1100)
                        .WithStandards(CustomerStandard.High)
                        .AllowDirectApproach(true)
                        .GuaranteeFirstSample(true)
                        .WithMutualRelationRequirement(minAt50: 2.5f, maxAt100: 4.0f)
                        .WithCallPoliceChance(0.15f)
                        .WithDependence(baseAddiction: 0.25f, dependenceMultiplier: 1.1f)
                        .WithAffinities(new[]
                        {
                            (DrugType.Marijuana, 0.65f), (DrugType.Cocaine, -0.3f)
                        })
                        // .WithPreferredPropertiesById("Munchies", "Energizing", "Cyclopean");
                        .WithPreferredProperties(Property.Munchies, Property.AntiGravity, Property.BrightEyed);
                })
                .WithRelationshipDefaults(r =>
                {
                    r.WithDelta(5.0f)
                        .SetUnlocked(false)
                        .SetUnlockType(NPCRelationship.UnlockType.DirectApproach)
                        .WithConnectionsById("kyle_cooley", "ludwig_meyer", "austin_steiner");
                    // .WithConnections(Get<KyleCooley>(), Get<LudwigMeyer>(), Get<AustinSteiner>());
                })
                .WithSchedule(plan =>
                {
                    plan
                        .EnsureDealSignal()
                        .Add(new UseVendingMachineSpec { StartTime = 875 })
                        .WalkTo(posA, 900, faceDestinationDir: true)
                        .StayInBuilding(Building.Get<NorthApartments>(), 1100, 60)
                        .Add(new LocationDialogueSpec { Destination = posA, StartTime = 1300, FaceDestinationDirection = true })
                        .Add(new UseVendingMachineSpec { StartTime = 1400 })
                        .StayInBuilding(Building.Get<NorthApartments>(), 1425, 240);
                });
        }
        
        public ExamplePhysicalNPC2() : base(
            id: "example_physical_npc2",
            firstName: "John",
            lastName: "Test2",
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
                
                SendTextMessage("Hello from physical NPC 2!");
                
                Aggressiveness = 5f;
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
                .Set<S1API.Entities.Appearances.CustomizationFields.HairStyle>("Avatar/Hair/BuzzCut/BuzzCut")
                .WithFaceLayer<S1API.Entities.Appearances.FaceLayerFields.Face>("Avatar/Layers/Face/Face_Agitated", Color.black)
                .WithBodyLayer<S1API.Entities.Appearances.BodyLayerFields.Shirts>("Avatar/Layers/Top/RolledButtonUp", Color.blue)
                .WithBodyLayer<S1API.Entities.Appearances.BodyLayerFields.Pants>("Avatar/Layers/Bottom/Jorts", new Color(0.15f, 0.2f, 0.3f))
                .WithAccessoryLayer<S1API.Entities.Appearances.AccessoryFields.Feet>("Avatar/Accessories/Feet/Sneakers/Sneakers", Color.blue);
        }
    }
}


