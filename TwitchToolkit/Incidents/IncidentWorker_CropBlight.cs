﻿using System;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

namespace TwitchToolkit.Incidents
{
    public class IncidentWorker_CropBlight : IncidentWorker
    {
        string Quote;

        private float Radius = 15f;

        private const float BaseBlightChance = 0.4f;

        public IncidentWorker_CropBlight(string quote, float radius = 15f)
        {
            Quote = quote;
            Radius = radius;
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Plant plant;
            return this.TryFindRandomBlightablePlant((Map)parms.target, out plant);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            Plant plant;
            if (!this.TryFindRandomBlightablePlant(map, out plant))
            {
                return false;
            }
            Room room = plant.GetRoom(RegionType.Set_Passable);
            plant.CropBlighted();
            int i = 0;
            int num = GenRadial.NumCellsInRadius(Radius);
            while (i < num)
            {
                IntVec3 intVec = plant.Position + GenRadial.RadialPattern[i];
                if (intVec.InBounds(map) && intVec.GetRoom(map, RegionType.Set_Passable) == room)
                {
                    Plant firstBlightableNowPlant = BlightUtility.GetFirstBlightableNowPlant(intVec, map);
                    if (firstBlightableNowPlant != null && firstBlightableNowPlant != plant)
                    {
                        if (Rand.Chance(0.4f * this.BlightChanceFactor(firstBlightableNowPlant.Position, plant.Position)))
                        {
                            firstBlightableNowPlant.CropBlighted();
                        }
                    }
                }
                i++;
            }

            var text = "LetterCropBlight".Translate();

            if (Quote != null)
            {
                text += "\n\n";
                text += Quote;
            }

            Find.LetterStack.ReceiveLetter("LetterLabelCropBlight".Translate(), text, LetterDefOf.NegativeEvent, new TargetInfo(plant.Position, map, false), null, null);
            return true;
        }

        private bool TryFindRandomBlightablePlant(Map map, out Plant plant)
        {
            Thing thing;
            bool result = (from x in map.listerThings.ThingsInGroup(ThingRequestGroup.Plant)
                           where ((Plant)x).BlightableNow
                           select x).TryRandomElement(out thing);
            plant = (Plant)thing;
            return result;
        }

        private float BlightChanceFactor(IntVec3 c, IntVec3 root)
        {
            return Mathf.InverseLerp(15f, 7.5f, c.DistanceTo(root));
        }
    }
}