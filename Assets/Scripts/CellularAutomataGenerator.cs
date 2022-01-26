using UnityEngine;
using UnityEngine.UI;

public class CellularAutomataGenerator : MonoBehaviour
{
    Texture2D _caTexture;
    int[,] _cellularAutomata;
    [SerializeField] Material _material;

    [SerializeField] int _width;
    [SerializeField] int _height;
    [SerializeField] string _seed;

    [SerializeField] Slider _fillSlider;
    [SerializeField] Text _fillSliderLabel;
    float _fillPercent = 0.5f;

    [SerializeField] Slider _liveNeighbourRequiredSlider;
    [SerializeField] Text _liveNeighbourRequiredLabel;
    int _liveNeighboursRequired = 4;

    void OnEnable()
    {
        if (!string.IsNullOrEmpty(_seed))
        {
            Random.InitState(_seed.GetHashCode());
        }

        _fillSlider.onValueChanged.AddListener(SetFillPercent);
        SetFillPercent(_fillSlider.value);

        _liveNeighbourRequiredSlider.onValueChanged.AddListener(SetRequiredNeighbourCount);
        SetRequiredNeighbourCount(_liveNeighbourRequiredSlider.value);

        ResetAutomata();
        _caTexture = new Texture2D(_width, _height);
        _caTexture.filterMode = FilterMode.Point;
        UpdateTexture();

        var displayQuad = new GameObject("CellularAutomata");
        var meshFilter = displayQuad.AddComponent<MeshFilter>();
        var meshRenderer = displayQuad.AddComponent<MeshRenderer>();

        var mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(-_width / 2f, _height / 2f, 0.0f),
            new Vector3(-_width / 2f, -_height / 2f, 0.0f),
            new Vector3(_width / 2f, _height / 2f, 0.0f),
            new Vector3(_width / 2f, -_height / 2f, 0.0f)
        };

        mesh.uv = new Vector2[]
        {
            new Vector2(0.0f, 0.0f),
            new Vector2(0.0f, 1.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f)
        };

        mesh.SetIndices(
            new int[] { 0, 2, 1, 1, 2, 3 },
            MeshTopology.Triangles,
            0);

        meshFilter.sharedMesh = mesh;
        meshRenderer.sharedMaterial = _material;
        _material.SetTexture("_MainTex", _caTexture);

        var gameCamera = FindObjectOfType<Camera>();
        displayQuad.transform.SetParent(gameCamera.transform);
        displayQuad.transform.localPosition = new Vector3(-15, 0, 90f);
    }

    public void OnStepButton()
    {
        Step();
        UpdateTexture();
    }

    public void OnResetButton()
    {
        ResetAutomata();
        UpdateTexture();
    }

    void SetFillPercent(float val)
    {
        _fillPercent = val;
        _fillSliderLabel.text = $"Fill Percent: {val}";
    }

    void SetRequiredNeighbourCount(float val)
    {
        _liveNeighboursRequired = Mathf.RoundToInt(val);
        _liveNeighbourRequiredLabel.text = $"Live Neighbour Req: {val}";
    }

    void ResetAutomata()
    {
        _cellularAutomata = new int[_width, _height];
        for (int x = 0; x < _width; ++x)
        {
            for (int y = 0; y < _height; ++y)
            {
                _cellularAutomata[x, y] = Random.value > _fillPercent ? 0 : 1;
            }
        }
    }

    void Step()
    {
        int[,] caBuffer = new int[_width, _height];

        for (int x = 0; x < _width; ++x)
        {
            for (int y = 0; y < _height; ++y)
            {
                int liveCellCount = _cellularAutomata[x, y] + GetNeighbourCellCount(x, y);
                caBuffer[x, y] = liveCellCount > _liveNeighboursRequired ? 1 : 0;
            }
        }

        for (int x = 0; x < _width; ++x)
        {
            for (int y = 0; y < _height; ++y)
            {
                _cellularAutomata[x, y] = caBuffer[x, y];
            }
        }
    }

    void UpdateTexture()
    {
        var pixels = _caTexture.GetPixels();
        for (int i = 0; i < pixels.Length; ++i)
        {
            var value = _cellularAutomata[i % _width, i / _height];
            pixels[i] = value * Color.white;
        }

        _caTexture.SetPixels(pixels);
        _caTexture.Apply();
    }

    int GetNeighbourCellCount(int x, int y)
    {
        int neighbourCellCount = 0;
        if (x > 0)
        {
            neighbourCellCount += _cellularAutomata[x - 1, y];
            if (y > 0)
            {
                neighbourCellCount += _cellularAutomata[x - 1, y - 1];
            }
        }

        if (y > 0)
        {
            neighbourCellCount += _cellularAutomata[x, y - 1];
            if (x < _width - 1)
            {
                neighbourCellCount += _cellularAutomata[x + 1, y - 1];
            }
        }

        if (x < _width - 1)
        {
            neighbourCellCount += _cellularAutomata[x + 1, y];
            if (y < _height - 1)
            {
                neighbourCellCount += _cellularAutomata[x + 1, y + 1];
            }
        }

        if (y < _height - 1)
        {
            neighbourCellCount += _cellularAutomata[x, y + 1];
            if (x > 0)
            {
                neighbourCellCount += _cellularAutomata[x - 1, y + 1];
            }
        }

        return neighbourCellCount;
    }
}