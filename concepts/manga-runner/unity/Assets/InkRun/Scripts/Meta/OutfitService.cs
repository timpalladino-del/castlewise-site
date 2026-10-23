using System.Collections.Generic;

namespace InkRun.Meta
{
    public class OutfitService
    {
        public IReadOnlyList<OutfitDefinition> All { get; }
        public OutfitService(OutfitDefinition[] catalog) { All = catalog ?? new OutfitDefinition[0]; }

        public OutfitDefinition Find(string id) { foreach (var o in All) if (o.Id == id) return o; return null; }

        /// <summary>Buys with banked coins if not owned, then wears it. Returns false if the player cannot afford it.</summary>
        public bool BuyOrWear(string id)
        {
            var o = Find(id); if (o == null) return false;
            var save = SaveData.Current;
            if (!save.Owns(id))
            {
                if (save.Bank < o.Price) return false;
                save.Bank -= o.Price; save.Owned.Add(id);
                Services.Services.Analytics.Log("outfit_bought", ("id", id), ("price", o.Price));
            }
            save.Outfit = id; save.Save();
            return true;
        }
    }
}
