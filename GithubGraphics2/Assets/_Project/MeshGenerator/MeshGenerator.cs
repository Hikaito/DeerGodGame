using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//source: https://www.youtube.com/watch?v=eJEpeUH1EMg
//source 2: https://www.youtube.com/watch?v=64NblGkAabk
//backface culling: the backside of a polygon isn't drawn
//unity determines the 'front' as the face of a polygon such that the vertices are given to unity in a clockwise direction
//further reading (perlin noise): https://medium.com/@yvanscher/playing-with-perlin-noise-generating-realistic-archipelagos-b59f004d8401#:~:text=Perlin%20noise%20combines%20multiple%20functions,3%20could%20be%20the%20rocks.

namespace DeerGod
{
    //requirements
    [RequireComponent(typeof(MeshFilter))]  //ensures there's always a mesh filter on the script object
    [RequireComponent(typeof(MeshCollider))]  //ensures there's always a mesh collider on the script object
    public class MeshGenerator : MonoBehaviour
    {

        //Biome and Flora----------------------------------------
        //NOTE: when adding biomes or flora, add to probability grids, object grids, and generation codes [and that other code that spawns flora]
        //enumerators for items
        enum biome {novo=0, empty=1, sparse=2, green=3, tall=4, pink=5, megatree = 6}   //biomes
        enum floraKey { novo=0, empty=1, bush=2, shortTreeGreen=3, tallTree=4, shortTreePink=5, megaTree=6, grass=7 } //implied floral enum.
        const int BIOMECOUNT = 7;           //number of biomes
        const int FLORACOUNT = 8;           //number of flora elements
        //game objects, assigned externally
        public GameObject tallTree;
        public GameObject shortTreeGreen;
        public GameObject shortTreePink;
        public GameObject megaTree;
        public GameObject bush;
        public GameObject grass;
        //probability math grids
        float[,] biomeStateTransition;      //holds the transition probabilities for biome generation
        float[,] biomeFloraTransition;      //holds the generation probabilities for a floral element given a biome
        GameObject[] floraSet;              //holds flora objects for procedural instantiation within code
        int[] biomeFloralPlacementCount;    //holds the max number of items per biome type to generate
        //runtime
        biome[] squareBiome;  //grid for biome generation, size is same as mesh
        //plant generation rules
        public int minFloraHeight = 55;

        //mesh generation----------------------------------------
        Mesh mesh;              //mesh object
        Vector3[] vertices;     //array for mesh vertices
        int[] triangles;

        //grid dimensions
        int xSize = 200;
        int zSize = 200;
        int squareSize = 10;

        //terrain generation rules----------------------------------------

        //random seeds
        public int floraSeed = 42;
        public float terrainSeed = 0;    //static because field initializer
        //0 is default, -50 has some nice hills

        //primary hill shape
        float perlin1XSeed = 0;   //0
        float perlin1ZSeed = 0;   //0
        float perlin1Resolution = .03f;
        float perlin1Amplitude = 100f;
        float halfAmp, halfAmpBeach;

        //subsea level hills
        float perlin2XSeed = 3500;  //3500
        float perlin2ZSeed = 3500;  //3500
        float perlin2Resolution = .7f;
        float perlin2Amplitude = 5f;
        float minimum2 = 40f;

        //minor hill 1
        float perlin3XSeed = 3000; //3000
        float perlin3ZSeed = 3000; //3000
        float perlin3Resolution = .1f;
        float perlin3Amplitude = 3f;

        //minor hill detail
        float perlin4XSeed = 1500;  //1500
        float perlin4ZSeed = 1500;  //1500
        float perlin4Resolution = .5f;
        float perlin4Amplitude = 15f;

        //clipping filter information-----------------
        float mesaBuffer = 20;  //size for mesa rim
        float mesaSlope = 1f;    //slope for mesa rim
        float eSlope = .2f;
        //set at Start [runtime]
        float radius;   //radius of 'all past here is zero'
        float xCenter;  
        float zCenter;

        // Start is called before the first frame update-----------------
        void Start()
        {
            //prepare runtime variables----------------------------------------
            //adjust terrain seeds
            perlin1XSeed += terrainSeed;
            perlin1ZSeed += terrainSeed;
            perlin2XSeed += terrainSeed;
            perlin2ZSeed += terrainSeed;
            perlin3XSeed += terrainSeed;
            perlin3ZSeed += terrainSeed;
            perlin4XSeed += terrainSeed;
            perlin4ZSeed += terrainSeed;

            //terrain generation value initializations
            radius = Mathf.Min(xSize / 2, zSize / 2);
            xCenter = xSize / 2;
            zCenter = zSize / 2;
            halfAmp = perlin1Amplitude / 2;
            halfAmpBeach = halfAmp + 10;

            //generation rules part 2 [arrays] ----------------------------------------
            //array for flora objects for procedural instantiation
            floraSet = new GameObject[FLORACOUNT]
            {
                null, //novo
                null,   //empty
                bush,
                shortTreeGreen,
                tallTree,
                shortTreePink,
                megaTree,
                grass
            };

            //biome mapping: BOMECOUNT x BIOMECOUNT array, representing [current biome, probability of indexing into biome x]
            //{novo=0, empty=1, sparse=2, green=3, tall=4, pink=5, megatree = 6}
            biomeStateTransition = new float[BIOMECOUNT, BIOMECOUNT] {
                {0,1,1,1,1,1,1}, //novo
                {0,30,10,1,1,1,1}, //empty
                {0,1,30,1,10,1,1}, //sparse
                {0,1,1,30,5,5,1}, //green
                {0,1,1,5,30,5,1}, //tall
                {0,1,1,5,5,20,1}, //pink
                {0,0,1,1,0,0,0}//megatree
                };

            //flora count per region: [current biome] = maximum tolerated items to generate
            //note: only 0-2 are valid numbers at present
            biomeFloralPlacementCount = new int[BIOMECOUNT] {
                0, //novo
                0, //empty
                2, //sparse
                2, //green
                2, //tall
                2, //pink
                1//megatree
            };

            //flora generation probability given a biome
            //flora mapping: BOMECOUNT x FLORACOUNT array, representing [current biome, probability of indexing into flora x]
            //{ novo=0, empty=1, bush=2, shortTreeGreen=3, tallTree=4, shortTreePink=5, megaTree=6, grass=7}
            biomeFloraTransition = new float[BIOMECOUNT, FLORACOUNT] {
                {1,0,0,0,0,0,0,0}, //novo
                {0,2,0,0,0,0,0,1}, //empty
                {0,3,1,.2f,0,0,0,0}, //sparse
                {0,4,0,1,0,0,0,1}, //green
                {0,4,0,0,1,0,0,0}, //tall
                {0,4,0,0,0,1,0,1}, //pink
                {0,0,0,0,0,0,1,0} //megatree
            };

            //initilaize random key sets from initial transition matricies [for actual probability selection math]
            //summation for biomeFloralPlacementCount
            for (int i = 0; i < BIOMECOUNT; i++)
            {
                float k = 0;
                for(int j = 0; j < BIOMECOUNT; j++)
                {
                    k += biomeStateTransition[i, j];
                    biomeStateTransition[i, j] = k;
                    //save 'stacked' probability for each outcome
                }

            }
            //summation for biomeFloraTransition
            //same as above but for flora generation array
            for (int i = 0; i < BIOMECOUNT; i++)
            {
                float k = 0;
                for (int j = 0; j < FLORACOUNT; j++)
                {
                    k += biomeFloraTransition[i, j];
                    biomeFloraTransition[i, j] = k;
                    //save 'stacked' probability for each outcome
                }

            }

            //perform generation activities----------------------------------------
            mesh = new Mesh();  //create mesh object
            GetComponent<MeshFilter>().mesh = mesh; //add mesh to mesh filter
            GetComponent<MeshCollider>().sharedMesh = mesh;

            CreateShapes(); //generate terrain
            UpdateMesh();   //update mesh object
            CreateFlora();  //generate biomes and populate with flora
        }


        //terrain generation [and triangles generation]======================================================================
        //a coroutine can be used to see this happen in realtime
        void CreateShapes()
        {
            //generate vertices
            vertices = new Vector3[(xSize + 1) * (zSize + 1)];

            //create grid of vertices
            //i is vertex number
            //for every vertex, create a vertex in space after calculating where it's y value belongs
            for (int i = 0, z = 0; z <= zSize; z++)
            {
                for (int x = 0; x <= xSize; x++)
                {
                    //generate terrain noise
                    float hill1 = Mathf.PerlinNoise((x + perlin1XSeed) * perlin1Resolution, (z + perlin1ZSeed) * perlin1Resolution) * perlin1Amplitude;
                    float subseaHill = minimum2 + Mathf.PerlinNoise((x + perlin2XSeed) * perlin2Resolution, (z + perlin2ZSeed) * perlin2Resolution) * perlin2Amplitude;
                    float hillDetail = Mathf.PerlinNoise((x + perlin3XSeed) * perlin3Resolution, (z + perlin3ZSeed) * perlin3Resolution) * perlin3Amplitude;
                    float minorHillDetail = Mathf.PerlinNoise((x + perlin4XSeed) * perlin4Resolution, (z + perlin4ZSeed) * perlin4Resolution) * perlin4Amplitude;

                    //generate y given raw terrain noise location
                    float y = Mathf.Max(hill1 * yFilterDome(x, z), subseaHill); //generate subsea hill
                    //change behavior when y is greater than the half amp level
                    if (y > halfAmp) {
                        float yAmp = Mathf.Max(0, y - halfAmpBeach) / perlin1Amplitude;
                        y = y + (yAmp * y * hillDetail) + (yAmp * minorHillDetail);
                    }

                    //generate vertex
                    vertices[i] = new Vector3(x * squareSize, y, z * squareSize);

                    //iterate vertex
                    i++;
                }
            }

            //generate triangles according to vertices
            int vertex = 0;     //iterates vertices
            int triangle = 0;   //iterates for each triangle pair
            triangles = new int[xSize * zSize * 6]; //generate triangle set
            for (int z = 0; z < zSize; z++){
                for (int x = 0; x < xSize; x++){
                    //generates triangles two at a time
                    //triangle 1
                    triangles[0 + triangle] = vertex + 0;
                    triangles[1 + triangle] = vertex + xSize + 1;
                    triangles[2 + triangle] = vertex + 1;
                    //triangle 2
                    triangles[3 + triangle] = vertex + 1;
                    triangles[4 + triangle] = vertex + xSize + 1;
                    triangles[5 + triangle] = vertex + xSize + 2;

                    vertex++;
                    triangle += 6;
                }
                vertex++;  //prevents wrapping between rows
            }
        }

        //flora generation======================================================================

        //generate flora mapping
        void CreateFlora()
        {
            //seed random number generator
            Random.InitState(floraSeed);

            //generate biomes 
            generateBiomes();

            //generate flora : triangles in the same square are 'twins' in terms of what biome they are assigned
            //determine what biome a triangle pair shares, how many items it can generate, and then generate those items with a height determined by placement within the triangles
            int vertex = 0;
            int triangle = 0;
            float rx, rz;
            Vector3 v1, v2, vn, randomPoint;
            biome thisBiome;

            Debug.Log("PEEK" + triangles[0] + " " + triangles[1] + " " + triangles[2] + " " + triangles[3] + " " + triangles[4] + " " + triangles[5]);
            //for each triangle pair, perform flora placement
            int maxCount = xSize * zSize;   //collects the number of biome squares
            for (int i = 0; i < maxCount; i++) {

                //select current biome based on triangle count
                thisBiome = squareBiome[i];

                //if no items are permitted for this biome, skip processing.
                if (biomeFloralPlacementCount[(int)thisBiome] == 0) {
                    continue;
                }

                //collect adequate vertices
                //line division: topLeft, bottomRight
                Vector3 bottomLeft = vertices[triangles[6 * i]];
                Vector3 topLeft = vertices[triangles[6 * i + 1]];
                Vector3 bottomRight = vertices[triangles[6 * i + 2]];
                Vector3 topRight = vertices[triangles[6 * i + 5]];

                //if two items are possible, roll a random factor to determine how many actually spawn
                if (biomeFloralPlacementCount[(int)thisBiome] == 2) {
                    int choice = Random.Range(0, 4);
                    
                    //if 1 in 4 chance, spawn only 1 item
                    if(choice == 2)
                    {
                        //point 1:========================================
                        //generate random point in square
                        rx = Random.Range(0, squareSize); //get random point along one length of triangle
                        rz = Random.Range(0, squareSize); //get other random point
                        randomPoint = new Vector3(bottomLeft.x + rx, 0, bottomLeft.z + rz);

                        //determine which triangle the point is in

                        //if point in wrong triangle, reverse triangle of point
                        //note: negative means wrong side for triangle 1 [?]
                        if (sideOfLine(topLeft, bottomRight, randomPoint))
                        {
                            //if the correct triangle is the close triangle, use the close triangle
                            //push to correct triangle
                            randomPoint.x = topRight.x - rx;
                            randomPoint.z = topRight.z - rz;
                        }

                        //generate line vectors from the corner of a triangle
                        v1 = new Vector3(topLeft.x - bottomLeft.x, topLeft.y - bottomLeft.y, topLeft.z - bottomLeft.z);
                        v2 = new Vector3(bottomRight.x - bottomLeft.x, bottomRight.y - bottomLeft.y, bottomRight.z - bottomLeft.z);


                        //generate cross product of vectors [plane normal]
                        vn = Vector3.Cross(v1, v2);

                        //solve for z
                        //a(x-x0) + b(y-y0) + c(z-z0) = 0, where V0 is a point in the plane and ABC is the normal vector
                        //vertices[vertex + 1] is present in both triangles, therefore present in both planes
                        //(-(a(x-x0) + b(y-y0)))/c + z0 = z
                        randomPoint.y = ((-(vn.x * (randomPoint.x - topLeft.x) + vn.z * (randomPoint.z - topLeft.z))) / vn.y) + topLeft.y;

                        //add plant for square
                        makeFlora(randomPoint, thisBiome);


                        //point 2:========================================
                        //generate random point in square
                        rx = Random.Range(0, squareSize); //get random point along one length of triangle
                        rz = Random.Range(0, squareSize); //get other random point
                        randomPoint = new Vector3(bottomLeft.x + rx, 0, bottomLeft.z + rz);

                        //determine which triangle the point is in
                        //if point in wrong triangle, reverse triangle of point
                        //note: positive means wrong side for triangle 2 [?]
                        if (!sideOfLine(topLeft, bottomRight, randomPoint))
                        {
                            //if the correct triangle is the close triangle, use the close triangle
                            //push to correct triangle
                            randomPoint.x = bottomLeft.x - rx;
                            randomPoint.z = bottomLeft.z - rz;
                        }

                        //generate vectors from the corner of a triangle
                        v1 = new Vector3(topRight.x - bottomRight.x, topRight.y - bottomRight.y, topRight.z - bottomRight.z);
                        v2 = new Vector3(topRight.x - topLeft.x, topRight.y - topLeft.y, topRight.z - topLeft.z);

                        //generate cross product of vectors [plane normal]
                        vn = Vector3.Cross(v1, v2);

                        //solve for z
                        //a(x-x0) + b(y-y0) + c(z-z0) = 0, where V0 is a point in the plane and ABC is the normal vector
                        //vertices[vertex + 1] is present in both triangles, therefore present in both planes
                        //(-(a(x-x0) + b(y-y0)))/c + z0 = z
                        randomPoint.y = ((-(vn.x * (randomPoint.x - topLeft.x) + vn.z * (randomPoint.z - topLeft.z))) / vn.y) + topLeft.y;

                        //add plant for square
                        makeFlora(randomPoint, thisBiome);

                        //continue
                        vertex++;
                        triangle += 2;
                        continue;
                    }
                }

                //implicit place only one object:------------------------
                //generate random point in square
                rx = Random.Range(0, squareSize); //get random point along one length of triangle
                rz = Random.Range(0, squareSize); //get other random point
                randomPoint = new Vector3(bottomLeft.x + rx, 0, bottomLeft.z + rz);

                //determine which triangle the point is in
                if (!sideOfLine(topLeft, bottomRight, randomPoint))
                {
                    //if the correct triangle is the close triangle, use the close triangle

                    //generate line vectors from the corner of a triangle
                    v1 = new Vector3(topLeft.x - bottomLeft.x, topLeft.y - bottomLeft.y, topLeft.z - bottomLeft.z);
                    v2 = new Vector3(bottomRight.x - bottomLeft.x, bottomRight.y - bottomLeft.y, bottomRight.z - bottomLeft.z);
                }
                else {
                    //otherwise use the far triangle

                    //generate vectors from the corner of a triangle
                    v1 = new Vector3(topRight.x - bottomRight.x, topRight.y - bottomRight.y, topRight.z - bottomRight.z);
                    v2 = new Vector3(topRight.x - topLeft.x, topRight.y - topLeft.y, topRight.z - topLeft.z);
                }

                //generate cross product of vectors [plane normal]
                vn = Vector3.Cross(v1, v2);

                //solve for z
                //a(x-x0) + b(y-y0) + c(z-z0) = 0, where V0 is a point in the plane and ABC is the normal vector
                //vertices[vertex + 1] is present in both triangles, therefore present in both planes
                //(-(a(x-x0) + b(y-y0)))/c + z0 = z
                randomPoint.y = ((-(vn.x * (randomPoint.x - topLeft.x) + vn.z * (randomPoint.z - topLeft.z))) / vn.y) + topLeft.y;

                //add plant for square
                makeFlora(randomPoint, thisBiome);
            }
        }

        //calculates which side of a line a point is on
        bool sideOfLine(Vector3 point1, Vector3 point2, Vector3 testPoint)
        {
            return (((point1.x - testPoint.x) * (point2.z - testPoint.z)) - ((point2.x - testPoint.x) * (point1.z - testPoint.z)) >= 0);
        }


        //generate flora
        void makeFlora(Vector3 position, biome parent)
        { 
            //if the elevation is too low, do not generate a plant
            //NOTE: remove this one line for no water clipping
            if (position.y < minFloraHeight) return;

            //generate object based on biome
            GameObject g = selectFlora(parent);

            //discard null
            if (g == null) return;

            //otherwise instantiate flora
            g = Instantiate(g, position, new Quaternion());

            //set random rotation
            g.transform.Rotate(Vector3.up * Random.Range(0f, 360f));
        }

        //function for selecting a flora choice from probability grid
        GameObject selectFlora(biome prior)
        {
            //generate random number that is in range somewhere
            float k = Random.Range(0, biomeFloraTransition[(int)prior, (FLORACOUNT - 1)]);

            //locate the probability that the generated number fits to
            int j = 0;
            for (j = 0; k > biomeFloraTransition[(int)prior, j]; j++) { }    //loop to appropriate position

            return floraSet[j];
        }

        //biome generation======================================================================
        //initialize biomes
        void generateBiomes()
        {
            //create grid for biomes
            squareBiome = new biome[xSize * zSize];

            //v2: start a random walk at every square
            for (int i = 0; i < xSize; i++)
            {
                for(int j = 0; j < zSize; j++)
                {
                    generateBiomeRandomWalk(i, j);
                }
            }
        }

        //generate biomes in the pattern of a random walk until all such accessible squares are exhausted
        void generateBiomeRandomWalk(int nextX, int nextY)
        {
            while (true) {

                //case 1: position not free, premature exit
                if (queryBiomePosition(nextX, nextY) != biome.novo) return;

                //collect possible directions
                int choiceCount = 0;                //number of possible directions
                int[] next = new int[4];            //array representing directions
                biome[] surround = new biome[4];    //array representing surroundings

                //initialize surroundings to novo
                for(int i = 0; i < surround.Length; i++)
                {
                    surround[i] = biome.novo;
                }

                //if direction is in range and novo, add to direction possibilities
                if (nextX + 1 < xSize) {
                    surround[0] = queryBiomePosition(nextX + 1, nextY);
                    if (surround[0] == biome.novo)
                    {
                        next[choiceCount] = 1;
                        choiceCount += 1;
                    }
                }
                if (nextX - 1 >= 0){
                    surround[1] = queryBiomePosition(nextX - 1, nextY);
                    if (surround[1] == biome.novo)
                    {
                        next[choiceCount] = 2;
                        choiceCount += 1;
                    }
                }

                if (nextY + 1 < zSize){
                    surround[2] = queryBiomePosition(nextX, nextY + 1);
                    if (surround[2] == biome.novo)
                    {
                        next[choiceCount] = 4;
                        choiceCount += 1;
                    }
                }
                if (nextY - 1 >= 0){
                    surround[3] = queryBiomePosition(nextX, nextY - 1);
                    if (surround[3] == biome.novo)
                    {
                        next[choiceCount] = 8;
                        choiceCount += 1;
                    }
                }

                //generate biome for this position
                setBiomePosition(nextX, nextY, getBiome(surround));

                //case 2: there are no directions to walk 
                if (choiceCount == 0) return;

                //case 3: there is only one direction to walk, so go that way
                //select a random direction from available directions
                int selectionNext = next[Random.Range(0, choiceCount)];

                //case 4: there is a selected direction from possible choices
                switch (selectionNext)
                {
                    case 1:
                        nextX = nextX + 1;
                        continue;
                    case 2:
                        nextX = nextX - 1;
                        continue;
                    case 4:
                        nextY = nextY + 1;
                        continue;
                    case 8:
                        nextY = nextY - 1;
                        continue;
                }

                return; //if something goes wrong, it will reach this statement.
            }
        }

        //checks the biome in a square
        biome queryBiomePosition(int x, int y)
        {
            return squareBiome[(y * xSize) + x];
        }

        //sets the biome in a square
        void setBiomePosition(int x, int y, biome b)
        {
            squareBiome[(y * xSize) + x] = b;
        }

        //randomly select an adjacent biome that is not an uninitialized biome
        biome getBiome(biome[] direction) {
            biome[] selection = new biome[4];
            int selectionCount = 0;

            //screen parent directions and randomly select one of their possibilities
            for(int i = 0; i < direction.Length; i++)
            {
                //if the biome is a biome that isn't novo, add its decision to the pool of possibilities
                if(direction[i] != biome.novo)
                {
                    selection[selectionCount] = readProbabilityBiome(direction[i]); 
                    selectionCount++;
                }
            }

            //if there are no directions, generate a novo response
            if(selectionCount == 0) return readProbabilityBiome(biome.novo);

            //return a random choice
            return selection[Random.Range(0, selectionCount)];
        }

        //select biome from biome probabilities
        biome readProbabilityBiome(biome last)
        {
            //check range of biome last
            if (((int)last < 0) || ((int)last > BIOMECOUNT - 1)){
                Debug.Log("WARNING: illegal biome delivered to 'readProbabilityBiome': " + last + "; set to 0.");
                last = biome.novo;
            }

            //generate a random number and find the appropriate biome to fit
            //generate random number that is in range somewhere
            float k = Random.Range(0, biomeStateTransition[(int)last, (BIOMECOUNT-1)]);

            int j;
            for (j = 0; k > biomeStateTransition[(int)last, j]; j++) { }    //loop to appropriate position

            return (biome)j;
        }

        //y generation edge filters======================================================================
        //sloped mesa y filter
        //sloped side mesa noise filter [0-1]
        float yFilterMesa(int x, int z)
        {
            //get real hypotenuse [distance to center]
            float hypoReal = Mathf.Sqrt(((xCenter - x) * (xCenter - x)) + ((zCenter - z) * (zCenter - z)));

            //check if inside mesa rim; return 0 if outside rim, 1 if inside mesa top
            //if (hypoReal >= radius) return 0; //enabling true cutoff makes the edge definition sharp
            if (hypoReal <= radius - mesaBuffer) return 1;

            //if inside mesa rim, calculate height: 1 / (distance from inner rim * slope)
            //minimum taken to prevent rounding error spikes
            return Mathf.Min(1 / ((hypoReal - (radius - mesaBuffer)) * mesaSlope), 1);
        }

        //Conical Mesa Y value filter
        //sloped side mesa noise filter [0-1] with conical sloping
        float yFilterConicalMesa(int x, int z) {
            //get real hypotenuse [distance to center]
            float hypoReal = Mathf.Sqrt(((xCenter - x) * (xCenter - x)) + ((zCenter - z) * (zCenter - z)));

            //check if inside mesa rim; return 0 if outside rim, 1 if inside mesa top
            if (hypoReal >= radius) return 0;
            if (hypoReal < radius - mesaBuffer) return 1;

            //return y cone filter of rim
            return Mathf.Min((radius - hypoReal) / mesaBuffer, 1);  //varies from 0 to 1 exclusively
        }

        //Conical Y value filter
        //conical noise filter [0-1]
        float yFilterCone(int x, int z)
        {                    
            //get real hypotenuse [distance to center]
            float hypoReal = Mathf.Sqrt(((xCenter - x) * (xCenter - x)) + ((zCenter - z) * (zCenter - z)));

            //get ratio of real hypotenuse to max distance hypotenuse
            return Mathf.Max(1 - (hypoReal / radius), 0);  //varies from 0 to 1 exclusively
        }

        //Dome Y value filter
        //dome noise filter [0-1], equation 1 - 1/e^x where x is distance from rim
        float yFilterDome(int x, int z)
        {
            //get real hypotenuse [distance to center]
            float hypoReal = Mathf.Sqrt(((xCenter - x) * (xCenter - x)) + ((zCenter - z) * (zCenter - z)));

            //get ratio of real hypotenuse to max distance hypotenuse, plus some math to make it grow exponentially
            return Mathf.Max(1 - (1 / Mathf.Exp((radius - hypoReal) * eSlope)), 0);  //varies from 0 to 1 exclusively
        }

        //mesh generation======================================================================
        void UpdateMesh()
        {
            mesh.Clear();   //clear mesh of previous data

            //set mesh values
            mesh.vertices = vertices;
            mesh.triangles = triangles;

            mesh.RecalculateNormals();  //fix lighting

            GetComponent<MeshFilter>().mesh = mesh; //add mesh to mesh filter
            GetComponent<MeshCollider>().sharedMesh = mesh;
        }

        //draw spheres on points [OBSELETE]
        private void OnDrawGizmos()
        {
            if (vertices == null) return;

            for (int i = 0; i < vertices.Length; i++)
            {
                Gizmos.DrawSphere(vertices[i], .1f);
            }
        }
    }
}
