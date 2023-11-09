using HarmonyLib;
using RimWorld;
using System;
using System.Linq;
using Verse;

namespace Cleanie
{
    [HarmonyPatch(typeof(WorkGiver_Scanner), nameof(WorkGiver_Scanner.GetPriority), new Type[] { typeof(Pawn), typeof(TargetInfo) })]
    public class WorkGiver_Filth_GetPriority_Patch
    {
        static bool Prefix(ref float __result, WorkGiver_Scanner __instance, Pawn pawn, TargetInfo t)
        {
            if (!(__instance is WorkGiver_CleanFilth))
            {
                return true;
            }

            var room = t.Cell.GetRoom(t.Map);

            if (room == null)
            {
                __result = 0;
            }
            else
            {
                var dnd = false;

                if (Cleanie.Settings.DoNotDisturb 
                    && !(room.Role == RoomRoleDefOf.Hospital || room.Role == RoomRoleDefOf.None) 
                    && room.ContainedBeds.Any(b => !b.ForHumanBabies && b.CurOccupants.Any()))
                {
                    dnd = true;
                }

                if (dnd)
                {
                    __result = 0;
                }
                else
                {
                    __result = GetRoomDefWeights(room.Role);

                    var distance = pawn.Position.DistanceToSquared(t.CenterCell);
                    var belowThreshold = room.GetStat(RoomStatDefOf.Cleanliness) <= GetRoomDefThreshold(room.Role);
                    var factor = 0.25f + Math.Min(Math.Max(0.75f - distance / 533f, 0f), 0.75f) * (belowThreshold ? 0.1f : 1f);
                    __result *= factor;
                }
            }
#if DEBUG
            Log.Message($"[Cleanie] {pawn.Name} {pawn.Position} {t.Cell} {room.Role.label} {__result}");
#endif

            return false;
        }

        static float GetRoomDefWeights(RoomRoleDef def)
        {
            if (Cleanie.Settings.Weights.TryGetValue(def.defName, out var pair))
            {
                return pair.Weight;
            }
            else
            {
                return 50f;
            }            
        }

        static float GetRoomDefThreshold(RoomRoleDef def)
        {
            if (Cleanie.Settings.Weights.TryGetValue(def.defName, out var pair))
            {
                return pair.Threshold;
            }
            else
            {
                return 0f;
            }
        }
    }
}
