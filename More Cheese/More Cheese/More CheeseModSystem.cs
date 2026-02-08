using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace More_Cheese
{
    /* there is no cow's milk, so make adjustments to cow's milk cheese recipes by using 25-50% less rennet and adding calcium chloride
     * calcium chloride can be obtained via antarcitite found in halite
     * orange/yellow cheddar can be obtained by either dying white cheddar before curing/aging or by feeding cows a lot of carrots (and getting yellow/orange milk)
     */
    public class More_CheeseModSystem : ModSystem
    {

        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            Mod.Logger.Notification("Hello from template mod: " + api.Side);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            Mod.Logger.Notification("Hello from template mod server side: " + Lang.Get("more cheese:hello"));
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            Mod.Logger.Notification("Hello from template mod client side: " + Lang.Get("more cheese:hello"));
        }

    }
}
