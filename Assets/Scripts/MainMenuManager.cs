using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    private Image fade;
    
    public void StartGame()
    {
        fade.gameObject.SetActive(true);
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        yield return null;
        
        fade.color = new Color(0, 0, 0, 0);

        var elapsed = 0f;

        var duration = 0.5f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            
            fade.color = new Color(0, 0, 0, elapsed / duration);
            
            yield return null;
        }
        
        SceneManager.LoadScene("GameScene");
    }
}
