using UnityEngine;
using TMPro;
using System.Collections;

public class FloatingMessageManager : MonoBehaviour
{
    public TextMeshPro messageText;
    public float displayDuration = 2.0f;

    private Coroutine currentMessage;

    void Start()
    {
        messageText.gameObject.SetActive(false);
    }

    public void ShowMessage(string text)
    {
        if (currentMessage != null)
            StopCoroutine(currentMessage);

        currentMessage = StartCoroutine(Display(text));
    }

    IEnumerator Display(string text)
    {
        messageText.text = text;
        messageText.gameObject.SetActive(true);

  
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + 180, 0);
        }

        yield return new WaitForSeconds(displayDuration);
        messageText.gameObject.SetActive(false);
    }
}
