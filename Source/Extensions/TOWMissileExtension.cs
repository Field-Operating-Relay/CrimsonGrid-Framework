using Verse;

namespace CrimsonGridFramework
{
    public class TOWMissileExtension : DefModExtension
    {
        public float maxTurnRateDegreesPerTick;
        public int wireSegments;
        public float wireWidth;
        public int wireSCount = 2;
        public FloatRange waveAnimSpeed = new FloatRange(8f);
        public float wireAmplitude;
        public bool useArcTrajectory;
    }
}
