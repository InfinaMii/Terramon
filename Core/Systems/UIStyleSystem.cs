using Terramon.Content.Configs;
using Terramon.Content.GUI;

namespace Terramon.Core.Systems;

public class UIStyleSystem: ModSystem
{
    //idk how to check for resource pack reloads specifically so this works
    public override void OnLocalizationsLoaded()
    {
        var configValue = ModContent.GetInstance<ClientConfig>().ResourcePackCompatibility;
        bool useVanillaAssets = false;
        bool useVanillaBestiary = false;

        if (configValue == ResourceCompat.Auto)
        {
            var list = Main.AssetSourceController.ActiveResourcePackList.EnabledPacks.ToList();
            foreach (var value in list)
            {
                if (value.GetContentSource().HasAsset("Images/Inventory_Back"))
                    useVanillaAssets = true;
                if (value.GetContentSource().HasAsset("Images/UI/Bestiary/Button_Back"))
                    useVanillaBestiary = true;
            }
        }
        else if (configValue == ResourceCompat.Forced)
        {
            useVanillaAssets = true;
            useVanillaBestiary = true;
        }

        CustomPartyItemSlot.SetAssets(useVanillaAssets);
        CustomPCItemSlot.SetAssets(useVanillaAssets);
        
        PokedexEntryIcon.SetAssets(useVanillaBestiary);
        PCInterface.SetAssets(useVanillaBestiary);
        HubUI.SetAssets(useVanillaBestiary);
    }
}