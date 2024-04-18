using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGen : MonoBehaviour
{
    [SerializeField] GameObject startingRoomPrefab;
    [SerializeField] List<GameObject> enemyRoomPrefabs;
    [SerializeField] GameObject fountainRoomPrefab;
    [SerializeField] GameObject upgradeRoomPrefab;
    [SerializeField] GameObject bossRoomPrefab;

    [SerializeField] GameObject verticalWallPrefab;
    [SerializeField] GameObject horizontalWallPrefab;
    [SerializeField] GameObject horizontalWallDoorPrefab;
    [SerializeField] GameObject verticalWallDoorPrefab;


    [SerializeField] GameObject cornerDownRightPrefab;
    [SerializeField] GameObject cornerDownLeftPrefab;
    [SerializeField] GameObject cornerDownLeftRightPrefab;
    [SerializeField] GameObject cornerUpRightPrefab;
    [SerializeField] GameObject cornerUpLeftPrefab;
    [SerializeField] GameObject cornerUpLeftRightPrefab;
    [SerializeField] GameObject cornerUpDownLeftPrefab;
    [SerializeField] GameObject cornerUpDownRightPrefab;
    [SerializeField] GameObject cornerUpDownLeftRightPrefab;

    [SerializeField] Transform gridTransform;

    public int numEnemyRooms;
    public List<Room> roomList;

    void Start(){
        roomList = GenerateRooms();
        GenerateWalls(roomList);
    }

    List<Room> GenerateRooms(){
        List<Room> rooms = new List<Room>();

        transform.position = Vector3.zero;
        string[] directions = {"Up", "Down", "Left", "Right"};
        Dictionary<string, Vector3> dirToWorld = new Dictionary<string, Vector3>{
            {"Up", new Vector3(0f, 9f, 0f)},
            {"Right", new Vector3(17f, 0f, 0f)},
            {"Down", new Vector3(0f, -9f, 0f)},
            {"Left", new Vector3(-17f, 0f, 0f)}
        };
        Dictionary<Vector3, string> worldToDir = new Dictionary<Vector3, string>{
            {new Vector3(0f, 9f, 0f), "Up"},
            {new Vector3(17f, 0f, 0f), "Right"},
            {new Vector3(0f, -9f, 0f), "Down"},
            {new Vector3(-17f, 0f, 0f), "Left"}
        };

        // Starting room
        rooms.Add(new Room{
            roomObject = startingRoomPrefab,
            worldSpacePosition = new Vector3(0f, 0f, 0f),
            roomTag = "Start"
        });

        // Generate rest of rooms
        Room currentRoom = rooms[0];
        while(rooms.Count < numEnemyRooms + 4){
            string dir = directions[Random.Range(0, 4)];
            Vector3 newPos = currentRoom.worldSpacePosition + dirToWorld[dir];
            if(rooms.Exists(x => x.worldSpacePosition == newPos)){
                currentRoom = rooms.Find(x => x.worldSpacePosition == newPos);
                continue;
            }
            rooms.Add(new Room{
                roomObject = enemyRoomPrefabs[Random.Range(0, enemyRoomPrefabs.Count)],
                worldSpacePosition = newPos,
                roomTag = "Enemy"
            });
        }

        // Check neighbors for each room
        foreach(Room room in rooms)
            foreach(string dir in directions)
                if(rooms.Exists(x => x.worldSpacePosition == room.worldSpacePosition + dirToWorld[dir]))
                    room.neighbors.Add(dir, rooms.Find(x => x.worldSpacePosition == room.worldSpacePosition + dirToWorld[dir]));

        // Generate Special rooms
        List<Room> validRooms = rooms.FindAll(x => x.roomTag != "Start");
        validRooms.Sort((x,y) => x.neighbors.Count.CompareTo(y.neighbors.Count));
        validRooms[0].roomTag = "Boss";
        validRooms[0].roomObject = bossRoomPrefab;
        validRooms[1].roomTag = "Upgrade";
        validRooms[1].roomObject = upgradeRoomPrefab;
        validRooms[2].roomTag = "Fountain";
        validRooms[2].roomObject = fountainRoomPrefab;

        // Instantiate rooms
        foreach(Room room in rooms){
            Instantiate(room.roomObject, room.worldSpacePosition, Quaternion.identity, gridTransform);
        }

        return rooms;
    }

    void GenerateWalls(List<Room> rooms){
        List<Vector3> horizontalWallPositions = new List<Vector3>();
        List<Vector3> verticalWallPositions = new List<Vector3>();
        foreach(Room room in rooms){
            Vector3[] vPos = {
                room.worldSpacePosition + new Vector3(9f, 0f, 0f),
                room.worldSpacePosition + new Vector3(-8f, 0f, 0f)
            };

            Vector3[] hPos = {
                room.worldSpacePosition + new Vector3(0f, 4f, 0f),
                room.worldSpacePosition + new Vector3(0f, -5f, 0f)
            };

            foreach(Vector3 pos in hPos)
                if(!horizontalWallPositions.Contains(pos)) horizontalWallPositions.Add(pos);
            foreach(Vector3 pos in vPos)
                if(!verticalWallPositions.Contains(pos)) verticalWallPositions.Add(pos);
        }

        foreach(Vector3 pos in horizontalWallPositions) Instantiate(
            horizontalWallPrefab,
            pos,
            Quaternion.identity,
            gridTransform
        );
        foreach(Vector3 pos in verticalWallPositions) Instantiate(
            verticalWallPrefab,
            pos,
            Quaternion.identity,
            gridTransform
        );

        GenerateCorners(horizontalWallPositions, verticalWallPositions);
    }

    void GenerateCorners(List<Vector3> horizontalWallPositions, List<Vector3> verticalWallPositions){
        List<Vector3> cornerPositions = new List<Vector3>();
        foreach(Vector3 pos in horizontalWallPositions){
            cornerPositions.Add(pos + new Vector3(9f, 0f, 0f));
            cornerPositions.Add(pos + new Vector3(-8f, 0f, 0f));
        }

        cornerPositions = cornerPositions.Distinct().ToList();

        foreach(Vector3 pos in cornerPositions){
            Debug.Log(pos.ToString());
            bool hasLeftWall = horizontalWallPositions.Contains(pos + new Vector3(8f, 0f, 0f));
            bool hasRightWall = horizontalWallPositions.Contains(pos + new Vector3(-9f, 0f, 0f));
            bool hasTopWall = verticalWallPositions.Contains(pos + new Vector3(0f, 5f, 0f));
            bool hasBottomWall = verticalWallPositions.Contains(pos + new Vector3(0f, -4f, 0f));
            GameObject cornerPrefab;
            if(hasLeftWall && hasRightWall && hasTopWall && hasBottomWall) cornerPrefab = cornerUpDownLeftRightPrefab;
            else if(hasLeftWall && hasRightWall && hasTopWall) cornerPrefab = cornerUpLeftRightPrefab;
            else if(hasLeftWall && hasRightWall && hasBottomWall) cornerPrefab = cornerDownLeftRightPrefab;
            else if(hasLeftWall && hasTopWall && hasBottomWall) cornerPrefab = cornerUpDownLeftPrefab;
            else if(hasRightWall && hasBottomWall && hasTopWall) cornerPrefab = cornerUpDownRightPrefab;
            else if(hasLeftWall && hasTopWall) cornerPrefab = cornerUpLeftPrefab;
            else if(hasLeftWall && hasBottomWall) cornerPrefab = cornerDownLeftPrefab;
            else if(hasRightWall && hasTopWall) cornerPrefab = cornerUpRightPrefab;
            else if(hasRightWall && hasBottomWall) cornerPrefab = cornerDownRightPrefab;
            else cornerPrefab = null;

            if(cornerPrefab != null) Instantiate(
                cornerPrefab,
                pos,
                Quaternion.identity,
                gridTransform
            );
        }
    }
}
