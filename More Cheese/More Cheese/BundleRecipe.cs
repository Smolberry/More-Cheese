using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Vintagestory.API.Util;
using Vintagestory.API.Common;
using Vintagestory.API;
using System.Runtime.CompilerServices;
using System.Reflection.Metadata.Ecma335;

namespace More_Cheese;

public class BundleRecipe : RecipeBase<BundleRecipe>
{
    /// <summary>
    /// The output for the bundle recipe
    /// </summary>

    [DocumentAsJson] public CraftingRecipeIngredient OutputCh;
    /// <summary>
    /// The input for the bundle recipe
    /// </summary>

    [DocumentAsJson] public CraftingRecipeIngredient InputCh;

    public override BundleRecipe Clone()
    {
        BundleRecipe recipe = new();
        CloneTo(recipe);
        return recipe;
    }
    protected void CloneTo(object recipe)
    {
        if (recipe is not BundleRecipe bundleRecipe)
        {
            throw new ArgumentException("CloneTo should take object os same class or it's subclass");
        }
        bundleRecipe.Enabled = Enabled;
        bundleRecipe.Name = Name;
        bundleRecipe.OutputCh = OutputCh?.Clone();
        bundleRecipe.InputCh = InputCh?.Clone();
    }

    public override Dictionary<string, string[]> GetNameToCodeMapping(IWorldAccessor world)
    {
        Dictionary<string, string[]> mappings = new Dictionary<string, string[]>();
        List<string> codes = new List<string>();
        if ((OutputCh.Name == null || OutputCh.Name.Length == 0) || (InputCh.Name == null || InputCh.Name.Length == 0)) return;
        AssetLocation assetLoc1 = InputCh.Code;
        AssetLocation assetLoc2 = OutputCh.Code;
        if (InputCh.Type == EnumItemClass.Block)
        {
            foreach (var block in world.Blocks)
            {
                if (block.IsMissing) continue;    // BlockList already performs the null check for us, in its enumerator

                if (val.Value.SkipVariants != null && WildcardUtil.MatchesVariants(assetloc, block.Code, val.Value.SkipVariants)) continue;

                if (WildcardUtil.Match(assetloc, block.Code, val.Value.AllowedVariants))
                {
                    string code = block.Code.Path.Substring(wildcardStartLen);
                    string codepart = code.Substring(0, code.Length - wildcardEndLen).DeDuplicate();
                    codes.Add(codepart);
                }
            }
        }
    }

    public override bool Resolve(IWorldAccessor world, string sourceForErrorLogging)
    {
        if (OutputCh == null)
        {
            world.Logger.Error($"Bundle Recipe '{Name}' has no output specified");
            return false;
        }
        if (InputCh == null)
        {
            world.Logger.Error($"Bundle Recipe with input '{Name}' has no input specified");
            return false;
        }
        return true;
    }
}
