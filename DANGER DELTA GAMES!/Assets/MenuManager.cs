using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public Animator startGameAnimation;
    public GameObject mainMenuFade;

    public GameObject[] Menus;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        StartCoroutine(StartTheGame());
    }

    IEnumerator StartTheGame()
    {
        mainMenuFade.SetActive(true);
        startGameAnimation.SetTrigger("StartGame");
        yield return new WaitForSeconds(5);
        SceneManager.LoadScene(1);

        yield return null;
    }

    public void OpenMenu(int MenuToOpen)
    {
        foreach(GameObject i in Menus)
        {
            if(i != Menus[MenuToOpen])
            {
                i.SetActive(false);
            }
            else
            {
                i.SetActive(true);
            }
        }
    }
}
