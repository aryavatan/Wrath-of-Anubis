using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PerkManager : MonoBehaviour
{
    [Header("Perk Sprites")]
    public Sprite IncreasedHealthSprite;
    public Sprite IncreasedDamageSprite;
    public Sprite ThirdGunSprite;
    public Sprite SlightOfHandSprite;

    private void Awake()
    {
        RandomizePerks();
    }

    /// <summary>
    /// Adds the corresponding perk icon to the player's HUD
    /// </summary>
    /// <param name="perk">The perk type</param>
    /// <param name="ui">A reference to the game's GameUI object</param>
    public void AddPerk(PerkType perk, GameUI ui)
    {
        switch (perk)
        {
            case PerkType.INCREASED_HEALTH:
                ui.AddPerk(IncreasedHealthSprite);
                break;
            case PerkType.INCREASED_DAMAGE:
                ui.AddPerk(IncreasedDamageSprite);
                break;
            case PerkType.SLIGHT_OF_HAND:
                ui.AddPerk(SlightOfHandSprite);
                break;
            case PerkType.THIRD_GUN:
                ui.AddPerk(ThirdGunSprite);
                break;
        }
    }

    /// <summary>
    /// Randomizes the locations of every perk
    /// </summary>
    void RandomizePerks()
    {
        // List of all perk locations
        List<Perk> perkLocations = FindObjectsOfType<Perk>().ToList<Perk>();

        // List of possible perks
        List<PerkType> perksList = new List<PerkType> {
            PerkType.INCREASED_HEALTH, 
            PerkType.INCREASED_DAMAGE, 
            PerkType.SLIGHT_OF_HAND, 
            PerkType.THIRD_GUN 
        };

        for (int i = 0; i < 4; i++)
        {
            int locationIndex = Random.Range(0, perkLocations.Count - 1);
            int perkIndex = Random.Range(0, perksList.Count - 1);

            perkLocations[locationIndex].perk = perksList[perkIndex];

            perkLocations.RemoveAt(locationIndex);
            perksList.RemoveAt(perkIndex);
        }
    }
}
