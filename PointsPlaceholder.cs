using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMOD
{
    public class PointsPlaceholder
    {
        public string placeholderName;
        public int points;
        

        public PointsPlaceholder(string _name, int _points, bool AddToManager = true)
        {
            placeholderName = _name;
            points = _points;
            if(AddToManager == true)
            {
                UpdateMG();
            }
        }
        public void AddPoints(int amount)
        {
            points += amount;
            UpdateMG();
        }

        public void SubstractPoints(int amount)
        {
            points -= amount;
            UpdateMG();
        }

        public void ResetPoints(int BaseAmount = 0)
        {
            points = BaseAmount;
            UpdateMG();
        }

        public void DeletePlaceholder()
        {
            PointsManager.RemovePlaceholder(placeholderName);
        }

        public void UpdateMG()
        {
            PointsManager.AddPlaceholder(this);
        }
    }
}
