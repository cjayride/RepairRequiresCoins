using System.Collections.Generic;

namespace RepairRequiresMats
{
    internal class RepairOfferLine
    {
        public string Name;
        public string Cost;
        public string Note;
        public UnityEngine.Sprite Icon;
        public bool IconUsed;
    }

    internal class RepairItemData : ItemDrop.ItemData
    {
        public List<string> reqstring;
        public ItemDrop.ItemData item;

        public RepairItemData(ItemDrop.ItemData item, List<string> reqstring = null)
        {
            this.reqstring = reqstring;
            this.item = item;
        }
    }
}