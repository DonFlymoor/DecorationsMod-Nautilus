﻿using DecorationsMod.Controllers;
using DecorationsMod.Fixers;
using System.Collections.Generic;
using UnityEngine;

namespace DecorationsMod.NewItems
{
    public class LabCart : DecorationItem
    {
        public LabCart() // Feeds abstract class
        {
            this.ClassID = "LabCart"; //af165b07-a2a3-4d85-8ad7-0c801334c115
            this.PrefabFileName = DecorationItem.DefaultResourcePath + this.ClassID;

            this.GameObject = Resources.Load<GameObject>("WorldEntities/Doodads/Debris/Wrecks/Decoration/discovery_lab_cart_01");

            this.TechType = SMLHelper.V2.Handlers.TechTypeHandler.AddTechType(this.ClassID,
                                                        LanguageHelper.GetFriendlyWord("LabCartName"),
                                                        LanguageHelper.GetFriendlyWord("LabCartDescription"),
                                                        true);

            CrafterLogicFixer.LabCart = this.TechType;
            KnownTechFixer.AddedNotifications.Add((int)this.TechType, false);

            if (ConfigSwitcher.LabCart_asBuildable)
                this.IsHabitatBuilder = true;

#if BELOWZERO
            this.Recipe = new SMLHelper.V2.Crafting.RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>(new Ingredient[1]
                    {
                        new Ingredient(TechType.Titanium, 1)
                    }),
            };
#else
            this.Recipe = new SMLHelper.V2.Crafting.TechData()
            {
                craftAmount = 1,
                Ingredients = new List<SMLHelper.V2.Crafting.Ingredient>(new SMLHelper.V2.Crafting.Ingredient[1]
                    {
                        new SMLHelper.V2.Crafting.Ingredient(TechType.Titanium, 1)
                    }),
            };
#endif
        }

        public override void RegisterItem()
        {
            if (this.IsRegistered == false)
            {
                // Associate recipe to the new TechType
                SMLHelper.V2.Handlers.CraftDataHandler.SetTechData(this.TechType, this.Recipe);

                if (ConfigSwitcher.LabCart_asBuildable)
                {
                    // Add to the custom buidables
                    SMLHelper.V2.Handlers.CraftDataHandler.AddBuildable(this.TechType);
                    SMLHelper.V2.Handlers.CraftDataHandler.AddToGroup(TechGroup.Miscellaneous, TechCategory.Misc, this.TechType);
                }
                else
                {
                    // Set item occupies 4 slots
                    SMLHelper.V2.Handlers.CraftDataHandler.SetItemSize(this.TechType, new Vector2int(2, 2));

                    // Add the new TechType to the hand-equipments
                    SMLHelper.V2.Handlers.CraftDataHandler.SetEquipmentType(this.TechType, EquipmentType.Hand);

                    // Set quick slot type.
                    SMLHelper.V2.Handlers.CraftDataHandler.SetQuickSlotType(this.TechType, QuickSlotType.Selectable);
                }

                // Set the buildable prefab
                SMLHelper.V2.Handlers.PrefabHandler.RegisterPrefab(this);

                // Set the custom sprite
                SMLHelper.V2.Handlers.SpriteHandler.RegisterSprite(this.TechType, AssetsHelper.Assets.LoadAsset<Sprite>("labcarticon"));

                this.IsRegistered = true;
            }
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = GameObject.Instantiate(this.GameObject);
            GameObject model = prefab.FindChild("discovery_lab_cart_01");

            prefab.name = this.ClassID;
            
            // Set TechTag
            TechTag techTag = prefab.AddComponent<TechTag>();
            techTag.type = this.TechType;

            // Update prefab identifier
            PrefabIdentifier prefabId = prefab.GetComponent<PrefabIdentifier>();
            prefabId.ClassId = this.ClassID;

            GameObject cube = prefab.FindChild("Cube");
            GameObject.DestroyImmediate(cube);

            // Remove rigid body
            Rigidbody rb = prefab.GetComponent<Rigidbody>();
            GameObject.DestroyImmediate(rb);

            // Add box collider
            BoxCollider collider = model.AddComponent<BoxCollider>();
            collider.size = new Vector3(1.026103f, 0.6288151f, 0.91f); // -90X: Y<=>Z
            collider.center = new Vector3(0.005f, 0.001f, 0.455f); // -90X: Y<=>Z

            // Update large world entity
            LargeWorldEntity lwe = prefab.GetComponent<LargeWorldEntity>();
            lwe.cellLevel = LargeWorldEntity.CellLevel.Near;

            // Get renderers
            Renderer[] rend = prefab.GetComponentsInChildren<Renderer>();

            if (!ConfigSwitcher.LabCart_asBuildable)
            {
                // Set proper shaders for crafting animation
                foreach (Renderer renderer in rend)
                {
                    renderer.material.shader = Shader.Find("MarmosetUBER");
                }
                
                // We can pick this item
                Pickupable pickupable = prefab.AddComponent<Pickupable>();
                pickupable.isPickupable = true;
                pickupable.randomizeRotationWhenDropped = true;

                // We can place this item
                prefab.AddComponent<CustomPlaceToolController>();
                var placeTool = prefab.AddComponent<GenericPlaceTool>();
                placeTool.allowedInBase = true;
                placeTool.allowedOnBase = true;
                placeTool.allowedOnCeiling = false;
                placeTool.allowedOnConstructable = false;
                placeTool.allowedOnGround = true;
                placeTool.allowedOnRigidBody = true;
                placeTool.allowedOnWalls = false;
                placeTool.allowedOutside = ConfigSwitcher.AllowPlaceOutside;
                placeTool.rotationEnabled = true;
                placeTool.enabled = true;
                placeTool.hasAnimations = false;
                placeTool.hasBashAnimation = false;
                placeTool.hasFirstUseAnimation = false;
                placeTool.mainCollider = collider;
                placeTool.pickupable = pickupable;

                // Add fabricating animation
                VFXFabricating fabricating = model.AddComponent<VFXFabricating>();
                fabricating.localMinY = -0.1f;
                fabricating.localMaxY = 0.9f;
                fabricating.posOffset = new Vector3(0f, 0f, 0.04f);
                fabricating.eulerOffset = new Vector3(-90f, 0f, 0f);
                fabricating.scaleFactor = 0.5f;
            }
            else
            {
                // Set as constructible
                Constructable constructible = prefab.AddComponent<Constructable>();
                constructible.techType = this.TechType;
                constructible.allowedOnWall = false;
                constructible.allowedInBase = true;
                constructible.allowedInSub = true;
                constructible.allowedOutside = ConfigSwitcher.AllowBuildOutside;
                constructible.allowedOnCeiling = false;
                constructible.allowedOnGround = true;
                constructible.allowedOnConstructables = false;
                constructible.deconstructionAllowed = true;
                constructible.controlModelState = true;
                constructible.model = model;
                constructible.placeMinDistance = 0.6f;

                // Add constructible bounds
                ConstructableBounds bounds = prefab.AddComponent<ConstructableBounds>();
            }

            // Update sky applier
            SkyApplier applier = prefab.GetComponent<SkyApplier>();
            applier.anchorSky = Skies.Auto;
            applier.renderers = rend;

            return prefab;
        }
    }
}
