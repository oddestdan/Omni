using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour {
    public Rigidbody myRigidbody;
    public float forceMin;
    public float forceMax;

    float lifetime = 4f;
    float fadetime = 2f;

    // Start is called before the first frame update
    void Start() {
        float force = Random.Range(forceMin, forceMax);
        myRigidbody.AddForce(transform.right * force);
        myRigidbody.AddTorque(Random.insideUnitSphere * force);

        StartCoroutine(Fade());
    }

    IEnumerator Fade() {
        yield return new WaitForSeconds(lifetime);

        float fadePercent = 0f;
        float fadeSpeed = 1 / fadetime;
        Material mat = GetComponent<Renderer>().material;
        Color initialColor = mat.color;

        while (fadePercent < 1) {
            fadePercent += Time.deltaTime * fadeSpeed;
            mat.color = Color.Lerp(initialColor, Color.clear, fadePercent);
            yield return null;
        }

        Destroy(gameObject);
    }
}
