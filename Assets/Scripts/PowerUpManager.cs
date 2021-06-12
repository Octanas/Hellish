using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    private float scaleMultiplier;
    private Vector3 scaleTarget;
    void Start()
    {
        scaleMultiplier = 10;
        scaleTarget = transform.localScale * scaleMultiplier;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0, 1, 0), Space.Self);

        if ((scaleTarget - transform.localScale).magnitude < 0.1) {
            return;
        }

        Vector3 newScale = Vector3.Lerp(transform.localScale, scaleTarget, Time.deltaTime * 2);

        transform.localScale = newScale;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Player")) {
            PlayerStats stats = collider.gameObject.GetComponent<PlayerStats>();
            if(gameObject.CompareTag("Heart")) {
                stats.UpgradeHealthBar();
            }
            else if (gameObject.CompareTag("Energy")) {
                stats.UpgradeManaBar();
            }
            else if (gameObject.CompareTag("FireBreath")) {
                stats.GainFireBreath();
            }
            else if (gameObject.CompareTag("FireWall")) {
                stats.GainFireWall();
            }
            else if (gameObject.CompareTag("Leap")) {
                stats.GainLeap();
            }
            Destroy(gameObject);
        }
    }
}
