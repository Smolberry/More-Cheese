using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
#nullable disable
namespace More_Cheese
{
    public class BECheeseMod : BlockEntityContainer
    {
        InventoryGeneric inv;
        public override InventoryBase Inventory => inv;

        public override string InventoryClassName => "cheesemod";

        public BECheeseMod()
        {
            inv = new InventoryGeneric(1, null, null);
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            inv.LateInitialize("cheesemod-" + Pos, api);
        }

        public override void OnBlockPlaced(ItemStack byItemStack = null)
        {
            if (byItemStack != null)
            {
                inv[0].Itemstack = byItemStack.Clone();
                inv[0].Itemstack.StackSize = 1;
            }
        }

        public int SlicesLeft
        {
            get
            {
                if (inv[0].Empty) return 0;
                ItemModCheese cheesemod = inv[0].Itemstack.Collectible as ItemModCheese;
                switch (cheesemod?.Part)
                {
                    case "1slice": return 1;
                    case "2slice": return 2;
                    case "3slice": return 3;
                    case "4slice": return 4;
                    default: return 0;
                }
            }
        }

        public ItemStack TakeSlice()
        {
            if (inv[0].Empty) return null;
            var attr = inv[0].Itemstack.Attributes;
            ItemModCheese cheesemod = inv[0].Itemstack.Collectible as ItemModCheese;
            MarkDirty(true);

            switch (cheesemod.Part)
            {
                case "1slice":
                    {
                        ItemStack stack = inv[0].Itemstack;
                        inv[0].Itemstack = null;
                        Api.World.BlockAccessor.SetBlock(0, Pos);
                        return stack;
                    }
                case "2slice":
                    {
                        inv[0].Itemstack = new(Api.World.GetItem(cheesemod.CodeWithVariant("part", "1slice"))) { Attributes = attr };
                        return inv[0].Itemstack.Clone();
                    }
                case "3slice":
                    {
                        inv[0].Itemstack = new(Api.World.GetItem(cheesemod.CodeWithVariant("part", "2slice"))) { Attributes = attr };
                        return new(Api.World.GetItem(cheesemod.CodeWithVariant("part", "1slice"))) { Attributes = attr };
                    }
                case "4slice":
                    {
                        inv[0].Itemstack = new(Api.World.GetItem(cheesemod.CodeWithVariant("part", "3slice"))) { Attributes = attr };
                        return new(Api.World.GetItem(cheesemod.CodeWithVariant("part", "1slice"))) { Attributes = attr };
                    }
            }

            return null;
        }


        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            if (inv[0].Empty) return true;

            tessThreadTesselator.TesselateShape(Block, (Api as ICoreClientAPI).TesselatorManager.GetCachedShape(inv[0].Itemstack.Item.Shape.Base), out MeshData modeldata);
            mesher.AddMeshData(modeldata);
            return true;
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            dsc.Append(BlockEntityShelf.PerishableInfoCompact(Api, inv[0], 0));
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);

            if (worldForResolving.Side == EnumAppSide.Client)
            {
                MarkDirty(true);
            }
        }
    }
}