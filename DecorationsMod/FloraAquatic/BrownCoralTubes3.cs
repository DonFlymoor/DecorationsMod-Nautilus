﻿using DecorationsMod.Controllers;
using System.Collections.Generic;
using UnityEngine;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

namespace DecorationsMod.FloraAquatic
{
    public class BrownCoralTubes3 : DecorationItem, ICustomFlora
    {
        public CustomFlora Config = new CustomFlora();
        CustomFlora ICustomFlora.Config
        {
            get => this.Config;
            set => this.Config = value;
        }

        public BrownCoralTubes3()
        {
            this.ClassID = "BrownCoralTubes3"; // 291856e5-9d72-4cc6-b09f-ac09a5a6206e
            this.ResourcePath = "WorldEntities/Doodads/Coral_reef/coral_reef_brown_coral_tubes_01";

            this.GameObject = Resources.Load<GameObject>(this.ResourcePath);

            this.TechType = TechTypeHandler.AddTechType(this.ClassID,
                                                        LanguageHelper.GetFriendlyWord("BrownCoralTubesName") + " (C)",
                                                        LanguageHelper.GetFriendlyWord("AlienFloraSampleDescription"),
                                                        true);

            this.Recipe = new TechData(new List<Ingredient>(1)
            {
                new Ingredient(ConfigSwitcher.FloraRecipiesResource, 1)
            });

            this.Config = ConfigSwitcher.config_BrownCoralTubes3;
        }

        public override void RegisterItem()
        {
            if (this.IsRegistered == false)
            {
                // Set item occupies 4 slots
                 CraftDataHandler.SetItemSize(this.TechType, new Vector2int(2, 2));

                // Add the new TechType to Harvest types
                CraftDataHandler.SetHarvestType(this.TechType, HarvestType.DamageAlive);
                CraftDataHandler.SetHarvestOutput(this.TechType, this.TechType);

                // Change item background to water-plant seed
                // TODO: Replace with a call to SMLHelper when pull request gets released
                DecorationsMod.CustomBackgroundTypes.Add(this.TechType, CraftData.BackgroundType.PlantWaterSeed);

                // Set item bioreactor charge
                // TODO: Replace with a call to SMLHelper when pull request gets released
                DecorationsMod.CustomCharges.Add(this.TechType, this.Config.Charge);

                // Specify bonus on final cut
                // TODO: Replace with a call to SMLHelper when pull request gets released
                DecorationsMod.CustomFinalCutBonusList.Add(this.TechType, 1);

                // Set the buildable prefab
                SMLHelper.CustomPrefabHandler.customPrefabs.Add(new SMLHelper.CustomPrefab(this.ClassID, $"{DecorationItem.DefaultResourcePath}{this.ClassID}", this.TechType, this.GetPrefab));

                // Set the custom sprite
                SpriteHandler.RegisterSprite(this.TechType, AssetsHelper.Assets.LoadAsset<Sprite>("flora_browncoraltubes01icon"));

                // Associate recipe to the new TechType
                CraftDataHandler.SetTechData(this.TechType, this.Recipe);

                this.IsRegistered = true;
            }
        }

        public override GameObject GetPrefab()
        {
            GameObject prefab = GameObject.Instantiate(this.GameObject);

            prefab.name = this.ClassID;
            
            // Scale models
            prefab.FindChild("coral_reef_brown_coral_tubes_01").transform.localScale *= 0.4f;
            prefab.FindChild("coral_reef_brown_coral_tubes_01_LOD1").transform.localScale *= 0.4f;
            prefab.FindChild("coral_reef_brown_coral_tubes_01_LOD2").transform.localScale *= 0.4f;
            prefab.FindChild("coral_reef_brown_coral_tubes_01_LOD3").transform.localScale *= 0.4f;

            // Scale and shrink colliders
            BoxCollider[] colliders = prefab.GetComponentsInChildren<BoxCollider>();
            if (colliders.Length > 0)
            {
                foreach (BoxCollider collider in colliders)
                {
                    collider.size *= 0.4f;
                    collider.size *= 0.001f;
                }
            }

            // Update rigid body
            var rb = prefab.GetComponent<Rigidbody>();
            rb.mass = 1.0f;
            rb.drag = 1.0f;
            rb.angularDrag = 1.0f;
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.None;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rb.constraints = RigidbodyConstraints.None;

            // Add EntityTag
            var entityTag = prefab.AddComponent<EntityTag>();
            entityTag.slotType = EntitySlot.Type.Small;

            // Add TechTag
            var techTag = prefab.AddComponent<TechTag>();
            techTag.type = this.TechType;

            // Update prefab identifier
            var prefabId = prefab.GetComponent<PrefabIdentifier>();
            prefabId.ClassId = this.ClassID;

            // Update large world entity
            var lwe = prefab.GetComponent<LargeWorldEntity>();
            lwe.cellLevel = LargeWorldEntity.CellLevel.Near;

            // Add box collider
            //BoxCollider collider = prefab.AddComponent<BoxCollider>();
            //collider.size = new Vector3(0.2f, 0.2f, 0.2f);

            // Add world forces
            var worldForces = prefab.AddComponent<WorldForces>();
            worldForces.handleGravity = true;
            worldForces.aboveWaterGravity = 9.81f;
            worldForces.underwaterGravity = 1.0f;
            worldForces.handleDrag = true;
            worldForces.aboveWaterDrag = 0.0f;
            worldForces.underwaterDrag = 10.0f;
            worldForces.useRigidbody = rb;

            // Add pickupable
            var pickupable = prefab.AddComponent<Pickupable>();
            pickupable.isPickupable = false;
            pickupable.destroyOnDeath = true;
            pickupable.cubeOnPickup = false;
            pickupable.randomizeRotationWhenDropped = true;
            pickupable.usePackUpIcon = false;

            // Add plantable
            var plantable = prefab.AddComponent<Plantable>();
            plantable.aboveWater = false;
            plantable.underwater = true;
            plantable.isSeedling = true;
            plantable.plantTechType = this.TechType;
            plantable.size = Plantable.PlantSize.Large;
            plantable.pickupable = pickupable;
            plantable.model = prefab;
            plantable.linkedGrownPlant = new GrownPlant();
            plantable.linkedGrownPlant.seed = plantable;
            plantable.linkedGrownPlant.seedUID = "BrownCoralTubes3";

            // Add generic plant controller
            PlantGenericController landPlant1Controller = prefab.AddComponent<PlantGenericController>();
            landPlant1Controller.GrowthDuration = Config.GrowthDuration;
            landPlant1Controller.Health = Config.Health;
            landPlant1Controller.Knifeable = Config.Knifeable;
            landPlant1Controller.RestoreBoxColliders = true;

            // Add flora serializer/deserializer
            CustomFloraSerializer customSerializer = prefab.AddComponent<CustomFloraSerializer>();

            // Add live mixin
            var liveMixin = prefab.AddComponent<LiveMixin>();
            liveMixin.health = Config.Health;
            liveMixin.data = ScriptableObject.CreateInstance<LiveMixinData>();
            liveMixin.data.broadcastKillOnDeath = false;
            liveMixin.data.canResurrect = false;
            liveMixin.data.destroyOnDeath = true;
            liveMixin.data.explodeOnDestroy = false;
            liveMixin.data.invincibleInCreative = false;
            liveMixin.data.minDamageForSound = 10.0f;
            liveMixin.data.passDamageDataOnDeath = true;
            liveMixin.data.weldable = false;
            liveMixin.data.knifeable = false;
            liveMixin.data.maxHealth = Config.Health;
            //liveMixin.startHealthPercent = 1.0f;

            return prefab;
        }
    }
}
