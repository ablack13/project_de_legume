using System.Collections.Generic;
using System.Linq;

namespace ProiectDeLegume.Scripts.Inventory;

public class Inventory
{
    public double MaxWeight { get; }
    public List<ItemStack> Items { get; } = new();

    public Inventory(double maxWeight = 15.0)
    {
        MaxWeight = maxWeight;
    }

    public double CurrentWeight => Items.Sum(s => s.TotalWeight);
    public double FreeWeight => MaxWeight - CurrentWeight;

    public bool CanAdd(ItemDef def, int count = 1)
    {
        return CurrentWeight + def.Weight * count <= MaxWeight;
    }

    public bool Add(ItemDef def, int count = 1)
    {
        if (!CanAdd(def, count)) return false;

        if (def.Stackable)
        {
            var existing = Items.FirstOrDefault(s => s.Def.Id == def.Id && s.CanAdd());
            if (existing != null)
            {
                int canFit = System.Math.Min(def.MaxStack - existing.Count, count);
                existing.Count += canFit;
                int remainder = count - canFit;
                if (remainder > 0) Items.Add(new ItemStack(def, remainder));
                return true;
            }
        }

        Items.Add(new ItemStack(def, count));
        return true;
    }

    public bool Remove(int index)
    {
        if (index < 0 || index >= Items.Count) return false;
        var stack = Items[index];
        if (stack.Count > 1) stack.Count--;
        else Items.RemoveAt(index);
        return true;
    }
}
