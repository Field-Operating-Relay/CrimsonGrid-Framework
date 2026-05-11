using Verse;

namespace CrimsonGridFramework
{
    public class Verb_ShootFlamethrower : Verb_Shoot
    {
        private bool warmupDone;
        private CompFlamethrower FlamethrowerComp => EquipmentSource.GetComp<CompFlamethrower>();

        public override float WarmupTime
        {
            get
            {
                if (warmupDone)
                    return 0f;
                return base.WarmupTime;
            }
        }

        public override void WarmupComplete()
        {
            base.WarmupComplete();
            warmupDone = true;
        }

        public override void Reset()
        {
            base.Reset();
            warmupDone = false;
        }

        protected override bool TryCastShot()
        {
            var comp = FlamethrowerComp;
            if (!base.TryCastShot())
                return false;
            comp.OnShot(CasterPawn);
            return true;
        }
    }
}
