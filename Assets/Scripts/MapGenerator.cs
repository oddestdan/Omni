using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {

    public int mapIndex;
    public Map[] maps;
    public Vector2 maxMapSize;

    [Range(0, 1)]
    public float outlinePercent;
    public float tileSize;

    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform navmeshMaskPrefab;
    public Transform navmeshFloor;

    List<Coord> allTileCoords;
    Queue<Coord> shuffledTileCoords;
    Map currentMap;

    void Start() {
        GenerateMap();
    }

    public void GenerateMap() {
        currentMap = maps[mapIndex];
        System.Random prng = new System.Random(currentMap.seed);
        GetComponent<BoxCollider>().size = new Vector3(currentMap.mapSize.x * tileSize, .5f, currentMap.mapSize.y * tileSize);

        // Generate coords
        allTileCoords = new List<Coord>();
        for (int x = 0; x < currentMap.mapSize.x; x++) {
            for (int y = 0; y < currentMap.mapSize.y; y++) {
                allTileCoords.Add(new Coord(x, y));
            }
        }
        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), currentMap.seed));

        // Create map holder object
        string holderName = "Generated Map";
        if (transform.Find(holderName)) {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        // Spawn tiles
        for (int x = 0; x < currentMap.mapSize.x; x++) {
            for (int y = 0; y < currentMap.mapSize.y; y++) {
                Vector3 tilePosition = CoordToPosition(x, y);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;

                newTile.parent = mapHolder;
            }
        }

        // Spawn obstacles
        bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y];

        int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);
        int currentObstacleCount = 0;
        for (int i = 0; i < obstacleCount; i++) {
            Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;

            if (randomCoord != currentMap.mapCenter && MapIsFullyAccessible(obstacleMap, currentObstacleCount)) {
                // Set position and height to obstacles
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)prng.NextDouble());
                Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);

                Transform newObstacle = Instantiate(
                    obstaclePrefab,
                    obstaclePosition + Vector3.up * obstacleHeight / 2,
                    Quaternion.identity
                ) as Transform;
                newObstacle.parent = mapHolder;
                newObstacle.localScale = new Vector3((1 - outlinePercent) * tileSize, obstacleHeight, (1 - outlinePercent) * tileSize);

                // Set color to obstacles
                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                float colorPercent = randomCoord.y / (float)currentMap.mapSize.y;
                obstacleMaterial.color = Color.Lerp(currentMap.foregroundColor, currentMap.backgroundColor, colorPercent);
                obstacleRenderer.sharedMaterial = obstacleMaterial;
            } else {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
        }

        // Create navmesh mask
        MaskOutAllSides(mapHolder);

        navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;
    }

    void MaskOutAllSides(Transform map) {
        MaskOutSide(map, Vector3.left * (currentMap.mapSize.x + maxMapSize.x), (maxMapSize.x - currentMap.mapSize.x) / 2f, currentMap.mapSize.y);
        MaskOutSide(map, Vector3.right * (currentMap.mapSize.x + maxMapSize.x), (maxMapSize.x - currentMap.mapSize.x) / 2f, currentMap.mapSize.y);
        MaskOutSide(map, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y), maxMapSize.x, (maxMapSize.y - currentMap.mapSize.y) / 2f);
        MaskOutSide(map, Vector3.back * (currentMap.mapSize.y + maxMapSize.y), maxMapSize.x, (maxMapSize.y - currentMap.mapSize.y) / 2f);
    }

    void MaskOutSide(Transform map, Vector3 quantifiedDirection, float xScale, float yScale) {
        Transform mask = Instantiate(
            navmeshMaskPrefab,
            quantifiedDirection / 4f * tileSize,
            Quaternion.identity
        ) as Transform;
        mask.parent = map;
        mask.localScale = new Vector3(xScale, 1, yScale) * tileSize;
    }

    bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount) {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coord> queue = new Queue<Coord>();
        // Center
        queue.Enqueue(currentMap.mapCenter);
        mapFlags[currentMap.mapCenter.x, currentMap.mapCenter.y] = true;
        int accessibleTileCount = 1;

        while (queue.Count > 0) {
            Coord tile = queue.Dequeue();

            // Loop through 8 neighboring tiles
            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {
                    int neighborX = tile.x + x;
                    int neighborY = tile.y + y;
                    // Don't check diagonals
                    if (x == 0 || y == 0) {
                        // Neighbor is on the map
                        if (neighborX >= 0 && neighborX < obstacleMap.GetLength(0)
                            && neighborY >= 0 && neighborY < obstacleMap.GetLength(1)) {
                            // Tile hasn't been checked yet and isn't an obstacle
                            if (!mapFlags[neighborX, neighborY] && !obstacleMap[neighborX, neighborY]) {
                                mapFlags[neighborX, neighborY] = true;
                                queue.Enqueue(new Coord(neighborX, neighborY));
                                accessibleTileCount++;
                            }
                        }
                    }
                }
            }
        }

        int targetAccessibleTileCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount);
        return targetAccessibleTileCount == accessibleTileCount;
    }

    Vector3 CoordToPosition(int x, int y) {
        return new Vector3(-currentMap.mapSize.x / 2f + 0.5f + x, 0, -currentMap.mapSize.y / 2f + 0.5f + y) * tileSize;
    }

    public Coord GetRandomCoord() {
        Coord randomCoord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }

    [System.Serializable]
    public struct Coord {
        public int x;
        public int y;

        public Coord(int _x, int _y) {
            x = _x;
            y = _y;
        }

        public static bool operator ==(Coord c1, Coord c2) {
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator !=(Coord c1, Coord c2) {
            return !(c1 == c2);
        }
    }

    [System.Serializable]
    public class Map {
        public Coord mapSize;
        [Range(0, 1)]
        public float obstaclePercent;
        public float maxObstacleHeight;
        public float minObstacleHeight;
        public int seed;
        public Color foregroundColor;
        public Color backgroundColor;

        public Coord mapCenter {
            get {
                return new Coord(mapSize.x / 2, mapSize.y / 2);
            }
        }

    }

}
