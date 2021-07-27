using PIPE_Valve_Console_Client;
using System;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class RemoteLoadManager : MonoBehaviour
{
    public static RemoteLoadManager instance;

    void Awake()
    {
        instance = this;
    }


    public void Load(RemotePlayer player, SaveList loadList)
    {
        try
        {
            RemotePartMaster pm = player.partMaster;
            SetOriginalTextures(pm);
            LoadBrakes(player, loadList);
            LoadHeightsAngles(player, loadList);
            LoadDriveSide(player, loadList);
            LoadMaterials(player, loadList);
            LoadTireTread(pm, loadList);
            foreach (PartColor p in loadList.partColors)
            {
                pm.SetColor(p.partNum, new Color(p.r, p.g, p.b, p.a));
            }
            LoadMeshes(player, loadList);
            LoadTextures(player, loadList);
            LoadPartPositions(pm, loadList);

            // Quick fix for weird normal map issue on left crank arm
            Material m = pm.GetMaterial(pm.rightCrank);
            pm.GetPart(pm.leftCrank).GetComponent<MeshRenderer>().material = m;
            Resources.UnloadUnusedAssets();
        }
        catch (Exception e)
        {
            Debug.Log(e.Message + "\n " + e.StackTrace + "\n " + e.Source + "\n ");
        }

    }

    public void LoadPartPositions(RemotePartMaster pm, SaveList loadList)
    {
        if (loadList.partPositions.Count == 0)
            return;
        foreach (PartPosition partPos in loadList.partPositions)
        {
            pm.SetPosition(partPos.partNum, new Vector3(partPos.x, partPos.y, partPos.z));
            pm.SetScale(partPos.partNum, new Vector3(partPos.scaleX, partPos.scaleY, partPos.scaleZ));
            pm.SetPartVisible(partPos.partNum, partPos.isVisible);
        }
    }

    private void LoadTextures(RemotePlayer player, SaveList loadList)
    {
        try
        {
            foreach (PartTexture p in loadList.partTextures)
            {
                Debug.Log("Loading Texture: " + p.url);

                // locally stored
                if (p.url.ToLower().Contains("frostypgamemanager"))
                {
                    int lastslash = p.url.LastIndexOf("/");
                    string name = p.url.Remove(0, lastslash + 1);


                    if (FileSyncing.CheckForFile(name))
                    {
                        Texture2D tex = GameManager.GetTexture(name);


                        if (!p.metallic && !p.normal)
                            TexHelper(player, tex, null, p.partNum, "_MainTex", "", p.url);
                        else if (p.normal)
                            TexHelper(player, ConvertToNormalMap(tex), null, p.partNum, "_BumpMap", "_NORMALMAP", p.url);
                        else
                            TexHelper(player, tex, null, p.partNum, "_MetallicGlossMap", "_METALLICGLOSSMAP", p.url);
                    }
                    else
                    {
                        // waitingrequest
                        FileSyncing.AddToRequestable((int)FileTypeByNum.Garage, name, player.id);



                    }

                   
                }
                else
                {

                if (!p.url.Equals("") && !p.url.Equals("."))
                {
                    if (!p.metallic && !p.normal)
                        SetTexture(player, p.partNum, p.url);
                    else if (p.normal)
                        SetNormal(player, p.partNum, p.url);
                    else
                        SetMetallic(player, p.partNum, p.url);
                }
                else if (p.url.Equals("."))
                {
                    if (!p.metallic && !p.normal)
                        FindObjectOfType<TextureManager>().RemoveTexture(p.partNum);
                    else if (p.normal)
                        FindObjectOfType<TextureManager>().RemoveNormal(p.partNum);
                    else
                        FindObjectOfType<TextureManager>().RemoveMetallic(p.partNum);
                }

                }



            }
        }
        catch (Exception e)
        {
            Debug.Log("An error occured when loading textures. " + e.Message + " : " + e.StackTrace);
        }
    }


    public void SetTexture(RemotePlayer player, int partNum, string url)
    {
        if (url == "" || url == null)
            return;
        StartCoroutine(SetTextureEnum(player, partNum, url));
        Resources.UnloadUnusedAssets();
    }

    public void SetNormal(RemotePlayer player, int partNum, string url)
    {
        if (url == "" || url == null)
            return;
        StartCoroutine(SetNormalEnum(player, partNum, url));
        Resources.UnloadUnusedAssets();
    }

    public void SetMetallic(RemotePlayer player, int partNum, string url)
    {
        if (url == "" || url == null)
            return;
        StartCoroutine(SetMetallicEnum(player, partNum, url));
        Resources.UnloadUnusedAssets();
    }

    IEnumerator SetTextureEnum(RemotePlayer player, int partNum, string url)
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();
            if (www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                TexHelper(player, DownloadHandlerTexture.GetContent(www), null, partNum, "_MainTex", "", url);

            }
        }
    }

    IEnumerator SetMetallicEnum(RemotePlayer player, int partNum, string url)
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();
            if (www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                TexHelper(player, DownloadHandlerTexture.GetContent(www), null, partNum, "_MetallicGlossMap", "_METALLICGLOSSMAP", url);

            }
        }
    }

    IEnumerator SetNormalEnum(RemotePlayer player, int partNum, string url)
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();
            if (www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                TexHelper(player, ConvertToNormalMap(DownloadHandlerTexture.GetContent(www)), null, partNum, "_BumpMap", "_NORMALMAP", url);

            }
        }
    }

    IEnumerator SetTextureBlank(RemotePlayer player, int partNum)
    {
        yield return new WaitForEndOfFrame();
        TexHelper(player, null, null, partNum, "_MainTex");
    }



    IEnumerator SetNormalBlank(RemotePlayer player, int partNum)
    {
        yield return new WaitForEndOfFrame();
        TexHelper(player, null, null, partNum, "_BumpMap", "_NORMALMAP");
    }

    IEnumerator SetMetallicBlank(RemotePlayer player, int partNum)
    {
        yield return new WaitForEndOfFrame();
        TexHelper(player, null, null, partNum, "_MetallicGlossMap", "_METALLICGLOSSMAP");
    }

    private Texture2D ConvertToNormalMap(Texture2D texture)
    {
        Texture2D normalTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, true, true);
        normalTexture.filterMode = FilterMode.Trilinear;
        Color[] pixels = texture.GetPixels(0, 0, texture.width, texture.height);
        normalTexture.SetPixels(pixels);
        normalTexture.Apply(true, false);
        return normalTexture;
    }

    /// <summary>
    /// Helper method for setting all types of textures
    /// </summary>
    /// <param name="tex"></param>
    /// <param name="list"></param>
    /// <param name="inputField"></param>
    /// <param name="partNum"></param>
    /// <param name="texType"></param>
    /// <param name="url"></param>
    /// <param name="matIndex"></param>
    /// <param name="enableKeyword"></param>
    void TexHelper(RemotePlayer player, Texture2D tex, Dictionary<int, string> list, int partNum, string texType, string enableKeyword = "", string url = "")
    {
        bool flag = (!enableKeyword.Equals("") && tex != null);
        bool flag2 = (!enableKeyword.Equals("") && tex == null);
        if (partNum == -1) //Front Tire Wall
        {
            Material[] mats = player.partMaster.GetMaterials(player.partMaster.frontTire);
            if (flag)
                mats[1].EnableKeyword(enableKeyword);
            if (flag2)
                mats[1].DisableKeyword(enableKeyword);
            mats[1].SetTexture(texType, tex);
            player.partMaster.SetMaterials(player.partMaster.frontTire, mats);
        }
        else if (partNum == -2) // Rear Tire Wall
        {
            Material[] mats2 = player.partMaster.GetMaterials(player.partMaster.rearTire);
            if (flag)
                mats2[1].EnableKeyword(enableKeyword);
            if (flag2)
                mats2[1].DisableKeyword(enableKeyword);
            mats2[1].SetTexture(texType, tex);
            player.partMaster.SetMaterials(player.partMaster.rearTire, mats2);
        }
        else if (partNum == -3)
        {
            if (player.brakesManager.IsEnabled())
            {
                if (flag)
                {
                    player.brakesManager.GetFrameBrakes().GetComponent<Renderer>().materials[2].EnableKeyword(enableKeyword);
                    player.brakesManager.GetFrameBrakes().GetComponent<Renderer>().materials[1].EnableKeyword(enableKeyword);
                }
                if (flag2)
                {
                    player.brakesManager.GetFrameBrakes().GetComponent<Renderer>().materials[2].DisableKeyword(enableKeyword);
                    player.brakesManager.GetFrameBrakes().GetComponent<Renderer>().materials[1].DisableKeyword(enableKeyword);
                }

                player.brakesManager.GetFrameBrakes().GetComponent<Renderer>().materials[2].SetTexture(texType, tex);
                player.brakesManager.GetBarBrakes().GetComponent<Renderer>().materials[1].SetTexture(texType, tex);
            }
        }
        else if (partNum == -4)
        {
            if (player.brakesManager.IsEnabled())
            {
                if (flag)
                {
                    player.brakesManager.GetFrameBrakes().GetComponent<Renderer>().materials[3].EnableKeyword(enableKeyword);
                    player.brakesManager.GetFrameBrakes().GetComponent<Renderer>().materials[2].EnableKeyword(enableKeyword);
                }
                if (flag2)
                {
                    player.brakesManager.GetFrameBrakes().GetComponent<Renderer>().materials[3].DisableKeyword(enableKeyword);
                    player.brakesManager.GetFrameBrakes().GetComponent<Renderer>().materials[2].DisableKeyword(enableKeyword);
                }
                player.brakesManager.GetFrameBrakes().GetComponent<Renderer>().materials[3].SetTexture(texType, tex);
                player.brakesManager.GetBarBrakes().GetComponent<Renderer>().materials[2].SetTexture(texType, tex);
            }
        }
        else // Everything else
        {
            if (flag)
                player.partMaster.GetMaterials(partNum)[0].EnableKeyword(enableKeyword);
            if (flag2)
                player.partMaster.GetMaterials(partNum)[0].DisableKeyword(enableKeyword);
            player.partMaster.GetMaterials(partNum)[0].SetTexture(texType, tex);
        }
    }

    private void LoadMeshes(RemotePlayer player, SaveList loadList)
    {
        try
        {
            foreach (PartMesh pm in loadList.partMeshes)
            {
                if (pm.isCustom)
                {
                    int indexer = pm.fileName.IndexOf("/GarageContent");
                    
                    pm.fileName = pm.fileName.Remove(0, indexer);
                    pm.fileName = pm.fileName.Insert(0, Application.dataPath);

                    if (!File.Exists(pm.fileName))
                    {
                        int lastslash = pm.fileName.LastIndexOf("/");
                        string shortfilename = pm.fileName.Remove(0, lastslash + 1);
                        FileSyncing.AddToRequestable((int)FileTypeByNum.Garage, shortfilename, player.id);
                        SavingManager.instance.ChangeAlertText($"Error loading mesh: {pm.fileName} for {player.username}. A file request has been left in Sync window");
                        if (!SavingManager.instance.infoBox.activeSelf)
                            SavingManager.instance.infoBox.SetActive(true);
                    }
                    else
                    {
                        player.partMaster.SetMesh(pm.partNum, FindObjectOfType<CustomMeshManager>().FindSpecific(pm.partName, pm.fileName));
                        continue;
                    }
                }
                switch (pm.partName)
                {
                    case "frame":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.frame, FindObjectOfType<CustomMeshManager>().frames[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.frame, FindObjectOfType<CustomMeshManager>().frames[0].mesh);
                        break;
                    case "bars":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.bars, FindObjectOfType<CustomMeshManager>().bars[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.bars, FindObjectOfType<CustomMeshManager>().bars[0].mesh);
                        break;
                    case "sprocket":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.sprocket, FindObjectOfType<CustomMeshManager>().sprockets[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.sprocket, FindObjectOfType<CustomMeshManager>().sprockets[0].mesh);
                        break;
                    case "stem":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                        {
                            player.partMaster.SetMesh(player.partMaster.stem, FindObjectOfType<CustomMeshManager>().stems[pm.partNum].mesh);
                            player.partMaster.SetMesh(player.partMaster.stemBolts, FindObjectOfType<CustomMeshManager>().boltsStem[pm.partNum].mesh);
                        }
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                        {
                            player.partMaster.SetMesh(player.partMaster.stem, FindObjectOfType<CustomMeshManager>().stems[0].mesh);
                            player.partMaster.SetMesh(player.partMaster.stemBolts, FindObjectOfType<CustomMeshManager>().boltsStem[0].mesh);
                        }
                        break;
                    case "cranks":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                        {
                            player.partMaster.SetMesh(player.partMaster.leftCrank, FindObjectOfType<CustomMeshManager>().cranks[pm.partNum].mesh);
                            player.partMaster.SetMesh(player.partMaster.rightCrank, FindObjectOfType<CustomMeshManager>().cranks[pm.partNum].mesh);
                            player.partMaster.SetMesh(player.partMaster.leftCrankBolt, FindObjectOfType<CustomMeshManager>().boltsCrank[pm.partNum].mesh);
                            player.partMaster.SetMesh(player.partMaster.rightCrankBolt, FindObjectOfType<CustomMeshManager>().boltsCrank[pm.partNum].mesh);
                        }
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                        {
                            player.partMaster.SetMesh(player.partMaster.leftCrank, FindObjectOfType<CustomMeshManager>().cranks[0].mesh);
                            player.partMaster.SetMesh(player.partMaster.rightCrank, FindObjectOfType<CustomMeshManager>().cranks[0].mesh);
                            player.partMaster.SetMesh(player.partMaster.leftCrankBolt, FindObjectOfType<CustomMeshManager>().boltsCrank[0].mesh);
                            player.partMaster.SetMesh(player.partMaster.rightCrankBolt, FindObjectOfType<CustomMeshManager>().boltsCrank[0].mesh);
                        }
                        break;
                    case "frontSpokes":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.frontSpokes, FindObjectOfType<CustomMeshManager>().spokes[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.frontSpokes, FindObjectOfType<CustomMeshManager>().spokes[0].mesh);
                        break;
                    case "rearSpokes":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.rearSpokes, FindObjectOfType<CustomMeshManager>().spokes[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.rearSpokes, FindObjectOfType<CustomMeshManager>().spokes[0].mesh);
                        break;
                    case "pedals":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                        {
                            player.partMaster.SetMesh(player.partMaster.leftPedal, FindObjectOfType<CustomMeshManager>().pedals[pm.partNum].mesh);
                            player.partMaster.SetMesh(player.partMaster.rightPedal, FindObjectOfType<CustomMeshManager>().pedals[pm.partNum].mesh);
                        }
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                        {
                            player.partMaster.SetMesh(player.partMaster.leftPedal, FindObjectOfType<CustomMeshManager>().pedals[0].mesh);
                            player.partMaster.SetMesh(player.partMaster.rightPedal, FindObjectOfType<CustomMeshManager>().pedals[0].mesh);
                        }
                        break;
                    case "forks":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.forks, FindObjectOfType<CustomMeshManager>().forks[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.forks, FindObjectOfType<CustomMeshManager>().forks[0].mesh);
                        break;
                    case "frontPegs":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.frontPegs, FindObjectOfType<CustomMeshManager>().pegs[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.frontPegs, FindObjectOfType<CustomMeshManager>().pegs[0].mesh);
                        break;
                    case "rearPegs":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.rearPegs, FindObjectOfType<CustomMeshManager>().pegs[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.rearPegs, FindObjectOfType<CustomMeshManager>().pegs[0].mesh);
                        break;
                    case "frontHub":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.frontHub, FindObjectOfType<CustomMeshManager>().hubs[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.frontHub, FindObjectOfType<CustomMeshManager>().hubs[0].mesh);
                        break;
                    case "rearHub":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.rearHub, FindObjectOfType<CustomMeshManager>().hubs[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.rearHub, FindObjectOfType<CustomMeshManager>().hubs[0].mesh);
                        break;
                    case "seat":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.seat, FindObjectOfType<CustomMeshManager>().seats[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.seat, FindObjectOfType<CustomMeshManager>().seats[0].mesh);
                        break;
                    case "frontRim":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.frontRim, FindObjectOfType<CustomMeshManager>().rims[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.frontRim, FindObjectOfType<CustomMeshManager>().rims[0].mesh);
                        break;
                    case "rearRim":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.rearRim, FindObjectOfType<CustomMeshManager>().rims[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.rearRim, FindObjectOfType<CustomMeshManager>().rims[0].mesh);
                        break;
                    case "frontSpokeAccessory":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.frontAcc, FindObjectOfType<CustomMeshManager>().accessories[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.frontAcc, FindObjectOfType<CustomMeshManager>().accessories[0].mesh);
                        break;
                    case "rearSpokeAccessory":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.rearAcc, FindObjectOfType<CustomMeshManager>().accessories[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.rearAcc, FindObjectOfType<CustomMeshManager>().accessories[0].mesh);
                        break;
                    case "barAccessory":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.barAcc, FindObjectOfType<CustomMeshManager>().barAccessories[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.barAcc, FindObjectOfType<CustomMeshManager>().barAccessories[0].mesh);
                        break;
                    case "frameAccessory":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.frameAcc, FindObjectOfType<CustomMeshManager>().frameAccessories[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.frameAcc, FindObjectOfType<CustomMeshManager>().frameAccessories[0].mesh);
                        break;
                    case "frontHubGuard":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.frontHubG, FindObjectOfType<CustomMeshManager>().frontHubGuards[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.frontHubG, FindObjectOfType<CustomMeshManager>().frontHubGuards[0].mesh);
                        break;
                    case "rearHubGuard":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.rearHubG, FindObjectOfType<CustomMeshManager>().rearHubGuards[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.rearHubG, FindObjectOfType<CustomMeshManager>().rearHubGuards[0].mesh);
                        break;
                    case "seatPost":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.seatPost, FindObjectOfType<CustomMeshManager>().seatPosts[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.seatPost, FindObjectOfType<CustomMeshManager>().seatPosts[0].mesh);
                        break;
                    default:
                        break;
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message + "\n " + e.StackTrace + "\n ");
        }
    }

    private void LoadTireTread(RemotePartMaster pm, SaveList loadList)
    {
        SetFrontTireTread(pm, loadList.frontTreadID);
        SetFrontTireWall(pm, loadList.frontWallID);
        SetRearTireTread(pm, loadList.rearTreadID);
        SetRearTireWall(pm, loadList.rearWallID);
    }

    public void SetFrontTireTread(RemotePartMaster pm, int id)
    {
        int index = (id % PartManager.instance.tireMats.Length);
        Material[] mats = pm.GetMaterials(pm.frontTire);
        mats[0] = PartManager.instance.tireMats[index];
        pm.SetMaterials(pm.frontTire, mats);
    }

    public void SetRearTireTread(RemotePartMaster pm, int id)
    {
        int index = (id % PartManager.instance.tireMats.Length);
        Material[] mats = pm.GetMaterials(pm.rearTire);
        mats[0] = PartManager.instance.tireMats[index];
        pm.SetMaterials(pm.rearTire, mats);
    }

    public void SetFrontTireWall(RemotePartMaster pm, int id)
    {
        int index = (id % PartManager.instance.tireWallMats.Length);
        Material[] mats = pm.GetMaterials(pm.frontTire);
        mats[1] = PartManager.instance.tireWallMats[index];
        pm.SetMaterials(pm.frontTire, mats);
    }

    public void SetRearTireWall(RemotePartMaster pm, int id)
    {
        int index = (id % PartManager.instance.tireWallMats.Length);
        Material[] mats = pm.GetMaterials(pm.rearTire);
        mats[1] = PartManager.instance.tireWallMats[index];
        pm.SetMaterials(pm.rearTire, mats);
    }
    private void LoadMaterials(RemotePlayer player, SaveList loadList)
    {
        try
        {
            foreach (PartMaterial p in loadList.partMaterials)
            {
                switch (p.matID)
                {
                    case 0:
                        player.partMaster.SetMaterial(p.partNum, FindObjectOfType<MaterialManager>().defaultMat);
                        break;
                    case 7:
                        break;
                    case 9:
                        break;
                    default:
                        player.partMaster.SetMaterial(p.partNum, FindObjectOfType<MaterialManager>().customMats[p.matID - 1]);
                        break;
                }

            }
            SetMaterialHelper(player.partMaster, player.partMaster.seatPost, loadList.seatPostMat);
            SetMaterialHelper(player.partMaster, player.partMaster.frontTire, loadList.frontTireMat);
            SetMaterialHelper(player.partMaster, player.partMaster.rearTire, loadList.rearTireMat);
            SetMaterialHelper(player.partMaster, player.partMaster.frontTire, loadList.frontTireWallMat, 1);
            SetMaterialHelper(player.partMaster, player.partMaster.rearTire, loadList.rearTireWallMat, 1);
            SetMaterialHelper(player.partMaster, player.partMaster.frontRim, loadList.frontRimMat);
            SetMaterialHelper(player.partMaster, player.partMaster.rearRim, loadList.rearRimMat);
            SetMaterialHelper(player.partMaster, player.partMaster.frontHub, loadList.frontHubMat);
            SetMaterialHelper(player.partMaster, player.partMaster.rearHub, loadList.rearHubMat);
            SetMaterialHelper(player.partMaster, player.partMaster.frontSpokes, loadList.frontSpokesMat);
            SetMaterialHelper(player.partMaster, player.partMaster.rearSpokes, loadList.rearSpokesMat);
            SetMaterialHelper(player.partMaster, player.partMaster.frontNipples, loadList.frontNipplesMat);
            SetMaterialHelper(player.partMaster, player.partMaster.rearNipples, loadList.rearNipplesMat);
            foreach (MatData matData in loadList.matData)
            {
                player.partMaster.SetMaterialData(matData.key, matData.glossiness, matData.glossMapScale);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message + "\n " + e.StackTrace + "\n ");
        }
    }

    public void SetMaterialHelper(RemotePartMaster pm, int key, int mat, int index = 0)
    {
        
        if (mat == 0)
            pm.GetPart(key).GetComponent<MeshRenderer>().materials[index] = FindObjectOfType<MaterialManager>().defaultMat;
        else if (mat == 9)
            return;
        else
            pm.GetPart(key).GetComponent<MeshRenderer>().materials[index] = FindObjectOfType<MaterialManager>().customMats[mat - 1];
    }


    private void LoadBrakes(RemotePlayer player, SaveList loadList)
    {
        player.brakesManager.SetBrakes(player.partMaster, loadList.brakes);
    }

    private void LoadHeightsAngles(RemotePlayer player, SaveList list)
    {
        BikeLoadOut BLO = player.BMX.GetComponentInChildren<BikeLoadOut>();
        player.BMX.transform.localScale = new Vector3(list.bikeScale, list.bikeScale, list.bikeScale);
        BLO.SetBackTireFatness(list.rearTireWidth);
        BLO.SetFrontTireFatness(list.frontTireWidth);
        BLO.seatApplyMod.SetSeatAnglePerc(list.seatAngle);
        BLO.SetSeatHeight(list.seatHeight);
        BLO.SetBarsAngle(list.barsAngle);
        BLO.barsApplyMod.SetFlanges(list.flanges);
        BLO.SetGripsID(list.gripsID % FindObjectOfType<BarsApplyMod>().gripMats.Length);
        BLO.SetSeatCover(list.seatID % FindObjectOfType<SeatApplyMod>().seatCovers.Length);
    }

    private void LoadDriveSide(RemotePlayer player, SaveList loadList)
    {
        if (loadList.LHD)
        {
            SetLHD(player.partMaster);
        }
        else
        {
            SetRHD(player.partMaster);
        }
    }

    public void SetRHD(RemotePartMaster pm)
    {
        pm.GetPart(pm.rearWheel).transform.localRotation = (Quaternion.Euler(0, 0, 0f));
        pm.GetPart(pm.chain).transform.localPosition = (new Vector3(0.089f, 0, 0));
        pm.GetPart(pm.sprocket).transform.localPosition = (new Vector3(0.0467f, 0.001f, 0));
        pm.GetPart(pm.sprocket).transform.localRotation = (Quaternion.Euler(0, 0, 180f));
    }

    public void SetLHD(RemotePartMaster pm)
    {
        pm.GetPart(pm.rearWheel).transform.localRotation = (Quaternion.Euler(0, 0, 90f));
        pm.GetPart(pm.chain).transform.localPosition = (new Vector3(0, 0, 0));
        pm.GetPart(pm.sprocket).transform.localPosition = (new Vector3(-0.0402f, 0.0013f, 0.0001f));
        pm.GetPart(pm.sprocket).transform.localRotation = (Quaternion.Euler(0, 0, 0));
    }

    private void SetOriginalTextures(RemotePartMaster pm)
    {

        pm.GetMaterial(pm.frame).SetTexture("_MainTexture", FindObjectOfType<TextureManager>().OriginalFrameTex);
        pm.GetMaterial(pm.bars).SetTexture("_MainTexture", FindObjectOfType<TextureManager>().OriginalBarsTex);
        pm.GetMaterial(pm.forks).SetTexture("_MainTexture", FindObjectOfType<TextureManager>().OriginalForksTex);
        pm.GetMaterial(pm.seat).SetTexture("_MainTexture", FindObjectOfType<TextureManager>().OriginalSeatTex);

        pm.GetMaterials(pm.frontTire)[0].SetTexture("_MainTexture", FindObjectOfType<TextureManager>().OriginalTire1Tex);
        pm.GetMaterials(pm.rearTire)[0].SetTexture("_MainTexture", FindObjectOfType<TextureManager>().OriginalTire2Tex);
        pm.GetMaterials(pm.frontTire)[1].SetTexture("_MainTexture", FindObjectOfType<TextureManager>().OriginalTire1WallTex);
        pm.GetMaterials(pm.rearTire)[1].SetTexture("_MainTexture", FindObjectOfType<TextureManager>().OriginalTire2WallTex);

        pm.GetMaterial(pm.frontRim).SetTexture("_MainTexture", FindObjectOfType<TextureManager>().OriginalRimTex);
        pm.GetMaterial(pm.rearRim).SetTexture("_MainTexture", FindObjectOfType<TextureManager>().OriginalRimTex);

        pm.GetMaterial(pm.frontHub).SetTexture("_MainTexture", FindObjectOfType<TextureManager>().OriginalHubTex);
        pm.GetMaterial(pm.rearHub).SetTexture("_MainTexture", FindObjectOfType<TextureManager>().OriginalHubTex);

        Resources.UnloadUnusedAssets();
    }

}
