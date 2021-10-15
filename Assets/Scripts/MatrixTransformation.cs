using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Mathf;

public class MatrixTransformation : MonoBehaviour
{
    // GameObjects
    public GameObject cube;
    public GameObject plane1;
    public GameObject plane2;
    public GameObject plane3;
    public GameObject plane4;
    public GameObject plane5;
    public GameObject fadeScreen;

    // Animators
    public Animator transition;
    public Animator transitionCanvas;
    public Animator AnimatorCustomCube;

    // Input fields for matrix
    public TMP_InputField inputX0;
    public TMP_InputField inputX1;
    public TMP_InputField inputX2;
    public TMP_InputField inputY0;
    public TMP_InputField inputY1;
    public TMP_InputField inputY2;
    public TMP_InputField inputZ0;
    public TMP_InputField inputZ1;
    public TMP_InputField inputZ2;
    public TMP_InputField inputAngle;

    // Animation curves for matrix inputs
    public AnimationCurve curveX0;
    public AnimationCurve curveX1;
    public AnimationCurve curveX2;
    public AnimationCurve curveY0;
    public AnimationCurve curveY1;
    public AnimationCurve curveY2;
    public AnimationCurve curveZ0;
    public AnimationCurve curveZ1;
    public AnimationCurve curveZ2;

    // Animation curves for other animations
    public AnimationCurve curveAngle;
    public AnimationCurve curve2DTo3DScene1;
    public AnimationCurve curve2DTo3DScene2;
    public AnimationCurve curve2DMatrixTo3DMatrixScene2;
    public AnimationCurve curveDisplayOtherPlanes;
    public AnimationCurve curveDisplayCube;

    // Matrix values
    private float x0;
    private float x1;
    private float x2;
    private float y0;
    private float y1;
    private float y2;
    private float z0;
    private float z1;
    private float z2;
    private float angle;

    private List<AnimationCurve> _curves;
    private bool _animationPlaying;
    private float _animationTime;
    private float _maxAnimationTime;
    private LinkedList<TransformInterval> _transformIntervals; // store the intervals that are using rotation
    private LinkedList<TransformInterval> _transformIntervalsCopy;

    // Stores cube data
    private Mesh _cubeMesh;
    private Vector3[] _cubeVertices;

    // Stores stacked planes data
    private Mesh _face1Mesh;
    private Vector3[] _face1Vertices;
    private Mesh _face2Mesh;
    private Vector3[] _face2Vertices;
    private Mesh _face3Mesh;
    private Vector3[] _face3Vertices;
    private Mesh _face4Mesh;
    private Vector3[] _face4Vertices;
    private Mesh _face5Mesh;
    private Vector3[] _face5Vertices;
    private List<GameObject> _planeList;
    private List<Mesh> _faceMeshList;
    private List<Vector3[]> _faceVerticesOriginalList;


    // Constants
    private const int InitialWaitTime = 2;
    private const string PiSymbol = "\u03C0";


    private void Awake()
    {
        fadeScreen.SetActive(true);
    }

    // Start is called before the first frame update
    void Start()
    {
        curveX0 = new AnimationCurve();
        curveX1 = new AnimationCurve();
        curveX2 = new AnimationCurve();
        curveY0 = new AnimationCurve();
        curveY1 = new AnimationCurve();
        curveY2 = new AnimationCurve();
        curveZ0 = new AnimationCurve();
        curveZ1 = new AnimationCurve();
        curveZ2 = new AnimationCurve();


        _curves = new List<AnimationCurve>
        {
            curveX0, curveX1, curveX2, curveY0, curveY1, curveY2,
            curveZ0, curveZ1, curveZ2, curve2DTo3DScene1, curve2DMatrixTo3DMatrixScene2, curve2DTo3DScene2,
            curveDisplayOtherPlanes, curveDisplayCube
        }; // If more curves, add them here

        _transformIntervals = new LinkedList<TransformInterval>();
        _transformIntervalsCopy = new LinkedList<TransformInterval>();
        SetText(inputX0, 1, "none");
        SetText(inputX1, 0, "none");
        SetText(inputX2, 0, "none");
        SetText(inputY0, 0, "none");
        SetText(inputY1, 1, "none");
        SetText(inputY2, 0, "none");
        SetText(inputZ0, 0, "none");
        SetText(inputZ1, 0, "none");
        SetText(inputZ2, 1, "none");


        _cubeMesh = cube.GetComponent<MeshFilter>().mesh;
        _cubeVertices = _cubeMesh.vertices;


        DisableFrontFaceCube(); // Fades the face of the cube where the Bug texture is


        if (SceneManager.GetActiveScene().name == "Scene12")
        {
            _face1Mesh = plane1.GetComponent<MeshFilter>().mesh;
            _face1Vertices = _face1Mesh.vertices;
        }
        else
        {
            _face1Mesh = plane1.GetComponent<MeshFilter>().mesh;
            _face1Vertices = AddZPosition(_face1Mesh.vertices, 0.5f);

            _face2Mesh = plane2.GetComponent<MeshFilter>().mesh;
            _face2Vertices = AddZPosition(_face2Mesh.vertices, 0.25f);

            _face3Mesh = plane3.GetComponent<MeshFilter>().mesh;
            _face3Vertices = AddZPosition(_face3Mesh.vertices, 0f);

            _face4Mesh = plane4.GetComponent<MeshFilter>().mesh;
            _face4Vertices = AddZPosition(_face4Mesh.vertices, -0.25f);

            _face5Mesh = plane5.GetComponent<MeshFilter>().mesh;
            _face5Vertices = AddZPosition(_face5Mesh.vertices, -0.5f);
        }

        _faceVerticesOriginalList = new List<Vector3[]>
            { _face1Vertices, _face2Vertices, _face3Vertices, _face4Vertices, _face5Vertices };
        _faceMeshList = new List<Mesh> { _face1Mesh, _face2Mesh, _face3Mesh, _face4Mesh, _face5Mesh };
        _planeList = new List<GameObject> { plane1, plane2, plane3, plane4, plane5 };

        CreateAnimationCurves(); // Create animation curves on start
        _transformIntervals = MergeRotationIntervals();
    }


    /// <summary>
    /// Create keyframes for animation curves
    /// in the 2D matrix
    /// </summary>
    private void CreateAnimationCurves()
    {
        _animationPlaying = false;
        _animationTime = 0f;

        curveX0.AddKey(0, 1);
        curveX1.AddKey(0, 0);
        curveX2.AddKey(0, 0);
        curveY0.AddKey(0, 0);
        curveY1.AddKey(0, 1);
        curveY2.AddKey(0, 0);
        curveZ0.AddKey(0, 0);
        curveZ1.AddKey(0, 0);
        curveZ2.AddKey(0, 1);
        curve2DTo3DScene2.AddKey(0, 0);
        curveDisplayOtherPlanes.AddKey(0, 0);
        curveDisplayCube.AddKey(0, 0);

        ///
        // Scene1:
        // + 2D identity matrix -> 3D identity matrix
        // + 3D transformations, without Translations
        ///
        if (SceneManager.GetActiveScene().name == "Scene1")
        {
            //////////////////////////////////////////////////////////
            /// Step 1: Transition from 2D view to 3D view
            /////////////////////////////////////////////////////////
            // 2D->3D transition starts immediately after pressing "space".
            // The animation (defined outside this script) is 3s long.
            // Please let me know if you want to adjust this transition
            curve2DTo3DScene1.AddKey(0, 1);


            //////////////////////////////////////////////////////////
            /// Step 2: Do 3D transformations
            /////////////////////////////////////////////////////////

            //////////////////////////////////////////////////////////
            // SCALE X
            ScaleX(1f, 1.5f, 1, 5); // iniWaitTime=5 means we wait 2s after the transition above
            ScaleX(1.5f, 1f, 1, 0.5f);
            // SCALE Y
            ScaleY(1f, 1.5f, 1, InitialWaitTime);
            ScaleY(1.5f, 1f, 1, 0.5f);
            // SCALE Z
            ScaleZ(1f, 1.5f, 1, InitialWaitTime);
            ScaleZ(1.5f, 1f, 1, 0.5f);

            ////////////////////////////////////////////////////////////
            // SHEAR X
            ShearX(0f, -0.5f, 0.5f, InitialWaitTime);
            ShearX(-0.5f, 0.5f, 1f, 0);
            ShearX(0.5f, 0f, 0.5f, 0);
            // SHEAR Y
            ShearY(0f, -0.5f, 0.5f, InitialWaitTime);
            ShearY(-0.5f, 0.5f, 1f, 0);
            ShearY(0.5f, 0f, 0.5f, 0);
            // SHEAR Z
            ShearZ(0f, -0.5f, 0.5f, InitialWaitTime);
            ShearZ(-0.5f, 0.5f, 1f, 0);
            ShearZ(0.5f, 0f, 0.5f, 0);

            ////////////////////////////////////////////////////////////
            // REFLECT ACROSS Y
            ScaleX(1f, -1f, 1.25f, InitialWaitTime);
            ScaleX(-1f, 1f, 1.25f, 0.5f);
            // REFLECT ACROSS X
            ScaleY(1f, -1f, 1.25f, InitialWaitTime);
            ScaleY(-1f, 1f, 1.25f, 0.5f);
            // REFLECT ACROSS Z
            ScaleZ(1f, -1f, 1.25f, InitialWaitTime);
            ScaleZ(-1f, 1f, 1.25f, 0.5f);


            ////////////////////////////////////////////////////////////
            // FULL ROTATION 0->2PI
            RotateByZ(0, 6.28f, 3, InitialWaitTime);
            RotateByY(0, 6.28f, 3, 0);
            RotateByX(0, 6.28f, 3, 0);
        }

        ///
        // Scene2:
        // + 2D identity matrix -> 3D identity matrix
        // + Translations
        ///
        if (SceneManager.GetActiveScene().name == "Scene2")
        {
            // 2D->3D transition starts immediately after pressing "space".
            curve2DMatrixTo3DMatrixScene2.AddKey(GetLastKeyTime(), 1);
            curve2DMatrixTo3DMatrixScene2.AddKey(GetLastKeyTime() + 0.1f, 0);

            //////////////////////////////////////////////////////////
            /// Step 1: Do 2D translations
            /////////////////////////////////////////////////////////
            // X-direction
            TranslateX(0f, 1f, 1.5f, 4);
            TranslateX(1f, 0f, 1.5f, 0.5f);
            // Y-direction
            TranslateY(0f, 1f, 1.5f, InitialWaitTime);
            TranslateY(1f, 0f, 1.5f, 0.5f);


            //////////////////////////////////////////////////////////
            /// Step 2: Transition from 2D->3D
            //////////////////////////////////////////////////////////
            // Then, transition from 2D to 3D, do 3D cube shearing
            var initialWaitTimeForTransition = GetLastKeyTime() + 1; // wait 1s after TranslateY_2D above 
            curve2DTo3DScene2.AddKey(initialWaitTimeForTransition, 1); // 2D plane to 3D cube
            curveDisplayCube.AddKey(initialWaitTimeForTransition, 1); // display cube


            //////////////////////////////////////////////////////////
            /// Step 3: Do 3D Shear in X-direction WITH CUBE
            //////////////////////////////////////////////////////////
            TranslateX(0f, 1, 1.5f, 3); // wait 3s to shear after 2D->3D transition
            TranslateX(1, 0f, 1.5f, 0.5f);

            //////////////////////////////////////////////////////////
            /// Step 4: Fade cube and show stacked planes
            //////////////////////////////////////////////////////////
            var fadeCubeShowPlanesTime = GetLastKeyTime() + 1; // wait 1s after step 3
            curveDisplayCube.AddKey(fadeCubeShowPlanesTime, 1);
            curveDisplayCube.AddKey(fadeCubeShowPlanesTime + 0.001f, 0); // fade cube
            curveDisplayOtherPlanes.AddKey(fadeCubeShowPlanesTime, 1); // show stacked planes, value = "1"


            //////////////////////////////////////////////////////////
            /// Step 5: Do X-Translation with stacked planes
            //////////////////////////////////////////////////////////
            TranslateX(0f, 1, 1.5f, 2); // wait 2s after the step 4
            TranslateX(1, 0f, 1.5f, 0.5f);

            //////////////////////////////////////////////////////////
            /// Step 6: Fade stacked planes, only the face left
            //////////////////////////////////////////////////////////
            curveDisplayOtherPlanes.AddKey(GetLastKeyTime() + 1, 1);
            curveDisplayOtherPlanes.AddKey(GetLastKeyTime() + 1 + 0.001f, 0); // fade planes beneath, value "0"

            //////////////////////////////////////////////////////////
            /// Step 7: Do X and Y Translations with one face
            //////////////////////////////////////////////////////////
            // X-direction
            TranslateX(0f, 1f, 1.5f, 0);
            TranslateX(1f, 0f, 1.5f, 0.5f);
            // Y-direction
            TranslateY(0f, 1f, 1.5f, 1);
            TranslateY(1f, 0f, 1.5f, 0.5f);


            // Keep these code
            SetCurveLinear(curveDisplayOtherPlanes, 0, GetLastKeyTime());
            SetCurveLinear(curveDisplayCube, 0, GetLastKeyTime());
        }
    }


    /// <summary>
    /// Press "1" to load Scene1. Press "2" to load Scene2
    /// </summary>
    void Update()
    {
        UseAnimationCurve();
        TransformMesh();

        if (Input.GetKeyDown("1"))
        {
            StartCoroutine(LoadScene("Scene1"));
        }

        if (Input.GetKeyDown("2"))
        {
            StartCoroutine(LoadScene("Scene2"));
        }
    }


    /// <summary>
    /// Press "Space" to start/reset the animation. The function evaluates
    /// the main curves (x0,x1,y0,y1) to apply transformation
    /// </summary>
    private void UseAnimationCurve()
    {
        if (Input.GetKeyDown("space")) // Press space to start playing the animation
        {
            Reset();
        }

        if (!_animationPlaying) return;
        if (inputX0.isFocused || inputX1.isFocused || inputY0.isFocused || inputY1.isFocused ||
            _animationTime >= _maxAnimationTime)
        {
            _animationPlaying = false;
            _animationTime = 0f;
            return;
        }

        _animationTime += 0.01f;


        var usingRotation = UsingRotation();

        SetText(inputX0, curveX0.Evaluate(_animationTime), usingRotation);
        SetText(inputX1, curveX1.Evaluate(_animationTime), usingRotation);
        SetText(inputX2, curveX2.Evaluate(_animationTime), usingRotation);

        SetText(inputY0, curveY0.Evaluate(_animationTime), usingRotation);
        SetText(inputY1, curveY1.Evaluate(_animationTime), usingRotation);
        SetText(inputY2, curveY2.Evaluate(_animationTime), usingRotation);

        SetText(inputZ0, curveZ0.Evaluate(_animationTime), usingRotation);
        SetText(inputZ1, curveZ1.Evaluate(_animationTime), usingRotation);
        SetText(inputZ2, curveZ2.Evaluate(_animationTime), usingRotation);

        SetText(inputAngle, curveAngle.Evaluate(_animationTime), usingRotation);

        // Animations for Scene1
        if (SceneManager.GetActiveScene().name == "Scene1")
        {
            // 2D view -> 3D view
            var evaluation2Dto3D = curve2DTo3DScene1.Evaluate(_animationTime);
            if (evaluation2Dto3D >= 1f)
            {
                transitionCanvas.Play("Scene1_2Dto3DCanvas");
                transition.Play("Scene1_2Dto3D");
            }
        }

        // Animations for Scene2
        if (SceneManager.GetActiveScene().name == "Scene2")
        {
            // 2D matrix -> 3D matrix anim
            var evaluation2DMatrixTo3DMatrix = curve2DMatrixTo3DMatrixScene2.Evaluate(_animationTime);
            if (evaluation2DMatrixTo3DMatrix != 0f)
            {
                transitionCanvas.Play("Scene2_2DMatrixTo3DMatrix");
            }

            // 2D view -> 3D view
            var evaluation2DTo3D = curve2DTo3DScene2.Evaluate(_animationTime);
            if (evaluation2DTo3D >= 1f)
            {
                AnimatorCustomCube.Play("2D_End_Quad");
                transitionCanvas.Play("2D_End_Labels");
            }


            // Displaying stacked planes anim
            var evaluationDisplayOtherPlanes = curveDisplayOtherPlanes.Evaluate(_animationTime);
            DisplayOtherPlanes(Math.Abs(evaluationDisplayOtherPlanes - 1f) < float.Epsilon);

            // Displaying cube anim
            var evaluateDisplayCube = curveDisplayCube.Evaluate(_animationTime);
            if (Math.Abs(evaluateDisplayCube - 1f) < float.Epsilon)
            {
                cube.SetActive(true);
                ResetAlpha(cube, 1f);
            }
            else
            {
                StartCoroutine(FadeOutMaterial(0.5f, cube));
            }
        }
    }

    /// Scale along X 
    /// 
    /// <param name="startX"></param> The starting value for x0
    /// <param name="endX"></param> The ending value for x0
    /// <param name="animDuration"></param> Duration of animation
    /// <param name="iniWaitTime"></param>  Wait time before animation begins
    private void ScaleX(float startX, float endX, float animDuration, float iniWaitTime)
    {
        var anim = GetAnimationDuration(animDuration, iniWaitTime);
        var startTime = anim.StartTime;
        var endTime = anim.EndTime;

        curveX0.AddKey(startTime, startX);
        curveX0.AddKey(endTime, endX);
        SetCurveLinear(curveX0, startTime, endTime);

        TransformInterval interval = new TransformInterval(startTime, endTime, "none");
        _transformIntervals.AddLast(interval);
    }

    /// Scale along Y
    /// 
    /// <param name="startY"></param> The starting value for y1
    /// <param name="endY"></param> The ending value for y1
    /// <param name="animDuration"></param> Duration of animation
    /// <param name="iniWaitTime"></param>  Wait time before animation begins
    private void ScaleY(float startY, float endY, float animDuration, float iniWaitTime)
    {
        var anim = GetAnimationDuration(animDuration, iniWaitTime);
        var startTime = anim.StartTime;
        var endTime = anim.EndTime;

        curveY1.AddKey(startTime, startY);
        curveY1.AddKey(endTime, endY);
        SetCurveLinear(curveY1, startTime, endTime);

        TransformInterval interval = new TransformInterval(startTime, endTime, "none");
        _transformIntervals.AddLast(interval);
    }

    /// Scale along Z
    /// 
    /// <param name="startZ"></param> The starting value for z2
    /// <param name="endZ"></param> The ending value for z2
    /// <param name="animDuration"></param> Duration of animation
    /// <param name="iniWaitTime"></param>  Wait time before animation begins
    private void ScaleZ(float startZ, float endZ, float animDuration, float iniWaitTime)
    {
        var anim = GetAnimationDuration(animDuration, iniWaitTime);
        var startTime = anim.StartTime;
        var endTime = anim.EndTime;

        curveZ2.AddKey(startTime, startZ);
        curveZ2.AddKey(endTime, endZ);
        SetCurveLinear(curveZ2, startTime, endTime);

        TransformInterval interval = new TransformInterval(startTime, endTime, "none");
        _transformIntervals.AddLast(interval);
    }

    /// Shear along X (by modifying y0)
    /// 
    /// <param name="startX"></param> The starting value for y0
    /// <param name="endX"></param> The ending value for y0
    /// <param name="animDuration"></param> Duration of animation
    /// <param name="iniWaitTime"></param>  Wait time before animation begins
    private void ShearX(float startX, float endX, float animDuration, float iniWaitTime)
    {
        var anim = GetAnimationDuration(animDuration, iniWaitTime);
        var startTime = anim.StartTime;
        var endTime = anim.EndTime;

        curveY0.AddKey(startTime, startX);
        curveY0.AddKey(endTime, endX);
        SetCurveLinear(curveY0, startTime, endTime);

        TransformInterval interval = new TransformInterval(startTime, endTime, "none");
        _transformIntervals.AddLast(interval);
    }

    /// Shear along Y (by modifying x1)
    /// 
    /// <param name="startY"></param> The starting value for x1
    /// <param name="endY"></param> The ending value for x1
    /// <param name="animDuration"></param> Duration of animation
    /// <param name="iniWaitTime"></param>  Wait time before animation begins
    private void ShearY(float startY, float endY, float animDuration, float iniWaitTime)
    {
        var anim = GetAnimationDuration(animDuration, iniWaitTime);
        var startTime = anim.StartTime;
        var endTime = anim.EndTime;

        curveX1.AddKey(startTime, startY);
        curveX1.AddKey(endTime, endY);
        SetCurveLinear(curveX1, startTime, endTime);

        TransformInterval interval = new TransformInterval(startTime, endTime, "none");
        _transformIntervals.AddLast(interval);
    }

    /// Shear along Z (by modifying x2)
    /// 
    /// <param name="startZ"></param> The starting value for x2
    /// <param name="endZ"></param> The ending value for x2
    /// <param name="animDuration"></param> Duration of animation
    /// <param name="iniWaitTime"></param>  Wait time before animation begins
    private void ShearZ(float startZ, float endZ, float animDuration, float iniWaitTime)
    {
        var anim = GetAnimationDuration(animDuration, iniWaitTime);
        var startTime = anim.StartTime;
        var endTime = anim.EndTime;

        curveX2.AddKey(startTime, startZ);
        curveX2.AddKey(endTime, endZ);
        SetCurveLinear(curveX2, startTime, endTime);

        TransformInterval interval = new TransformInterval(startTime, endTime, "none");
        _transformIntervals.AddLast(interval);
    }


    /// Apply rotation around the z-axis from a to b
    /// 
    /// <param name="fromRadAngle"></param> The initial angle (in radians)
    /// <param name="toRadAngle"></param> The ending angle (in radians)
    /// <param name="animDuration"></param> Duration of animation
    /// <param name="iniWaitTime"></param>  Wait time before animation begins
    private void RotateByZ(float fromRadAngle, float toRadAngle, float animDuration, float iniWaitTime)
    {
        var anim = GetAnimationDuration(animDuration, iniWaitTime);
        var startTime = anim.StartTime;
        var endTime = anim.EndTime;

        var smallAngle = toRadAngle / 20;
        var smallDuration = animDuration / 20f;

        var sinFromAngle = Sin(fromRadAngle);
        var cosFromAngle = Cos(fromRadAngle);
        var sinToAngle = Sin(toRadAngle);
        var cosToAngle = Cos(toRadAngle);

        curveX0.AddKey(startTime, cosFromAngle);
        curveX1.AddKey(startTime, sinFromAngle);
        curveY0.AddKey(startTime, -sinFromAngle);
        curveY1.AddKey(startTime, cosFromAngle);
        curveAngle.AddKey(startTime, fromRadAngle);


        for (int i = 1; i <= 20; i++)
        {
            float time = startTime + i * smallDuration;
            float cosSmallAngle = Cos(smallAngle * i);
            float sinSmallAngle = Sin(smallAngle * i);
            curveX0.AddKey(time, cosSmallAngle);
            curveX1.AddKey(time, sinSmallAngle);
            curveY0.AddKey(time, -sinSmallAngle);
            curveY1.AddKey(time, cosSmallAngle);
            var smallAngleRadians = (sinSmallAngle > 0) ? Acos(cosSmallAngle) : -Acos(cosSmallAngle);
            if (smallAngleRadians < 0) smallAngleRadians += 6.28f;
            curveAngle.AddKey(time, smallAngleRadians);
        }

        curveX0.AddKey(endTime, cosToAngle);
        curveX1.AddKey(endTime, sinToAngle);
        curveY0.AddKey(endTime, -sinToAngle);
        curveY1.AddKey(endTime, cosToAngle);
        var angleRadian = (sinToAngle > 0) ? Acos(cosToAngle) : -Acos(cosToAngle);
        if (angleRadian < 0) angleRadian += 6.28f;
        curveAngle.AddKey(endTime, angleRadian);


        var afterEnd = endTime + 2f;
        curveX0.AddKey(afterEnd, cosToAngle);
        curveX1.AddKey(afterEnd, sinToAngle);
        curveY0.AddKey(afterEnd, -sinToAngle);
        curveY1.AddKey(afterEnd, cosToAngle);
        curveAngle.AddKey(afterEnd, angleRadian);
        curveAngle.AddKey(afterEnd + 0.001f, 0);

        SetCurveLinear(curveX0, 0, startTime);
        SetCurveLinear(curveX1, 0, startTime);
        SetCurveLinear(curveY0, 0, startTime);
        SetCurveLinear(curveY1, 0, startTime);
        SetCurveLinear(curveX0, endTime, afterEnd);
        SetCurveLinear(curveX1, endTime, afterEnd);
        SetCurveLinear(curveY0, endTime, afterEnd);
        SetCurveLinear(curveY1, endTime, afterEnd);
        SetCurveLinear(curveAngle, 0, afterEnd);

        TransformInterval interval = new TransformInterval(startTime, afterEnd, "rotateByZ");
        RemoveDuplicateIntervals(interval);
        _transformIntervals.AddLast(interval);
    }

    /// Apply rotation around the x-axis from a to b
    /// 
    /// <param name="fromRadAngle"></param> The initial angle (in radians)
    /// <param name="toRadAngle"></param> The ending angle (in radians)
    /// <param name="animDuration"></param> Duration of animation
    /// <param name="iniWaitTime"></param>  Wait time before animation begins
    private void RotateByX(float fromRadAngle, float toRadAngle, float animDuration, float iniWaitTime)
    {
        var anim = GetAnimationDuration(animDuration, iniWaitTime);
        var startTime = anim.StartTime;
        var endTime = anim.EndTime;

        var smallAngle = toRadAngle / 20;
        var smallDuration = animDuration / 20f;

        var sinFromAngle = Sin(fromRadAngle);
        var cosFromAngle = Cos(fromRadAngle);
        var sinToAngle = Sin(toRadAngle);
        var cosToAngle = Cos(toRadAngle);

        curveY1.AddKey(startTime, cosFromAngle);
        curveY2.AddKey(startTime, sinFromAngle);
        curveZ1.AddKey(startTime, -sinFromAngle);
        curveZ2.AddKey(startTime, cosFromAngle);
        curveAngle.AddKey(startTime, fromRadAngle);


        for (int i = 1; i <= 20; i++)
        {
            float time = startTime + i * smallDuration;
            float cosSmallAngle = Cos(smallAngle * i);
            float sinSmallAngle = Sin(smallAngle * i);
            curveY1.AddKey(time, cosSmallAngle);
            curveY2.AddKey(time, sinSmallAngle);
            curveZ1.AddKey(time, -sinSmallAngle);
            curveZ2.AddKey(time, cosSmallAngle);
            var smallAngleRadians = (sinSmallAngle > 0) ? Acos(cosSmallAngle) : -Acos(cosSmallAngle);
            if (smallAngleRadians < 0) smallAngleRadians += 6.28f;
            curveAngle.AddKey(time, smallAngleRadians);
        }

        curveY1.AddKey(endTime, cosToAngle);
        curveY2.AddKey(endTime, sinToAngle);
        curveZ1.AddKey(endTime, -sinToAngle);
        curveZ2.AddKey(endTime, cosToAngle);
        var angleRadian = (sinToAngle > 0) ? Acos(cosToAngle) : -Acos(cosToAngle);
        if (angleRadian < 0) angleRadian += 6.28f;
        curveAngle.AddKey(endTime, angleRadian);

        var afterEnd = endTime + 2f;
        curveY1.AddKey(afterEnd, cosToAngle);
        curveY2.AddKey(afterEnd, sinToAngle);
        curveZ1.AddKey(afterEnd, -sinToAngle);
        curveZ2.AddKey(afterEnd, cosToAngle);
        curveAngle.AddKey(afterEnd, angleRadian);
        curveAngle.AddKey(afterEnd + 0.001f, 0);

        SetCurveLinear(curveY1, 0, startTime);
        SetCurveLinear(curveY2, 0, startTime);
        SetCurveLinear(curveZ1, 0, startTime);
        SetCurveLinear(curveZ2, 0, startTime);
        SetCurveLinear(curveY1, endTime, afterEnd);
        SetCurveLinear(curveY2, endTime, afterEnd);
        SetCurveLinear(curveZ1, endTime, afterEnd);
        SetCurveLinear(curveZ2, endTime, afterEnd);
        SetCurveLinear(curveAngle, 0, afterEnd);

        TransformInterval interval = new TransformInterval(startTime, afterEnd, "rotateByX");
        RemoveDuplicateIntervals(interval);
        _transformIntervals.AddLast(interval);
    }

    /// Apply rotation around the y-axis from a to b
    /// 
    /// <param name="fromRadAngle"></param> The initial angle (in radians)
    /// <param name="toRadAngle"></param> The ending angle (in radians)
    /// <param name="animDuration"></param> Duration of animation
    /// <param name="iniWaitTime"></param>  Wait time before animation begins
    private void RotateByY(float fromRadAngle, float toRadAngle, float animDuration, float iniWaitTime)
    {
        var anim = GetAnimationDuration(animDuration, iniWaitTime);
        var startTime = anim.StartTime;
        var endTime = anim.EndTime;

        var smallAngle = toRadAngle / 20;
        var smallDuration = animDuration / 20f;

        var sinFromAngle = Sin(fromRadAngle);
        var cosFromAngle = Cos(fromRadAngle);
        var sinToAngle = Sin(toRadAngle);
        var cosToAngle = Cos(toRadAngle);

        curveX0.AddKey(startTime, cosFromAngle);
        curveZ0.AddKey(startTime, sinFromAngle);
        curveX2.AddKey(startTime, -sinFromAngle);
        curveZ2.AddKey(startTime, cosFromAngle);
        curveAngle.AddKey(startTime, fromRadAngle);


        for (int i = 1; i <= 20; i++)
        {
            float time = startTime + i * smallDuration;
            float cosSmallAngle = Cos(smallAngle * i);
            float sinSmallAngle = Sin(smallAngle * i);
            curveX0.AddKey(time, cosSmallAngle);
            curveZ0.AddKey(time, sinSmallAngle);
            curveX2.AddKey(time, -sinSmallAngle);
            curveZ2.AddKey(time, cosSmallAngle);
            var smallAngleRadians = (sinSmallAngle > 0) ? Acos(cosSmallAngle) : -Acos(cosSmallAngle);
            if (smallAngleRadians < 0) smallAngleRadians += 6.28f;
            curveAngle.AddKey(time, smallAngleRadians);
        }

        curveX0.AddKey(endTime, cosToAngle);
        curveZ0.AddKey(endTime, sinToAngle);
        curveX2.AddKey(endTime, -sinToAngle);
        curveZ2.AddKey(endTime, cosToAngle);
        var angleRadian = (sinToAngle > 0) ? Acos(cosToAngle) : -Acos(cosToAngle);
        if (angleRadian < 0) angleRadian += 6.28f;
        curveAngle.AddKey(endTime, angleRadian);


        var afterEnd = endTime + 2f;
        curveX0.AddKey(afterEnd, cosToAngle);
        curveZ0.AddKey(afterEnd, sinToAngle);
        curveX2.AddKey(afterEnd, -sinToAngle);
        curveZ2.AddKey(afterEnd, cosToAngle);
        curveAngle.AddKey(afterEnd, angleRadian);
        curveAngle.AddKey(afterEnd + 0.001f, 0);

        SetCurveLinear(curveX0, 0, startTime);
        SetCurveLinear(curveZ0, 0, startTime);
        SetCurveLinear(curveX2, 0, startTime);
        SetCurveLinear(curveZ2, 0, startTime);
        SetCurveLinear(curveX0, endTime, afterEnd);
        SetCurveLinear(curveZ0, endTime, afterEnd);
        SetCurveLinear(curveX2, endTime, afterEnd);
        SetCurveLinear(curveZ2, endTime, afterEnd);
        SetCurveLinear(curveAngle, 0, afterEnd);

        TransformInterval interval = new TransformInterval(startTime, afterEnd, "rotateByY");
        RemoveDuplicateIntervals(interval);
        _transformIntervals.AddLast(interval);
    }

    /// 2D Translation along X
    /// 
    /// <param name="startX"></param> The starting value for z0
    /// <param name="endX"></param> The ending value for z0
    /// <param name="animDuration"></param> Duration of animation
    /// <param name="iniWaitTime"></param>  Wait time before animation begins
    private void TranslateX(float startX, float endX, float animDuration, float iniWaitTime)
    {
        var anim = GetAnimationDuration(animDuration, iniWaitTime);
        var startTime = anim.StartTime;
        var endTime = anim.EndTime;

        curveZ0.AddKey(startTime, startX);
        curveZ0.AddKey(endTime, endX);
        SetCurveLinear(curveZ0, startTime, endTime);

        TransformInterval interval = new TransformInterval(startTime, endTime, "none");
        _transformIntervals.AddLast(interval);
    }

    /// 2D Translation along Y
    /// 
    /// <param name="startY"></param> The starting value for z1
    /// <param name="endY"></param> The ending value for z1
    /// <param name="animDuration"></param> Duration of animation
    /// <param name="iniWaitTime"></param>  Wait time before animation begins
    private void TranslateY(float startY, float endY, float animDuration, float iniWaitTime)
    {
        var anim = GetAnimationDuration(animDuration, iniWaitTime);
        var startTime = anim.StartTime;
        var endTime = anim.EndTime;

        curveZ1.AddKey(startTime, startY);
        curveZ1.AddKey(endTime, endY);
        SetCurveLinear(curveZ1, startTime, endTime);

        TransformInterval interval = new TransformInterval(startTime, endTime, "none");
        _transformIntervals.AddLast(interval);
    }


    ///////////////////////////////////////////////////////////////////////////////////////
    /// HELPER FUNCTIONS
    ///////////////////////////////////////////////////////////////////////////////////////

    // Reset states after pressing "Space"
    private void Reset()
    {
        _animationPlaying = true;
        _animationTime = 0f;
        _maxAnimationTime =
            GetLastKeyTime(); // Get max animation time by getting the time of the last key from all curves
        _transformIntervalsCopy = new LinkedList<TransformInterval>(_transformIntervals);

        // Reset the animation states
        AnimatorCustomCube.Play("New State");
        transitionCanvas.Play("New State");
        transition.Play("New State");
        if (SceneManager.GetActiveScene().name.StartsWith("Scene2"))
        {
            for (var i = 0; i < 5; i++)
            {
                _faceMeshList[i].vertices = _faceVerticesOriginalList[i];
            }
        }
    }

    private String UsingRotation()
    {
        var usingRotation = "none";
        if (_transformIntervalsCopy.Count > 0)
        {
            TransformInterval interval = _transformIntervalsCopy.First.Value;
            if (_animationTime <= interval.EndTime)
            {
                usingRotation = _animationTime >= interval.StartTime ? interval.RotationType : "none";
            }
            else
            {
                _transformIntervalsCopy.RemoveFirst();
            }
        }

        return usingRotation;
    }

    IEnumerator LoadScene(string sceneName)
    {
        transitionCanvas.Play("Fade_Start", 1);
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(sceneName);
        
    }

    private void DisableFrontFaceCube()
    {
        var tris = _cubeMesh.triangles;
        for (int i = 4; i < 6; i++)
        {
            tris[i * 3] = 0;
            tris[i * 3 + 1] = 0;
            tris[i * 3 + 2] = 0;
        }

        _cubeMesh.triangles = tris;
    }

    private Vector3[] AddZPosition(Vector3[] face2MeshVertices, float z2)
    {
        for (var i = 0; i < face2MeshVertices.Length; i++)
        {
            face2MeshVertices[i] = new Vector3(face2MeshVertices[i].x, face2MeshVertices[i].y, z2);
        }

        return face2MeshVertices;
    }

    private void DisplayOtherPlanes(bool display)
    {
        if (display)
        {
            for (var i = 1; i < 5; i++)
            {
                StartCoroutine(FadeInMaterial(1.5f, _planeList[i]));
            }
        }
        else
        {
            for (var i = 1; i < 5; i++)
            {
                StartCoroutine(FadeOutMaterial(0.5f, _planeList[i]));
            }
        }
    }

    private void ResetAlpha(GameObject o, float alphaVal)
    {
        var mat = o.transform.GetComponent<Renderer>().material;
        Color matColor = mat.color;
        mat.color = new Color(matColor.r, matColor.g, matColor.b, alphaVal);
    }

    IEnumerator FadeOutMaterial(float fadeSpeed, GameObject objectToFade)
    {
        Renderer rend = objectToFade.transform.GetComponent<Renderer>();
        Color matColor = rend.material.color;
        float alphaValue = rend.material.color.a;
        while (rend.material.color.a > 0f)
        {
            alphaValue -= Time.deltaTime / fadeSpeed;
            rend.material.color = new Color(matColor.r, matColor.g, matColor.b, alphaValue);
            yield return null;
        }

        rend.material.color = new Color(matColor.r, matColor.g, matColor.b, 0f);
        objectToFade.SetActive(false);
    }

    IEnumerator FadeInMaterial(float fadeSpeed, GameObject objectToFade)
    {
        objectToFade.SetActive(true);
        Renderer rend = objectToFade.transform.GetComponent<Renderer>();
        Color matColor = rend.material.color;
        float alphaValue = rend.material.color.a;
        while (rend.material.color.a < 1f)
        {
            alphaValue += Time.deltaTime / fadeSpeed;
            rend.material.color = new Color(matColor.r, matColor.g, matColor.b, alphaValue);
            yield return null;
        }

        rend.material.color = new Color(matColor.r, matColor.g, matColor.b, 1f);
    }

    private AnimationDuration GetAnimationDuration(float animDuration, float initialWaitTime)
    {
        var lastKeyTime = GetLastKeyTime();
        var startTime = lastKeyTime + initialWaitTime;
        var endTime = startTime + animDuration;

        return new AnimationDuration(startTime, endTime);
    }

    private float GetLastKeyTime()
    {
        var lastKeyTime = 0f;
        // Get max animation time by getting the time of the last key from all curves
        foreach (var curve in _curves)
        {
            if (curve.length != 0) lastKeyTime = Max(lastKeyTime, curve.keys[curve.length - 1].time);
        }

        return lastKeyTime;
    }


    /// <summary>
    /// Transform the cube based on 2D matrix values x0, x1, y0, y1
    /// </summary>
    private void TransformMesh()
    {
        var updatedVertices = new Vector3[_cubeVertices.Length];
        var x = new Vector3(x0, x1, x2);
        var y = new Vector3(y0, y1, y2);
        var z = new Vector3(z0, z1, z2);
        var updatedVertices2 = new Vector3[_face1Vertices.Length];

        for (var i = 0; i < _cubeVertices.Length; i++)
        {
            updatedVertices[i] = TransformPoint(_cubeVertices[i], x, y, z);
        }


        // 2,0,3,1  or 3,2,1,0
        // 4,5,20,19 or 20, 4 19,5,  19, 20, 5,4
        // 5,19, 4,20

        if (SceneManager.GetActiveScene().name.StartsWith("Scene1") || !plane2.activeSelf)
        {
            int[] pos = new int[4];
            if (SceneManager.GetActiveScene().name.StartsWith("Scene1"))
            {
                pos[0] = 4;
                pos[1] = 5;
                pos[2] = 20;
                pos[3] = 19;
            }
            else
            {
                pos[0] = 19;
                pos[1] = 20;
                pos[2] = 5;
                pos[3] = 4;
            }

            var pt = cube.transform.TransformPoint(updatedVertices[pos[0]]);
            updatedVertices2[0] = plane1.transform.InverseTransformPoint(pt);

            pt = cube.transform.TransformPoint(updatedVertices[pos[1]]);
            updatedVertices2[1] = plane1.transform.InverseTransformPoint(pt);

            pt = cube.transform.TransformPoint(updatedVertices[pos[2]]);
            updatedVertices2[2] = plane1.transform.InverseTransformPoint(pt);

            pt = cube.transform.TransformPoint(updatedVertices[pos[3]]);
            updatedVertices2[3] = plane1.transform.InverseTransformPoint(pt);

            _cubeMesh.vertices = updatedVertices;
            _face1Mesh.vertices = updatedVertices2;
        }
        else
        {
            var a = new Vector3[_face1Vertices.Length];
            var b = new Vector3[_face1Vertices.Length];
            var c = new Vector3[_face1Vertices.Length];
            var d = new Vector3[_face1Vertices.Length];
            var e = new Vector3[_face1Vertices.Length];

            for (var i = 0; i < _face1Vertices.Length; i++)
            {
                a[i] = TransformPoint(_face1Vertices[i], x, y, z);
                b[i] = TransformPoint(_face2Vertices[i], x, y, z);
                c[i] = TransformPoint(_face3Vertices[i], x, y, z);
                d[i] = TransformPoint(_face4Vertices[i], x, y, z);
                e[i] = TransformPoint(_face5Vertices[i], x, y, z);
            }

            _face1Mesh.vertices = e;
            _face2Mesh.vertices = d;
            _face3Mesh.vertices = c;
            _face4Mesh.vertices = b;
            _face5Mesh.vertices = a;
        }
    }

    private static Vector3 TransformPoint(Vector3 point, Vector3 x, Vector3 y, Vector3 z)
    {
        var newX = point.x * x.x + point.y * y.x + point.z * z.x;
        var newY = point.x * x.y + point.y * y.y + point.z * z.y;
        var newZ = point.x * x.z + point.y * y.z + point.z * z.z;
        return new Vector3(newX, newY, newZ);
    }


    /// <summary>
    /// Set the display texts of the 2D matrix
    /// </summary>
    /// <param name="field"></param> the input field (e.g. x0,x1,y0,y1)
    /// <param name="value"></param> value
    /// <param name="usingRotation"></param> whether the animation is using rotation or not (to display "sin" and "cos")
    private void SetText(TMP_InputField field, float value, string usingRotation)
    {
        field.pointSize = 13;
        if (field.name == "angle") angle = value;
        switch (field.name)
        {
            case "x0":
                x0 = value;
                break;
            case "x1":
                x1 = value;
                break;
            case "x2":
                x2 = value;
                break;
            case "y0":
                y0 = value;
                break;
            case "y1":
                y1 = value;
                break;
            case "y2":
                y2 = value;
                break;
            case "z0":
                z0 = value;
                break;
            case "z1":
                z1 = value;
                break;
            case "z2":
                z2 = value;
                break;
        }

        string textDisplay = $"{value:0.0}"; // value to display (1 decimal)
        if (usingRotation == "none")
        {
            field.text = textDisplay;
        }
        else
        {
            var formattedAngle = $"{Math.Round(angle, 2) / 3.14f:0.0}" + PiSymbol; // radians (1 decimal) + pi symbol
            switch (field.name)
            {
                case "x0":
                    field.text = usingRotation == "rotateByZ" || usingRotation == "rotateByY"
                        ? "cos(" + formattedAngle + ")"
                        : textDisplay;
                    break;
                case "x1":
                    field.text = usingRotation == "rotateByZ"
                        ? "sin(" + formattedAngle + ")"
                        : textDisplay;
                    break;
                case "x2":
                    field.text = usingRotation == "rotateByY"
                        ? "-sin(" + formattedAngle + ")"
                        : textDisplay;
                    break;
                case "y0":
                    field.text = usingRotation == "rotateByZ"
                        ? "-sin(" + formattedAngle + ")"
                        : textDisplay;
                    break;
                case "y1":
                    field.text = usingRotation == "rotateByZ" || usingRotation == "rotateByX"
                        ? "cos(" + formattedAngle + ")"
                        : textDisplay;
                    break;
                case "y2":
                    field.text = usingRotation == "rotateByX"
                        ? "sin(" + formattedAngle + ")"
                        : textDisplay;
                    break;

                case "z0":
                    field.text = usingRotation == "rotateByY"
                        ? "sin(" + formattedAngle + ")"
                        : textDisplay;
                    break;
                case "z1":
                    field.text = usingRotation == "rotateByX"
                        ? "-sin(" + formattedAngle + ")"
                        : textDisplay;
                    break;
                case "z2":
                    field.text = usingRotation == "rotateByY" || usingRotation == "rotateByX"
                        ? "cos(" + formattedAngle + ")"
                        : textDisplay;
                    break;
            }
        }
    }

    private static void SetCurveLinear(AnimationCurve curve, float startTime, float endTime)
    {
        for (int i = 0; i < curve.keys.Length; ++i)
        {
            // if (curve[i].time < startTime || curve[i].time > endTime) continue;
            float inTangent = 0;
            float outTangent = 0;
            bool inTangentSet = false;
            bool outTangentSet = false;
            Vector2 point1;
            Vector2 point2;
            Vector2 delta;
            Keyframe key = curve[i];

            if (i == 0)
            {
                inTangent = 0;
                inTangentSet = true;
            }

            if (i == curve.keys.Length - 1)
            {
                outTangent = 0;
                outTangentSet = true;
            }

            if (!inTangentSet)
            {
                point1.x = curve.keys[i - 1].time;
                point1.y = curve.keys[i - 1].value;
                point2.x = curve.keys[i].time;
                point2.y = curve.keys[i].value;

                delta = point2 - point1;

                inTangent = delta.y / delta.x;
            }

            if (!outTangentSet)
            {
                point1.x = curve.keys[i].time;
                point1.y = curve.keys[i].value;
                point2.x = curve.keys[i + 1].time;
                point2.y = curve.keys[i + 1].value;

                delta = point2 - point1;

                outTangent = delta.y / delta.x;
            }

            key.inTangent = inTangent;
            key.outTangent = outTangent;
            curve.MoveKey(i, key);
        }
    }

    // Merge rotation intervals in the list
    private LinkedList<TransformInterval> MergeRotationIntervals()
    {
        List<TransformInterval> curList = new List<TransformInterval>(_transformIntervals);
        List<TransformInterval> mergedList = new List<TransformInterval>();

        for (int i = 0; i < curList.Count; i++)
        {
            int lastIdxMergeList = mergedList.Count - 1;
            // last element in list is Rotation, merge
            if (mergedList.Count > 0 && curList[i].RotationType != "none" &&
                mergedList[lastIdxMergeList].RotationType != "none")
            {
                TransformInterval lastRotationInterval = mergedList[lastIdxMergeList];

                if (curList[i].RotationType == mergedList[lastIdxMergeList].RotationType)
                {
                    float newEndTime = Max(mergedList[lastIdxMergeList].EndTime, curList[i].EndTime);
                    lastRotationInterval.EndTime = newEndTime;
                    mergedList.RemoveAt(lastIdxMergeList);
                    mergedList.Add(lastRotationInterval);
                }
                else
                {
                    float newEndTime = curList[i].StartTime;
                    lastRotationInterval.EndTime = newEndTime;
                    mergedList.RemoveAt(lastIdxMergeList);
                    mergedList.Add(lastRotationInterval);
                    mergedList.Add(curList[i]);
                }
            }
            else
            {
                mergedList.Add(curList[i]);
            }
        }

        return new LinkedList<TransformInterval>(mergedList);
    }

    // Remove the previous duplicate intervals with the same [startTime,endTime]
    // since the current interval is using rotation
    private void RemoveDuplicateIntervals(TransformInterval interval)
    {
        while (_transformIntervals.Count > 0 &&
               Math.Abs(_transformIntervals.Last.Value.StartTime - interval.StartTime) < float.Epsilon)
        {
            if (_transformIntervals.Last.Value.RotationType == "none")
            {
                _transformIntervals.RemoveLast();
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////
    /// STRUCTS 
    ///////////////////////////////////////////////////////////////////////////////////////
    readonly struct AnimationDuration
    {
        public float StartTime { get; }
        public float EndTime { get; }

        public AnimationDuration(float start, float end)
        {
            StartTime = start;
            EndTime = end;
        }
    }

    struct TransformInterval
    {
        public float StartTime { get; }
        public float EndTime { get; set; }
        public string RotationType { get; } // rotation or not?

        public TransformInterval(float start, float end, string type)
        {
            StartTime = start;
            EndTime = end;
            RotationType = type;
        }
    }
}