using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CrimsonGridFramework
{
    public class MapComponent_ProjectileTracker : MapComponent
    {
        private HashSet<Projectile> explosiveProjectiles = new HashSet<Projectile>();

        public IEnumerable<Projectile> ExplosiveProjectiles => explosiveProjectiles;

        public MapComponent_ProjectileTracker(Map map) : base(map)
        {
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            RescanAllProjectiles();
        }

        private void RescanAllProjectiles()
        {
            explosiveProjectiles.Clear();

            if (map == null)
            {
                return;
            }

            var allProjectiles = map.listerThings.ThingsInGroup(ThingRequestGroup.Projectile);

            foreach (var thing in allProjectiles)
            {
                if (thing is Projectile projectile)
                {
                    RegisterProjectile(projectile);
                }
            }
        }

        public void RegisterProjectile(Projectile projectile)
        {
            if (projectile?.def?.projectile != null && IsExplosiveProjectile(projectile))
            {
                Log.Message("Tracking projectile");
                explosiveProjectiles.Add(projectile);
            }
        }

        public void UnregisterProjectile(Projectile projectile)
        {
            Log.Message("Untracking projectile");
            explosiveProjectiles.Remove(projectile);
        }

        private bool IsExplosiveProjectile(Projectile projectile)
        {
            var verbProps = projectile.def.Verbs?.FirstOrDefault();
            if (verbProps != null && verbProps.CausesExplosion)
            {
                return true;
            }

            return projectile.def.projectile.explosionRadius > 0f;
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }
    }
}