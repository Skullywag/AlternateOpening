using System;
using Verse;
using Verse.Sound;
using RimWorld;
using UnityEngine;
using System.Collections.Generic;

namespace AlternateOpening
{
    public class DropPod : ThingWithComps
    {
        public int age;

        public DropPodInfo info;
        public bool opened = false; // Whether the pod has opened or not

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref this.age, "age", 0, false);
            Scribe_Deep.LookDeep<DropPodInfo>(ref this.info, "info", new object[0]);
        }

        public override void Tick()
        {
            base.Tick();
            // If we havent opened yet
            if (opened == false)
            {
                // Increment its age
                this.age++;
                if (this.age > this.info.openDelay)
                {
                    // Open the pod
                    this.PodOpen();
                    opened = true;
                }
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            for (int i = this.info.containedThings.Count - 1; i >= 0; i--)
            {
                this.info.containedThings[i].Destroy(DestroyMode.Vanish);
            }
            base.Destroy(mode);
            if (mode == DestroyMode.Kill)
            {
                for (int j = 0; j < 1; j++)
                {
                    Thing thing = ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel, null);
                    GenPlace.TryPlaceThing(thing, base.Position, ThingPlaceMode.Near);
                }
            }
        }

        private void PodOpen()
        {
            for (int i = 0; i < this.info.containedThings.Count; i++)
            {
                Thing thing = this.info.containedThings[i];
                GenPlace.TryPlaceThing(thing, base.Position, ThingPlaceMode.Near);
                Pawn pawn = thing as Pawn;
                if (pawn != null && pawn.RaceProps.Humanlike)
                {
                    TaleRecorder.RecordTale(TaleDef.Named("LandedInPod"), new object[]
                    {
                        pawn
                    });
                }
            }
            this.info.containedThings.Clear();
            if (this.info.leaveSlag)
            {
                for (int j = 0; j < 1; j++)
                {
                    Thing thing2 = ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel, null);
                    GenPlace.TryPlaceThing(thing2, base.Position, ThingPlaceMode.Near);
                }
            }
            SoundDef.Named("DropPodOpen").PlayOneShot(base.Position);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            // Do base gizmos if required
            foreach (Gizmo curGizmo in base.GetGizmos())
            {
                yield return curGizmo;
            }

            // Setup a new command
            var comActScav = new Command_Action();
            if (comActScav != null)
            {
                // Command icon
                comActScav.icon = ContentFinder<Texture2D>.Get("UI/Gizmo/Scavenge");
                // Command label
                comActScav.defaultLabel = "Scavenge";
                // Command description
                comActScav.defaultDesc = "Find tools and materials within this building";
                // Command sound when activated
                comActScav.activateSound = SoundDef.Named("Click");
                // Action to call
                comActScav.action = new Action(ScavengeBuilding);
                // Add new command
                if (comActScav.action != null)
                {
                    yield return comActScav;
                }
            }
            // No command set something went wrong
            yield break;
        }

        void ScavengeBuilding()
        {
            // When command above is fired mark the building for deconstruction
            var designator = new Designator_Deconstruct();
            designator.DesignateThing(this);
        }
    }
}
