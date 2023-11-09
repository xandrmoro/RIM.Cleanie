using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace Cleanie
{

    public class Cleanie : Mod
    {
        public static CleanieSettings Settings { get; private set; }

        public static Assembly DubsAssembly { get; private set; }

        public Cleanie(ModContentPack contentPack) : base(contentPack)
        {
            Settings = GetSettings<CleanieSettings>();

            var badHygiene = LoadedModManager.RunningMods.FirstOrDefault(m => m.PackageId.EqualsIgnoreCase("Dubwise.DubsBadHygiene"));

            if (badHygiene != null)
            {
                Log.Message("[Cleanie] Dubs Bad Hygiene detected, applying patch");
                DubsAssembly = badHygiene.assemblies.loadedAssemblies.Single(a => a.GetName().Name == "BadHygiene");
            }

            new Harmony(Content.PackageIdPlayerFacing).PatchAll();
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
        }

        private static Vector2 scrollPosition;

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var rect = new Rect(inRect.x + 20, inRect.y, inRect.width - 20f, inRect.height);

            var defs = DefDatabase<RoomRoleDef>.AllDefs.OrderBy(def => def.defName == "None" ? "Outdoors" : def.PostProcessedLabelCap).ToList();

            var view = new Rect(0, 0, rect.width - 16f, defs.Count * 24f + 5f);

            Listing_Standard listingInternal = new Listing_Standard(view, () => scrollPosition), listingExternal = new Listing_Standard(), listingHeaders = new Listing_Standard();

            if (Settings.Weights.Count < defs.Count)
            {
                foreach (RoomRoleDef roomDef in defs)
                {
                    if (!Settings.Weights.ContainsKey(roomDef.defName))
                    {
                        Settings.Weights.Add(roomDef.defName, new CleanieSettings.Pair { Weight = 50f, Threshold = -0.5f });
                    }
                }
            }

            var scaleFactor = 200f / inRect.width;

            listingExternal.ColumnWidth = rect.width - 20f;

            listingExternal.Begin(rect);
            {
                listingExternal.CheckboxLabeled("Do Not Disturb", ref Settings.DoNotDisturb, labelPct: scaleFactor);
                listingExternal.Gap(4);

                var headersRect = new Rect(0, listingExternal.CurHeight, rect.width, 100f);

                listingHeaders.Begin(headersRect);
                {
                    listingHeaders.ColumnWidth = 200f;
                    listingHeaders.Label("Room");
                    listingHeaders.NewColumn();

                    listingHeaders.ColumnWidth = 200f;
                    listingHeaders.Label("Priority");
                    listingHeaders.NewColumn();

                    listingHeaders.ColumnWidth = 40f;
                    listingHeaders.Label("");
                    listingHeaders.NewColumn();

                    listingHeaders.ColumnWidth = 200f;
                    listingHeaders.Label("Threshold");
                    listingHeaders.NewColumn();

                    listingHeaders.ColumnWidth = 40f;
                    listingHeaders.Label("");

                } listingHeaders.End();

                listingExternal.Gap(listingHeaders.CurHeight);
                listingExternal.GapLine(10f);
                listingExternal.Gap(5f);

                Rect scrollRect = new Rect(0, listingExternal.CurHeight, rect.width, rect.height - listingExternal.CurHeight - 44f);

                listingExternal.Gap(scrollRect.height + 12f);

                Widgets.BeginScrollView(scrollRect, ref scrollPosition, view);
                {
                    listingInternal.Begin(view);
                    {
                        listingInternal.ColumnWidth = 200f;

                        foreach (RoomRoleDef roomDef in defs)
                        {
                            listingInternal.Label(roomDef.defName == "None" ? "Outdoors" : roomDef.PostProcessedLabelCap);
                        }

                        listingInternal.NewColumn();

                        listingInternal.ColumnWidth = 200;

                        listingInternal.Gap(4f);

                        foreach (RoomRoleDef roomDef in defs)
                        {
                            Settings.Weights[roomDef.defName].Weight = (float)Math.Round(listingInternal.Slider(Settings.Weights[roomDef.defName].Weight, 0, 100f));
                        }

                        listingInternal.NewColumn();

                        listingInternal.ColumnWidth = 40;

                        foreach (RoomRoleDef roomDef in defs)
                        {
                            listingInternal.Label(Settings.Weights[roomDef.defName].Weight.ToString());
                        }

                        listingInternal.NewColumn();

                        listingInternal.ColumnWidth = 200;

                        listingInternal.Gap(4f);

                        foreach (RoomRoleDef roomDef in defs)
                        {
                            if (roomDef.defName == "None")
                            {
                                listingInternal.Gap(22f);
                            }
                            else
                            {
                                Settings.Weights[roomDef.defName].Threshold = (float)(Math.Round(listingInternal.Slider(Settings.Weights[roomDef.defName].Threshold, -5f, 1f) * 4) / 4f);
                            }
                        }

                        listingInternal.NewColumn();

                        listingInternal.ColumnWidth = 40;

                        foreach (RoomRoleDef roomDef in defs)
                        {
                            listingInternal.Label(Settings.Weights[roomDef.defName].Threshold.ToString());
                        }

                    } listingInternal.End();
                } Widgets.EndScrollView();

                if (listingExternal.ButtonText("Default", widthPct: scaleFactor))
                {
                    var keys = Settings.Weights.Keys.ToList();

                    foreach (var key in keys)
                    {
                        Settings.Weights[key].Weight = 50f;
                        Settings.Weights[key].Threshold = key == "None" ? 0 : -0.5f;
                    }

                    Settings.DoNotDisturb = false;
                }
            } listingExternal.End();

            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Cleanie";
        }
    }
}
