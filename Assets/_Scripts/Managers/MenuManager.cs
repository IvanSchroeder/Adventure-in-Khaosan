using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using UnityEngine.SceneManagement;



public class MenuManager : MonoBehaviour {
    public static MenuManager instance;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            ExitGame();
        }
        // else if (Input.GetKeyDown(KeyCode.R)) {
        //     RestartLevel();
        // }
        else if (Input.GetKeyDown(KeyCode.KeypadEnter)) {
            PauseEditor();
        }
    }

    public void LoadGame() {
        SceneManager.UnloadSceneAsync((int)SceneIndexes.TITLE_SCREEN);
        SceneManager.LoadSceneAsync((int)SceneIndexes.LEVEL, LoadSceneMode.Additive);
    }

    public void ExitGame() {
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void RestartLevel() {
        this.RestartScene();
    }

    public void PauseEditor() {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPaused = UnityEditor.EditorApplication.isPaused.Toggle();
        #endif
    }
}
