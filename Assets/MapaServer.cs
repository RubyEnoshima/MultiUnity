using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapaServer : MonoBehaviour
{
    private ServerHandler sh = null;
    public Tilemap Objetos;
    public Tilemap Fondo;
    public Tile Caja;
    public Tile Obstaculo;
    public Tile Municion;
    public Tile Bandera;

    Vector3Int esquinaSupIzq = new Vector3Int(-7,6,0);
    Vector3Int esquinaInfDer = new Vector3Int(6,-7,0);

    public int maxCajas = 20;
    public int minCajas = 4;
    int nCajas = 0;

    bool BanderaSpawneada = false;
    bool BanderaAgarrada = false;
    Vector3 posBandera = new Vector3Int(-1000,-1000,-1000);

    int puntosAzul = 0;
    int puntosRojo = 0;


    /**                  FUNCIONES TILES                    **/
    // Calcula el vector resultante de poner pos en el SR de la esquina superior izquierda
    // Asume que la esquina sup izq es el primer tile muro de Muros
    private Vector3Int CalcPos(Vector3 pos){
        // Vector3Int esquina = new Vector3Int(Objetos.origin.x,Objetos.origin.y+Objetos.size.y-1,0);
        // return new Vector3Int(esquina.x+pos.x,esquina.y-pos.y,0);
        return Objetos.WorldToCell(pos);
    }

    public void PonerTile(Vector3 pos, Tile tile, Tilemap tilemap){
        tilemap.SetTile(CalcPos(pos),tile);
        sh.EnviarTile(pos,tile.name,tilemap.name);
    }

    public void EliminarTile(Vector3 pos, Tilemap tilemap){
        PonerTile(pos,null,tilemap);
    }

    public TileBase ObtTile(Vector3 pos,Tilemap tilemap){
        return tilemap.GetTile(CalcPos(pos));
    }

    /** --------------------------------------------------- **/

    public void SpawnearMunicion(Vector3 pos){
        PonerTile(pos,Municion,Objetos);
    }

    public void SpawnearBandera(Vector3 pos){
        PonerTile(pos,Bandera,Objetos);
        BanderaSpawneada = true;
        posBandera = pos;
    }

    public void DestruirCaja(Vector3 pos){
        if(ObtTile(pos,Objetos)==Caja){
            EliminarTile(pos,Objetos);

            // Decide si poner municion o una bandera y spawnea
            if(Random.Range(0f,1f)>0.65f){
                SpawnearMunicion(pos);
            }else if(!BanderaSpawneada && Random.Range(0f,1f)>0.2f){
                SpawnearBandera(pos);
            }

            nCajas--;
        }
    }

    // Spawnea n cajas en el mapa
    public void SpawnearCajas(int n){
        //Vector3Int esquina = new Vector3Int(-1,2,0);
        for(int i=0;i<n;i++){

            int x = Random.Range(esquinaSupIzq.x,esquinaInfDer.x+1);
            int y = Random.Range(esquinaInfDer.y,esquinaSupIzq.y+1);
            
            // No pongamos cajas donde ya hayan cosas
            // FALTA MIRAR QUE NO SE PONGAN ENCIMA DE UN PERSONAJE
            while(ObtTile(new Vector3Int(x,y,0),Objetos)!=null){
                x = Random.Range(esquinaSupIzq.x,esquinaInfDer.x+1);
                y = Random.Range(esquinaInfDer.y,esquinaSupIzq.y+1);
            }

            PonerTile(new Vector3Int(x,y,0),Caja,Objetos);
        }
        nCajas += n;
    }

    void ComprobarVictoria(){
        if(BanderaSpawneada && !BanderaAgarrada){
            TileBase t = ObtTile(posBandera,Fondo);
            if(t.name=="Azul"){
                puntosAzul++;

                EliminarTile(posBandera,Objetos);
                SpawnearCajas(maxCajas-nCajas);
            }else if(t.name=="Rojo"){
                puntosRojo++;

                EliminarTile(posBandera,Objetos);
                SpawnearCajas(maxCajas-nCajas);
            }  
        }
    }    

    // Start is called before the first frame update
    void Start()
    {
        var g = GameObject.FindWithTag("Handler");
        sh = g.GetComponent<ServerHandler>();
        Random.InitState(System.DateTime.Now.Millisecond);
        SpawnearCajas(maxCajas);
        // MANDAR UN OK A TODOS???
    }

    // Update is called once per frame
    void Update()
    {
        // Respawnear cajas
        if(nCajas==minCajas) SpawnearCajas(minCajas*2);

        ComprobarVictoria();

        // DEBUG
        if(Input.GetMouseButtonDown(0)){
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log(CalcPos(worldPoint));
            var tile = ObtTile(worldPoint,Objetos);

            if(tile)
            {
                DestruirCaja(worldPoint);
            }
        }
    }
}
