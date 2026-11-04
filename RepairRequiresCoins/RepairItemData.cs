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

    internal class RepairItemData
    {
        public List<string> reqstring;
        public List<string> reqplain;
        public ItemDrop.ItemData item;

        public RepairItemData(ItemDrop.ItemData item, List<string> reqstring = null, List<string> reqplain = null)
        {
            this.reqstring = reqstring;
            this.reqplain = reqplain;
            this.item = item;
        }
    }
}
