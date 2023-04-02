using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class path
{
    public List<int> pathIndexes = new List<int>();

    public void AddNewPathIndexes(int first, int last)
    {
        for(int i = first; i < last; i++)
        {
            pathIndexes.Add(i);
        }
    }
    public void DeleteThePath()
    {
        pathIndexes.Clear();
    }
    public int GetFirstOrLast(string state)
    {
        if(pathIndexes.Count > 1)
        {
            if (state == "first")
            {
                return pathIndexes[0];
            }
            else
            {
                return pathIndexes[pathIndexes.Count - 1];
            }
        }
        else
        {
            return pathIndexes[0];
        }
    }
    public int GetPathLength()
    {
        return pathIndexes.Count;
    }
}
public class NavPointHolder : MonoBehaviour
{
    public List<Transform> points = new List<Transform>();
    public List<path> pathsOnMap = new List<path>();
    public Transform GetPointByIndex(int index)
    {
        return points[index];
    }
    public Transform GetSpawnPoint()
    {
        return points[0];
    }
    public void AddNewPath(int first, int last)
    {
        path newPath = new path();
        newPath.AddNewPathIndexes(first, last);
        pathsOnMap.Add(newPath);
    }
    public path GetPathByIndex(int index = 0)
    {
        if(index < pathsOnMap.Count)
        {
            return pathsOnMap[index];
        }
        else
        {
            return null;
        }
    }
    public int GetPointListLength()
    {
        return points.Count;
    }
}
