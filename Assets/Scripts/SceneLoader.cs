using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadProspector()
    {
        SceneManager.LoadScene("__Prospector_Scene_0");
    }

    public void LoadPyramid()
    {
        SceneManager.LoadScene("Pyramid Solitaire");
    }

}
