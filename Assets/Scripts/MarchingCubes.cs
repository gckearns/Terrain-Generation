
//________________________________________________

//-----------------------------------------------------------------------------
// Vertex structure

using System.Collections.Generic;
/**
* @file    MarchingCubes.h
* @author  Thomas Lewiner <thomas.lewiner@polytechnique.org>
* @author  Math Dept, PUC-Rio
* @version 0.2
* @date    12/08/2002
*
* @brief   MarchingCubes Algorithm
*//** \struct Vertex "MarchingCubes.h" MarchingCubes
* Position and normal of a vertex
* \brief vertex structure
* \param x X coordinate
* \param y Y coordinate
* \param z Z coordinate
* \param nx X component of the normal
* \param ny Y component of the normal
* \param nz Z component of the normal
*/
public struct Vertex
{
    public float x, y, z;  /**< Vertex coordinates */
    public float nx, ny, nz;  /**< Vertex normal */
}

//-----------------------------------------------------------------------------
// Triangle structure
/** \struct Triangle "MarchingCubes.h" MarchingCubes
 * Indices of the oriented triange vertices
 * \brief triangle structure
 * \param v1 First vertex index
 * \param v2 Second vertex index
 * \param v3 Third vertex index
 */
public struct Triangle
{
    public int v1, v2, v3;  /**< Triangle vertices */
}
//_____________________________________________________________________________



//_____________________________________________________________________________
/** Marching Cubes algorithm wrapper */
/** \class MarchingCubes
  * \brief Marching Cubes algorithm.
  */
public class MarchingCubes
//-----------------------------------------------------------------------------
{
    // Constructors
    /**
     * Main and default constructor
     * \brief constructor
     * \param size_x width  of the grid
     * \param size_y depth  of the grid
     * \param size_z height of the grid
     */
    public MarchingCubes(int size_x = -1, int size_y = -1, int size_z = -1)
    {
        _originalMC = false;
        _ext_data = false;
        _size_x = size_x;
        _size_y = size_y;
        _size_z = size_z;
        _data = null;
        _x_verts = null;
        _y_verts = null;
        _z_verts = null;
        _nverts = 0;
        _ntrigs = 0;
        _Nverts = 0;
        _Ntrigs = 0;
        _vertices = null;
        _triangles = null;
    }

    /** Destructor */
    ~MarchingCubes()
    {
        clean_all();
    }

    //-----------------------------------------------------------------------------
    // Accessors
    /** accesses the number of vertices of the generated mesh */
    public int nverts() { return _nverts; }
    /** accesses the number of triangles of the generated mesh */
    public int ntrigs() { return _ntrigs; }
    /** accesses a specific vertex of the generated mesh */
    public Vertex? vert(int i) { if (i < 0 || i >= _nverts) return null; return _vertices[i]; }
    /** accesses a specific triangle of the generated mesh */
    public Triangle? trig(int i) { if (i < 0 || i >= _ntrigs) return null; return _triangles[i]; }

    /** accesses the vertex buffer of the generated mesh */
    public Vertex[] vertices { get { return _vertices; } }
    /** accesses the triangle buffer of the generated mesh */
    public Triangle[] triangles { get { return _triangles; } }
    //public Triangle[] triangles { get { return _triangles.ToArray(); } }

    /**  accesses the width  of the grid */
    public int size_x() { return _size_x; }
    /**  accesses the depth  of the grid */
    public int size_y() { return _size_y; }
    /**  accesses the height of the grid */
    public int size_z() { return _size_z; }

    /**
     * changes the size of the grid
     * \param size_x width  of the grid
     * \param size_y depth  of the grid
     * \param size_z height of the grid
     */
    public void set_resolution(int size_x, int size_y, int size_z) { _size_x = size_x; _size_y = size_y; _size_z = size_z; }
    /**
     * selects wether the algorithm will use the enhanced topologically controlled lookup table or the original MarchingCubes
     * \param originalMC true for the original Marching Cubes
     */
    public void set_method(bool originalMC = false) { _originalMC = originalMC; }
    /**
     * selects to use data from another class
     * \param data is the pointer to the external data, allocated as a size_x*size_y*size_z vector running in x first
     */
    public void set_ext_data(float[] data)
    {
        if (!_ext_data)
            _data = null;
        _ext_data = data != null;
        if (_ext_data)
            _data = data;
    }
    /**
     * selects to allocate data
     */
    public void set_int_data() { _ext_data = false; _data = null; }

    // Data access
    /**
     * accesses a specific cube of the grid
     * \param i abscisse of the cube
     * \param j ordinate of the cube
     * \param k height of the cube
     */
    public float get_data(int i, int j, int k) { return _data[i + j * _size_x + k * _size_x * _size_y]; }
    /**
     * sets a specific cube of the grid
     * \param val new value for the cube
     * \param i abscisse of the cube
     * \param j ordinate of the cube
     * \param k height of the cube
     */
    public void set_data(float val, int i, int j, int k) { _data[i + j * _size_x + k * _size_x * _size_y] = val; }

    // Data initialization
    /** inits temporary structures (must set sizes before call) : the grid and the vertex index per cube */
    public void init_temps()
    {
        if (!_ext_data)
            _data = new float[_size_x * _size_y * _size_z];
        _x_verts = new int[_size_x * _size_y * _size_z];
        _y_verts = new int[_size_x * _size_y * _size_z];
        _z_verts = new int[_size_x * _size_y * _size_z];
    }

    /** inits all structures (must set sizes before call) : the temporary structures and the mesh buffers */
    public void init_all()
    {
        init_temps();

        _nverts = _ntrigs = 0;
        _Nverts = _Ntrigs = 65499;
        _vertices = new Vertex[_Nverts];
        _triangles = new Triangle[_Ntrigs];
        //_triangles = new List<Triangle>();
    }

    /** clears temporary structures : the grid and the main */
    public void clean_temps()
    {
        if (!_ext_data)
            _data = null;
        _x_verts = null;
        _y_verts = null;
        _z_verts = null;
    }

    /** clears all structures : the temporary structures and the mesh buffers */
    public void clean_all()
    {
        clean_temps();
        _vertices = null;
        _triangles = null;
        _nverts = _ntrigs = 0;
        _Nverts = _Ntrigs = 0;

        _size_x = _size_y = _size_z = -1;
    }


    //-----------------------------------------------------------------------------
    // Algorithm
    /**
     * Main algorithm : must be called after init_all
     * \param iso isovalue
     */
    public void run(float iso = (float)0.0)
    {
        // Uncomment the following 2 lines to get the calculation time
        //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        //stopwatch.Start();

        compute_intersection_points(iso);

        for (_k = 0; _k < _size_z - 1; _k++)
            for (_j = 0; _j < _size_y - 1; _j++)
                for (_i = 0; _i < _size_x - 1; _i++)
                {
                    _lut_entry = 0;
                    for (int p = 0; p < 8; ++p)
                    {
                        _cube[p] = get_data(_i + ((p ^ (p >> 1)) & 1),
                                _j + ((p >> 1) & 1), _k + ((p >> 2) & 1)) - iso;
                        if (System.Math.Abs(_cube[p]) < UnityEngine.Mathf.Epsilon)
                            _cube[p] = UnityEngine.Mathf.Epsilon;
                        if (_cube[p] > 0)
                            _lut_entry += (byte)(1 << p);
                    }
                    /*
                     if( ( _cube[0] = get_data( _i , _j , _k ) ) > 0 ) _lut_entry +=   1 ;
                     if( ( _cube[1] = get_data(_i+1, _j , _k ) ) > 0 ) _lut_entry +=   2 ;
                     if( ( _cube[2] = get_data(_i+1,_j+1, _k ) ) > 0 ) _lut_entry +=   4 ;
                     if( ( _cube[3] = get_data( _i ,_j+1, _k ) ) > 0 ) _lut_entry +=   8 ;
                     if( ( _cube[4] = get_data( _i , _j ,_k+1) ) > 0 ) _lut_entry +=  16 ;
                     if( ( _cube[5] = get_data(_i+1, _j ,_k+1) ) > 0 ) _lut_entry +=  32 ;
                     if( ( _cube[6] = get_data(_i+1,_j+1,_k+1) ) > 0 ) _lut_entry +=  64 ;
                     if( ( _cube[7] = get_data( _i ,_j+1,_k+1) ) > 0 ) _lut_entry += 128 ;
                     */
                    process_cube();
                }
        // Uncomment the following to get the calculation time
        //stopwatch.Stop();

        //UnityEngine.Debug.Log(string.Format("New Marching Cubes 33:ran in {0} secs.\n",
        //(double)(stopwatch.ElapsedMilliseconds / 1000f)));
    }

    /** tesselates one cube */
    protected void process_cube()
    {

        if (_originalMC)
        {
            byte nt = 0;
            while (LookupTable.casesClassic[_lut_entry][3 * nt] != -1)
                nt++;
            add_triangle(LookupTable.casesClassic[_lut_entry], nt);
            return;
        }

        int v12 = -1;
        _case = (byte)LookupTable.cases[_lut_entry][0];
        _config = (byte)LookupTable.cases[_lut_entry][1];
        _subconfig = 0;

        switch (_case)
        {

            case 0:
                break;

            case 1:
                add_triangle(LookupTable.tiling1[_config], 1);
                break;

            case 2:
                add_triangle(LookupTable.tiling2[_config], 2);
                break;

            case 3:
                if (test_face(LookupTable.test3[_config]))
                    add_triangle(LookupTable.tiling3_2[_config], 4); // 3.2
                else
                    add_triangle(LookupTable.tiling3_1[_config], 2); // 3.1
                break;

            case 4:
                if (modified_test_interior(LookupTable.test4[_config]))
                    add_triangle(LookupTable.tiling4_1[_config], 2); // 4.1.1
                else
                    add_triangle(LookupTable.tiling4_2[_config], 6); // 4.1.2
                break;

            case 5:
                add_triangle(LookupTable.tiling5[_config], 3);
                break;

            case 6:
                if (test_face(LookupTable.test6[_config][0]))
                    add_triangle(LookupTable.tiling6_2[_config], 5); // 6.2
                else
                {
                    if (modified_test_interior(LookupTable.test6[_config][1]))
                        add_triangle(LookupTable.tiling6_1_1[_config], 3); // 6.1.1
                    else
                    {
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling6_1_2[_config], 9, v12); // 6.1.2
                    }
                }
                break;

            case 7:
                if (test_face(LookupTable.test7[_config][0]))
                    _subconfig += 1;
                if (test_face(LookupTable.test7[_config][1]))
                    _subconfig += 2;
                if (test_face(LookupTable.test7[_config][2]))
                    _subconfig += 4;
                switch (_subconfig)
                {
                    case 0:
                        add_triangle(LookupTable.tiling7_1[_config], 3);
                        break;
                    case 1:
                        add_triangle(LookupTable.tiling7_2[_config][0], 5);
                        break;
                    case 2:
                        add_triangle(LookupTable.tiling7_2[_config][1], 5);
                        break;
                    case 3:
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling7_3[_config][0], 9, v12);
                        break;
                    case 4:
                        add_triangle(LookupTable.tiling7_2[_config][2], 5);
                        break;
                    case 5:
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling7_3[_config][1], 9, v12);
                        break;
                    case 6:
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling7_3[_config][2], 9, v12);
                        break;
                    case 7:
                        if (modified_test_interior(LookupTable.test7[_config][3]))
                            add_triangle(LookupTable.tiling7_4_1[_config], 5);
                        else
                            add_triangle(LookupTable.tiling7_4_2[_config], 9);
                        break;
                }
                break;

            case 8:
                add_triangle(LookupTable.tiling8[_config], 2);
                break;

            case 9:
                add_triangle(LookupTable.tiling9[_config], 4);
                break;

            case 10:
                if (test_face(LookupTable.test10[_config][0]))
                {
                    if (test_face(LookupTable.test10[_config][1]))
                    {
                        if (modified_test_interior((sbyte)-LookupTable.test10[_config][2]))
                            add_triangle(LookupTable.tiling10_1_1_[_config], 4); // 10.1.1
                        else
                            add_triangle(LookupTable.tiling10_1_2[5 - _config], 8); // 10.1.2

                    }
                    else
                    {
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling10_2[_config], 8, v12); // 10.2
                    }
                }
                else
                {
                    if (test_face(LookupTable.test10[_config][1]))
                    {
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling10_2_[_config], 8, v12); // 10.2
                    }
                    else
                    {
                        if (modified_test_interior(LookupTable.test10[_config][2]))
                            add_triangle(LookupTable.tiling10_1_1[_config], 4); // 10.1.1
                        else
                            add_triangle(LookupTable.tiling10_1_2[_config], 8); // 10.1.2
                    }
                }
                break;

            case 11:
                add_triangle(LookupTable.tiling11[_config], 4);
                break;

            case 12:
                if (test_face(LookupTable.test12[_config][0]))
                {
                    if (test_face(LookupTable.test12[_config][1]))
                    {
                        if (modified_test_interior((sbyte)-LookupTable.test12[_config][2]))
                            add_triangle(LookupTable.tiling12_1_1_[_config], 4); // 12.1.1
                        else
                            add_triangle(LookupTable.tiling12_1_2[23 - _config], 8); // 12.1.2
                    }
                    else
                    {
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling12_2[_config], 8, v12); // 12.2
                    }
                }
                else
                {
                    if (test_face(LookupTable.test12[_config][1]))
                    {
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling12_2_[_config], 8, v12); // 12.2
                    }
                    else
                    {
                        if (modified_test_interior(LookupTable.test12[_config][2]))
                            add_triangle(LookupTable.tiling12_1_1[_config], 4); // 12.1.1
                        else
                            add_triangle(LookupTable.tiling12_1_2[_config], 8); // 12.1.2
                    }
                }
                break;

            case 13:
                if (test_face(LookupTable.test13[_config][0]))
                    _subconfig += 1;
                if (test_face(LookupTable.test13[_config][1]))
                    _subconfig += 2;
                if (test_face(LookupTable.test13[_config][2]))
                    _subconfig += 4;
                if (test_face(LookupTable.test13[_config][3]))
                    _subconfig += 8;
                if (test_face(LookupTable.test13[_config][4]))
                    _subconfig += 16;
                if (test_face(LookupTable.test13[_config][5]))
                    _subconfig += 32;
                switch (LookupTable.subconfig13[_subconfig])
                {
                    case 0:/* 13.1 */
                        add_triangle(LookupTable.tiling13_1[_config], 4);
                        break;

                    case 1:/* 13.2 */
                        add_triangle(LookupTable.tiling13_2[_config][0], 6);
                        break;
                    case 2:/* 13.2 */
                        add_triangle(LookupTable.tiling13_2[_config][1], 6);
                        break;
                    case 3:/* 13.2 */
                        add_triangle(LookupTable.tiling13_2[_config][2], 6);
                        break;
                    case 4:/* 13.2 */
                        add_triangle(LookupTable.tiling13_2[_config][3], 6);
                        break;
                    case 5:/* 13.2 */
                        add_triangle(LookupTable.tiling13_2[_config][4], 6);
                        break;
                    case 6:/* 13.2 */
                        add_triangle(LookupTable.tiling13_2[_config][5], 6);
                        break;

                    case 7:/* 13.3 */
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling13_3[_config][0], 10, v12);
                        break;
                    case 8:/* 13.3 */
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling13_3[_config][1], 10, v12);
                        break;
                    case 9:/* 13.3 */
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling13_3[_config][2], 10, v12);
                        break;
                    case 10:/* 13.3 */
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling13_3[_config][3], 10, v12);
                        break;
                    case 11:/* 13.3 */
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling13_3[_config][4], 10, v12);
                        break;
                    case 12:/* 13.3 */
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling13_3[_config][5], 10, v12);
                        break;
                    case 13:/* 13.3 */
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling13_3[_config][6], 10, v12);
                        break;
                    case 14:/* 13.3 */
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling13_3[_config][7], 10, v12);
                        break;
                    case 15:/* 13.3 */
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling13_3[_config][8], 10, v12);
                        break;
                    case 16:/* 13.3 */
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling13_3[_config][9], 10, v12);
                        break;
                    case 17:/* 13.3 */
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling13_3[_config][10], 10, v12);
                        break;
                    case 18:/* 13.3 */
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling13_3[_config][11], 10, v12);
                        break;

                    case 19:/* 13.4 */
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling13_4[_config][0], 12, v12);
                        break;
                    case 20:/* 13.4 */
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling13_4[_config][1], 12, v12);
                        break;
                    case 21:/* 13.4 */
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling13_4[_config][2], 12, v12);
                        break;
                    case 22:/* 13.4 */
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling13_4[_config][3], 12, v12);
                        break;

                    case 23:/* 13.5 */
                        _subconfig = 0;
                        if (_config == 0)
                        {
                            if (interior_test_case13())
                                add_triangle(LookupTable.tiling13_5_1[0][0], 6);
                            else
                            {
                                if (tunnelOrientation == 1)
                                    add_triangle(LookupTable.tiling13_5_2[0][0], 10);
                                else
                                    add_triangle(LookupTable.tiling13_5_2[1][2], 10);
                            }
                        }
                        else
                        {
                            if (interior_test_case13())
                                add_triangle(LookupTable.tiling13_5_1[1][0], 6);
                            else
                            {
                                if (tunnelOrientation == 1)
                                    add_triangle(LookupTable.tiling13_5_2[1][0], 10);
                                else
                                    add_triangle(LookupTable.tiling13_5_2[0][2], 10);
                            }
                        }
                        break;

                    case 24:/* 13.5 */
                        _subconfig = 1;
                        if (_config == 0)
                        {
                            if (interior_test_case13())
                                add_triangle(LookupTable.tiling13_5_1[0][1], 6);
                            else
                            {
                                if (tunnelOrientation == 1)
                                    add_triangle(LookupTable.tiling13_5_2[0][1], 10);
                                else
                                    add_triangle(LookupTable.tiling13_5_2[1][0], 10);
                            }
                        }
                        else
                        {
                            if (interior_test_case13())
                                add_triangle(LookupTable.tiling13_5_1[1][1], 6);
                            else
                            {
                                if (tunnelOrientation == 1)
                                    add_triangle(LookupTable.tiling13_5_2[1][1], 10);
                                else
                                    add_triangle(LookupTable.tiling13_5_2[0][3], 10);
                            }
                        }

                        break;

                    case 25:/* 13.5 */
                        _subconfig = 2;
                        if (_config == 0)
                        {
                            if (interior_test_case13())
                                add_triangle(LookupTable.tiling13_5_1[0][2], 6);
                            else
                            {
                                if (tunnelOrientation == 1)
                                    add_triangle(LookupTable.tiling13_5_2[0][2], 10);
                                else
                                    add_triangle(LookupTable.tiling13_5_2[1][3], 10);
                            }
                        }
                        else
                        {
                            if (interior_test_case13())
                                add_triangle(LookupTable.tiling13_5_1[1][2], 6);
                            else
                            {
                                if (tunnelOrientation == 1)
                                    add_triangle(LookupTable.tiling13_5_2[1][2], 10);
                                else
                                    add_triangle(LookupTable.tiling13_5_2[0][0], 10);
                            }

                        }
                        break;

                    case 26: /* 13.5 */
                        _subconfig = 3;
                        if (_config == 0)
                        {
                            if (interior_test_case13())
                                add_triangle(LookupTable.tiling13_5_1[0][3], 6);
                            else
                            {
                                if (tunnelOrientation == 1)
                                    add_triangle(LookupTable.tiling13_5_2[0][3], 10);
                                else
                                    add_triangle(LookupTable.tiling13_5_2[1][1], 10);
                            }
                        }
                        else
                        {
                            if (interior_test_case13())
                                add_triangle(LookupTable.tiling13_5_1[1][3], 6);
                            else
                            {
                                if (tunnelOrientation == 1)
                                    add_triangle(LookupTable.tiling13_5_2[1][3], 10);
                                else
                                    add_triangle(LookupTable.tiling13_5_2[0][2], 10);
                            }
                        }
                        /* 13.4  common node is negative*/
                        // v12 = add_c_vertex() ;
                        // add_triangle( LookupTable.tiling13_4[_config][3], 12, v12 ) ;
                        break;

                    case 27:/* 13.3 */
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling13_3_[_config][0], 10, v12);
                        break;
                    case 28:/* 13.3 */
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling13_3_[_config][1], 10, v12);
                        break;
                    case 29:/* 13.3 */
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling13_3_[_config][2], 10, v12);
                        break;
                    case 30:/* 13.3 */
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling13_3_[_config][3], 10, v12);
                        break;
                    case 31:/* 13.3 */
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling13_3_[_config][4], 10, v12);
                        break;
                    case 32:/* 13.3 */
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling13_3_[_config][5], 10, v12);
                        break;
                    case 33:/* 13.3 */
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling13_3_[_config][6], 10, v12);
                        break;
                    case 34:/* 13.3 */
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling13_3_[_config][7], 10, v12);
                        break;
                    case 35:/* 13.3 */
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling13_3_[_config][8], 10, v12);
                        break;
                    case 36:/* 13.3 */
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling13_3_[_config][9], 10, v12);
                        break;
                    case 37:/* 13.3 */
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling13_3_[_config][10], 10, v12);
                        break;
                    case 38:/* 13.3 */
                        v12 = add_c_vertex();
                        add_triangle(LookupTable.tiling13_3_[_config][11], 10, v12);
                        break;

                    case 39:/* 13.2 */
                        add_triangle(LookupTable.tiling13_2_[_config][0], 6);
                        break;
                    case 40:/* 13.2 */
                        add_triangle(LookupTable.tiling13_2_[_config][1], 6);
                        break;
                    case 41:/* 13.2 */
                        add_triangle(LookupTable.tiling13_2_[_config][2], 6);
                        break;
                    case 42:/* 13.2 */
                        add_triangle(LookupTable.tiling13_2_[_config][3], 6);
                        break;
                    case 43:/* 13.2 */
                        add_triangle(LookupTable.tiling13_2_[_config][4], 6);
                        break;
                    case 44:/* 13.2 */
                        add_triangle(LookupTable.tiling13_2_[_config][5], 6);
                        break;

                    case 45:/* 13.1 */
                        add_triangle(LookupTable.tiling13_1_[_config], 4);
                        break;

                    default:
                        UnityEngine.Debug.Log("Marching Cubes: Impossible case 13?\n");
                        print_cube();
                        break;
                }
                break;

            case 14:
                add_triangle(LookupTable.tiling14[_config], 4);
                break;
        };

    }

    /** tests if the components of the tesselation of the cube should be connected by the interior of an ambiguous face */
    protected bool test_face(sbyte face)
    {
        float A, B, C, D;

        switch (face)
        {
            case -1:
            case 1:
                A = _cube[0];
                B = _cube[4];
                C = _cube[5];
                D = _cube[1];
                break;
            case -2:
            case 2:
                A = _cube[1];
                B = _cube[5];
                C = _cube[6];
                D = _cube[2];
                break;
            case -3:
            case 3:
                A = _cube[2];
                B = _cube[6];
                C = _cube[7];
                D = _cube[3];
                break;
            case -4:
            case 4:
                A = _cube[3];
                B = _cube[7];
                C = _cube[4];
                D = _cube[0];
                break;
            case -5:
            case 5:
                A = _cube[0];
                B = _cube[3];
                C = _cube[2];
                D = _cube[1];
                break;
            case -6:
            case 6:
                A = _cube[4];
                B = _cube[7];
                C = _cube[6];
                D = _cube[5];
                break;

            default:
                UnityEngine.Debug.Log(string.Format("Invalid face code {0}\n", face));
                print_cube();
                A = B = C = D = 0;
                break;
        };

        if (System.Math.Abs(A * C - B * D) < UnityEngine.Mathf.Epsilon)
            return face >= 0;

        return face * A * (A * C - B * D) >= 0; // face and A invert signs
    }

    /** tests if the components of the tesselation of the cube should be connected through the interior of the cube */
    //protected bool test_interior(sbyte s) { }

    /** tests if the components of the tesselation of the cube should be connected through the interior of the cube */
    protected bool modified_test_interior(sbyte s)
    {

        int edge = -1;
        int amb_face;

        int inter_amb = 0;

        switch (_case)
        {
            case 4:

                amb_face = 1;
                edge = interior_ambiguity(amb_face, s);
                inter_amb += interior_ambiguity_verification(edge);

                amb_face = 2;
                edge = interior_ambiguity(amb_face, s);
                inter_amb += interior_ambiguity_verification(edge);

                amb_face = 5;
                edge = interior_ambiguity(amb_face, s);
                inter_amb += interior_ambiguity_verification(edge);

                if (inter_amb == 0)
                    return false;
                else
                    return true;
            //break;

            case 6:

                amb_face = System.Math.Abs(LookupTable.test6[_config][0]);

                edge = interior_ambiguity(amb_face, s);
                inter_amb = interior_ambiguity_verification(edge);

                if (inter_amb == 0)
                    return false;
                else
                    return true;

            //break;

            case 7:
                s = (sbyte)(s * -1);

                amb_face = 1;
                edge = interior_ambiguity(amb_face, s);
                inter_amb += interior_ambiguity_verification(edge);

                amb_face = 2;
                edge = interior_ambiguity(amb_face, s);
                inter_amb += interior_ambiguity_verification(edge);

                amb_face = 5;
                edge = interior_ambiguity(amb_face, s);
                inter_amb += interior_ambiguity_verification(edge);

                if (inter_amb == 0)
                    return false;
                else
                    return true;
            //break;

            case 10:

                amb_face = System.Math.Abs(LookupTable.test10[_config][0]);

                edge = interior_ambiguity(amb_face, s);
                inter_amb = interior_ambiguity_verification(edge);

                if (inter_amb == 0)
                    return false;
                else
                    return true;
            //break;

            case 12:
                amb_face = System.Math.Abs(LookupTable.test12[_config][0]);
                edge = interior_ambiguity(amb_face, s);
                inter_amb += interior_ambiguity_verification(edge);


                amb_face = System.Math.Abs(LookupTable.test12[_config][1]);
                edge = interior_ambiguity(amb_face, s);
                inter_amb += interior_ambiguity_verification(edge);

                if (inter_amb == 0)
                    return false;
                else
                    return true;
            //break;

            default:
                return false;
        }

    }
    // new/corrected--------------------------------------------
    protected int interior_ambiguity(int amb_face, int s)
    {
        int edge = -1;

        switch (amb_face)
        {
            case 1:
            case 3:
                if (((_cube[1] * s) > 0) && ((_cube[7] * s) > 0))
                    edge = 4;
                if (((_cube[0] * s) > 0) && ((_cube[6] * s) > 0))
                    edge = 5;
                if (((_cube[3] * s) > 0) && ((_cube[5] * s) > 0))
                    edge = 6;
                if (((_cube[2] * s) > 0) && ((_cube[4] * s) > 0))
                    edge = 7;

                break;

            case 2:
            case 4:
                if (((_cube[1] * s) > 0) && ((_cube[7] * s) > 0))
                    edge = 0;
                if (((_cube[2] * s) > 0) && ((_cube[4] * s) > 0))
                    edge = 1;
                if (((_cube[3] * s) > 0) && ((_cube[5] * s) > 0))
                    edge = 2;
                if (((_cube[0] * s) > 0) && ((_cube[6] * s) > 0))
                    edge = 3;
                break;

            case 5:
            case 6:
            case 0:
                if (((_cube[0] * s) > 0) && ((_cube[6] * s) > 0))
                    edge = 8;
                if (((_cube[1] * s) > 0) && ((_cube[7] * s) > 0))
                    edge = 9;
                if (((_cube[2] * s) > 0) && ((_cube[4] * s) > 0))
                    edge = 10;
                if (((_cube[3] * s) > 0) && ((_cube[5] * s) > 0))
                    edge = 11;

                break;
        }

        return edge;
    }

    //--------------------------------------------------------------------------------
    protected int interior_ambiguity_verification(int edge)
    {
        float t, At = 0, Bt = 0, Ct = 0, Dt = 0, a = 0, b = 0;
        float verify;

        switch (edge)
        {

            case 0:
                a = (_cube[0] - _cube[1]) * (_cube[7] - _cube[6])
                        - (_cube[4] - _cube[5]) * (_cube[3] - _cube[2]);
                b = _cube[6] * (_cube[0] - _cube[1]) + _cube[1] * (_cube[7] - _cube[6])
                        - _cube[2] * (_cube[4] - _cube[5])
                        - _cube[5] * (_cube[3] - _cube[2]);

                if (a > 0)
                    return 1;

                t = -b / (2 * a);
                if (t < 0 || t > 1)
                    return 1;

                At = _cube[1] + (_cube[0] - _cube[1]) * t;
                Bt = _cube[5] + (_cube[4] - _cube[5]) * t;
                Ct = _cube[6] + (_cube[7] - _cube[6]) * t;
                Dt = _cube[2] + (_cube[3] - _cube[2]) * t;

                verify = At * Ct - Bt * Dt;

                if (verify > 0)
                    return 0;
                if (verify < 0)
                    return 1;

                break;

            case 1:
                a = (_cube[3] - _cube[2]) * (_cube[4] - _cube[5])
                        - (_cube[0] - _cube[1]) * (_cube[7] - _cube[6]);
                b = _cube[5] * (_cube[3] - _cube[2]) + _cube[2] * (_cube[4] - _cube[5])
                        - _cube[6] * (_cube[0] - _cube[1])
                        - _cube[1] * (_cube[7] - _cube[6]);

                if (a > 0)
                    return 1;

                t = -b / (2 * a);
                if (t < 0 || t > 1)
                    return 1;

                At = _cube[2] + (_cube[3] - _cube[2]) * t;
                Bt = _cube[1] + (_cube[0] - _cube[1]) * t;
                Ct = _cube[5] + (_cube[4] - _cube[5]) * t;
                Dt = _cube[6] + (_cube[7] - _cube[6]) * t;

                verify = At * Ct - Bt * Dt;

                if (verify > 0)
                    return 0;
                if (verify < 0)
                    return 1;
                break;

            case 2:
                a = (_cube[2] - _cube[3]) * (_cube[5] - _cube[4])
                        - (_cube[6] - _cube[7]) * (_cube[1] - _cube[0]);
                b = _cube[4] * (_cube[2] - _cube[3]) + _cube[3] * (_cube[5] - _cube[4])
                        - _cube[0] * (_cube[6] - _cube[7])
                        - _cube[7] * (_cube[1] - _cube[0]);
                if (a > 0)
                    return 1;

                t = -b / (2 * a);
                if (t < 0 || t > 1)
                    return 1;

                At = _cube[3] + (_cube[2] - _cube[3]) * t;
                Bt = _cube[7] + (_cube[6] - _cube[7]) * t;
                Ct = _cube[4] + (_cube[5] - _cube[4]) * t;
                Dt = _cube[0] + (_cube[1] - _cube[0]) * t;

                verify = At * Ct - Bt * Dt;

                if (verify > 0)
                    return 0;
                if (verify < 0)
                    return 1;
                break;

            case 3:
                a = (_cube[1] - _cube[0]) * (_cube[6] - _cube[7])
                        - (_cube[2] - _cube[3]) * (_cube[5] - _cube[4]);
                b = _cube[7] * (_cube[1] - _cube[0]) + _cube[0] * (_cube[6] - _cube[7])
                        - _cube[4] * (_cube[2] - _cube[3])
                        - _cube[3] * (_cube[5] - _cube[4]);
                if (a > 0)
                    return 1;

                t = -b / (2 * a);
                if (t < 0 || t > 1)
                    return 1;

                At = _cube[0] + (_cube[1] - _cube[0]) * t;
                Bt = _cube[3] + (_cube[2] - _cube[3]) * t;
                Ct = _cube[7] + (_cube[6] - _cube[7]) * t;
                Dt = _cube[4] + (_cube[5] - _cube[4]) * t;

                verify = At * Ct - Bt * Dt;

                if (verify > 0)
                    return 0;
                if (verify < 0)
                    return 1;
                break;

            case 4:

                a = (_cube[2] - _cube[1]) * (_cube[7] - _cube[4])
                        - (_cube[3] - _cube[0]) * (_cube[6] - _cube[5]);
                b = _cube[4] * (_cube[2] - _cube[1]) + _cube[1] * (_cube[7] - _cube[4])
                        - _cube[5] * (_cube[3] - _cube[0])
                        - _cube[0] * (_cube[6] - _cube[5]);

                if (a > 0)
                    return 1;

                t = -b / (2 * a);
                if (t < 0 || t > 1)
                    return 1;

                At = _cube[1] + (_cube[2] - _cube[1]) * t;
                Bt = _cube[0] + (_cube[3] - _cube[0]) * t;
                Ct = _cube[4] + (_cube[7] - _cube[4]) * t;
                Dt = _cube[5] + (_cube[6] - _cube[5]) * t;

                verify = At * Ct - Bt * Dt;

                if (verify > 0)
                    return 0;
                if (verify < 0)
                    return 1;
                break;

            case 5:

                a = (_cube[3] - _cube[0]) * (_cube[6] - _cube[5])
                        - (_cube[2] - _cube[1]) * (_cube[7] - _cube[4]);
                b = _cube[5] * (_cube[3] - _cube[0]) + _cube[0] * (_cube[6] - _cube[5])
                        - _cube[4] * (_cube[2] - _cube[1])
                        - _cube[1] * (_cube[7] - _cube[4]);
                if (a > 0)
                    return 1;

                t = -b / (2 * a);
                if (t < 0 || t > 1)
                    return 1;

                At = _cube[0] + (_cube[3] - _cube[0]) * t;
                Bt = _cube[1] + (_cube[2] - _cube[1]) * t;
                Ct = _cube[5] + (_cube[6] - _cube[5]) * t;
                Dt = _cube[4] + (_cube[7] - _cube[4]) * t;

                verify = At * Ct - Bt * Dt;

                if (verify > 0)
                    return 0;
                if (verify < 0)
                    return 1;
                break;

            case 6:
                a = (_cube[0] - _cube[3]) * (_cube[5] - _cube[6])
                        - (_cube[4] - _cube[7]) * (_cube[1] - _cube[2]);
                b = _cube[6] * (_cube[0] - _cube[3]) + _cube[3] * (_cube[5] - _cube[6])
                        - _cube[2] * (_cube[4] - _cube[7])
                        - _cube[7] * (_cube[1] - _cube[2]);
                if (a > 0)
                    return 1;

                t = -b / (2 * a);
                if (t < 0 || t > 1)
                    return 1;

                At = _cube[3] + (_cube[0] - _cube[3]) * t;
                Bt = _cube[7] + (_cube[4] - _cube[7]) * t;
                Ct = _cube[6] + (_cube[5] - _cube[6]) * t;
                Dt = _cube[2] + (_cube[1] - _cube[2]) * t;

                verify = At * Ct - Bt * Dt;

                if (verify > 0)
                    return 0;
                if (verify < 0)
                    return 1;
                break;

            case 7:
                a = (_cube[1] - _cube[2]) * (_cube[4] - _cube[7])
                        - (_cube[0] - _cube[3]) * (_cube[5] - _cube[6]);
                b = _cube[7] * (_cube[1] - _cube[2]) + _cube[2] * (_cube[4] - _cube[7])
                        - _cube[6] * (_cube[0] - _cube[3])
                        - _cube[3] * (_cube[5] - _cube[6]);
                if (a > 0)
                    return 1;

                t = -b / (2 * a);
                if (t < 0 || t > 1)
                    return 1;

                At = _cube[2] + (_cube[1] - _cube[2]) * t;
                Bt = _cube[3] + (_cube[0] - _cube[3]) * t;
                Ct = _cube[7] + (_cube[4] - _cube[7]) * t;
                Dt = _cube[6] + (_cube[5] - _cube[6]) * t;

                verify = At * Ct - Bt * Dt;

                if (verify > 0)
                    return 0;
                if (verify < 0)
                    return 1;
                break;

            case 8:
                a = (_cube[4] - _cube[0]) * (_cube[6] - _cube[2])
                        - (_cube[7] - _cube[3]) * (_cube[5] - _cube[1]);
                b = _cube[2] * (_cube[4] - _cube[0]) + _cube[0] * (_cube[6] - _cube[2])
                        - _cube[1] * (_cube[7] - _cube[3])
                        - _cube[3] * (_cube[5] - _cube[1]);
                if (a > 0)
                    return 1;

                t = -b / (2 * a);
                if (t < 0 || t > 1)
                    return 1;

                At = _cube[0] + (_cube[4] - _cube[0]) * t;
                Bt = _cube[3] + (_cube[7] - _cube[3]) * t;
                Ct = _cube[2] + (_cube[6] - _cube[2]) * t;
                Dt = _cube[1] + (_cube[5] - _cube[1]) * t;

                verify = At * Ct - Bt * Dt;

                if (verify > 0)
                    return 0;
                if (verify < 0)
                    return 1;
                break;

            case 9:
                a = (_cube[5] - _cube[1]) * (_cube[7] - _cube[3])
                        - (_cube[4] - _cube[0]) * (_cube[6] - _cube[2]);
                b = _cube[3] * (_cube[5] - _cube[1]) + _cube[1] * (_cube[7] - _cube[3])
                        - _cube[2] * (_cube[4] - _cube[0])
                        - _cube[0] * (_cube[6] - _cube[2]);
                if (a > 0)
                    return 1;

                t = -b / (2 * a);
                if (t < 0 || t > 1)
                    return 1;

                At = _cube[1] + (_cube[5] - _cube[1]) * t;
                Bt = _cube[0] + (_cube[4] - _cube[0]) * t;
                Ct = _cube[3] + (_cube[7] - _cube[3]) * t;
                Dt = _cube[2] + (_cube[6] - _cube[2]) * t;

                verify = At * Ct - Bt * Dt;

                if (verify > 0)
                    return 0;
                if (verify < 0)
                    return 1;
                break;

            case 10:
                a = (_cube[6] - _cube[2]) * (_cube[4] - _cube[0])
                        - (_cube[5] - _cube[1]) * (_cube[7] - _cube[3]);
                b = _cube[0] * (_cube[6] - _cube[2]) + _cube[2] * (_cube[4] - _cube[0])
                        - _cube[3] * (_cube[5] - _cube[1])
                        - _cube[1] * (_cube[7] - _cube[3]);
                if (a > 0)
                    return 1;

                t = -b / (2 * a);
                if (t < 0 || t > 1)
                    return 1;

                At = _cube[2] + (_cube[6] - _cube[2]) * t;
                Bt = _cube[1] + (_cube[5] - _cube[1]) * t;
                Ct = _cube[0] + (_cube[4] - _cube[0]) * t;
                Dt = _cube[3] + (_cube[7] - _cube[3]) * t;

                verify = At * Ct - Bt * Dt;

                if (verify > 0)
                    return 0;
                if (verify < 0)
                    return 1;
                break;

            case 11:
                a = (_cube[7] - _cube[3]) * (_cube[5] - _cube[1])
                        - (_cube[6] - _cube[2]) * (_cube[4] - _cube[0]);
                b = _cube[1] * (_cube[7] - _cube[3]) + _cube[3] * (_cube[5] - _cube[1])
                        - _cube[0] * (_cube[6] - _cube[2])
                        - _cube[2] * (_cube[4] - _cube[0]);
                if (a > 0)
                    return 1;

                t = -b / (2 * a);
                if (t < 0 || t > 1)
                    return 1;

                At = _cube[3] + (_cube[7] - _cube[3]) * t;
                Bt = _cube[2] + (_cube[6] - _cube[2]) * t;
                Ct = _cube[1] + (_cube[5] - _cube[1]) * t;
                Dt = _cube[0] + (_cube[4] - _cube[0]) * t;

                verify = At * Ct - Bt * Dt;

                if (verify > 0)
                    return 0;
                if (verify < 0)
                    return 1;
                break;
        }
        return 0;
    }

    //_____________________________________________________________________________
    // NEWS INTERIOR TEST FOR CASE 13
    // Return true if the interior is empty(two faces)
    protected bool interior_test_case13()
    {
        float t1, t2, At1 = 0, Bt1 = 0, Ct1 = 0, Dt1 = 0, At2 = 0, Bt2 = 0, Ct2 = 0, Dt2 = 0, a = 0, b = 0, c = 0;

        a = (_cube[0] - _cube[1]) * (_cube[7] - _cube[6])
                - (_cube[4] - _cube[5]) * (_cube[3] - _cube[2]);
        b = _cube[6] * (_cube[0] - _cube[1]) + _cube[1] * (_cube[7] - _cube[6])
                - _cube[2] * (_cube[4] - _cube[5])
                - _cube[5] * (_cube[3] - _cube[2]);

        c = _cube[1] * _cube[6] - _cube[5] * _cube[2];

        double delta = b * b - 4 * a * c;

        t1 = (float)((-b + System.Math.Sqrt(delta)) / (2 * a));
        t2 = (float)((-b - System.Math.Sqrt(delta)) / (2 * a));

        UnityEngine.Debug.Log(string.Format("t1 = {0}, t2 = {1}\n", t1, t2));

        if ((t1 < 1) && (t1 > 0) && (t2 < 1) && (t2 > 0))
        {

            At1 = _cube[1] + (_cube[0] - _cube[1]) * t1;
            Bt1 = _cube[5] + (_cube[4] - _cube[5]) * t1;
            Ct1 = _cube[6] + (_cube[7] - _cube[6]) * t1;
            Dt1 = _cube[2] + (_cube[3] - _cube[2]) * t1;

            float x1 = (At1 - Dt1) / (At1 + Ct1 - Bt1 - Dt1);
            float y1 = (At1 - Bt1) / (At1 + Ct1 - Bt1 - Dt1);

            At2 = _cube[1] + (_cube[0] - _cube[1]) * t2;
            Bt2 = _cube[5] + (_cube[4] - _cube[5]) * t2;
            Ct2 = _cube[6] + (_cube[7] - _cube[6]) * t2;
            Dt2 = _cube[2] + (_cube[3] - _cube[2]) * t2;

            float x2 = (At2 - Dt2) / (At2 + Ct2 - Bt2 - Dt2);
            float y2 = (At2 - Bt2) / (At2 + Ct2 - Bt2 - Dt2);

            if ((x1 < 1) && (x1 > 0) && (x2 < 1) && (x2 > 0) && (y1 < 1) && (y1 > 0) && (y2 < 1) && (y2 > 0))
                return false;
        }

        return true;

    }

    //--------------------------------------------------------------------------------------------------------------------
    // control the tunnel orientation triangulation
    int tunnelOrientation = 0;

    protected bool interior_test_case13_2(float isovalue)
    {

        double critival_point_value1 = -1, critival_point_value2 = -1;

        double a = -_cube[0] + _cube[1] + _cube[3] - _cube[2] + _cube[4] - _cube[5] - _cube[7] + _cube[6],
                b = _cube[0] - _cube[1] - _cube[3] + _cube[2],
                c = _cube[0] - _cube[1] - _cube[4] + _cube[5],
                d = _cube[0] - _cube[3] - _cube[4] + _cube[7],
                e = -_cube[0] + _cube[1],
                f = -_cube[0] + _cube[3],
                g = -_cube[0] + _cube[4],
                h = _cube[0];

        double x1, y1, z1, x2, y2, z2;
        int numbercritivalpoints = 0;

        double dx = b * c - a * e, dy = b * d - a * f, dz = c * d - a * g;

        if (dx != 0.0f && dy != 0.0f && dz != 0.0f)
        {
            if (dx * dy * dz < 0)
                return true;

            double disc = System.Math.Sqrt(dx * dy * dz);

            x1 = (-d * dx - disc) / (a * dx);
            y1 = (-c * dy - disc) / (a * dy);
            z1 = (-b * dz - disc) / (a * dz);

            if ((x1 > 0) && (x1 < 1) && (y1 > 0) && (y1 < 1)
                    && (z1 > 0) && (z1 < 1))
            {
                numbercritivalpoints++;

                critival_point_value1 = a * x1 * y1 * z1 + b * x1 * y1 + c * x1 * z1
                        + d * y1 * z1 + e * x1 + f * y1 + g * z1 + h - isovalue;
            }

            x2 = (-d * dx + disc) / (a * dx);
            y2 = (-c * dy + disc) / (a * dy);
            z2 = (-b * dz + disc) / (a * dz);

            if ((x2 > 0) && (x2 < 1) && (y2 > 0) && (y2 < 1)
                    && (z2 > 0) && (z2 < 1))
            {
                numbercritivalpoints++;

                critival_point_value2 = a * x2 * y2 * z2 + b * x2 * y2 + c * x2 * z2
                        + d * y2 * z2 + e * x2 + f * y2 + g * z2 + h - isovalue;

            }

            if (numbercritivalpoints < 2)
                return true;
            else
            {
                if ((critival_point_value1 * critival_point_value2 > 0))
                {
                    if (critival_point_value1 > 0)
                        tunnelOrientation = 1;
                    else
                        tunnelOrientation = -1;
                }

                return critival_point_value1 * critival_point_value2 < 0;
            }

        }
        else
            return true;
    }

    //-----------------------------------------------------------------------------
    // Operations
    /**
     * computes almost all the vertices of the mesh by interpolation along the cubes edges
     * \param iso isovalue
     */
    protected void compute_intersection_points(float iso)
    {
        for (_k = 0; _k < _size_z; _k++)
            for (_j = 0; _j < _size_y; _j++)
                for (_i = 0; _i < _size_x; _i++)
                {
                    _cube[0] = get_data(_i, _j, _k) - iso;
                    if (_i < _size_x - 1)
                        _cube[1] = get_data(_i + 1, _j, _k) - iso;
                    else
                        _cube[1] = _cube[0];

                    if (_j < _size_y - 1)
                        _cube[3] = get_data(_i, _j + 1, _k) - iso;
                    else
                        _cube[3] = _cube[0];

                    if (_k < _size_z - 1)
                        _cube[4] = get_data(_i, _j, _k + 1) - iso;
                    else
                        _cube[4] = _cube[0];

                    if (System.Math.Abs(_cube[0]) < UnityEngine.Mathf.Epsilon)
                        _cube[0] = UnityEngine.Mathf.Epsilon;
                    if (System.Math.Abs(_cube[1]) < UnityEngine.Mathf.Epsilon)
                        _cube[1] = UnityEngine.Mathf.Epsilon;
                    if (System.Math.Abs(_cube[3]) < UnityEngine.Mathf.Epsilon)
                        _cube[3] = UnityEngine.Mathf.Epsilon;
                    if (System.Math.Abs(_cube[4]) < UnityEngine.Mathf.Epsilon)
                        _cube[4] = UnityEngine.Mathf.Epsilon;

                    if (_cube[0] < 0)
                    {
                        if (_cube[1] > 0)
                            set_x_vert(add_x_vertex(), _i, _j, _k);
                        if (_cube[3] > 0)
                            set_y_vert(add_y_vertex(), _i, _j, _k);
                        if (_cube[4] > 0)
                            set_z_vert(add_z_vertex(), _i, _j, _k);
                    }
                    else
                    {
                        if (_cube[1] < 0)
                            set_x_vert(add_x_vertex(), _i, _j, _k);
                        if (_cube[3] < 0)
                            set_y_vert(add_y_vertex(), _i, _j, _k);
                        if (_cube[4] < 0)
                            set_z_vert(add_z_vertex(), _i, _j, _k);
                    }
                }
    }

    /**
     * routine to add a triangle to the mesh
     * \param trig the code for the triangle as a sequence of edges index
     * \param n    the number of triangles to produce
     * \param v12  the index of the interior vertex to use, if necessary
     */
    protected void add_triangle(sbyte[] trig, byte n, int v12 = -1)
    {

        int[] tv = new int[3];

        for (int t = 0; t < 3 * n; t++)
        {
            switch (trig[t])
            {
                case 0:
                    tv[t % 3] = get_x_vert(_i, _j, _k);
                    break;
                case 1:
                    tv[t % 3] = get_y_vert(_i + 1, _j, _k);
                    break;
                case 2:
                    tv[t % 3] = get_x_vert(_i, _j + 1, _k);
                    break;
                case 3:
                    tv[t % 3] = get_y_vert(_i, _j, _k);
                    break;
                case 4:
                    tv[t % 3] = get_x_vert(_i, _j, _k + 1);
                    break;
                case 5:
                    tv[t % 3] = get_y_vert(_i + 1, _j, _k + 1);
                    break;
                case 6:
                    tv[t % 3] = get_x_vert(_i, _j + 1, _k + 1);
                    break;
                case 7:
                    tv[t % 3] = get_y_vert(_i, _j, _k + 1);
                    break;
                case 8:
                    tv[t % 3] = get_z_vert(_i, _j, _k);
                    break;
                case 9:
                    tv[t % 3] = get_z_vert(_i + 1, _j, _k);
                    break;
                case 10:
                    tv[t % 3] = get_z_vert(_i + 1, _j + 1, _k);
                    break;
                case 11:
                    tv[t % 3] = get_z_vert(_i, _j + 1, _k);
                    break;
                case 12:
                    tv[t % 3] = v12;
                    break;
                default:
                    break;
            }

            if (tv[t % 3] == -1)
            {

                UnityEngine.Debug.Log(string.Format("Marching Cubes: invalid triangle {0}\n", _ntrigs + 1));

                print_cube();
            }

            if (t % 3 == 2)
            {
                if (_ntrigs >= _Ntrigs)
                {
                    Triangle[] temp = (Triangle[])_triangles.Clone();
                    _triangles = new Triangle[2 * _Ntrigs];

                    System.Array.Copy(_triangles, temp, _Ntrigs);
                    temp = null;

                    UnityEngine.Debug.Log(string.Format("{0} allocated triangles\n", _Ntrigs));
                    _Ntrigs *= 2;
                }

                Triangle T = new Triangle();
                T.v1 = tv[0];
                T.v2 = tv[1];
                T.v3 = tv[2];
                _triangles[_ntrigs++] = T;
                //_triangles.Add(T);
                //_ntrigs++;
            }
        }
    }

    /** tests and eventually doubles the vertex buffer capacity for a new vertex insertion */
    protected void test_vertex_addition()
    {
        if (_nverts >= _Nverts)
        {
            Vertex[] temp = (Vertex[])_vertices.Clone();
            _vertices = new Vertex[_Nverts * 2];

            System.Array.Copy(_vertices, temp, _Nverts);
            temp = null;

            UnityEngine.Debug.Log(string.Format("{0} allocated vertices\n", _Nverts));
            _Nverts *= 2;
        }
    }

    /** adds a vertex on the current horizontal edge */
    protected int add_x_vertex()
    {
        test_vertex_addition();
        Vertex vert = new Vertex();


        float u = (_cube[0]) / (_cube[0] - _cube[1]);

        vert.x = (float)_i + u;
        vert.y = (float)_j;
        vert.z = (float)_k;

        vert.nx = (1 - u) * get_x_grad(_i, _j, _k) + u * get_x_grad(_i + 1, _j, _k);
        vert.ny = (1 - u) * get_y_grad(_i, _j, _k) + u * get_y_grad(_i + 1, _j, _k);
        vert.nz = (1 - u) * get_z_grad(_i, _j, _k) + u * get_z_grad(_i + 1, _j, _k);

        u = (float)System.Math.Sqrt(vert.nx * vert.nx + vert.ny * vert.ny + vert.nz * vert.nz);
        if (u > 0)
        {
            vert.nx /= u;
            vert.ny /= u;
            vert.nz /= u;
        }

        _vertices[_nverts++] = vert;
        return _nverts - 1;
    }

    /** adds a vertex on the current longitudinal edge */
    protected int add_y_vertex()
    {
        test_vertex_addition();
        Vertex vert = new Vertex();

        float u = (_cube[0]) / (_cube[0] - _cube[3]);

        vert.x = (float)_i;
        vert.y = (float)_j + u;
        vert.z = (float)_k;

        vert.nx = (1 - u) * get_x_grad(_i, _j, _k)
                + u * get_x_grad(_i, _j + 1, _k);
        vert.ny = (1 - u) * get_y_grad(_i, _j, _k)
                + u * get_y_grad(_i, _j + 1, _k);
        vert.nz = (1 - u) * get_z_grad(_i, _j, _k)
                + u * get_z_grad(_i, _j + 1, _k);

        u = (float)System.Math.Sqrt(
                vert.nx * vert.nx + vert.ny * vert.ny + vert.nz * vert.nz);
        if (u > 0)
        {
            vert.nx /= u;
            vert.ny /= u;
            vert.nz /= u;
        }

        _vertices[_nverts++] = vert;
        return _nverts - 1;
    }

    /** adds a vertex on the current vertical edge */
    protected int add_z_vertex()
    {
        test_vertex_addition();
        Vertex vert = new Vertex();

        float u = (_cube[0]) / (_cube[0] - _cube[4]);

        vert.x = (float)_i;
        vert.y = (float)_j;
        vert.z = (float)_k + u;

        vert.nx = (1 - u) * get_x_grad(_i, _j, _k)
                + u * get_x_grad(_i, _j, _k + 1);
        vert.ny = (1 - u) * get_y_grad(_i, _j, _k)
                + u * get_y_grad(_i, _j, _k + 1);
        vert.nz = (1 - u) * get_z_grad(_i, _j, _k)
                + u * get_z_grad(_i, _j, _k + 1);

        u = (float)System.Math.Sqrt(
                vert.nx * vert.nx + vert.ny * vert.ny + vert.nz * vert.nz);
        if (u > 0)
        {
            vert.nx /= u;
            vert.ny /= u;
            vert.nz /= u;
        }

        _vertices[_nverts++] = vert;
        return _nverts - 1;
    }

    /** adds a vertex inside the current cube */
    protected int add_c_vertex()
    {
        test_vertex_addition();
        Vertex vert = new Vertex();

        float u = 0;
        int vid;

        vert.x = vert.y = vert.z = vert.nx = vert.ny = vert.nz = 0;

        // Computes the average of the intersection points of the cube
        vid = get_x_vert(_i, _j, _k);
        if (vid != -1)
        {
            ++u;
            Vertex v = _vertices[vid];
            vert.x += v.x;
            vert.y += v.y;
            vert.z += v.z;
            vert.nx += v.nx;
            vert.ny += v.ny;
            vert.nz += v.nz;
        }
        vid = get_y_vert(_i + 1, _j, _k);
        if (vid != -1)
        {
            ++u;
            Vertex v = _vertices[vid];
            vert.x += v.x;
            vert.y += v.y;
            vert.z += v.z;
            vert.nx += v.nx;
            vert.ny += v.ny;
            vert.nz += v.nz;
        }
        vid = get_x_vert(_i, _j + 1, _k);
        if (vid != -1)
        {
            ++u;
            Vertex v = _vertices[vid];
            vert.x += v.x;
            vert.y += v.y;
            vert.z += v.z;
            vert.nx += v.nx;
            vert.ny += v.ny;
            vert.nz += v.nz;
        }
        vid = get_y_vert(_i, _j, _k);
        if (vid != -1)
        {
            ++u;
            Vertex v = _vertices[vid];
            vert.x += v.x;
            vert.y += v.y;
            vert.z += v.z;
            vert.nx += v.nx;
            vert.ny += v.ny;
            vert.nz += v.nz;
        }
        vid = get_x_vert(_i, _j, _k + 1);
        if (vid != -1)
        {
            ++u;
            Vertex v = _vertices[vid];
            vert.x += v.x;
            vert.y += v.y;
            vert.z += v.z;
            vert.nx += v.nx;
            vert.ny += v.ny;
            vert.nz += v.nz;
        }
        vid = get_y_vert(_i + 1, _j, _k + 1);
        if (vid != -1)
        {
            ++u;
            Vertex v = _vertices[vid];
            vert.x += v.x;
            vert.y += v.y;
            vert.z += v.z;
            vert.nx += v.nx;
            vert.ny += v.ny;
            vert.nz += v.nz;
        }
        vid = get_x_vert(_i, _j + 1, _k + 1);
        if (vid != -1)
        {
            ++u;
            Vertex v = _vertices[vid];
            vert.x += v.x;
            vert.y += v.y;
            vert.z += v.z;
            vert.nx += v.nx;
            vert.ny += v.ny;
            vert.nz += v.nz;
        }
        vid = get_y_vert(_i, _j, _k + 1);
        if (vid != -1)
        {
            ++u;
            Vertex v = _vertices[vid];
            vert.x += v.x;
            vert.y += v.y;
            vert.z += v.z;
            vert.nx += v.nx;
            vert.ny += v.ny;
            vert.nz += v.nz;
        }
        vid = get_z_vert(_i, _j, _k);
        if (vid != -1)
        {
            ++u;
            Vertex v = _vertices[vid];
            vert.x += v.x;
            vert.y += v.y;
            vert.z += v.z;
            vert.nx += v.nx;
            vert.ny += v.ny;
            vert.nz += v.nz;
        }
        vid = get_z_vert(_i + 1, _j, _k);
        if (vid != -1)
        {
            ++u;
            Vertex v = _vertices[vid];
            vert.x += v.x;
            vert.y += v.y;
            vert.z += v.z;
            vert.nx += v.nx;
            vert.ny += v.ny;
            vert.nz += v.nz;
        }
        vid = get_z_vert(_i + 1, _j + 1, _k);
        if (vid != -1)
        {
            ++u;
            Vertex v = _vertices[vid];
            vert.x += v.x;
            vert.y += v.y;
            vert.z += v.z;
            vert.nx += v.nx;
            vert.ny += v.ny;
            vert.nz += v.nz;
        }
        vid = get_z_vert(_i, _j + 1, _k);
        if (vid != -1)
        {
            ++u;
            Vertex v = _vertices[vid];
            vert.x += v.x;
            vert.y += v.y;
            vert.z += v.z;
            vert.nx += v.nx;
            vert.ny += v.ny;
            vert.nz += v.nz;
        }

        vert.x /= u;
        vert.y /= u;
        vert.z /= u;

        u = (float)System.Math.Sqrt(
                vert.nx * vert.nx + vert.ny * vert.ny + vert.nz * vert.nz);
        if (u > 0)
        {
            vert.nx /= u;
            vert.ny /= u;
            vert.nz /= u;
        }

        return _nverts - 1;
    }

    /**
     * interpolates the horizontal gradient of the implicit function at the lower vertex of the specified cube
     * \param i abscisse of the cube
     * \param j ordinate of the cube
     * \param k height of the cube
     */
    protected float get_x_grad(int i, int j, int k)
    {
        if (i > 0)
        {
            if (i < _size_x - 1)
                return (get_data(i + 1, j, k) - get_data(i - 1, j, k)) / 2;
            else
                return get_data(i, j, k) - get_data(i - 1, j, k);
        }
        else
            return get_data(i + 1, j, k) - get_data(i, j, k);
    }

    /**
     * interpolates the longitudinal gradient of the implicit function at the lower vertex of the specified cube
     * \param i abscisse of the cube
     * \param j ordinate of the cube
     * \param k height of the cube
     */
    protected float get_y_grad(int i, int j, int k)
    {
        if (j > 0)
        {
            if (j < _size_y - 1)
                return (get_data(i, j + 1, k) - get_data(i, j - 1, k)) / 2;
            else
                return get_data(i, j, k) - get_data(i, j - 1, k);
        }
        else
            return get_data(i, j + 1, k) - get_data(i, j, k);
    }

    /**
     * interpolates the vertical gradient of the implicit function at the lower vertex of the specified cube
     * \param i abscisse of the cube
     * \param j ordinate of the cube
     * \param k height of the cube
     */
    protected float get_z_grad(int i, int j, int k)
    {
        if (k > 0)
        {
            if (k < _size_z - 1)
                return (get_data(i, j, k + 1) - get_data(i, j, k - 1)) / 2;
            else
                return get_data(i, j, k) - get_data(i, j, k - 1);
        }
        else
            return get_data(i, j, k + 1) - get_data(i, j, k);
    }

    /**
     * accesses the pre-computed vertex index on the lower horizontal edge of a specific cube
     * \param i abscisse of the cube
     * \param j ordinate of the cube
     * \param k height of the cube
     */
    protected int get_x_vert(int i, int j, int k) { return _x_verts[i + j * _size_x + k * _size_x * _size_y]; }
    /**
     * accesses the pre-computed vertex index on the lower longitudinal edge of a specific cube
     * \param i abscisse of the cube
     * \param j ordinate of the cube
     * \param k height of the cube
     */
    protected int get_y_vert(int i, int j, int k) { return _y_verts[i + j * _size_x + k * _size_x * _size_y]; }
    /**
     * accesses the pre-computed vertex index on the lower vertical edge of a specific cube
     * \param i abscisse of the cube
     * \param j ordinate of the cube
     * \param k height of the cube
     */
    protected int get_z_vert(int i, int j, int k) { return _z_verts[i + j * _size_x + k * _size_x * _size_y]; }

    /**
     * sets the pre-computed vertex index on the lower horizontal edge of a specific cube
     * \param val the index of the new vertex
     * \param i abscisse of the cube
     * \param j ordinate of the cube
     * \param k height of the cube
     */
    protected void set_x_vert(int val, int i, int j, int k) { _x_verts[i + j * _size_x + k * _size_x * _size_y] = val; }
    /**
     * sets the pre-computed vertex index on the lower longitudinal edge of a specific cube
     * \param val the index of the new vertex
     * \param i abscisse of the cube
     * \param j ordinate of the cube
     * \param k height of the cube
     */
    protected void set_y_vert(int val, int i, int j, int k) { _y_verts[i + j * _size_x + k * _size_x * _size_y] = val; }
    /**
     * sets the pre-computed vertex index on the lower vertical edge of a specific cube
     * \param val the index of the new vertex
     * \param i abscisse of the cube
     * \param j ordinate of the cube
     * \param k height of the cube
     */
    protected void set_z_vert(int val, int i, int j, int k) { _z_verts[i + j * _size_x + k * _size_x * _size_y] = val; }

    /** prints cube for debug */
    protected void print_cube()
    {

        UnityEngine.Debug.Log(string.Format("\t{0} {1} {2} {3} {4} {5} {6} {7}\n", _cube[0], _cube[1], _cube[2],
                _cube[3], _cube[4], _cube[5], _cube[6], _cube[7]));
    }

    //-----------------------------------------------------------------------------
    // Elements

    protected bool _originalMC;   /**< selects wether the algorithm will use the enhanced topologically controlled lookup table or the original MarchingCubes */
    protected bool _ext_data;   /**< selects wether to allocate data or use data from another class */

    protected int _size_x;  /**< width  of the grid */
    protected int _size_y;  /**< depth  of the grid */
    protected int _size_z;  /**< height of the grid */
    protected float[] _data;  /**< implicit function values sampled on the grid */

    protected int[] _x_verts;  /**< pre-computed vertex indices on the lower horizontal   edge of each cube */
    protected int[] _y_verts;  /**< pre-computed vertex indices on the lower longitudinal edge of each cube */
    protected int[] _z_verts;  /**< pre-computed vertex indices on the lower vertical     edge of each cube */

    protected int _nverts;  /**< number of allocated vertices  in the vertex   buffer */
    protected int _ntrigs;  /**< number of allocated triangles in the triangle buffer */
    protected int _Nverts;  /**< size of the vertex   buffer */
    protected int _Ntrigs;  /**< size of the triangle buffer */
    protected Vertex[] _vertices;  /**< vertex   buffer */
    protected Triangle[] _triangles;  /**< triangle buffer */
    //protected List<Triangle> _triangles;  /**< triangle buffer */

    protected int _i;  /**< abscisse of the active cube */
    protected int _j;  /**< height of the active cube */
    protected int _k;  /**< ordinate of the active cube */

    protected float[] _cube = new float[8];  /**< values of the implicit function on the active cube */
    protected byte _lut_entry;  /**< cube sign representation in [0..255] */
    protected byte _case;  /**< case of the active cube in [0..15] */
    protected byte _config;  /**< configuration of the active cube */
    protected byte _subconfig;  /**< subconfiguration of the active cube */
};
//_____________________________________________________________________________

