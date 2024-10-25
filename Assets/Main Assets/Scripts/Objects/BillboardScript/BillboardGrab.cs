using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardGrab : MonoBehaviour
{
    public LayerMask targetLayer; // Il Layer del GameObject target

    void Update()
    {
        // Trova tutti i GameObject nel layer specificato
        GameObject[] targets = FindObjectsOfType<GameObject>();

        foreach (GameObject target in targets)
        {
            if (((1 << target.layer) & targetLayer) != 0) // Controlla se il GameObject è nel Layer specificato
            {
                // Calcola la direzione verso il target
                Vector3 direction = target.transform.position - transform.position;

                // Aggiorna la rotazione in modo che il GameObject punti verso il target
                transform.rotation = Quaternion.LookRotation(-direction);
                break; // Ruota verso il primo oggetto trovato nel Layer
            }
        }
    }
}
