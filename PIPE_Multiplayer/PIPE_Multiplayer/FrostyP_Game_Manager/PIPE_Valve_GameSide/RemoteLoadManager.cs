using PIPE_Valve_Console_Client;
using System;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

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
                        TextureManager.instance.RemoveTexture(p.partNum);
                    else if (p.normal)
                        TextureManager.instance.RemoveNormal(p.partNum);
                    else
                        TextureManager.instance.RemoveMetallic(p.partNum);
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
        WWW www = new WWW(url);
        while (!www.isDone)
        {
            yield return new WaitForEndOfFrame();
        }
        TexHelper(player, www.texture, null, partNum, "_MainTex", "", url);
    }

    IEnumerator SetMetallicEnum(RemotePlayer player, int partNum, string url)
    {
        WWW www = new WWW(url);
        while (!www.isDone)
        {
            yield return new WaitForEndOfFrame();
        }
        TexHelper(player, www.texture, null, partNum, "_MetallicGlossMap", "_METALLICGLOSSMAP", url);
    }

    IEnumerator SetNormalEnum(RemotePlayer player, int partNum, string url)
    {
        WWW www = new WWW(url);
        while (!www.isDone)
        {
            yield return new WaitForEndOfFrame();
        }
        Texture2D normalTexture = ConvertToNormalMap(www.texture);

        TexHelper(player, normalTexture, null, partNum, "_BumpMap", "_NORMALMAP", url);
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
                        FileSyncing.AddToRequestable((int)FileTypeByNum.Mesh, shortfilename, player.id);
                        SavingManager.instance.ChangeAlertText($"Error loading mesh: {pm.fileName} for {player.username}. A file request has been left in Sync window");
                        if (!SavingManager.instance.infoBox.activeSelf)
                            SavingManager.instance.infoBox.SetActive(true);
                    }
                }
                switch (pm.partName)
                {
                    case "frame":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.frame, CustomMeshManager.instance.frames[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.frame, CustomMeshManager.instance.frames[0].mesh);
                        break;
                    case "bars":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.bars, CustomMeshManager.instance.bars[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.bars, CustomMeshManager.instance.bars[0].mesh);
                        break;
                    case "sprocket":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.sprocket, CustomMeshManager.instance.sprockets[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.sprocket, CustomMeshManager.instance.sprockets[0].mesh);
                        break;
                    case "stem":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                        {
                            player.partMaster.SetMesh(player.partMaster.stem, CustomMeshManager.instance.stems[pm.partNum].mesh);
                            player.partMaster.SetMesh(player.partMaster.stemBolts, CustomMeshManager.instance.boltsStem[pm.partNum].mesh);
                        }
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                        {
                            player.partMaster.SetMesh(player.partMaster.stem, CustomMeshManager.instance.stems[0].mesh);
                            player.partMaster.SetMesh(player.partMaster.stemBolts, CustomMeshManager.instance.boltsStem[0].mesh);
                        }
                        break;
                    case "cranks":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                        {
                            player.partMaster.SetMesh(player.partMaster.leftCrank, CustomMeshManager.instance.cranks[pm.partNum].mesh);
                            player.partMaster.SetMesh(player.partMaster.rightCrank, CustomMeshManager.instance.cranks[pm.partNum].mesh);
                            player.partMaster.SetMesh(player.partMaster.leftCrankBolt, CustomMeshManager.instance.boltsCrank[pm.partNum].mesh);
                            player.partMaster.SetMesh(player.partMaster.rightCrankBolt, CustomMeshManager.instance.boltsCrank[pm.partNum].mesh);
                        }
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                        {
                            player.partMaster.SetMesh(player.partMaster.leftCrank, CustomMeshManager.instance.cranks[0].mesh);
                            player.partMaster.SetMesh(player.partMaster.rightCrank, CustomMeshManager.instance.cranks[0].mesh);
                            player.partMaster.SetMesh(player.partMaster.leftCrankBolt, CustomMeshManager.instance.boltsCrank[0].mesh);
                            player.partMaster.SetMesh(player.partMaster.rightCrankBolt, CustomMeshManager.instance.boltsCrank[0].mesh);
                        }
                        break;
                    case "frontSpokes":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.frontSpokes, CustomMeshManager.instance.spokes[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.frontSpokes, CustomMeshManager.instance.spokes[0].mesh);
                        break;
                    case "rearSpokes":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.rearSpokes, CustomMeshManager.instance.spokes[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.rearSpokes, CustomMeshManager.instance.spokes[0].mesh);
                        break;
                    case "pedals":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                        {
                            player.partMaster.SetMesh(player.partMaster.leftPedal, CustomMeshManager.instance.pedals[pm.partNum].mesh);
                            player.partMaster.SetMesh(player.partMaster.rightPedal, CustomMeshManager.instance.pedals[pm.partNum].mesh);
                        }
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                        {
                            player.partMaster.SetMesh(player.partMaster.leftPedal, CustomMeshManager.instance.pedals[0].mesh);
                            player.partMaster.SetMesh(player.partMaster.rightPedal, CustomMeshManager.instance.pedals[0].mesh);
                        }
                        break;
                    case "forks":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.forks, CustomMeshManager.instance.forks[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.forks, CustomMeshManager.instance.forks[0].mesh);
                        break;
                    case "frontPegs":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.frontPegs, CustomMeshManager.instance.pegs[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.frontPegs, CustomMeshManager.instance.pegs[0].mesh);
                        break;
                    case "rearPegs":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.rearPegs, CustomMeshManager.instance.pegs[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.rearPegs, CustomMeshManager.instance.pegs[0].mesh);
                        break;
                    case "frontHub":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.frontHub, CustomMeshManager.instance.hubs[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.frontHub, CustomMeshManager.instance.hubs[0].mesh);
                        break;
                    case "rearHub":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.rearHub, CustomMeshManager.instance.hubs[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.rearHub, CustomMeshManager.instance.hubs[0].mesh);
                        break;
                    case "seat":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.seat, CustomMeshManager.instance.seats[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.seat, CustomMeshManager.instance.seats[0].mesh);
                        break;
                    case "frontRim":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.frontRim, CustomMeshManager.instance.rims[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.frontRim, CustomMeshManager.instance.rims[0].mesh);
                        break;
                    case "rearRim":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.rearRim, CustomMeshManager.instance.rims[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.rearRim, CustomMeshManager.instance.rims[0].mesh);
                        break;
                    case "frontSpokeAccessory":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.frontAcc, CustomMeshManager.instance.accessories[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.frontAcc, CustomMeshManager.instance.accessories[0].mesh);
                        break;
                    case "rearSpokeAccessory":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.rearAcc, CustomMeshManager.instance.accessories[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.rearAcc, CustomMeshManager.instance.accessories[0].mesh);
                        break;
                    case "barAccessory":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.barAcc, CustomMeshManager.instance.barAccessories[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.barAcc, CustomMeshManager.instance.barAccessories[0].mesh);
                        break;
                    case "frameAccessory":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.frameAcc, CustomMeshManager.instance.frameAccessories[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.frameAcc, CustomMeshManager.instance.frameAccessories[0].mesh);
                        break;
                    case "frontHubGuard":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.frontHubG, CustomMeshManager.instance.frontHubGuards[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.frontHubG, CustomMeshManager.instance.frontHubGuards[0].mesh);
                        break;
                    case "rearHubGuard":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.rearHubG, CustomMeshManager.instance.rearHubGuards[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.rearHubG, CustomMeshManager.instance.rearHubGuards[0].mesh);
                        break;
                    case "seatPost":
                        if ((pm.isCustom && File.Exists(pm.fileName)) || !pm.isCustom)
                            player.partMaster.SetMesh(player.partMaster.seatPost, CustomMeshManager.instance.seatPosts[pm.partNum].mesh);
                        else if (pm.isCustom && !File.Exists(pm.fileName))
                            player.partMaster.SetMesh(player.partMaster.seatPost, CustomMeshManager.instance.seatPosts[0].mesh);
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
        int index = (id % PartManager.instance.tireMats.Length);
        Material[] mats = pm.GetMaterials(pm.frontTire);
        mats[1] = PartManager.instance.tireMats[index];
        pm.SetMaterials(pm.frontTire, mats);
    }

    public void SetRearTireWall(RemotePartMaster pm, int id)
    {
        int index = (id % PartManager.instance.tireMats.Length);
        Material[] mats = pm.GetMaterials(pm.rearTire);
        mats[1] = PartManager.instance.tireMats[index];
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
                        player.partMaster.SetMaterial(p.partNum, MaterialManager.instance.defaultMat);
                        break;
                    case 7:
                        break;
                    case 9:
                        break;
                    default:
                        player.partMaster.SetMaterial(p.partNum, MaterialManager.instance.customMats[p.matID - 1]);
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
        MaterialManager matman = FindObjectOfType<MaterialManager>();
        if (mat == 0)
            pm.GetPart(key).GetComponent<MeshRenderer>().materials[index] = matman.defaultMat;
        else if (mat == 9)
            return;
        else
            pm.GetPart(key).GetComponent<MeshRenderer>().materials[index] = matman.customMats[mat - 1];
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
        BLO.SetGripsID(list.gripsID);
        BLO.SetSeatCover(list.seatID);
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
        pm.GetPart(pm.rearWheel).transform.localRotation = (Quaternion.Euler(0, 0, 270f));
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

        pm.GetMaterial(pm.frame).SetTexture("_MainTexture", TextureManager.instance.OriginalFrameTex);
        pm.GetMaterial(pm.bars).SetTexture("_MainTexture", TextureManager.instance.OriginalBarsTex);
        pm.GetMaterial(pm.forks).SetTexture("_MainTexture", TextureManager.instance.OriginalForksTex);
        pm.GetMaterial(pm.seat).SetTexture("_MainTexture", TextureManager.instance.OriginalSeatTex);

        pm.GetMaterials(pm.frontTire)[0].SetTexture("_MainTexture", TextureManager.instance.OriginalTire1Tex);
        pm.GetMaterials(pm.rearTire)[0].SetTexture("_MainTexture", TextureManager.instance.OriginalTire2Tex);
        pm.GetMaterials(pm.frontTire)[1].SetTexture("_MainTexture", TextureManager.instance.OriginalTire1WallTex);
        pm.GetMaterials(pm.rearTire)[1].SetTexture("_MainTexture", TextureManager.instance.OriginalTire2WallTex);

        pm.GetMaterial(pm.frontRim).SetTexture("_MainTexture", TextureManager.instance.OriginalRimTex);
        pm.GetMaterial(pm.rearRim).SetTexture("_MainTexture", TextureManager.instance.OriginalRimTex);

        pm.GetMaterial(pm.frontHub).SetTexture("_MainTexture", TextureManager.instance.OriginalHubTex);
        pm.GetMaterial(pm.rearHub).SetTexture("_MainTexture", TextureManager.instance.OriginalHubTex);

        Resources.UnloadUnusedAssets();
    }

}
