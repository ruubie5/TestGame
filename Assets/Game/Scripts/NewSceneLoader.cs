using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewSceneLoader : MonoBehaviour {

    public void loadNewScene(int scene)
    {
        SceneManager.LoadScene(scene);
    }
}
