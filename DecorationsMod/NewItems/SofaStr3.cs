﻿using SMLHelper;
using SMLHelper.Patchers;
using System.Collections.Generic;
using UnityEngine;

namespace DecorationsMod.NewItems
{
    public class SofaStr3 : DecorationItem
    {
        public GameObject newsofa = null;

        private Texture normal = null;
        private Texture spec = null;

        public SofaStr3()
        {
            // Feed DecortionItem interface
            this.ClassID = "SofaStr3";
            this.ResourcePath = "Submarine/Build/Bench";

            this.GameObject = Resources.Load<GameObject>(this.ResourcePath);

            this.TechType = TechTypePatcher.AddTechType(this.ClassID,
                                                        LanguageHelper.GetFriendlyWord("SofaStr3Name"),
                                                        LanguageHelper.GetFriendlyWord("SofaStr3Description"),
                                                        true);

            this.newsofa = AssetsHelper.Assets.LoadAsset<GameObject>("descent_bar_sofa_str_03");

            if (ConfigSwitcher.SofaStr3_asBuidable)
                this.IsHabitatBuilder = true;

            this.Recipe = new TechDataHelper()
            {
                _craftAmount = 1,
                _ingredients = new List<IngredientHelper>(new IngredientHelper[2]
                    {
                        new IngredientHelper(TechType.Titanium, 2),
                        new IngredientHelper(TechType.FiberMesh, 3)
                    }),
                _techType = this.TechType
            };
        }

        public override void RegisterItem()
        {
            if (this.IsRegistered == false)
            {
                normal = AssetsHelper.Assets.LoadAsset<Texture>("descent_bar_sofa_01_normal");
                spec = AssetsHelper.Assets.LoadAsset<Texture>("descent_bar_sofa_01_spec");

                if (ConfigSwitcher.SofaStr3_asBuidable)
                {
                    // Add new TechType to the buildables
                    CraftDataPatcher.customBuildables.Add(this.TechType);
                    CraftDataPatcher.AddToCustomGroup(TechGroup.Miscellaneous, TechCategory.Misc, this.TechType);
                }
                else
                {
                    // Set item occupies 9 slots
                    CraftDataPatcher.customItemSizes[this.TechType] = new Vector2int(3, 3);

                    // Add the new TechType to Hand Equipment type.
                    CraftDataPatcher.customEquipmentTypes.Add(this.TechType, EquipmentType.Hand);
                }

                // Set the buildable prefab
                CustomPrefabHandler.customPrefabs.Add(new CustomPrefab(this.ClassID, DecorationItem.DefaultResourcePath + this.ClassID, this.TechType, this.GetPrefab));

                // Set the custom sprite
                CustomSpriteHandler.customSprites.Add(new CustomSprite(this.TechType, AssetsHelper.Assets.LoadAsset<Sprite>("sofastr03icon")));

                // Associate recipe to the new TechType
                CraftDataPatcher.customTechData[this.TechType] = this.Recipe;

                this.IsRegistered = true;
            }
        }

        public override GameObject GetPrefab()
        {
            GameObject prefab = GameObject.Instantiate(this.GameObject);
            GameObject newsofaPrefab = GameObject.Instantiate(newsofa);

            prefab.name = this.ClassID;

            // Retrieve model node
            GameObject model = prefab.FindChild("model");
            
            // Modify prefab identifier
            var prefabId = prefab.GetComponent<PrefabIdentifier>();
            prefabId.ClassId = this.ClassID;

            // Add large world entity
            var lwe = prefab.AddComponent<LargeWorldEntity>();
            lwe.cellLevel = LargeWorldEntity.CellLevel.Near;

            // Modify tech tag
            var techTag = prefab.GetComponent<TechTag>();
            techTag.type = this.TechType;

            // Modify box colliders
            var collider = prefab.FindChild("Collider").GetComponent<BoxCollider>();
            collider.size = new Vector3(2.4f, 0.43f, 0.85f);
            var builderTrigger = prefab.FindChild("Builder Trigger").GetComponent<BoxCollider>();
            builderTrigger.size = new Vector3(2.4f, 0.43f, 0.85f);

            // Disable renderers
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in renderers)
            {
                rend.enabled = false;
            }
            newsofaPrefab.transform.parent = prefab.transform;
            newsofaPrefab.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            newsofaPrefab.transform.localScale = new Vector3(3.48f, 3.48f, 3.48f);
            newsofaPrefab.transform.localEulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
            newsofaPrefab.SetActive(true);

            // Get bench
            var bench = prefab.GetComponent<Bench>();
            bench.cinematicController.animatedTransform.localPosition = new Vector3(bench.cinematicController.animatedTransform.localPosition.x, bench.cinematicController.animatedTransform.localPosition.y, bench.cinematicController.animatedTransform.localPosition.z + 0.31f);

            // Set proper shaders
            Shader shader = Shader.Find("MarmosetUBER");
            renderers = prefab.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                foreach (Material tmpMat in renderer.materials)
                {
                    // Associate MarmosetUBER shader
                    tmpMat.shader = shader;

                    if (tmpMat.name.CompareTo("descent_bar_sofa_01 (Instance)") == 0)
                    {
                        tmpMat.SetTexture("_BumpMap", normal);
                        tmpMat.SetTexture("_SpecTex", spec);
                        tmpMat.EnableKeyword("MARMO_NORMALMAP");
                    }
                }
            }

            // Update sky applier
            var skyapplier = prefab.GetComponent<SkyApplier>();
            skyapplier.renderers = renderers;
            skyapplier.anchorSky = Skies.Auto;

            if (ConfigSwitcher.SofaStr3_asBuidable)
            {
                // Update contructable
                var constructible = prefab.GetComponent<Constructable>();
                constructible.allowedInBase = true;
                constructible.allowedInSub = true;
                constructible.allowedOutside = ConfigSwitcher.AllowBuildOutside;
                constructible.allowedOnCeiling = false;
                constructible.allowedOnGround = true;
                constructible.allowedOnConstructables = false;
                constructible.controlModelState = true;
                constructible.deconstructionAllowed = true;
                constructible.rotationEnabled = true;
                constructible.model = prefab.FindChild("descent_bar_sofa_str_03(Clone)");
                constructible.techType = this.TechType;
                constructible.enabled = true;
            }
            else
            {
                // Remove constructable
                var constructable = prefab.GetComponent<Constructable>();
                var constructBounds = prefab.GetComponent<ConstructableBounds>();
                GameObject.DestroyImmediate(constructable);
                GameObject.DestroyImmediate(constructBounds);
                
                // We can pick this item
                var pickupable = prefab.AddComponent<Pickupable>();
                pickupable.isPickupable = true;
                pickupable.randomizeRotationWhenDropped = true;

                // We can place this item
                var placeTool = prefab.AddComponent<PlaceTool>();
                placeTool.allowedInBase = true;
                placeTool.allowedOnBase = false;
                placeTool.allowedOnCeiling = false;
                placeTool.allowedOnConstructable = true;
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
                placeTool.drawTime = 0.5f;
                placeTool.dropTime = 1;
                placeTool.holsterTime = 0.35f;

                // Add fabricating animation
                var fabricatingA = prefab.AddComponent<VFXFabricating>();
                fabricatingA.localMinY = -0.2f;
                fabricatingA.localMaxY = 0.7f;
                fabricatingA.posOffset = new Vector3(0f, 0f, 0.1f);
                fabricatingA.eulerOffset = new Vector3(0f, 0f, 0f);
                fabricatingA.scaleFactor = 0.25f;
            }

            return prefab;
        }
    }
}
