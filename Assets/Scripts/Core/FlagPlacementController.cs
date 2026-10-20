using UnityEngine;

public class FlagPlacementController : MonoBehaviour
{
    private enum PlacementState { Idle, Placing }

    [SerializeField] private GhostBase _ghostBasePrefab;
    [SerializeField] private BaseFlag _flagPrefab;

    private PlacementState _state = PlacementState.Idle;
    private GhostBase _activeGhost;
    private Base _currentBase;
    private Camera _camera;
    private GameObject _groundObject;
    private BoxCollider _groundCube;
    private RaycastHandler _raycastHandler;
    private InputControler _inputControler;

    private void Awake()
    {
        SceneContext context = FindFirstObjectByType<SceneContext>();

        if (context != null)
        {
            _camera = context.MainCamera;
            _groundObject = context.GroundObject;
            _groundCube = context.GroundCube;
            _raycastHandler = context.RaycastHandler;
            _inputControler = context.InputControler;
        }
    }

    private void OnEnable()
    {
        if (_raycastHandler != null)
        {
            _raycastHandler.BaseRightClicked += StartPlacing;
            _raycastHandler.GroundRightClicked += CancelPlacing;
        }

        if (_inputControler != null)
            _inputControler.MouseClicked += HandleMouseClick;
    }

    private void OnDisable()
    {
        if (_raycastHandler != null)
        {
            _raycastHandler.BaseRightClicked -= StartPlacing;
            _raycastHandler.GroundRightClicked -= CancelPlacing;
        }

        if (_inputControler != null)
            _inputControler.MouseClicked -= HandleMouseClick;
    }

    private void StartPlacing(Base baseRef)
    {
        if (_state != PlacementState.Idle)
            return;

        _state = PlacementState.Placing;
        _currentBase = baseRef;

        _activeGhost = Instantiate(_ghostBasePrefab);

        _activeGhost.Initialize(_groundCube.bounds);
    }

    private void CancelPlacing(Vector3 point)
    {
        if (_state != PlacementState.Placing)
            return;

        Cleanup();
    }

    private void HandleMouseClick(Vector2 screenPosition)
    {
        if (_state != PlacementState.Placing || _activeGhost == null || _currentBase == null)
            return;

        Vector3 flagPosition = _activeGhost.transform.position;

        if (_activeGhost.IsValid)
        {
            if (_currentBase.HasFlag)
            {
                BaseFlag existingFlag = _currentBase.GetFlag();
                existingFlag.PlaceAt(flagPosition);
            }
            else
            {
                BaseFlag newFlag = Instantiate(_flagPrefab);
                newFlag.PlaceAt(flagPosition);
                _currentBase.SetFlag(newFlag);
            }
        }

        Cleanup();
    }

    private void LateUpdate()
    {
        if (_state == PlacementState.Placing && _activeGhost != null)
        {
            if (GetGroundHitPoint(Input.mousePosition, out Vector3 hitPoint))
                _activeGhost.SetTargetPosition(hitPoint);
        }
    }

    private bool GetGroundHitPoint(Vector2 screenPos, out Vector3 hitPoint)
    {
        hitPoint = Vector3.zero;

        if (_camera == null)
            return false;

        Ray ray = _camera.ScreenPointToRay(screenPos);
        RaycastHit[] allHits = Physics.RaycastAll(ray);
        System.Array.Sort(allHits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in allHits)
        {
            if (hit.collider.GetComponentInParent<GhostBase>() != null)
                continue;

            if (_groundObject != null && hit.collider.gameObject == _groundObject)
            {
                hitPoint = hit.point;
                return true;
            }
        }

        return false;
    }

    private void Cleanup()
    {
        if (_activeGhost != null)
        {
            Destroy(_activeGhost.gameObject);
            _activeGhost = null;
        }

        _currentBase = null;
        _state = PlacementState.Idle;
    }
}