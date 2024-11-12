using System.Collections.Generic;

public class SectionActivationData
{
    // Dati sugli elementi attivati per tipo di sezione
    public Dictionary<string, List<string>> sectionTypeToActivatedElements = new Dictionary<string, List<string>>();

    // Registra gli ID degli elementi attivati per un tipo di sezione
    public void RecordActivation(string sectionType, List<string> activatedElementIDs)
    {
        if (sectionTypeToActivatedElements.ContainsKey(sectionType))
        {
            sectionTypeToActivatedElements[sectionType] = activatedElementIDs;
        }
        else
        {
            sectionTypeToActivatedElements.Add(sectionType, activatedElementIDs);
        }
    }

    // Ottiene gli elementi attivati per un tipo di sezione
    public List<string> GetActivatedElementsForSectionType(string sectionType)
    {
        if (sectionTypeToActivatedElements.ContainsKey(sectionType))
        {
            return sectionTypeToActivatedElements[sectionType];
        }
        else
        {
            return new List<string>();
        }
    }
}
