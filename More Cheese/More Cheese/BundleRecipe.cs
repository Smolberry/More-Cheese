using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Vintagestory.API.Util;
using Vintagestory.API.Common;
using Vintagestory.API;

namespace More_Cheese;

public class BundleRecipe : RecipeBase<BundleRecipe>
{
    /// <summary>
    /// The output for the bundle recipe
    /// </summary>
    [DocumentAsJson("Required", "Asset Path")]
    public AssetLocation? OutputCh { get; set; }
    /// <summary>
    /// The input for the bundle recipe
    /// </summary>
    [DocumentAsJson("Required", "Asset Path")]
    public AssetLocation? InputCh { get; set; }
}
