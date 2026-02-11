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
        AddIngredientMapping(world, mappings, InputCh);
        AddIngredientMapping(world, mappings, OutputCh);

        return mappings;
    }

    private void AddIngredientMapping(IWorldAccessor world, Dictionary<string, string[]> mappings, CraftingRecipeIngredient ingredient)
    {
        if (ingredient?.Name == null || ingredient.Name.Length == 0) return;

        AssetLocation assetloc = ingredient.Code;
        int wildcardStartLen = assetloc.Path.IndexOf('*');
        if (wildcardStartLen == -1) return;

        int wildcardEndLen = assetloc.Path.Length - wildcardStartLen - 1;

        List<string> codes = new List<string>();

        if (ingredient.Type == EnumItemClass.Block)
        {
            foreach (var block in world.Blocks)
            {
                if (block.IsMissing) continue;

                if (ingredient.SkipVariants != null &&
                    WildcardUtil.MatchesVariants(assetloc, block.Code, ingredient.SkipVariants)) continue;

                if (WildcardUtil.Match(assetloc, block.Code, ingredient.AllowedVariants))
                {
                    string code = block.Code.Path.Substring(wildcardStartLen);
                    string codepart = code.Substring(0, code.Length - wildcardEndLen).DeDuplicate();
                    codes.Add(codepart);
                }
            }
        }
        else
        {
            foreach (var item in world.Items)
            {
                if (item?.Code == null || item.IsMissing) continue;

                if (ingredient.SkipVariants != null &&
                    WildcardUtil.MatchesVariants(assetloc, item.Code, ingredient.SkipVariants)) continue;

                if (WildcardUtil.Match(assetloc, item.Code, ingredient.AllowedVariants))
                {
                    string code = item.Code.Path.Substring(wildcardStartLen);
                    string codepart = code.Substring(0, code.Length - wildcardEndLen).DeDuplicate();
                    codes.Add(codepart);
                }
            }
        }

            mappings[ingredient.Name] = codes.ToArray();
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
