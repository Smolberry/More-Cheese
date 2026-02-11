using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

#nullable disable

namespace More_Cheese
{
    public class BlockCheeseCurdsBundleMod : Block
    {
        public Dictionary<string, MeshData> meshes = new Dictionary<string, MeshData>();
        public AssetLocation outputCh = new AssetLocation();

        WorldInteraction[] interactions;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            ItemStack[] stickStack = new ItemStack[] { new ItemStack(api.World.GetItem(new AssetLocation("stick"))) };
            ItemStack[] saltStack = new ItemStack[] { new ItemStack(api.World.GetItem(new AssetLocation("salt")), 5) };

            interactions = new WorldInteraction[] {
                new WorldInteraction
                {
                    ActionLangCode = "morecheese:blockhelp-curdbundle-unbundled",
                    MouseButton = EnumMouseButton.Right,
                    Itemstacks = null,
                    ShouldApply = (WorldInteraction wi, BlockSelection blockSelection, EntitySelection entitySelection) =>
                    {
                        BECheeseCurdsBundleMod beccb = api.World.BlockAccessor.GetBlockEntity(blockSelection.Position) as BECheeseCurdsBundleMod;
                        return beccb?.State == EnumCurdsBundleModState.Unbundled;
                    }
                },
                new WorldInteraction() {
                    ActionLangCode = "blockhelp-curdbundle-addstick",
                    MouseButton = EnumMouseButton.Right,
                    Itemstacks = stickStack,
                    GetMatchingStacks = (WorldInteraction wi, BlockSelection blockSelection, EntitySelection entitySelection) =>
                    {
                        BECheeseCurdsBundleMod beccb = api.World.BlockAccessor.GetBlockEntity(blockSelection.Position) as BECheeseCurdsBundleMod;
                        return beccb?.State == EnumCurdsBundleModState.Bundled ? stickStack : null;
                    }
                },
                new WorldInteraction() {
                    ActionLangCode = "blockhelp-curdbundle-squeeze",
                    MouseButton = EnumMouseButton.Right,
                    Itemstacks = null,
                    ShouldApply = (WorldInteraction wi, BlockSelection blockSelection, EntitySelection entitySelection) =>
                    {
                        BECheeseCurdsBundleMod beccb = api.World.BlockAccessor.GetBlockEntity(blockSelection.Position) as BECheeseCurdsBundleMod;
                        return beccb?.State == EnumCurdsBundleModState.BundledStick && !beccb.Squuezed;
                    }
                },
                new WorldInteraction() {
                    ActionLangCode = "blockhelp-curdbundle-open",
                    MouseButton = EnumMouseButton.Right,
                    Itemstacks = null,
                    ShouldApply = (WorldInteraction wi, BlockSelection blockSelection, EntitySelection entitySelection) =>
                    {
                        BECheeseCurdsBundleMod beccb = api.World.BlockAccessor.GetBlockEntity(blockSelection.Position) as BECheeseCurdsBundleMod;
                        return beccb?.State == EnumCurdsBundleModState.BundledStick && beccb.Squuezed;
                    }
                },
                new WorldInteraction() {
                    ActionLangCode = "blockhelp-curdbundle-addsalt",
                    MouseButton = EnumMouseButton.Right,

                    Itemstacks = saltStack,
                    GetMatchingStacks = (WorldInteraction wi, BlockSelection blockSelection, EntitySelection entitySelection) =>
                    {
                        BECheeseCurdsBundleMod beccb = api.World.BlockAccessor.GetBlockEntity(blockSelection.Position) as BECheeseCurdsBundleMod;
                        return beccb?.State == EnumCurdsBundleModState.Opened ? saltStack : null;
                    }
                },
                new WorldInteraction() {
                    ActionLangCode = "blockhelp-curdbundle-pickupcheese",
                    MouseButton = EnumMouseButton.Right,
                    Itemstacks = null,
                    ShouldApply = (WorldInteraction wi, BlockSelection blockSelection, EntitySelection entitySelection) =>
                    {
                        BECheeseCurdsBundleMod beccb = api.World.BlockAccessor.GetBlockEntity(blockSelection.Position) as BECheeseCurdsBundleMod;
                        return beccb?.State == EnumCurdsBundleModState.OpenedSalted;
                    }
                }
            };
        }


        public Shape GetShape(EnumCurdsBundleModState state)
        {
            string path = "shapes/block/food/curdbundle-plain.json";
            if (state == EnumCurdsBundleModState.BundledStick) path = "shapes/block/food/curdbundle-stick.json";
            else if (state == EnumCurdsBundleModState.Opened) path = "shapes/item/food/dairy/cheese/linen-raw.json";
            else if (state == EnumCurdsBundleModState.OpenedSalted) path = "shapes/item/food/dairy/cheese/linen-salted.json";

            return Vintagestory.API.Common.Shape.TryGet(api, path);
        }


        public MeshData GetMesh(EnumCurdsBundleModState state, float angle)
        {
            string key = (int)state + "-" + angle;
            if (!meshes.ContainsKey(key))
            {
                Shape shape = GetShape(state);
                ICoreClientAPI capi = api as ICoreClientAPI;
                capi.Tesselator.TesselateShape(this, shape, out MeshData mesh, new Vec3f(0, angle * GameMath.RAD2DEG, 0));

                meshes[key] = mesh;
            }

            return meshes[key];
        }

        public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
        {
            bool val = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);

            if (val)
            {
                BECheeseCurdsBundleMod bect = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BECheeseCurdsBundleMod;
                if (bect != null)
                {
                    BlockPos targetPos = blockSel.DidOffset ? blockSel.Position.AddCopy(blockSel.Face.Opposite) : blockSel.Position;
                    double dx = byPlayer.Entity.Pos.X - (targetPos.X + blockSel.HitPosition.X);
                    double dz = (float)byPlayer.Entity.Pos.Z - (targetPos.Z + blockSel.HitPosition.Z);
                    float angleHor = (float)Math.Atan2(dx, dz);
                    float deg22dot5rad = GameMath.PIHALF / 4;
                    float roundRad = ((int)Math.Round(angleHor / deg22dot5rad)) * deg22dot5rad;
                    bect.MeshAngle = roundRad;
                }
            }

            return val;
        }


        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BECheeseCurdsBundleMod beccb = api.World.BlockAccessor.GetBlockEntity(blockSel.Position) as BECheeseCurdsBundleMod;
            ItemSlot hotbarSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
            List<String> bundles = More_Cheese.More_CheeseModSystem.validBundleInputs;

            if (beccb == null) return false;

            if (beccb.State == EnumCurdsBundleModState.Unbundled)
            {
                if (bundles.Contains(hotbarSlot.Itemstack?.Collectible.Code) && hotbarSlot.StackSize >= 25) {
                    Dictionary<String, String> quickComp = More_Cheese.More_CheeseModSystem.BundleRecipes;
                    api.Logger.Debug($"Bundle recipe has been triggered with{quickComp[hotbarSlot.Itemstack?.Collectible.Code]}");
                    AssetLocation inputCh = new AssetLocation(hotbarSlot.Itemstack?.Collectible.Code);
                    ItemStack thing = new ItemStack(api.World.GetItem(inputCh));
                    thing.StackSize = 25;
                    beccb.Inventory[0].Itemstack = thing;
                    outputCh = new AssetLocation((quickComp[hotbarSlot.Itemstack?.Collectible.Code]));
                    beccb.State = EnumCurdsBundleModState.Bundled;
                    hotbarSlot.TakeOut(25);
                    hotbarSlot.MarkDirty();
                }
                return true;
            }

            if (beccb.State == EnumCurdsBundleModState.Bundled)
            {
                if (hotbarSlot.Itemstack?.Collectible.Code.Path == "stick")
                {
                    beccb.State = EnumCurdsBundleModState.BundledStick;
                    hotbarSlot.TakeOut(1);
                    hotbarSlot.MarkDirty();
                }
                return true;
            }

            if (beccb.State == EnumCurdsBundleModState.BundledStick && !beccb.Squuezed)
            {
                beccb.StartSqueeze(byPlayer);
                return true;
            }

            if (beccb.Rotten || beccb.Inventory.Empty)
            {
                beccb.Inventory.DropAll(beccb.Pos.ToVec3d().Add(0.5, 0.2, 0.5));
                api.World.BlockAccessor.SetBlock(api.World.GetBlock(new AssetLocation("linen-normal-down")).Id, blockSel.Position);
                return true;
            }

            if (beccb.State == EnumCurdsBundleModState.BundledStick && beccb.Squuezed)
            {
                beccb.State = EnumCurdsBundleModState.Opened;
                api.World.PlaySoundAt(Sounds.Place, blockSel.Position, -0.5, byPlayer);

                ItemStack stick = new ItemStack(api.World.GetItem(new AssetLocation("stick")));
                if (!byPlayer.InventoryManager.TryGiveItemstack(stick, true))
                {
                    api.World.SpawnItemEntity(stick, byPlayer.Entity.Pos.XYZ.Add(0, 0.5, 0));
                }
                api.World.Logger.Audit("{0} Took 1x{1} at {2}.",
                    byPlayer.PlayerName,
                    stick.Collectible.Code,
                    blockSel.Position
                );
                return true;
            }

            if (beccb.State == EnumCurdsBundleModState.Opened)
            {
                if (hotbarSlot.Itemstack?.Collectible.Code.Path == "salt" && hotbarSlot.StackSize >= 5)
                {
                    beccb.State = EnumCurdsBundleModState.OpenedSalted;
                    hotbarSlot.TakeOut(5);
                    hotbarSlot.MarkDirty();
                }

                return true;
            }

            if (beccb.State == EnumCurdsBundleModState.OpenedSalted)
            {
                ItemStack cheeseRoll = new ItemStack(api.World.GetItem(outputCh));
                api.Logger.Debug($"Bundle recipe has registered output as with{cheeseRoll.Item.Code}");

                if (!byPlayer.InventoryManager.TryGiveItemstack(cheeseRoll, true))
                {
                    api.World.SpawnItemEntity(cheeseRoll, byPlayer.Entity.Pos.XYZ.Add(0, 0.5, 0));
                }
                api.World.Logger.Audit("{0} Took 1x{1} at {2}.",
                    byPlayer.PlayerName,
                    cheeseRoll.Collectible.Code,
                    blockSel.Position
                );

                api.World.BlockAccessor.SetBlock(api.World.GetBlock(new AssetLocation("linen-normal-down")).Id, blockSel.Position);
                return true;
            }



            return true;
        }



        public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            return base.OnBlockInteractStep(secondsUsed, world, byPlayer, blockSel);
        }

        public override void OnBlockInteractStop(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            base.OnBlockInteractStop(secondsUsed, world, byPlayer, blockSel);
        }

        public override bool OnBlockInteractCancel(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, EnumItemUseCancelReason cancelReason)
        {
            return base.OnBlockInteractCancel(secondsUsed, world, byPlayer, blockSel, cancelReason);
        }

        public void SetContents(ItemStack blockstack, ItemStack contents)
        {
            blockstack.Attributes.SetItemstack("contents", contents);
        }

        public ItemStack GetContents(ItemStack blockstack)
        {
            ItemStack stack = blockstack.Attributes.GetItemstack("contents");
            stack?.ResolveBlockOrItem(api.World);
            return stack;
        }


        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            return interactions;
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
        }
    }
}