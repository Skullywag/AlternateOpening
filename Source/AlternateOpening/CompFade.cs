using Verse;
using UnityEngine;

namespace AlternateOpening
{
    public class CompFade : ThingComp
    {
        public int remainingTicks = -1; // Ticks up based on lifetime of comp/parent
        public CompProperties_Lifespan Props
        {
            get
            {
                return (CompProperties_Lifespan)this.props;
            }
        }
        public override void PostSpawnSetup()
        {
            // Do base setup
            base.PostSpawnSetup();
            // If remaining ticks is less than 0
            if (this.remainingTicks < 0)
            {
                // Set remaining ticks to the life span amount from xml/default
                this.remainingTicks = this.Props.lifespanTicks;
            }
        }
        public override void PostExposeData()
        {
            // Base date to save
            base.PostExposeData();
            // Savwe remaing ticks to save file
            Scribe_Values.LookValue<int>(ref this.remainingTicks, "remainingTicks", this.Props.lifespanTicks, true);
        }
        public override void CompTick()
        {
            base.CompTick();
            // Normal tick is 1 tick
            this.TickDown(1);
            // grab the parent things graphic
            Graphic graphic = this.parent.Graphic;
            // Get the material element and set to a temporary variable
            Material material = graphic.MatSingle;
            // Get the color and set it to temporary variable
            Color color = material.color;
            // Drop the color transparancy based on game ticks
            color.a = (float)remainingTicks / (float)this.Props.lifespanTicks;
            // Set the color with changed transparency back on the material
            material.color = color;
        }
        public override void CompTickRare()
        {
            base.CompTickRare();
            // Rare tick is 250 ticks
            this.TickDown(250);
            // grab the parent things graphic
            Graphic graphic = this.parent.Graphic;
            // Get the material element and set to a temporary variable
            Material material = graphic.MatSingle;
            // Get the color and set it to temporary variable
            Color color = material.color;
            // Drop the color transparancy based on game ticks
            color.a = (float)remainingTicks / (float)this.Props.lifespanTicks;
            // Set the color with changed transparency back on the material
            material.color = color;
        }
        public void TickDown(int down)
        {
            // Tick down based on passed params
            this.remainingTicks -= down;
            // If remaining ticks is more than 0 keep looping
            if (this.remainingTicks > 0)
            {
                return;
            }
            // Remaining ticks have reached 0, remove parent thing
            this.parent.Destroy(DestroyMode.Vanish);
        }
    }
}
