using MelonLoader;
using S1API.Entities;
using S1API.Entities.Schedule;
using S1API.Map;
using S1API.UI;
using S1API.Avatar;
using UnityEngine;

namespace CustomNPCTest.NPCs
{
    public sealed class CharacterCustomizerNPC : NPC
    {
        public override bool IsPhysical => true;

        protected override void ConfigurePrefab(NPCPrefabBuilder builder)
        {
            Vector3 spawnPos = new Vector3(-59.9f, 0.975f, 88.0f);

            builder.WithIdentity("character_customizer", "Character", "Customizer")
                .WithAppearanceDefaults(av =>
                {
                    av.Gender = 0.5f;
                    av.Height = 1.0f;
                    av.Weight = 0.5f;
                    av.SkinColor = new Color(0.8f, 0.7f, 0.6f);
                    av.LeftEyeLidColor = av.SkinColor;
                    av.RightEyeLidColor = av.SkinColor;
                    av.EyeBallTint = new Color(1.0f, 1.0f, 1.0f);
                    av.PupilDilation = 1.0f;
                    av.HairColor = new Color(0.3f, 0.2f, 0.1f);
                    av.HairPath = "Avatar/Hair/Spiky/Spiky";
                    av.WithFaceLayer("Avatar/Layers/Face/Face_SlightSmile", Color.black);
                    av.WithBodyLayer("Avatar/Layers/Top/T-Shirt", new Color(0.5f, 0.3f, 0.8f));
                    av.WithBodyLayer("Avatar/Layers/Bottom/Jeans", new Color(0.2f, 0.2f, 0.4f));
                    av.WithAccessoryLayer("Avatar/Accessories/Feet/Sneakers/Sneakers", new Color(0.9f, 0.9f, 0.9f));
                })
                .WithSpawnPosition(spawnPos)
                .WithRelationshipDefaults(r =>
                {
                    r.WithDelta(5.0f)
                        .SetUnlocked(true)
                        .SetUnlockType(NPCRelationship.UnlockType.DirectApproach);
                })
                .WithSchedule(plan =>
                {
                    // Simple schedule - just stand at spawn position
                    plan.Add(new WalkToSpec { Destination = spawnPos, StartTime = 0600, FaceDestinationDirection = true });
                });
        }

        public CharacterCustomizerNPC() : base()
        {
        }

        protected override void OnCreated()
        {
            try
            {
                base.OnCreated();
                Appearance.Build();

                Aggressiveness = 0f; // Friendly NPC
                Region = Region.Westville;

                Schedule.Enable();

                // Set up dialogue container for interaction
                SetupDialogue();

                MelonLogger.Msg("CharacterCustomizerNPC created successfully!");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"CharacterCustomizerNPC OnCreated failed: {ex.Message}");
                MelonLogger.Error($"StackTrace: {ex.StackTrace}");
            }
        }

        private void SetupDialogue()
        {
            try
            {
                Dialogue.BuildAndRegisterContainer("customizer_dialogue", builder =>
                {
                    builder.AddNode("ENTRY", "Hey there! I can help you customize your appearance. Would you like to open the character creator?", choices =>
                    {
                        choices.Add("open_creator", "Yes, open it!", "open_creator");
                        choices.Add("exit", "No, thanks.", "exit");
                    });

                    builder.AddNode("open_creator", "Opening the character creator for you!");

                    builder.AddNode("exit", "Alright, come back if you change your mind!");
                });

                Dialogue.OnChoiceSelected("open_creator", () =>
                {
                    // Pre-register character creator as active UI element before ending dialogue
                    // This prevents dialogue from restoring camera when it closes
                    CharacterCreatorManager.PreRegisterAsActiveUI();
                    // End dialogue - ends the DialogueContainer so our CharacterCreator works properly
                    Dialogue.End();
                    // Open character creator after dialogue closes
                    OpenCharacterCreator();
                });

                Dialogue.UseContainerOnInteract("customizer_dialogue");

                MelonLogger.Msg("CharacterCustomizerNPC dialogue setup complete!");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"CharacterCustomizerNPC SetupDialogue failed: {ex.Message}");
                MelonLogger.Error($"StackTrace: {ex.StackTrace}");
            }
        }

        private void OpenCharacterCreator()
        {
            try
            {
                MelonLogger.Msg("Opening Character Creator...");

                // Subscribe to the OnCompleted event to handle when player finishes customization
                CharacterCreatorManager.OnCompleted += OnCharacterCustomizationCompleted;
                CharacterCreatorManager.OnClosed += OnCharacterCustomizationClosed;

                // Open the character creator with current player settings
                // This will register as active UI element before opening, preventing dialogue from restoring camera
                CharacterCreatorManager.Open();
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"OpenCharacterCreator failed: {ex.Message}");
                MelonLogger.Error($"StackTrace: {ex.StackTrace}");
            }
        }

        private void OnCharacterCustomizationCompleted(BasicAvatarSettings settings)
        {
            try
            {
                MelonLogger.Msg("Character customization completed!");
                MelonLogger.Msg($"New settings - Gender: {settings.Gender}, Weight: {settings.Weight}");
                MelonLogger.Msg($"Skin Color: {settings.SkinColor}");
                MelonLogger.Msg($"Hair Style: {settings.HairStyle}, Hair Color: {settings.HairColor}");

                // Unsubscribe from events
                CharacterCreatorManager.OnCompleted -= OnCharacterCustomizationCompleted;
                CharacterCreatorManager.OnClosed -= OnCharacterCustomizationClosed;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"OnCharacterCustomizationCompleted failed: {ex.Message}");
            }
        }

        private void OnCharacterCustomizationClosed()
        {
            try
            {
                MelonLogger.Msg("Character creator closed without completing.");

                // Unsubscribe from events
                CharacterCreatorManager.OnCompleted -= OnCharacterCustomizationCompleted;
                CharacterCreatorManager.OnClosed -= OnCharacterCustomizationClosed;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"OnCharacterCustomizationClosed failed: {ex.Message}");
            }
        }
    }
}