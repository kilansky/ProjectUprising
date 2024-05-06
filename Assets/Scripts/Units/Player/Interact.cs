using UnityEngine;

//[RequireComponent(typeof(AudioSource))]
public class Interact : MonoBehaviour
{
    //[SerializeField] private AudioClip click, pop;
    [SerializeField] private LayerMask interactMask;

    //Debug purposes only
    [SerializeField] private bool debug;
    private Path lastPath;

    private Camera mainCam;
    private Tile currentTile;
    private Unit selectedUnit;
    private Pathfinder pathfinder;
    private PathIllustrator illustrator;

    private void Start()
    {
        mainCam = Camera.main;
        pathfinder = Pathfinder.Instance;
        illustrator = pathfinder.Illustrator;
    }

    private void Update()
    {
        Clear();
        MouseUpdate();
    }

    private void MouseUpdate()
    {
        //Check if mouse hit object on an interactable layer (tiles, units, etc)
        if (Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, float.MaxValue, interactMask))
        {
            //If a tile was moused over, set as the current tile
            if (hit.transform.GetComponent<Tile>())
            {
                currentTile = hit.transform.GetComponent<Tile>();

            }
            //If a unit was moused over, set the current tile as its occupied tile
            else if (hit.transform.GetComponent<Unit>())
            {
                currentTile = hit.transform.GetComponent<Unit>().occupiedTile;
            }

            InspectTile();
        }
        else //The mouse is over a non-interactable object
        {
            //Check for left click input in open area to deselect units/paths
            if (Input.GetMouseButtonDown(0))
            {
                ClearAllSelections();
            }
        }

        //Check for right click input to deselect units/paths
        if (Input.GetMouseButtonDown(1))
        {
            ClearAllSelections();
        }
    }

    /// <summary>
    /// Deselect/Clear all units and paths unless a selected unit is moving
    /// </summary>
    private void ClearAllSelections()
    {
        if (selectedUnit && selectedUnit.Moving)
            return;

        ClearPath(lastPath);
        DeselectUnit();
    }

    private void InspectTile()
    {
        if (currentTile.Occupied)
            InspectUnit();
        else if (selectedUnit != null)
            NavigateToTile();

        //Alter tile type by left/right clicking on open tile while no unit is selected
        if (!currentTile.Occupied && selectedUnit == null)
        {
            if(Input.GetMouseButtonDown(0))
            {
                currentTile.ChangeTile(1);
            }
            else if(Input.GetMouseButtonDown(1))
            {
                currentTile.ChangeTile(-1);
            }
        }
    }

    private void InspectUnit()
    {
        //Exit if unit is moving
        if (currentTile.occupyingUnit.Moving)
            return;

        //Clear drawn paths and highlight this unit's tile
        ClearPath(lastPath);
        currentTile.Highlight();

        //Mouse clicked on unit
        if (Input.GetMouseButtonDown(0))
        {
            //No unit selected - select it
            if(selectedUnit == null)
            {
                SelectUnit();
            }
            //Unit is selected - deselect it
            else
            {
                DeselectUnit();
            }
        }
    }

    private void Clear()
    {
        if (currentTile == null  || currentTile.Occupied == false)
            return;

        //currentTile.ModifyCost(currentTile.terrainCost-1);//Reverses to previous cost and color after being highlighted
        currentTile.ClearHighlight();
        currentTile = null;
    }

    public void SelectUnit()
    {
        selectedUnit = currentTile.occupyingUnit;
        CameraController.Instance.followTarget = selectedUnit.transform;
        //GetComponent<AudioSource>().PlayOneShot(pop);
    }

    public void DeselectUnit()
    {
        selectedUnit = null;
        CameraController.Instance.followTarget = null;
        //GetComponent<AudioSource>().PlayOneShot(pop);
    }

    /// <summary>
    /// If a unit is selected and a tile hovered over - draw a path
    /// </summary>
    private void NavigateToTile()
    {
        if (selectedUnit == null || selectedUnit.Moving == true)
            return;

        if (RetrievePath(out Path newPath))
        {
            if (Input.GetMouseButtonDown(0))
            {
                //GetComponent<AudioSource>().PlayOneShot(click);
                selectedUnit.StartMove(newPath);
            }
        }
    }

    bool RetrievePath(out Path path)
    {
        path = pathfinder.FindPath(selectedUnit.occupiedTile, currentTile);
        
        if (path == null || path == lastPath)
            return false;

        ClearPath(lastPath);
        DrawPath(path);

        lastPath = path;
        return true;
    }

    private void DrawPath(Path path)
    {
        illustrator.HighlightPath(path);

        if (debug) //Debug only
            illustrator.DebugPathCosts(path);
        else
            illustrator.DisplayPathDistances(path);
    }

    private void ClearPath(Path path)
    {
        if (path != null)
        {
            illustrator.ClearPathHighlights(path);
        }
    }
}