using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestManager : MonoBehaviour
{
    public GameObject[] Colectables;
    private Transform cover;
    private GameObject interior;
    private bool alreadyPicked;
    private float targetAngle;
    private float targetBrightness;
    private float h, s, v;
    void Start()
    {
        cover = transform.GetChild(0).GetChild(0);
        interior = transform.GetChild(1).gameObject;
        alreadyPicked = false;
        targetAngle = 0f;
        targetBrightness = 0.9f;
    }

    // Update is called once per frame
    void Update()
    {
        if (alreadyPicked && cover.localEulerAngles.y > 110f)
        {
            return;
        }
        if (interior)
        {
            Color.RGBToHSV(interior.GetComponent<Renderer>().material.color, out h, out s, out v);
            if (Mathf.Abs(targetBrightness - v) <= 0.1f)
            {
                targetBrightness = (targetBrightness == 1f ? 0.5f : 1f);
            }

            float newBrightness = Mathf.Lerp(v, targetBrightness, Time.deltaTime * 3);
            interior.GetComponent<Renderer>().material.color = Color.HSVToRGB(h, s, newBrightness);
        }

        if (Mathf.Abs(targetAngle - cover.transform.localEulerAngles.y) <= 0.01f)
        {
            return;
        }

        Vector3 newHingeAngle = Vector3.Lerp(cover.localEulerAngles, new Vector3(cover.localEulerAngles.x, targetAngle, cover.localEulerAngles.z), Time.deltaTime * 3);
        cover.localEulerAngles = newHingeAngle;

    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (!alreadyPicked)
                FMODUnity.RuntimeManager.PlayOneShotAttached("event:/Chest/chest-opening", gameObject);

            StartCoroutine(setTargetAngle(120f, 0.7f));
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (!alreadyPicked)
                FMODUnity.RuntimeManager.PlayOneShotAttached("event:/Chest/chest-close", gameObject);

            targetAngle = 0f;
        }
    }

    private IEnumerator setTargetAngle(float targetAngle, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        this.targetAngle = targetAngle;
    }

    public void pickUpItems()
    {
        if (alreadyPicked)
        {
            return;
        }
        alreadyPicked = true;
        Destroy(interior);

        for (int i = 0; i < Colectables.Length; i++)
        {
            GameObject powerUp = Instantiate(Colectables[i], transform.position + transform.up, transform.rotation);
            Rigidbody rigid = powerUp.GetComponent<Rigidbody>();
            Vector3 direction = Random(rigid.transform.forward, -3f, 3f);
            rigid.AddForce(rigid.transform.up * 5 + direction, ForceMode.Impulse);
        }
    }
    public static Vector3 Random(Vector3 myVector, float min, float max)
    {
        Vector3 random = new Vector3(UnityEngine.Random.Range(min, max), 0, UnityEngine.Random.Range(min, max));
        return myVector + random;
    }
}
