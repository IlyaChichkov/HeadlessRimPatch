using System;
using HarmonyLib;
using Verse;
using Verse.Sound;
using RimWorld;
using UnityEngine;

namespace HeadlessRim
{
    public static class HeadlessPatches
    {
        // CRITICAL STARTUP PATCHES (Run Early)
        public static void ApplyStartupPatches(Harmony harmony)
        {
            Log.Message("[HeadlessRim] Applying CRITICAL STARTUP patches...");

            // Texture Atlasing (Prevents Unity Crashes in BatchMode)
            Patch(harmony, typeof(GlobalTextureAtlasManager), "TryInsertStatic", nameof(SkipPrefix));
            Patch(harmony, typeof(Graphic_Single), "TryInsertIntoAtlas", nameof(SkipPrefix));
            Patch(harmony, typeof(Graphic_Multi), "TryInsertIntoAtlas", nameof(SkipPrefix));
            Patch(harmony, typeof(Graphic_Collection), "TryInsertIntoAtlas", nameof(SkipPrefix));
            
            var graphicRandomType = AccessTools.TypeByName("Verse.Graphic_Random");
            if (graphicRandomType != null) Patch(harmony, graphicRandomType, "TryInsertIntoAtlas", nameof(SkipPrefix));
            
            var graphicLinkedType = AccessTools.TypeByName("Verse.Graphic_Linked");
            if (graphicLinkedType != null) Patch(harmony, graphicLinkedType, "TryInsertIntoAtlas", nameof(SkipPrefix));

            // UI & Initialization (Prevents NREs during Loading)
            Patch(harmony, typeof(Designator_Build), "UpdateIcon", nameof(SkipPrefix));
            Patch(harmony, typeof(LongEventHandler), "LongEventsOnGUI", nameof(SkipPrefix));

            // Prevent shader database from crashing/spamming in batchmode
            Patch(harmony, typeof(ShaderDatabase), "get_DefaultShader", nameof(ReturnNullShaderPrefix));

            var widgetsType = AccessTools.TypeByName("Verse.Widgets");
            if (widgetsType != null)
            {
                var method = AccessTools.Method(widgetsType, "CroppedTerrainTextureRect", new Type[] { typeof(Texture2D) });
                if (method != null) harmony.Patch(method, prefix: new HarmonyMethod(typeof(HeadlessPatches), nameof(ReturnEmptyRectPrefix)));
            }

            Log.Message("[HeadlessRim] Finished applying CRITICAL STARTUP patches.");
        }

        // RUNTIME PATCHES (Run Late)
        public static void ApplyRuntimePatches(Harmony harmony)
        {
            Log.Message("[HeadlessRim] Applying RUNTIME patches (UI, Map, Audio)...");

            // UI LOOP & DRAWING
            Patch(harmony, typeof(UIRoot_Entry), "UIRootUpdate", nameof(EntryUpdateReplacement));
            Patch(harmony, typeof(UIRoot_Play), "UIRootUpdate", nameof(SkipPrefix));
            Patch(harmony, typeof(UIRoot_Entry), "UIRootOnGUI", nameof(SkipPrefix));
            Patch(harmony, typeof(UIRoot_Play), "UIRootOnGUI", nameof(SkipPrefix));
            Patch(harmony, typeof(MapInterface), "MapInterfaceOnGUI_BeforeMainTabs", nameof(SkipPrefix));

            // MAP MESH GENERATION (Prevents Gameplay NREs)
            Patch(harmony, typeof(Section), "RegenerateAllLayers", nameof(SkipPrefix));
            Patch(harmony, typeof(MapDrawer), "WholeMapChanged", nameof(SkipPrefix));
            Patch(harmony, typeof(MapDrawer), "SectionChanged", nameof(SkipPrefix));
            Patch(harmony, typeof(MapDrawer), "RegenerateEverythingNow", nameof(SkipPrefix));
            Patch(harmony, typeof(MapDrawer), "MapMeshDrawerUpdate_First", nameof(SkipPrefix));
            Patch(harmony, typeof(MapDrawer), "DrawMapMesh", nameof(SkipPrefix));
            Patch(harmony, typeof(MapDrawer), "Dispose", nameof(SkipPrefix));
            Patch(harmony, typeof(Graphic), "Print", nameof(SkipPrefix));

            // SCENE MANAGEMENT & SAFETY
            Patch(harmony, typeof(Root_Play), "Update", nameof(SafeRootPlayUpdatePrefix));

            // AUDIO
            Patch(harmony, typeof(SoundStarter), "PlayOneShot", nameof(SkipPrefix));
            Patch(harmony, typeof(SoundRoot), "Update", nameof(SkipPrefix));
            Patch(harmony, typeof(MusicManagerPlay), "MusicUpdate", nameof(SkipPrefix));
            Patch(harmony, typeof(MusicManagerEntry), "MusicManagerEntryUpdate", nameof(SkipPrefix));

            // PORTRAITS
            var portraitOriginal = AccessTools.Method(typeof(PortraitsCache), "Get", new Type[] { typeof(Pawn), typeof(Vector2), typeof(Rot4), typeof(Vector3), typeof(float), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(System.Collections.Generic.Dictionary<Apparel, Color>) });
            if (portraitOriginal != null)
                harmony.Patch(portraitOriginal, prefix: new HarmonyMethod(typeof(HeadlessPatches), nameof(ReturnNullTexturePrefix)));

            Log.Message("[HeadlessRim] Finished applying RUNTIME patches.");
        }

        // Helpers
        private static void Patch(Harmony harmony, Type type, string methodName, string patchMethodName)
        {
            try
            {
                var original = AccessTools.Method(type, methodName);
                if (original != null) harmony.Patch(original, prefix: new HarmonyMethod(typeof(HeadlessPatches), patchMethodName));
            }
            catch { }
        }

        public static bool SkipPrefix() => false;
        public static bool ReturnNullTexturePrefix(ref Texture __result) { __result = null; return false; }
        public static bool ReturnNullShaderPrefix(ref Shader __result) { __result = null; return false; }
        public static bool ReturnEmptyRectPrefix(ref Rect __result) { __result = Rect.zero; return false; }

        public static bool EntryUpdateReplacement()
        {
            bool sceneChanged = false;
            LongEventHandler.LongEventsUpdate(out sceneChanged);
            return false;
        }

        public static bool SafeRootPlayUpdatePrefix()
        {
            try
            {
                if (LongEventHandler.ShouldWaitForEvent) return false;

                bool sceneChanged = false;
                LongEventHandler.LongEventsUpdate(out sceneChanged);
                if (sceneChanged) return false;

                // Extra safety: if we are supposed to be in main menu but Root_Play is still ticking, skip.
                if (Current.ProgramState != ProgramState.Playing) return false;

                return true; // Continue to original Root_Play.Update
            }
            catch (Exception ex)
            {
                Log.ErrorOnce($"[HeadlessRim] SafeRootPlayUpdate Error: {ex}", 123123);
                return false;
            }
        }
    }
}