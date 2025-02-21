using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
   private Tilemap m_Tilemap;

   public int Width;
   public int Height;
   public Tile[] GroundTiles;

   // Start is called before the first frame update
   void Start()
   {
       m_Tilemap = GetComponentInChildren<Tilemap>();

       for (int y = 0; y < Height; ++y)
       {
           for(int x = 0; x < Width; ++x)
           {
               int tileNumber = Random.Range(0, GroundTiles.Length);
               m_Tilemap.SetTile(new Vector3Int(x, y, 0), GroundTiles[tileNumber]);
           }
       }
   }

}