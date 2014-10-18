using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tac
{
    internal class Settings
    {
        internal bool showStage { get; set; }
        internal bool showFullMass { get; set; }
        internal bool showResourceMass { get; set; }
        internal bool showEmptyMass { get; set; }
        internal bool showFullCost { get; set; }
        internal bool showResourceCost { get; set; }
        internal bool showEmptyCost { get; set; }

        internal Settings()
        {
            showStage = true;
            showFullMass = true;
            showResourceMass = true;
            showEmptyMass = true;
            showFullCost = true;
            showResourceCost = true;
            showEmptyCost = true;
        }

        internal void Load(ConfigNode config)
        {
            showStage = Utilities.GetValue(config, "showStage", showStage);
            showFullMass = Utilities.GetValue(config, "showFullMass", showFullMass);
            showResourceMass = Utilities.GetValue(config, "showResourceMass", showResourceMass);
            showEmptyMass = Utilities.GetValue(config, "showEmptyMass", showEmptyMass);
            showFullCost = Utilities.GetValue(config, "showFullCost", showFullCost);
            showResourceCost = Utilities.GetValue(config, "showResourceCost", showResourceCost);
            showEmptyCost = Utilities.GetValue(config, "showEmptyCost", showEmptyCost);
        }

        internal void Save(ConfigNode config)
        {
            config.AddValue("showStage", showStage);
            config.AddValue("showFullMass", showFullMass);
            config.AddValue("showResourceMass", showResourceMass);
            config.AddValue("showEmptyMass", showEmptyMass);
            config.AddValue("showFullCost", showFullCost);
            config.AddValue("showResourceCost", showResourceCost);
            config.AddValue("showEmptyCost", showEmptyCost);
        }
    }
}
