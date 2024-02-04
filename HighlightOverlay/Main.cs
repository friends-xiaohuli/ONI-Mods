﻿using HighlightOverlay.Enums;
using HighlightOverlay;
using HighlightOverlay.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HighlightOverlay {
   public static class Main {
      public const string debugPrefix = "[HighlightOverlay] > ";

      public static readonly Color whiteHighlightColor = new Color(0.82f, 0.82f, 0.82f, 0f);
      public static readonly Color whiteBackgroundColor = new Color(0.95f, 0.95f, 0.95f, 1f);
      public static readonly Color blackBackgroundColor = new Color(0.15114269f, 0.15114269f, 0.15114269f, 1f);// should be unique because of technical reasons

      public static readonly Color checkboxHoverColor = new Color(0.5f, 0.5f, 0.5f, 1f);

      public static bool highlightInTrueColor = false;
      public static bool preservePreviousHighlightOptions = false;


      public static Dictionary<Tag, List<GameObject>> speciesMorphs = new Dictionary<Tag, List<GameObject>>();
      public static Dictionary<Tag, List<GameObject>> speciesMorphsBabies = new Dictionary<Tag, List<GameObject>>();


      public static HighlightMode highlightMode = default;

      public static GameObject selectedObj = null;
      public static int selectedCell = -1;
      public static GameObject selectedTile = null;
      public static ObjectProperties selectedObjProperties = default;
      public static Color selectedCellHighlightColor = Color.clear;

      public static Dictionary<ObjectType, bool> considerOption1 = new Dictionary<ObjectType, bool>();
      public static HighlightOptions highlightOption = HighlightOptions.NONE;

      public static Dictionary<ObjectType, HighlightOptions> lastHighlightOption = new Dictionary<ObjectType, HighlightOptions>();

      public static Color[] cellColors;
      public static Color[] tileColors;


      static Main() {
         foreach(ObjectType objectType in typeof(ObjectType).GetEnumValues())
         {
            considerOption1.Add(objectType, objectType.DefaultConsiderOption1());

            lastHighlightOption.Add(objectType, HighlightOptions.NONE);
         }
      }
   }
}