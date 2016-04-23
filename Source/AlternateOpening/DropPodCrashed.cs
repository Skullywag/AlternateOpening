using System;
using Verse;
using System.Collections.Generic;
using Verse.Sound;
using Verse.AI;
using RimWorld;
using UnityEngine;

namespace AlternateOpening
{
    public class DropPodCrashed : Building
    {
        public int age; // Life of thing in ticks
        private static readonly SoundDef OpenSound = SoundDef.Named("DropPodOpen"); // Open sound
        public DropPodInfo info; // Drop pods contents and config
        public bool opened = false; // Whether the pod has opened or not

        public override void ExposeData()
        {
            // Base data to save
            base.ExposeData();
            // Save age ticks to save file
            Scribe_Values.LookValue<int>(ref this.age, "age", 0, false);
            // Save pods content
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
                    this.ShipOpen();
                    opened = true;
                }
            }
        }
        
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            // Call base destory method
            base.Destroy(mode);
            // Check destroy mod passed
            if (mode == DestroyMode.Deconstruct || mode == DestroyMode.Kill)
            {
                // iterate a few times
                for (int j = 0; j < 2; j++)
                {
                    // Drop some steel slag
                    Thing thing = ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel, null);
                    GenPlace.TryPlaceThing(thing, base.Position, ThingPlaceMode.Near);
                }
            }
        }

        private void ShipOpen()
        {
            // loop over all contents
            for (int i = 0; i < this.info.containedThings.Count; i++)
            {
                // Setup a new thing
                Thing thing = this.info.containedThings[i];
                // Place thing in world
                GenPlace.TryPlaceThing(thing, GenAdj.RandomAdjacentCell8Way(this.Position), ThingPlaceMode.Near);
                // If its a pawn
                Pawn pawn = thing as Pawn;
                // if its a humanlike pawn
                if (pawn != null && pawn.RaceProps.Humanlike)
                {
                    // Record a tale
                    TaleRecorder.RecordTale(TaleDef.Named("LandedInPod"), new object[]
                    {
                        pawn
                    });
                }
            }
            // All contents dealt with, clear list
            this.info.containedThings.Clear();
            // Play open sound
            DropPodCrashed.OpenSound.PlayOneShot(base.Position);
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            Action lootAction = () =>
            {
                DesignateForScavenge();
                selPawn.drafter.TakeOrderedJob(new Job(JobDefOf.Deconstruct, this));
            };
            yield return new FloatMenuOption("Scavenge the " + this.Label, lootAction);
        }

        public void DesignateForScavenge()
        {
            this.SetFaction(Faction.OfColony);
            Find.DesignationManager.AddDesignation(new Designation(this, DesignationDefOf.Deconstruct));
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
                comActScav.defaultDesc = "Scavenge some materials from this pod";
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
            SetFaction(Faction.OfColony);
            Find.DesignationManager.AddDesignation(new Designation(this, DesignationDefOf.Deconstruct));
        }
    }
}
