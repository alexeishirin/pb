using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PartyGenerator : MonoBehaviour
{

  public static PartyGenerator instance = null;

  public static string SKILLS_PERSIST_PATH = "skills/skills";
  public static string SKILLS_ARCHETYPES_PERSIST_PATH = "skills/archetypes";
  public static string ABILITIES_PERSIST_PATH = "skills/abilities";

  public int specifiedMapSeed = 0;
  public System.Random randomGenerator;

  public PartyGenerationSettings partyGenerationSettings;

  public List<Skill> skillArchetypes;
  public List<Skill> skills;
  public List<Ability> abilities;

  public List<Skill> passiveArchetypes;
  public List<Skill> activeArchetypes;

  public static PartyGenerator getInstance()
  {
    if(instance == null)
    {
      instance = new PartyGenerator();
    }

    return instance;
  }

  // Use this for initialization
  public Party generateParty(string applicationPersistentPath, PartyGenerationSettings partyGenerationSettings)
  {
    this.partyGenerationSettings = partyGenerationSettings;

    string skillArchetypesJson = DataLoader.loadFile(Application.persistentDataPath, SKILLS_ARCHETYPES_PERSIST_PATH);
    this.skillArchetypes = JsonHelper.FromJson<Skill>(skillArchetypesJson);

    string skillsJson = DataLoader.loadFile(Application.persistentDataPath, SKILLS_PERSIST_PATH);
    this.skills = JsonHelper.FromJson<Skill>(skillsJson);

    string abilitiesJson = DataLoader.loadFile(Application.persistentDataPath, ABILITIES_PERSIST_PATH);
    this.abilities = JsonHelper.FromJson<Ability>(abilitiesJson);

    Debug.Log(this.partyGenerationSettings);

    foreach(Skill skill in this.skillArchetypes)
    {
      Debug.Log(skill);
    }

    foreach (Skill skill in this.skills)
    {
      Debug.Log(skill);
    }

    foreach (Ability ability in this.abilities)
    {
      Debug.Log(ability);
    }

    this.prepareSkillLists();
    Party party = this.generateParty();
    foreach(Character character in party.characters)
    {
      Debug.Log(character);
    }

    return party;
  }

  public void setMapSeed(int newSeed)
  {
    this.specifiedMapSeed = newSeed;
  }


  public Party generateParty()
  {
    int mapSeed = this.specifiedMapSeed != 0 ? this.specifiedMapSeed : UnityEngine.Random.Range(0, 100);
    randomGenerator = new System.Random(mapSeed);

    Party party = new Party();
    int numberOfCharacters = randomGenerator.Next(this.partyGenerationSettings.maxPartyMembers / 2, this.partyGenerationSettings.maxPartyMembers + 1);
    Debug.Log(numberOfCharacters);
    for(int i = 1; i <= numberOfCharacters; i++)
    {
      Character character = generateCharacter();
      party.characters.Add(character);
    }

    return party;
  }

  public void prepareSkillLists()
  {
    this.passiveArchetypes = this.skillArchetypes.FindAll(skillArchetype => skillArchetype.passive && skillArchetype.used);
    this.activeArchetypes = this.skillArchetypes.FindAll(skillArchetype => !skillArchetype.passive && skillArchetype.used);
  }

  public Character generateCharacter()
  {
    Character character = new Character();
    int passiveReservePoints = randomGenerator.Next(this.partyGenerationSettings.minPassiveReserve, this.partyGenerationSettings.maxPassiveReserve + 1);
    foreach(Skill passiveArchetypeSkill in this.passiveArchetypes)
    {
      Skill characterSkill = passiveArchetypeSkill.clone();
      characterSkill.points = 1;
      character.skills.Add(characterSkill);
    }
    for(int i = 0; i < passiveReservePoints - this.passiveArchetypes.Count; i++)
    {
      character.skills[randomGenerator.Next(character.skills.Count)].points++;
    }

    int remainingPoints = this.partyGenerationSettings.startingArchetypePoints - passiveReservePoints;

    while(remainingPoints > 0)
    {
      Skill randomActiveSkill = this.activeArchetypes[randomGenerator.Next(this.activeArchetypes.Count)];
      bool alreadyHasSkill = false;
      foreach(Skill characterSkill in character.skills)
      {
        if(characterSkill.name == randomActiveSkill.name) {
          alreadyHasSkill = true;
          if(characterSkill.points < this.partyGenerationSettings.archetypeLevelCap)
          {
            characterSkill.points++;
            remainingPoints--;
          }
          break;
        }
      }

      if(!alreadyHasSkill)
      {
        Skill newSkill = randomActiveSkill.clone();
        newSkill.points = 1;
        character.skills.Add(newSkill);
        remainingPoints--;
      }
    }

    this.shuffleList(character.skills);
    int maxPromotedSkills = this.partyGenerationSettings.maxPromotedSkills;
    List<Skill> additionalSkills = new List<Skill>();
    foreach(Skill parentSkill in character.skills)
    {
      List<Skill> childSkills = this.skills.FindAll(skill => skill.used && skill.parent == parentSkill.name);
      foreach(Skill childSkill in childSkills)
      {
        int roll = randomGenerator.Next(100);
        if(roll < this.partyGenerationSettings.skillPromotionChance)
        {
          int points = randomGenerator.Next(1, this.partyGenerationSettings.maxPromotionBonus + 1);
          Skill newSkill = childSkill.clone();
          newSkill.points = points;
          additionalSkills.Add(newSkill);
          maxPromotedSkills--;

          if (this.partyGenerationSettings.archetypeDrop == 1)
          {
            parentSkill.points -= this.partyGenerationSettings.dropValue;
            if(parentSkill.points <= 0)
            {
              parentSkill.points = 1;
            }
          }
        }
        if (maxPromotedSkills == 0)
        {
          break;
        }
      }
      
      if(maxPromotedSkills == 0)
      {
        break;
      }
    }

    int maxAbilities = this.partyGenerationSettings.maxAbilities;
    foreach (Skill parentSkill in character.skills)
    {
      int roll = randomGenerator.Next(100);
      if(roll <= this.partyGenerationSettings.abilityChance)
      {
        List<Skill> childSkills = additionalSkills.FindAll(skill => skill.used && skill.parent == parentSkill.name);
        if(childSkills.Count > 0)
        {
          Skill randomSkill = childSkills[randomGenerator.Next(childSkills.Count)];
          List<Ability> skillAbilities = this.abilities.FindAll(ability => ability.parentSkill == randomSkill.name);
          if (skillAbilities.Count > 0)
          {
            Ability randomAbility = skillAbilities[randomGenerator.Next(skillAbilities.Count)];
            character.abilities.Add(randomAbility);
            maxAbilities--;
          }
        }
      }
      if (maxAbilities == 0)
      {
        break;
      }
    }

    character.skills.AddRange(additionalSkills);

    return character;
  }

  public void shuffleList<T>(List<T> hexList)
  {
    for (int i = hexList.Count; i > 1; i--)
    {
      int position = randomGenerator.Next(i);
      T temporary = hexList[i - 1];
      hexList[i - 1] = hexList[position];
      hexList[position] = temporary;
    }
  }
}
