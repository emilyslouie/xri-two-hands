using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateBallPit : MonoBehaviour
{
  public GameObject[] balls;
  public int number;
  public GameObject lid;

    float settleTime = 3.0f;

    // Start is called before the first frame update
    void Start()
    {
      foreach (GameObject ball in balls)
      {
        for (int i = 0; i < number; ++i)
        {
          Instantiate(ball, transform);
        }
      }
    }

    // Update is called once per frame
    void Update()
    {
        if (settleTime < 0.0f)
            return;

        settleTime -= Time.deltaTime;
        if (settleTime <= 0.0f)
        {
            if (lid != null)
            {
                lid.SetActive(false);
            }
        }
    }

    private void OnTriggerExit(Collider other)
      {
        //Debug.Log("hit: " + other.gameObject.name);
        if (other.gameObject.name.Contains("Ball"))
        {
          Destroy(other.gameObject);
        }
      }
}
