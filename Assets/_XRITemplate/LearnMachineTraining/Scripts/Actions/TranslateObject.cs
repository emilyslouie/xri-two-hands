using UnityEngine;

public class TranslateObject : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
            TranslateY(0.1f);

        if (Input.GetKeyDown(KeyCode.DownArrow))
            TranslateY(-0.1f);
    }

    public void TranslateX(float amount)
    {
        transform.position += transform.right * amount;
    }

    public void TranslateY(float amount)
    {
        transform.position += transform.up * amount;
    }

    public void TranslateZ(float amount)
    {
        transform.position += transform.forward * amount;
    }

    public void SetLocalXPosition(float newPosition)
    {
        transform.localPosition = new Vector3(newPosition, transform.localPosition.y, transform.localPosition.z);
    }

    public void SetLocalYPosition(float newPosition)
    {
        transform.localPosition = new Vector3(transform.localPosition.x, newPosition, transform.localPosition.z);
    }

    public void SetLocalZPosition(float newPosition)
    {
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, newPosition);
    }

}
