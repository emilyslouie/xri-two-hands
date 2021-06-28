using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cymbal : MonoBehaviour
{

    private AudioSource cymbalAudio;
    // Start is called before the first frame update
    void Start()
    {
      cymbalAudio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
      if (other.transform.GetComponent<Cymbal>() != null)
      {
        cymbalAudio.Play();
        //Debug.Log("cymbal hit");
      }
    }
}
