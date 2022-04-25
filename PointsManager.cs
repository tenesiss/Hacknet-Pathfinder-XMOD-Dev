using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMOD
{
    public class PointsManager
    {
        public static List<PointsPlaceholder> placeholders = new List<PointsPlaceholder>();

        public static void AddPlaceholder(PointsPlaceholder placeholderToAdd)
        {
            if(placeholders.Find(pc => pc.placeholderName == placeholderToAdd.placeholderName) == null)
            {
                placeholders.Add(placeholderToAdd);
            } else
            {
                placeholders.Remove(placeholderToAdd);
                placeholders.Add(placeholderToAdd);
            }
        }

        public static PointsPlaceholder GetPlaceholder(string PlaceholderName)
        {
            return placeholders.Find(pc => pc.placeholderName == PlaceholderName);
        }
        public static void RemovePlaceholder(string PlaceholderName)
        {
            if(placeholders.Find(pc => pc.placeholderName == PlaceholderName) != null)
            {
                PointsPlaceholder placeholderFR = placeholders.Find(pc => pc.placeholderName == PlaceholderName);
                placeholders.Remove(placeholderFR);
            }
        }
    }
}
