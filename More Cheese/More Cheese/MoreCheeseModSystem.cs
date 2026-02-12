using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vintagestory.GameContent;
using System.Reflection.Metadata.Ecma335;
using System;

namespace More_Cheese
{
    /* there is no cow's milk, so make adjustments to cow's milk cheese recipes by using 25-50% less rennet and adding calcium chloride
     * calcium chloride can be obtained via antarcitite found in halite
     * orange/yellow cheddar can be obtained by either dying white cheddar before curing/aging or by feeding cows a lot of carrots (and getting yellow/orange milk)
     * 
     * todo
     * add a cheese press/rock on cheese cloth system to replace/add an alternative to the cheesewrap
     * implement workability with a culinary artillery
     * mold cultivation process
     */
    public class More_CheeseModSystem : ModSystem
    {
        public List<BundleRecipe> Bundles = new List<BundleRecipe>();
        public static Dictionary<String, String> BundleRecipes = new();
        public static List<String> validBundleInputs = new List<String>();
        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            Mod.Logger.Notification("Hello from template mod: " + api.Side);
            api.RegisterBlockClass("BlockCheeseCurdsBundleMod", typeof(BlockCheeseCurdsBundleMod));
            api.RegisterBlockClass("BlockCheeseMod", typeof(BlockCheeseMod));
            api.RegisterBlockEntityClass("CheeseCurdsBundleMod", typeof(BECheeseCurdsBundleMod));
            api.RegisterBlockEntityClass("CheeseMod", typeof(BECheeseMod));
            api.RegisterItemClass("ItemModCheese", typeof(ItemModCheese));
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            Mod.Logger.Notification("Hello from template mod server side: " + Lang.Get("more cheese:hello"));
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            Mod.Logger.Notification("Hello from template mod client side: " + Lang.Get("more cheese:hello"));
        }
        public override void AssetsLoaded(ICoreAPI api) {
            LoadRecipes<BundleRecipe>(api, "bundle recipe", "recipes/bundles", (r) => RegisterBundleRecipe(r));
            getValidBundleInputs(Bundles);
            if (BundleRecipes.Count == 0)
            {
                getStringifiedBundleRecipies(Bundles);
            }
            //else
            //{
            //    BundleRecipes.Clear();
            //    getStringifiedBundleRecipies(Bundles);
            //}
        }
        public void LoadRecipes<TRecipe>(ICoreAPI api, string name, string path, System.Action<TRecipe> RegisterMethod) where TRecipe : BundleRecipe
        {
            Dictionary<AssetLocation, JToken> files = api.Assets.GetMany<JToken>(api.Logger, path);
            ///int recipeQuantity = 0;
            ///int recipesLoaded = 0;
            ///int failedResolveCount = 0;
            foreach ((AssetLocation location, JToken content) in files) {
                if (content is JObject recipeObject)
                {
                    TRecipe? parsedContent = recipeObject.ToObject<TRecipe>(location.Domain);
                    if (parsedContent == null)
                    {
                        api.Logger.Error($"Failed to parse {name} recipe: {location}");
                        continue;
                    }
                    LoadRecipe(api, location, parsedContent);
                    //parsedContent.
                }
            }

        }
        public void LoadRecipe(ICoreAPI api, AssetLocation loc, BundleRecipe recipe)
        {
            if (!recipe.Enabled) return;
            if (recipe.Name == null) recipe.Name = loc;

            Dictionary<string, string[]> nameToCodeMapping = recipe.GetNameToCodeMapping(api.World);

            if (nameToCodeMapping.Count > 0)
            {
                List<BundleRecipe> subRecipes = new List<BundleRecipe>();

                int qCombs = 0;
                bool first = true;
                foreach (var val2 in nameToCodeMapping)
                {
                    if (first) qCombs = val2.Value.Length;
                    else qCombs *= val2.Value.Length;
                    first = false;
                }
                first = true;
                foreach (var val in nameToCodeMapping)
                {
                    string variantCode = val.Key;
                    string[] variants = val.Value;
                    for (int i = 0; i < qCombs; i++)
                    {
                        BundleRecipe rec;

                        if (first) subRecipes.Add(rec = recipe.Clone());
                        else rec = subRecipes[i];
                        CraftingRecipeIngredient ingred = rec.Ingredient;
                        if (ingred.Name == variantCode)
                        {
                            ingred.Code.Path = ingred.Code.Path.Replace("*", variants[i % variants.Length]);
                        }
                        if (ingred.ReturnedStack?.Code != null)
                        {
                            ingred.ReturnedStack.Code.Path.Replace("{" + variantCode + "}", variants[i % variants.Length]);
                        }

                        rec.Output.FillPlaceHolder(variantCode, variants[i % variants.Length]);
                    }
                    first = false;
                }
                foreach (BundleRecipe subRecipe in subRecipes)
                {
                    if (!subRecipe.Resolve(api.World, "RecipeLoader")) continue;
                    RegisterBundleRecipe(subRecipe);
                }
            }
            else
            {
                if (!recipe.Resolve(api.World, "RecipeLoader")) return;
                RegisterBundleRecipe(recipe);
            }
        }
        public void RegisterBundleRecipe(BundleRecipe recipe)
        {
            recipe.RecipeId = Bundles.Count + 1;
            Bundles.Add(recipe);
            ///Bundles[1].OutputCh.Code.Path
        }
        public void getValidBundleInputs(List<BundleRecipe> recipes)
        {
            foreach (BundleRecipe recipe in recipes)
            {
                if (recipe.InputCh != null)
                {
                    validBundleInputs.Add(item: recipe.InputCh.Code);
                }
            }
        }
        public void getStringifiedBundleRecipies(List<BundleRecipe> recipes)
        {
            foreach (BundleRecipe recipe in recipes)
            {
                if (recipe.InputCh != null && recipe.OutputCh != null)
                {
                    BundleRecipes.Add(recipe.InputCh.Code, recipe.OutputCh.Code);
                }
            }
        }

    }
}
