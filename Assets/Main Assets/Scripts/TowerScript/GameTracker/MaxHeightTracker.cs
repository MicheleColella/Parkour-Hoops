using UnityEngine;
using TMPro;

public class MaxHeightTracker : MonoBehaviour
{
    public Transform player;              // Riferimento al Transform del player
    public TextMeshProUGUI heightText;    // Riferimento al componente TMPUGUI per visualizzare l'altezza
    public TextMeshProUGUI heightTextGui;    // Riferimento al componente TMPUGUI per visualizzare l'altezza

    [ReadOnly]
    public int maxHeight = 0;            // Altezza massima raggiunta (intero senza virgola)
    private bool hasTouchedTrigger = false; // Flag per controllare se il trigger è stato toccato

    void Update()
    {
        if (!hasTouchedTrigger)
        {
            // Aggiorna l'altezza massima se il player sale più in alto
            int currentHeight = Mathf.FloorToInt(player.position.y);
            if (currentHeight > maxHeight)
            {
                maxHeight = currentHeight;
                heightText.text = maxHeight.ToString() + " m";
                heightTextGui.text = maxHeight.ToString() + " m";
            }
        }
    }

    // Metodo da chiamare quando il player tocca il trigger
    public void StopTrackingHeight()
    {
        hasTouchedTrigger = true;
        heightText.text = maxHeight.ToString() + " m";
        heightTextGui.text = maxHeight.ToString() + " m";
    }
}
