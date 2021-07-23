using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PIPE_Valve_Console_Client
{
public class RemotePartMaster : MonoBehaviour
{

    public struct TransformData
    {
        public Vector3 LocalPosition;
        public Vector3 LocalEulerRotation;
        public Vector3 LocalScale;

        public TransformData(Transform transform)
        {
            LocalPosition = transform.localPosition;
            LocalEulerRotation = transform.localEulerAngles;
            LocalScale = transform.localScale;
        }

        public void ApplyTo(Transform transform)
        {
            transform.localPosition = LocalPosition;
            transform.localEulerAngles = LocalEulerRotation;
            transform.localScale = LocalScale;
        }
    }

    public Dictionary<int, GameObject> partList;
    public Dictionary<int, TransformData> origTrans;
    public bool isDone = false;

    public int frame = 0;
    public int bars = 1;
    public int forks = 2;
    public int stem = 3;
    public int frontSpokes = 4;
    public int rearSpokes = 5;
    public int frontRim = 6;
    public int rearRim = 7;
    public int frontHub = 8;
    public int rearHub = 9;
    public int frontTire = 10;
    public int rearTire = 11;
    public int frontNipples = 12;
    public int rearNipples = 13;
    public int frontWheel = 14;
    public int rearWheel = 15;
    public int leftGrip = 16;
    public int rightGrip = 17;
    public int leftPedal = 18;
    public int rightPedal = 19;
    public int leftPedalAxle = 20;
    public int rightPedalAxle = 21;
    public int leftPedalCap = 22;
    public int rightPedalCap = 23;
    public int stemBolts = 24;
    public int leftCrankBolt = 25;
    public int rightCrankBolt = 26;
    public int leftAnchor = 27;
    public int rightAnchor = 28;
    public int barEnds = 29;
    public int headSet = 30;
    public int headSetSpacers = 31;
    public int frontPegs = 32;
    public int rearPegs = 33;
    public int sprocket = 34;
    public int bottomBracket = 35;
    public int seat = 36;
    public int seatPost = 37;
    public int seatPostAnchor = 38;
    public int seatClamp = 39;
    public int seatClampBolt = 40;
    public int leftCrank = 41;
    public int rightCrank = 42;
    public int chain = 43;
    public int barsJ = 44;
    public int frameJ = 45;
    public int frontWheelCol = 46;
    public int rearWheelCol = 47;
    public int frontAcc = 48;
    public int rearAcc = 49;
    public int barAcc = 50;
    public int frameAcc = 51;
    public int frontHubG = 52;
    public int rearHubG = 53;

    GameObject accFront;
    GameObject accRear;
    GameObject barAccessory;
    GameObject frameAccesory;
    GameObject frontHubGuard;
    GameObject rearHubGuard;

       
   
    void Awake()
    {
        origTrans = new Dictionary<int, TransformData>();
    }

    public void AccessoriesSetup()
    {
        accFront = new GameObject("FrontAccessory");
        accFront.AddComponent<MeshFilter>();
        accFront.GetComponent<MeshFilter>().mesh = CustomMeshManager.instance.accessoryMeshes[0];
        accFront.AddComponent<MeshRenderer>();
        accFront.GetComponent<MeshRenderer>().material = CustomMeshManager.instance.accMats[0];
        accFront = Instantiate(accFront, GetPart(frontSpokes).transform);

        accRear = new GameObject("RearAccessory");
        accRear.AddComponent<MeshFilter>();
        accRear.GetComponent<MeshFilter>().mesh = CustomMeshManager.instance.accessoryMeshes[0];
        accRear.AddComponent<MeshRenderer>();
        accRear.GetComponent<MeshRenderer>().material = CustomMeshManager.instance.accMats[0];
        accRear = Instantiate(accRear, GetPart(rearSpokes).transform);

        barAccessory = new GameObject("barAccessory");
        barAccessory.AddComponent<MeshFilter>();
        barAccessory.GetComponent<MeshFilter>().mesh = CustomMeshManager.instance.accessoryMeshes[0];
        barAccessory.AddComponent<MeshRenderer>();
        barAccessory.GetComponent<MeshRenderer>().material = MaterialManager.instance.defaultMat;
        barAccessory = Instantiate(barAccessory, GetPart(bars).transform);

        frameAccesory = new GameObject("frameAccesory");
        frameAccesory.AddComponent<MeshFilter>();
        frameAccesory.GetComponent<MeshFilter>().mesh = CustomMeshManager.instance.accessoryMeshes[0];
        frameAccesory.AddComponent<MeshRenderer>();
        frameAccesory.GetComponent<MeshRenderer>().material = MaterialManager.instance.defaultMat;
        frameAccesory = Instantiate(frameAccesory, GetPart(frame).transform);

        frontHubGuard = new GameObject("frontHubGuard");
        frontHubGuard.AddComponent<MeshFilter>();
        frontHubGuard.GetComponent<MeshFilter>().mesh = CustomMeshManager.instance.accessoryMeshes[0];
        frontHubGuard.AddComponent<MeshRenderer>();
        frontHubGuard.GetComponent<MeshRenderer>().material = MaterialManager.instance.defaultMat;
        frontHubGuard = Instantiate(frontHubGuard, GetPart(frontPegs).transform);

        rearHubGuard = new GameObject("rearHubGuard");
        rearHubGuard.AddComponent<MeshFilter>();
        rearHubGuard.GetComponent<MeshFilter>().mesh = CustomMeshManager.instance.accessoryMeshes[0];
        rearHubGuard.AddComponent<MeshRenderer>();
        rearHubGuard.GetComponent<MeshRenderer>().material = MaterialManager.instance.defaultMat;
        rearHubGuard = Instantiate(rearHubGuard, GetPart(rearPegs).transform);


       
        partList.Add(frontAcc, accFront);
        partList.Add(rearAcc, accRear);
        partList.Add(barAcc, barAccessory);
        partList.Add(frameAcc, frameAccesory);
        partList.Add(frontHubG, frontHubGuard);
        partList.Add(rearHubG, rearHubGuard);

        SetMaterial(forks, MaterialManager.instance.defaultMat);
        SetMaterial(rightCrank, MaterialManager.instance.defaultMat);
        SetMaterial(leftCrank, MaterialManager.instance.defaultMat);

        foreach (KeyValuePair<int, GameObject> pair in partList)
        {
            origTrans.Add(pair.Key, new TransformData(pair.Value.transform));
        }
    }


        public void SetMaterialData(int key, float glossiness, float glossMapScale)
        {
            Material material = GetMaterial(key);
            if (material == null)
                return;
            material.SetFloat("_Glossiness", glossiness);
            material.SetFloat("_GlossMapScale", glossMapScale);
            SetMaterial(key, material);
        }


        public void InitPartList(GameObject bmx)
        {
        string errorPath = Application.dataPath + "//GarageContent/GarageErrorLog.txt";
        List<int> ints = new List<int>();
        try
        {
            partList = new Dictionary<int, GameObject>();
            Transform[] barsJoint = bmx.transform.FindDeepChild("BMX:Bars_Joint").gameObject.GetComponentsInChildren<Transform>(true);
            Transform[] frameJoint = bmx.transform.FindDeepChild("BMX:Frame_Joint").gameObject.GetComponentsInChildren<Transform>(true);

            foreach (Transform t in barsJoint)
            {
                    Debug.Log(t.gameObject.name);
                switch (t.gameObject.name)
                {
                    case "Pegs Mesh":
                            ints.Add(frontPegs);
                        partList.Add(frontPegs, t.gameObject);
                        break;
                    case "Nipples Mesh":
                            ints.Add(frontNipples);
                        partList.Add(frontNipples, t.gameObject);
                        break;
                    case "Spokes Mesh":
                            ints.Add(frontSpokes);
                        partList.Add(frontSpokes, t.gameObject);
                        break;
                    case "Rim Mesh":
                            ints.Add(frontRim);
                        partList.Add(frontRim, t.gameObject);
                        break;
                    case "Hub Mesh":
                            ints.Add(frontHub);
                        partList.Add(frontHub, t.gameObject);
                        break;
                    case "Tire Mesh":
                            ints.Add(frontTire);
                        partList.Add(frontTire, t.gameObject);
                        break;
                    case "Left Anchor":
                            ints.Add(leftAnchor);
                        partList.Add(leftAnchor, t.gameObject);
                        break;
                    case "Right Anchor":
                            ints.Add(rightAnchor);
                        partList.Add(rightAnchor, t.gameObject);
                        break;
                    case "Stem Bolts Mesh":
                            ints.Add(stemBolts);
                        partList.Add(stemBolts, t.gameObject);
                        break;
                    case "Bars Mesh":
                            ints.Add(bars);
                        partList.Add(bars, t.gameObject);
                        break;
                    case "BMX:Wheel":
                            ints.Add(frontWheel);
                        partList.Add(frontWheel, t.gameObject);
                        break;
                    case "Bar Ends Mesh":
                            ints.Add(barEnds);
                        partList.Add(barEnds, t.gameObject);
                        break;
                    case "Left Grip":
                            ints.Add(leftGrip);
                        Transform[] tran1 = t.gameObject.GetComponentsInChildren<Transform>();
                        if (tran1[0].gameObject.name == "Left Grip Mesh") partList.Add(leftGrip, tran1[0].gameObject);
                        else partList.Add(leftGrip, tran1[1].gameObject);
                        break;
                    case "Right Grip":
                            ints.Add(rightGrip);
                        Transform[] tran2 = t.gameObject.GetComponentsInChildren<Transform>();
                        if (tran2[0].gameObject.name == "Right Grip Mesh") partList.Add(rightGrip, tran2[0].gameObject);
                        else partList.Add(rightGrip, tran2[1].gameObject);
                        break;
                    case "Forks Mesh":
                            ints.Add(forks);
                        partList.Add(forks, t.gameObject);
                        break;
                    case "Headset Mesh":
                            ints.Add(headSet);
                        partList.Add(headSet, t.gameObject);
                        break;
                    case "Stem Mesh":
                            ints.Add(stem);
                        if (!(t.gameObject.GetComponent<MeshFilter>() == null)) partList.Add(stem, t.gameObject);
                        break;
                    case "Headset Spacers Mesh":
                            ints.Add(headSetSpacers);
                        partList.Add(headSetSpacers, t.gameObject);
                        break;
                    default:
                        break;
                }

            }

            foreach (Transform t in frameJoint)
            {
                    Debug.Log(t.gameObject.name);
                switch (t.gameObject.name)
                {
                    case "Pegs Mesh":
                            ints.Add(rearPegs);
                        partList.Add(rearPegs, t.gameObject);
                        break;
                    case "Nipples Mesh":
                            ints.Add(rearNipples);
                        partList.Add(rearNipples, t.gameObject);
                        break;
                    case "BMX:Wheel 1":
                            ints.Add(rearWheel);
                        partList.Add(rearWheel, t.gameObject);
                        break;
                    case "Spokes Mesh":
                            ints.Add(rearSpokes);
                        partList.Add(rearSpokes, t.gameObject);
                        break;
                    case "Rim Mesh":
                            ints.Add(rearRim);
                        partList.Add(rearRim, t.gameObject);
                        break;
                    case "Hub Mesh":
                            ints.Add(rearHub);
                        partList.Add(rearHub, t.gameObject);
                        break;
                    case "Tire Mesh":
                            ints.Add(rearTire);
                        partList.Add(rearTire, t.gameObject);
                        break;
                    case "Seat Post":
                            ints.Add(seatPost);
                        partList.Add(seatPost, t.gameObject);
                        break;
                    case "Seat Post Anchor":
                            ints.Add(seatPostAnchor);
                        partList.Add(seatPostAnchor, t.gameObject);
                        break;
                    case "Seat Mesh":
                            ints.Add(seat);
                        partList.Add(seat, t.gameObject);
                        break;
                    case "Seat Clamp Mesh":
                            ints.Add(seatClamp);
                        partList.Add(seatClamp, t.gameObject);
                        break;
                    case "Seat_Clamp_Bolt":
                            ints.Add(seatClampBolt);
                        partList.Add(seatClampBolt, t.gameObject);
                        break;
                    case "Chain Mesh":
                            ints.Add(chain);
                        partList.Add(chain, t.gameObject);
                        break;
                    case "Frame Mesh":
                            ints.Add(frame);
                            if(!partList.TryGetValue(0, out GameObject obj)) partList.Add(frame, t.gameObject);
                        break;
                    case "BB Mesh":
                            ints.Add(bottomBracket);
                        partList.Add(bottomBracket, t.gameObject);
                        break;
                    case "Right Crank Arm Mesh":
                            ints.Add(rightCrank);
                        partList.Add(rightCrank, t.gameObject);
                        break;
                    case "Left Crank Arm Mesh":
                            ints.Add(leftCrank);
                        partList.Add(leftCrank, t.gameObject);
                        break;
                    case "Sprocket Mesh":
                            ints.Add(sprocket);
                        partList.Add(sprocket, t.gameObject);
                        break;
                    case "Right_Crankarm_Cap":
                            ints.Add(rightCrankBolt);
                        partList.Add(rightCrankBolt, t.gameObject);
                        break;
                    case "Left_Crankarm_Cap":
                            ints.Add(leftCrankBolt);
                        partList.Add(leftCrankBolt, t.gameObject);
                        break;
                    case "BMX:LeftPedal_Joint":
                            ints.Add(leftPedal);
                            ints.Add(leftPedalAxle);
                            ints.Add(leftPedalCap);
                        Transform[] tran1 = t.gameObject.GetComponentsInChildren<Transform>();
                        foreach (Transform tr in tran1)
                        {
                            if (tr.gameObject.name.Equals("Pedal Mesh"))
                                partList.Add(leftPedal, tr.gameObject);
                            if (tr.gameObject.name.Equals("Pedal_01_axis"))
                                partList.Add(leftPedalAxle, tr.gameObject);
                            if (tr.gameObject.name.Equals("Pedal Cap Mesh"))
                                partList.Add(leftPedalCap, tr.gameObject);

                        }
                        break;
                    case "BMX:RightPedal_Joint":
                            ints.Add(rightPedal);
                            ints.Add(rightPedalAxle);
                            ints.Add(rightPedalCap);
                        Transform[] tran2 = t.gameObject.GetComponentsInChildren<Transform>();
                        foreach (Transform tr in tran2)
                        {
                            if (tr.gameObject.name.Equals("Pedal Mesh"))
                                partList.Add(rightPedal, tr.gameObject);
                            if (tr.gameObject.name.Equals("Pedal_01_axis"))
                                partList.Add(rightPedalAxle, tr.gameObject);
                            if (tr.gameObject.name.Equals("Pedal Cap Mesh"))
                                partList.Add(rightPedalCap, tr.gameObject);

                        }
                        break;
                    default:
                        break;
                }
            }
                ints.Add(barsJ);
                ints.Add(frameJ);
                ints.Add(frontWheelCol);
                ints.Add(rearWheelCol);
                partList.Add(barsJ, bmx.transform.FindDeepChild("BMX:Bars_Joint").gameObject);
            partList.Add(frameJ, bmx.transform.FindDeepChild("BMX:Frame_Joint").gameObject);
            partList.Add(frontWheelCol, bmx.transform.FindDeepChild("FrontWheelCollider").gameObject);
            partList.Add(rearWheelCol, bmx.transform.FindDeepChild("BackWheelCollider").gameObject);

        }
        catch (Exception e)
        {
            File.AppendAllText(errorPath, "\n" + DateTime.Now + "\nRANDOM ERRORS: " + "Error while initializing part list in PartMaster.cs. " + e.Message + e.StackTrace);
               foreach(int i in ints)
                {
                    Debug.Log(i);
                }

            Debug.Log("Error while initializing part list in PartMaster.cs. " + e.Message + e.StackTrace);
        }
        isDone = true;
        }

    /// <summary>
    ///  Get a bike part's GameObject
    /// </summary>
    /// <param name="key"> The part number associated with the bike part </param>
    /// <returns> The GameObject at partList[key] </returns>
    public GameObject GetPart(int key)
    {
        if (!partList.ContainsKey(key))
        {
            Debug.Log("Key not found in part list at GetPart() method");
            return null;
        }
        return partList[key];
    }

    /// <summary>
    /// Get a bike part's mesh
    /// </summary>
    /// <param name="key"> The part number associated with the bike part </param>
    /// <returns> The mesh at partList[key] </returns>
    public Mesh GetMesh(int key)
    {
        if (!partList.ContainsKey(key) || partList[key].GetComponent<MeshFilter>() == null)
        {
            Debug.Log("Key not found in part list at GetMesh() method");
            return null;
        }
        return partList[key].GetComponent<MeshFilter>().mesh;
    }

    /// <summary>
    /// Change the mesh of a bike part
    /// </summary>
    /// <param name="key"> The part number associated with the bike part </param>
    /// <param name="mesh"> The new mesh to change to </param>
    public void SetMesh(int key, Mesh mesh)
    {
        if (!partList.ContainsKey(key) || partList[key].GetComponent<MeshFilter>() == null)
        {
            Debug.Log("Key not found in part list at SetMesh() method");
            return;
        }
        partList[key].GetComponent<MeshFilter>().mesh = mesh;

    }

    /// <summary>
    /// Get the bike part's main material
    /// </summary>
    /// <param name="key"> The part number associated with the bike part </param>
    /// <returns> The Material at partList[key]</returns>
    public Material GetMaterial(int key)
    {
        if (!partList.ContainsKey(key) || partList[key].GetComponent<MeshRenderer>() == null)
        {
            Debug.Log("Key not found in part list at GetMaterial() method");
            return null;
        }
        return partList[key].GetComponent<MeshRenderer>().material;
    }

    /// <summary>
    /// Change the material of a bike part
    /// </summary>
    /// <param name="key"> The part number associated with the bike part </param>
    /// <param name="mat"> The material to change to </param>
    public void SetMaterial(int key, Material mat)
    {
        if (!partList.ContainsKey(key) || partList[key].GetComponent<MeshRenderer>() == null)
        {
            Debug.Log("Key not found in part list at SetMaterial() method");
            return;
        }
        Material[] mats = partList[key].GetComponent<MeshRenderer>().materials;
        if (mats.Length > 1)
        {
            Debug.Log("More than one material");
            mats[0] = mat;
        }
        partList[key].GetComponent<Renderer>().material = mat;
    }

    /// <summary>
    /// Change a specific material of a bike part that uses more than one material
    /// </summary>
    /// <param name="index"> index of the list of materials associated with the bike part</param>
    /// <param name="key"> The part number associated with the bike part </param>
    /// <param name="mat"> The material to change to </param>
    public void SetMaterial(int index, int key, Material mat)
    {
        if (!partList.ContainsKey(key) || partList[key].GetComponent<MeshRenderer>() == null)
        {
            Debug.Log("Key not found in part list at SetMaterial(index, key, mat) method");
            return;
        }
        partList[key].GetComponent<MeshRenderer>().materials[index] = mat;
    }

    /// <summary>
    /// Get all the materials assigned to a bike part
    /// </summary>
    /// <param name="key"> The part number associated with the bike part </param>
    /// <returns> The material array at partList[key] </returns>
    public Material[] GetMaterials(int key)
    {
        if (!partList.ContainsKey(key) || partList[key].GetComponent<MeshRenderer>() == null)
        {
            Debug.Log("Key not found in part list at GetMaterials() method");
            return null;
        }
        return partList[key].GetComponent<MeshRenderer>().materials;
    }

    public void SetMaterials(int key, Material[] mats)
    {
        if (!partList.ContainsKey(key) || partList[key].GetComponent<MeshRenderer>() == null)
        {
            Debug.Log("Key not found in part list at SetMaterials() method");
        }

        partList[key].GetComponent<MeshRenderer>().materials = mats;
    }

    /// <summary>
    /// Experimental method to add collision to a bike part
    /// </summary>
    /// <param name="key"></param>
    public void AddCollision(int key)
    {
        if (!partList.ContainsKey(key))
        {
            Debug.Log("Key not found in part list at AddCollision() method");
            return;
        }
        partList[key].AddComponent<MeshCollider>();
        partList[key].GetComponent<MeshCollider>().sharedMesh = GetMesh(key);
        partList[key].layer = 0;
    }

    public void MovePart(int key, string axis, float pos)
    {
        Transform part = GetPart(key).transform;
        Debug.Log("Moving " + part.gameObject.name);
        if (axis.Equals("x"))
            part.localPosition = new Vector3(part.localPosition.x + pos, part.localPosition.y, part.localPosition.z);
        if (axis.Equals("y"))
            part.localPosition = new Vector3(part.localPosition.x, part.localPosition.y + pos, part.localPosition.z);
        if (axis.Equals("z"))
            part.localPosition = new Vector3(part.localPosition.x, part.localPosition.y, part.localPosition.z + pos);
    }

    public Vector3 GetPosition(int key)
    {
        return GetPart(key).transform.localPosition;
    }

    public void SetPosition(int key, Vector3 pos)
    {
        GetPart(key).transform.localPosition = pos;
    }

    public void SetPartsVisible()
    {
        foreach (int key in ColourSetter.instance.GetActivePartList())
        {
            GameObject obj = GetPart(key);
            obj.SetActive(!obj.activeSelf);
        }
    }

    public void SetPartVisible(int key, bool isVisible)
    {
        GameObject obj = GetPart(key);
        obj.SetActive(isVisible);
    }

    public bool GetPartVisibe(int key)
    {
        GameObject obj = GetPart(key);
        return obj.activeInHierarchy;
    }

    public void Scale(bool positive)
    {
        foreach (int key in ColourSetter.instance.GetActivePartList())
        {
            GameObject obj = GetPart(key);
            if (positive)
                obj.transform.localScale += new Vector3(0.1f, 0.1f, 0.1f);
            else
                obj.transform.localScale -= new Vector3(0.1f, 0.1f, 0.1f);
        }
    }

    public Vector3 GetScale(int key)
    {
        return GetPart(key).transform.localScale;
    }

    public void SetScale(int key, Vector3 scale)
    {
        GameObject obj = GetPart(key);
        obj.transform.localScale = scale;
    }

    public void DuplicatePart()
    {
        foreach (int key in ColourSetter.instance.GetActivePartList())
        {
            GameObject obj = GetPart(key);
            obj = Instantiate(obj, obj.transform.parent);
            //obj.transform.localPosition = new Vector3(0,0,0);
        }
    }

    public void ResetTransforms()
    {
        foreach (int key in ColourSetter.instance.GetActivePartList())
        {
            if (!origTrans.ContainsKey(key))
            {
                Debug.Log("Original transform not found for part number " + key);
                return;
            }
            GameObject obj = GetPart(key);
            TransformData td = origTrans[key];
            td.ApplyTo(obj.transform);
        }
    }

        public void SetColor(int key, Color c)
        {
            switch (key)
            {
                case -1:
                    GetMaterials(frontTire)[1].color = c;
                    return;
                case -2:
                    GetMaterials(rearTire)[1].color = c;
                    return;
            }
            if (key >= 0 && key < 54)
                GetMaterial(key).color = c;
        }
    }

}

