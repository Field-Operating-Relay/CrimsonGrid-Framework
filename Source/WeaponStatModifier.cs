using System.Xml;
using Verse;
using RimWorld;

namespace CrimsonGridFramework
{
    public class WeaponStatModifier
    {
        public StatDef stat;
        public float offset = 0f;
        public float factor = 1f;

        public bool HasOffset => offset != 0f;
        public bool HasFactor => factor != 1f;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "stat", xmlRoot.Name);

            foreach (XmlNode childNode in xmlRoot.ChildNodes)
            {
                switch (childNode.Name)
                {
                    case "offset":
                        offset = ParseHelper.FromString<float>(childNode.InnerText);
                        break;
                    case "factor":
                        factor = ParseHelper.FromString<float>(childNode.InnerText);
                        break;
                }
            }
        }

        public override string ToString()
        {
            if (stat == null)
            {
                return "(null stat)";
            }
            return $"{stat.defName} (offset: {offset}, factor: {factor})";
        }
    }
}