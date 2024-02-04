﻿using HighlightOverlay.Components;
using HighlightOverlay.Enums;
using HighlightOverlay.Structs;
using HighlightOverlay;
using Klei.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Components;
using static ElementConverter;
using static Game;
using static HighlightOverlay.Structs.ObjectProperties;

namespace HighlightOverlay {
   public class HighlightMode : OverlayModes.Mode {
      public static HashedString ID = (HashedString)"glampi_HighlightMode";
      public bool isEnabled = false;

      private HashSet<PrimaryElement> highlightedObjects = new HashSet<PrimaryElement>();

      private bool dataIsClear = true;
      private bool defaultBackgroundColor = true;

      private ComputedShouldHighlightValues shouldHighlightObjects = new ComputedShouldHighlightValues();

      private readonly int targetLayer;
      private readonly int cameraLayerMask;

      public HighlightMode() {
         targetLayer = LayerMask.NameToLayer("MaskedOverlay");
         cameraLayerMask = LayerMask.GetMask("MaskedOverlay", "MaskedOverlayBG");
      }

      public override void Enable() {
         base.Enable();
         Camera.main.cullingMask |= cameraLayerMask;
         int mask = LayerMask.GetMask("MaskedOverlay");
         SelectTool.Instance.SetLayerMask(SelectTool.Instance.GetDefaultLayerMask() | mask);

         if(!dataIsClear)
         {
            foreach(var obj in highlightedObjects)
            {
               UpdateObjectHighlight(obj);
            }
         }

         isEnabled = true;
         UpdateObjectHighlight(Main.selectedObj);
         UpdateCellHighlight(Main.selectedCell, true);
         UpdateTileHighlight(Main.selectedTile, true);
      }

      public override void Update() {
         if(!ModConfig.Instance.AllowNotPaused && !Game.Instance.IsPaused)
         {
            if(!dataIsClear || !defaultBackgroundColor)
            {
               ClearAllData(true, true);
            }
         }
         else
         {
            if(dataIsClear)
            {
               if(Main.selectedObjProperties.objectType == ObjectType.NOTVALID || Main.highlightOption == HighlightOptions.NONE)
                  ClearAllData(!Main.preservePreviousHighlightOptions, false);

               dataIsClear = false;
               defaultBackgroundColor = false;

               if(Main.selectedObjProperties.objectType == ObjectType.NOTVALID || Main.highlightOption == HighlightOptions.NONE)
                  return;


               WorldContainer activeWorld = ClusterManager.Instance.activeWorld;
               Vector2I min = new Vector2I((int)activeWorld.minimumBounds.x, (int)activeWorld.minimumBounds.y);
               Vector2I max = new Vector2I((int)activeWorld.maximumBounds.x, (int)activeWorld.maximumBounds.y);
               Extents extents = new Extents(min.x, min.y, max.x - min.x, max.y - min.y);

               List<ScenePartitionerEntry> visibleObjects = new List<ScenePartitionerEntry>();

               GameScenePartitioner.Instance.GatherEntries(extents, GameScenePartitioner.Instance.pickupablesLayer, visibleObjects);
               foreach(ScenePartitionerEntry visibleObject in visibleObjects)
               {
                  if(((Component)visibleObject.obj).gameObject.TryGetComponent(out PrimaryElement element))
                  {
                     TryAddObjectToHighlightedObjects(element, min, max);
                  }
               }

               visibleObjects.Clear();

               GameScenePartitioner.Instance.GatherEntries(extents, GameScenePartitioner.Instance.completeBuildings, visibleObjects);
               foreach(ScenePartitionerEntry visibleObject in visibleObjects)
               {
                  BuildingComplete buildingComplete = (BuildingComplete)visibleObject.obj;

                  if(buildingComplete.gameObject.layer != 0)
                     continue;

                  if(buildingComplete.gameObject.TryGetComponent(out PrimaryElement element))
                  {
                     TryAddObjectToHighlightedObjects(element, min, max);
                  }
               }

               visibleObjects.Clear();

               GameScenePartitioner.Instance.GatherEntries(extents, GameScenePartitioner.Instance.plants, visibleObjects);
               foreach(ScenePartitionerEntry visibleObject in visibleObjects)
               {
                  if(((Component)visibleObject.obj).gameObject.TryGetComponent(out PrimaryElement element))
                  {
                     TryAddObjectToHighlightedObjects(element, min, max);
                  }
               }

               GatherVisibleObjectsOnBuildingsLayer(extents, out HashSet<GameObject> buildings);// including geysers, gravitas buildings
               foreach(GameObject building in buildings)
               {
                  if(building.HasTag(GameTags.GeyserFeature))
                  {
                     if(!ModConfig.Instance.HighlightBurriedGeysers && Utils.IsGeyserBurried(building))
                        continue;
                  }

                  if(Utils.IsTile(building, out _))
                     continue;// tiles are highlighted via the cells system

                  if(!building.HasTag(GameTags.UnderConstruction) && building.TryGetComponent(out PrimaryElement element))
                  {
                     TryAddObjectToHighlightedObjects(element, min, max);
                  }
               }

               if(Main.selectedObj != null)// it is not guaranteed that the selected obj will get checked in the methods above
               {
                  if(Main.selectedObj.TryGetComponent(out PrimaryElement element))
                  {
                     TryAddObjectToHighlightedObjects(element, min, max);
                  }
               }

               //----------------------Updating cells color----------------------DOWN
               for(int cell = 0; cell < Main.cellColors.Length; cell++)
               {
                  bool isTile = Utils.IsTile(cell, out SimCellOccupier tile);
                  if(isTile && tile.TryGetComponent(out PrimaryElement primaryElement))
                  {
                     if(TargetAppliesToAllFilters(primaryElement))
                     {
                        UpdateTileHighlight(tile.gameObject);
                        continue;// don't have to calculate whether the cell should be highlighted because the tile is
                     }
                     else
                     {
                        if(!Main.preservePreviousHighlightOptions)
                           RemoveTileHighlight(tile.gameObject);
                     }
                  }

                  if(!isTile || !tile.doReplaceElement)
                  {
                     if(TargetAppliesToAllFilters(null, cell))
                     {
                        UpdateCellHighlight(cell);
                     }
                     else
                     {
                        if(!Main.preservePreviousHighlightOptions)
                           RemoveCellHighlight(cell);
                     }
                  }
               }
               //----------------------Updating cells color----------------------UP
            }
         }
      }
      private static void GatherVisibleObjectsOnBuildingsLayer(Extents extents, out HashSet<GameObject> buildings) {
         buildings = new HashSet<GameObject>();

         int max_x = extents.x + extents.width;
         int max_y = extents.y + extents.height;
         for(int x = extents.x; x < max_x; x++)
         {
            for(int y = extents.y; y < max_y; y++)
            {
               int cell = Grid.XYToCell(x, y);
               if(Grid.IsVisible(cell))
               {
                  GameObject building = Grid.Objects[cell, (int)ObjectLayer.Building];
                  if(building != null)
                     buildings.Add(building);
               }
            }
         }
      }
      private void TryAddObjectToHighlightedObjects(PrimaryElement targetObject, Vector2I vis_min, Vector2I vis_max) {
         if(highlightedObjects.Contains(targetObject))
            return;

         if(TargetAppliesToAllFilters(targetObject))
            AddTargetIfVisible(targetObject, vis_min, vis_max, highlightedObjects, targetLayer, target => {
               UpdateObjectHighlight(target);
            });
      }


      public void UpdateHighlightColor() {
         foreach(PrimaryElement target in highlightedObjects)
         {
            UpdateObjectHighlight(target);
         }

         for(int cell = 0; cell < Main.cellColors.Length; cell++)
         {
            if(Main.tileColors[cell] != Main.blackBackgroundColor)
            {
               if(Utils.IsTile(cell, out SimCellOccupier tile))
                  UpdateTileHighlight(tile.gameObject);
            }
            else if(Main.cellColors[cell] != Main.blackBackgroundColor)
            {
               UpdateCellHighlight(cell);
            }
         }

         UpdateObjectHighlight(Main.selectedObj);
         UpdateCellHighlight(Main.selectedCell, true);
         UpdateTileHighlight(Main.selectedTile, true);
      }

      public void UpdateSelectedObjHighlight(GameObject oldSelected, int oldSelectedCell, GameObject oldSelectedTile) {
         if(!isEnabled)
            return;

         if(Main.selectedObj != oldSelected)
         {
            RemoveObjectHighlight(oldSelected, true);
            UpdateObjectHighlight(Main.selectedObj);
         }

         if(Main.selectedCell != oldSelectedCell)
         {
            RemoveCellHighlight(oldSelectedCell, true);
            UpdateCellHighlight(Main.selectedCell, true);
         }

         if(Main.selectedTile != oldSelectedTile)
         {
            RemoveTileHighlight(oldSelectedTile, true);
            UpdateTileHighlight(Main.selectedTile, true);
         }
      }

      private void UpdateObjectHighlight(PrimaryElement obj) {
         UpdateObjectHighlight(obj.gameObject);
      }
      private void UpdateObjectHighlight(GameObject obj) {
         if(obj == null)
            return;

         if(obj.TryGetComponent(out TintManagerCmp tintManager))
         {
            tintManager.SetTintColor(Main.highlightInTrueColor ? Color.clear : Main.whiteHighlightColor);
            tintManager.animController.SetLayer(targetLayer);
         }

         foreach(Storage storage in obj.GetComponents<Storage>())
         {
            if(!storage.defaultStoredItemModifers.Contains(Storage.StoredItemModifier.Hide))
            {
               foreach(GameObject storedItem in storage.items)
               {
                  if(storedItem != null && storedItem.TryGetComponent(out TintManagerCmp tintManager2))
                  {
                     tintManager2.SetTintColor(Main.highlightInTrueColor ? Color.clear : Main.whiteHighlightColor);
                  }
               }
            }
         }
      }
      private void RemoveObjectHighlight(PrimaryElement obj, bool removeSelectedHighlight = false) {
         RemoveObjectHighlight(obj.gameObject, removeSelectedHighlight);
      }
      private void RemoveObjectHighlight(GameObject obj, bool removeSelectedHighlight = false) {
         if(obj == null)
            return;

         if((removeSelectedHighlight && highlightedObjects.Contains(obj.GetComponent<PrimaryElement>())) ||
            (!removeSelectedHighlight && obj == Main.selectedObj))
            return;

         if(obj.TryGetComponent(out TintManagerCmp tintManager))
         {
            tintManager.ResetTintColor();
            tintManager.animController.SetLayer(tintManager.animController.GetComponent<KPrefabID>().defaultLayer);
         }

         foreach(Storage storage in obj.GetComponents<Storage>())
         {
            if(!storage.defaultStoredItemModifers.Contains(Storage.StoredItemModifier.Hide))
            {
               foreach(GameObject storedItem in storage.items)
               {
                  if(storedItem != null && storedItem.TryGetComponent(out TintManagerCmp tintManager2))
                  {
                     tintManager2.ResetTintColor();
                  }
               }
            }
         }
      }

      private void UpdateCellHighlight(int cell, bool updateSelectedHighlight = false) {
         if(cell < 0)
            return;

         Color highlightColor = Main.highlightInTrueColor ? (Color)Grid.Element[cell].substance.uiColour : Main.whiteBackgroundColor;
         if(updateSelectedHighlight || cell == Main.selectedCell)
         {
            Main.selectedCellHighlightColor = highlightColor;
         }
         if(!updateSelectedHighlight)
         {
            Main.cellColors[cell] = highlightColor;
         }
      }
      private void RemoveCellHighlight(int cell, bool removeSelectedHighlight = false) {
         if(cell < 0)
            return;

         Color highlightColor = Main.blackBackgroundColor;
         if(removeSelectedHighlight)
         {
            if(cell == Main.selectedCell)
               Main.selectedCellHighlightColor = highlightColor;
         }
         else
         {
            Main.cellColors[cell] = highlightColor;
         }
      }

      private void UpdateTileHighlight(GameObject tile, bool updateSelectedHighlight = false) {
         if(tile == null)
            return;

         Color highlightColor = Main.highlightInTrueColor ? (Color)tile.GetComponent<PrimaryElement>().Element.substance.uiColour : Main.whiteBackgroundColor;
         if(updateSelectedHighlight || tile == Main.selectedTile)
         {
            Main.selectedCellHighlightColor = highlightColor;
         }
         if(!updateSelectedHighlight)
         {
            Main.tileColors[Utils.PosToCell(tile)] = highlightColor;
         }
      }
      private void RemoveTileHighlight(GameObject tile, bool removeSelectedHighlight = false) {
         if(tile == null)
            return;

         Color highlightColor = Main.blackBackgroundColor;
         if(removeSelectedHighlight)
         {
            if(tile == Main.selectedTile)
               Main.selectedCellHighlightColor = highlightColor;
         }
         else
         {
            Main.tileColors[Utils.PosToCell(tile)] = highlightColor;
         }
      }

      public void ClearAllData(bool setDefaultCellColors, bool forceRemoveHighlightedObjects) {
         if(forceRemoveHighlightedObjects || !Main.preservePreviousHighlightOptions)
            RemoveAllHighlightedObjects(true);

         shouldHighlightObjects.Clear();

         if(setDefaultCellColors)
         {
            Utils.SetDefaultCellColors();
            defaultBackgroundColor = true;
         }

         dataIsClear = true;
      }

      private void RemoveAllHighlightedObjects(bool clearSet) {
         foreach(PrimaryElement target in highlightedObjects)
         {
            RemoveHighlightedObject(target, false);
         }
         
         if(clearSet)
            highlightedObjects.Clear();
      }

      private void RemoveHighlightedObject(PrimaryElement target, bool removeFromSet = true) {
         if(target != null)
         {
            RemoveObjectHighlight(target);
         }

         if(removeFromSet)
            highlightedObjects.Remove(target);
      }



      public bool TargetAppliesToAllFilters(PrimaryElement primaryElement, int cell = -1) {
         if(primaryElement == null && cell == -1)
            return false;

         if(Main.selectedObjProperties.objectType == ObjectType.NOTVALID)
            return false;

         ObjectProperties selectedProperties = Main.selectedObjProperties;

         //----------------------Stored items----------------------DOWN
         if(primaryElement != null)
         {
            foreach(Storage storage in primaryElement.GetComponents<Storage>())
            {
               if(storage.showInUI)
               {
                  foreach(GameObject storedItem in storage.items)
                  {
                     if(storedItem != null && storedItem.TryGetComponent(out PrimaryElement elem))
                     {
                        if(TargetAppliesToAllFilters(elem))
                           return true;
                     }
                  }
               }
            }
         }
         //----------------------Stored items----------------------UP
         //----------------------Equipped items(clothing, suits)----------------------DOWN
         if(primaryElement != null)
         {
            if(primaryElement.TryGetComponent(out MinionIdentity minionIdentity))
            {
               foreach(EquipmentSlotInstance slot in minionIdentity.GetEquipment().Slots)
               {
                  Equippable equippable = slot.assignable as Equippable;
                  if(equippable != null && equippable.isEquipped && equippable.TryGetComponent(out PrimaryElement elem))
                  {
                     if(TargetAppliesToAllFilters(elem))
                        return true;
                  }
               }
            }
         }
         //----------------------Equipped items(clothing, suits)----------------------UP

         ObjectProperties targetProperties;
         if(primaryElement != null)
         {
            targetProperties = new ObjectProperties(primaryElement.gameObject);
         }
         else
         {
            targetProperties = new ObjectProperties(cell);
         }

         bool shouldHighlight;

         if(WasShouldHighlightAlreadyComputed(targetProperties, out shouldHighlight))
         {
            return shouldHighlight;
         }

         shouldHighlight = ComputeShouldHighlight(selectedProperties, targetProperties);

         StoreShouldHighlight(targetProperties, shouldHighlight);
         return shouldHighlight;
      }



      private static bool ComputeShouldHighlight(ObjectProperties selectedProperties, ObjectProperties targetProperties) {
         if((selectedProperties.highlightOptions & Main.highlightOption) == 0 ||
            (Main.highlightOption.Reverse() != HighlightOptions.NONE && ((targetProperties.highlightOptions & Main.highlightOption.Reverse()) == 0)))
            return false;

         string dictKey = selectedProperties.objectType.ToString() + "_" + Main.highlightOption.ToString() + "_" + targetProperties.objectType.ToString();

         if(!ShouldHighlightCases.caseNameToMethod.ContainsKey(dictKey))
            throw new Exception(Main.debugPrefix + $"Key {dictKey} not found inside of {nameof(ShouldHighlightCases.caseNameToMethod)} dictionary");

         return ShouldHighlightCases.caseNameToMethod[dictKey].Invoke(selectedProperties, targetProperties);
      }



      private bool WasShouldHighlightAlreadyComputed(ObjectProperties objProperties, out bool shouldHighlight) {
         if(shouldHighlightObjects.TryGetValue(objProperties, out shouldHighlight))
            return true;
         return false;
      }
      private void StoreShouldHighlight(ObjectProperties objProperties, bool shouldHighlight) {
         shouldHighlightObjects.StoreValue(objProperties, shouldHighlight);
      }



      public override void Disable() {
         base.Disable();
         Camera.main.cullingMask &= ~cameraLayerMask;
         SelectTool.Instance.ClearLayerMask();

         if(!Main.preservePreviousHighlightOptions)
         {
            ClearAllData(true, true);
         }
         else// removing highlight but keeping objects saved in the HashSet; doing nothing to cells
         {
            RemoveAllHighlightedObjects(false);

            dataIsClear = false;
         }

         RemoveObjectHighlight(Main.selectedObj, true);
         RemoveCellHighlight(Main.selectedCell, true);
         RemoveTileHighlight(Main.selectedTile, true);

         isEnabled = false;
      }

      public override HashedString ViewMode() => ID;
      public override string GetSoundName() => "SuitRequired";
   }
}