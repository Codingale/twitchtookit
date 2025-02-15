﻿using System;
using Verse;
using RimWorld;

namespace TwitchToolkit.Incidents
{
    public class IncidentWorker_WildManWandersIn : IncidentWorker
    {
        readonly string Quote;

        public IncidentWorker_WildManWandersIn(string quote)
        {
            Quote = quote;
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
            Faction faction;
            if (!this.TryFindFormerFaction(out faction))
            {
                return false;
            }
            Map map = (Map)parms.target;
            IntVec3 intVec;
            return !map.GameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout) && map.mapTemperature.SeasonAcceptableFor(ThingDefOf.Human) && this.TryFindEntryCell(map, out intVec);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            IntVec3 loc;
            if (!this.TryFindEntryCell(map, out loc))
            {
                return false;
            }
            Faction faction;
            if (!this.TryFindFormerFaction(out faction))
            {
                return false;
            }
            Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.WildMan, faction);
            pawn.SetFaction(null, null);
            GenSpawn.Spawn(pawn, loc, map, WipeMode.Vanish);
            string label = this.def.letterLabel.Formatted(pawn.LabelShort, pawn.Named("PAWN"));
            string text = this.def.letterText.Formatted(pawn.LabelShort, pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN").CapitalizeFirst();

            if (Quote != null)
            {
                text += "\n\n";
                var q = Quote;
                var l1 = "female";
                var l2 = "male";
                if (pawn.gender == Gender.Female)
                {
                    l1 = "male";
                    l2 = "female";
                }
                var a = Quote.IndexOf("{#" + l1 + "}", StringComparison.InvariantCultureIgnoreCase);
                var b = Quote.IndexOf("{/" + l1 + "}", StringComparison.InvariantCultureIgnoreCase);
                q = Quote.Remove(a, b - a + l1.Length + 3);
                q = q.Replace("{#" + l2 + "}", "");
                q = q.Replace("{/" + l2 + "}", "");
                text += q;
            }

            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref label, pawn);
            Find.LetterStack.ReceiveLetter(label, text, this.def.letterDef, pawn, null, null);
            return true;
        }

        private bool TryFindEntryCell(Map map, out IntVec3 cell)
        {
            return CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => map.reachability.CanReachColony(c), map, CellFinder.EdgeRoadChance_Ignore, out cell);
        }

        private bool TryFindFormerFaction(out Faction formerFaction)
        {
            return Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out formerFaction, false, true, TechLevel.Undefined);
        }
    }
}