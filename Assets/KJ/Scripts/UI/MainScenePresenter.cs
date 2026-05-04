using UnityEngine;

public class MainScenePresenter
{
    IMainSceneView view;

    public MainScenePresenter(IMainSceneView view)
    {
        this.view = view;
    }
}
