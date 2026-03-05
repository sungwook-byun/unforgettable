using UnityEngine;

public class MazeCell : MonoBehaviour
{
    [SerializeField] GameObject leftWall;
    [SerializeField] GameObject rightWall;
    [SerializeField] GameObject frontWall;
    [SerializeField] GameObject backWall;
    [SerializeField] GameObject unvisitedBlock;

    public bool IsVisited { get; private set; }
    public bool HasRightWall() => rightWall.activeSelf;

    public void Visit()
    {
        IsVisited = true;
        unvisitedBlock.SetActive(false);
    }

    public void ClearLeftWall()
    {
        leftWall.SetActive(false);
    }

    public void ClearRightWall()
    {
        rightWall.SetActive(false);
    }

    public void ClearFrontWall()
    {
        frontWall.SetActive(false);
    }

    public void ClearBackWall()
    {
        backWall.SetActive(false);
    }
}
