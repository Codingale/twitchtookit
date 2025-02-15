﻿using System;
using System.Linq;
using Verse;
using RimWorld;

namespace TwitchToolkit.Incidents
{
    public class IncidentWorker_AnimalInsanitySingle : IncidentWorker
    {
        readonly string Quote;

        public IncidentWorker_AnimalInsanitySingle(string quote)
        {
            Quote = quote;
        }

        private const int FixedPoints = 30;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
            Map map = (Map)parms.target;
            Pawn pawn;
            return this.TryFindRandomAnimal(map, out pawn);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            Pawn pawn;
            if (!this.TryFindRandomAnimal(map, out pawn))
            {
                return false;
            }
            IncidentWorker_AnimalInsanityMass.DriveInsane(pawn);
            string text = "AnimalInsanitySingle".Translate(pawn.Label, pawn.Named("ANIMAL"));

            if (Quote != null)
            {
                text += "\n\n";
                text += Helper.ReplacePlaceholder(Quote, animal: pawn.Label);
            }

            Find.LetterStack.ReceiveLetter("LetterLabelAnimalInsanitySingle".Translate(pawn.Label, pawn.Named("ANIMAL")), text, LetterDefOf.ThreatSmall, pawn, null, null);
            return true;
        }

        private bool TryFindRandomAnimal(Map map, out Pawn animal)
        {
            int maxPoints = 150;
            /*if (GenDate.DaysPassed < 7)
            {
              maxPoints = 40;
            }*/
            return (from p in map.mapPawns.AllPawnsSpawned
                    where p.RaceProps.Animal && p.kindDef.combatPower <= (float)maxPoints && IncidentWorker_AnimalInsanityMass.AnimalUsable(p)
                    select p).TryRandomElement(out animal);
        }
    }
}