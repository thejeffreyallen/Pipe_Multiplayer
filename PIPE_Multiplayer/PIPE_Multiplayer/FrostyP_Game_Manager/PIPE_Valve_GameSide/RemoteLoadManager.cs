using PIPE_Valve_Console_Client;
using System;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class RemoteLoadManager : MonoBehaviour
{
    public static RemoteLoadManager instance;
    private RemotePartMaster rpm;

    void Awake()
    {
        instance = this;
    }



    public void Load(RemotePlayer player, SaveList loadList)
    {
        try
        {
            rpm = player.partMaster;
            SetOriginalTextures(rpm);
            LoadBrakes(player, loadList);
            LoadHeightsAngles(player, loadList);
            LoadDriveSide(player, loadList);
            LoadMaterials(player, loadList);
            LoadTireTread(rpm, loadList);
            foreach (PartColor p in loadList.partColors)
            {
                rpm.SetColor(player, p.partNum, new Color(p.r, p.g, p.b, p.a));
            }
            LoadMeshes(player, loadList);
            LoadTextures(player, loadList);
            LoadPartPositions(rpm, loadList);

            // Quick fix for weird normal map issue on left crank arm
            Material m = rpm.GetMaterial(rpm.rightCrank);
            rpm.GetPart(rpm.leftCrank).GetComponent<MeshRenderer>().material = m;

            if (player.partMaster.GetPart(player.partMaster.rightPedal).transform.localEulerAngles.y < 180.0f)
            {
                player.partMaster.GetPart(player.partMaster.rightPedal).transform.localEulerAngles += new Vector3(0, 180f, 0);
            }

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
            pm.SetRotation(partPos.partNum, new Vector3(partPos.rotX, partPos.rotY, partPos.rotZ));
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

                    int cut = p.url.LastIndexOf("frostypgamemanager");
                    string shortened = p.url.Remove(0, cut);
                    int _lastslash = p.url.LastIndexOf("/");
                    string dir = Application.dataPath + shortened.Remove(_lastslash+1,shortened.Length-_lastslash-1);


                    if (FileSyncing.CheckForFile(name,dir))
                    {
                        Texture2D tex = GameManager.GetTexture(name,dir);


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
                        FileSyncing.AddToRequestable((int)FileTypeByNum.Garage, name, player.id,dir);



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
                            TextureManager.instance.RemoveTexture(p.partNum);
                        else if (p.normal)
                            TextureManager.instance.RemoveNormal(p.partNum);
                        else
                            TextureManager.instance.RemoveMetallic(p.partNum);
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
        WWW www = new WWW(url);
        yield return www;
        TexHelper(player, www.texture, null, partNum, "_MainTex", "", url);
    }

    IEnumerator SetMetallicEnum(RemotePlayer player, int partNum, string url)
    {
        WWW www = new WWW(url);
        yield return www;
        TexHelper(player, www.texture, null, partNum, "_MetallicGlossMap", "_METALLICGLOSSMAP", url);
    }

    IEnumerator SetNormalEnum(RemotePlayer player, int partNum, string url)
    {
        WWW www = new WWW(url);
        yield return www;
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
                string fullPath = Application.dataPath + "/GarageContent/" + pm.fileName;
                int lastslash = fullPath.LastIndexOf("/") + 1;
                string dir = fullPath.Remove(lastslash, fullPath.Length - lastslash);
                Debug.Log("PartMesh: " + pm.partName + ". Index = " + pm.index + ": Dir:" + fullPath);

                if (pm.isCustom)
                {
                    if (!File.Exists(fullPath))
                    {
                        FileSyncing.AddToRequestable((int)FileTypeByNum.Garage, pm.fileName, player.id,dir);
                        SavingManager.instance.ChangeAlertText($"Error loading mesh: {pm.fileName} for {player.username}. A file request has been left in Sync window");
                        if (!SavingManager.instance.infoBox.activeSelf)
                            SavingManager.instance.infoBox.SetActive(true);
                    }
                    else
                    {
                        if (pm.partName.Equals("cranks"))
                        {
                            Mesh temp = FindObjectOfType<CustomMeshManager>().FindSpecific(pm.partName, pm.fileName);
                            player.partMaster.SetMesh(player.partMaster.leftCrank, temp);
                            player.partMaster.SetMesh(player.partMaster.rightCrank, temp);
                            continue;
                        }
                        else if (pm.partName.Equals("stem"))
                        {
                            string path = "StemBolts/" + Path.GetFileName(pm.fileName);
                            Mesh stem = FindObjectOfType<CustomMeshManager>().FindSpecific(pm.partName, pm.fileName);
                            Mesh bolts = FindObjectOfType<CustomMeshManager>().FindSpecific("stemBolts", path);
                            player.partMaster.SetMesh(player.partMaster.stem, stem);
                            player.partMaster.SetMesh(player.partMaster.stemBolts, bolts);
                            continue;
                        }
                        else
                        {
                            Mesh temp = FindObjectOfType<CustomMeshManager>().FindSpecific(pm.partName, pm.fileName);
                            player.partMaster.SetMesh(pm.key, temp);
                            continue;
                        }
                    }
                }
                switch (pm.partName)
                {
                    case "frame":
                        if ((pm.isCustom && File.Exists(fullPath)) || !pm.isCustom)
                        {
                            MeshObject mo = FindObjectOfType<CustomMeshManager>().frames[pm.index];
                            if (mo == null)
                            {
                                Debug.Log("Unable to find MeshObject for " + pm.partName + " at index " + pm.index);
                                continue;
                            }
                            player.partMaster.SetMesh(player.partMaster.frame, mo.mesh);
                        }
                        else if (pm.isCustom && !File.Exists(fullPath))
                            player.partMaster.SetMesh(player.partMaster.frame, FindObjectOfType<CustomMeshManager>().frames[0].mesh);
                        break;
                    case "bars":
                        if ((pm.isCustom && File.Exists(fullPath)) || !pm.isCustom)
                        {
                            MeshObject mo = FindObjectOfType<CustomMeshManager>().bars[pm.index];
                            if (mo == null)
                            {
                                Debug.Log("Unable to find MeshObject for " + pm.partName + " at index " + pm.index);
                                continue;
                            }
                            player.partMaster.SetMesh(player.partMaster.bars, mo.mesh);
                        }
                        else if (pm.isCustom && !File.Exists(fullPath))
                            player.partMaster.SetMesh(player.partMaster.bars, FindObjectOfType<CustomMeshManager>().bars[0].mesh);
                        break;
                    case "sprocket":
                        if ((pm.isCustom && File.Exists(fullPath)) || !pm.isCustom)
                        {
                            MeshObject mo = FindObjectOfType<CustomMeshManager>().sprockets[pm.index];
                            if (mo == null)
                            {
                                Debug.Log("Unable to find MeshObject for " + pm.partName + " at index " + pm.index);
                                continue;
                            }
                            player.partMaster.SetMesh(player.partMaster.sprocket, mo.mesh);
                        }
                        else if (pm.isCustom && !File.Exists(fullPath))
                            player.partMaster.SetMesh(player.partMaster.sprocket, FindObjectOfType<CustomMeshManager>().sprockets[0].mesh);
                        break;
                    case "stem":
                        if ((pm.isCustom && File.Exists(fullPath)) || !pm.isCustom)
                        {
                            MeshObject mo1 = FindObjectOfType<CustomMeshManager>().stems[pm.index];
                            MeshObject mo2 = FindObjectOfType<CustomMeshManager>().boltsStem[pm.index];
                            if (mo1 == null || mo2 == null)
                            {
                                Debug.Log("Unable to find MeshObject for " + pm.partName + " at index " + pm.index);
                                continue;
                            }

                            player.partMaster.SetMesh(player.partMaster.stem, mo1.mesh);
                            player.partMaster.SetMesh(player.partMaster.stemBolts, mo2.mesh);
                        }
                        else if (pm.isCustom && !File.Exists(fullPath))
                        {
                            player.partMaster.SetMesh(player.partMaster.stem, FindObjectOfType<CustomMeshManager>().stems[0].mesh);
                            player.partMaster.SetMesh(player.partMaster.stemBolts, FindObjectOfType<CustomMeshManager>().boltsStem[0].mesh);
                        }
                        break;
                    case "cranks":
                        if ((pm.isCustom && File.Exists(fullPath)) || !pm.isCustom)
                        {
                            MeshObject mo1 = FindObjectOfType<CustomMeshManager>().cranks[pm.index];
                            MeshObject mo2 = (pm.index >= 0 && pm.index < 2) ? FindObjectOfType<CustomMeshManager>().boltsCrank[pm.index] : FindObjectOfType<CustomMeshManager>().boltsCrank[1];
                            if (mo1 == null || mo2 == null)
                            {
                                Debug.Log("Unable to find MeshObject for " + pm.partName + " at index " + pm.index);
                                continue;
                            }
                            player.partMaster.SetMesh(player.partMaster.leftCrank, mo1.mesh);
                            player.partMaster.SetMesh(player.partMaster.rightCrank, mo1.mesh);
                            player.partMaster.SetMesh(player.partMaster.leftCrankBolt, mo2.mesh);
                            player.partMaster.SetMesh(player.partMaster.rightCrankBolt, mo2.mesh);
                        }
                        else if (pm.isCustom && !File.Exists(fullPath))
                        {
                            player.partMaster.SetMesh(player.partMaster.leftCrank, FindObjectOfType<CustomMeshManager>().cranks[0].mesh);
                            player.partMaster.SetMesh(player.partMaster.rightCrank, FindObjectOfType<CustomMeshManager>().cranks[0].mesh);
                            player.partMaster.SetMesh(player.partMaster.leftCrankBolt, FindObjectOfType<CustomMeshManager>().boltsCrank[0].mesh);
                            player.partMaster.SetMesh(player.partMaster.rightCrankBolt, FindObjectOfType<CustomMeshManager>().boltsCrank[0].mesh);
                        }
                        break;
                    case "frontSpokes":
                        if ((pm.isCustom && File.Exists(fullPath)) || !pm.isCustom)
                        {
                            MeshObject mo = FindObjectOfType<CustomMeshManager>().spokes[pm.index];
                            if (mo == null)
                            {
                                Debug.Log("Unable to find MeshObject for " + pm.partName + " at index " + pm.index);
                                continue;
                            }
                            player.partMaster.SetMesh(player.partMaster.frontSpokes, mo.mesh);
                        }
                        else if (pm.isCustom && !File.Exists(fullPath))
                            player.partMaster.SetMesh(player.partMaster.frontSpokes, FindObjectOfType<CustomMeshManager>().spokes[0].mesh);
                        break;
                    case "rearSpokes":
                        if ((pm.isCustom && File.Exists(fullPath)) || !pm.isCustom)
                        {
                            MeshObject mo = FindObjectOfType<CustomMeshManager>().spokes[pm.index];
                            if (mo == null)
                            {
                                Debug.Log("Unable to find MeshObject for " + pm.partName + " at index " + pm.index);
                                continue;
                            }
                            player.partMaster.SetMesh(player.partMaster.rearSpokes, mo.mesh);
                        }
                        else if (pm.isCustom && !File.Exists(fullPath))
                            player.partMaster.SetMesh(player.partMaster.rearSpokes, FindObjectOfType<CustomMeshManager>().spokes[0].mesh);
                        break;
                    case "pedals":
                        if ((pm.isCustom && File.Exists(fullPath)) || !pm.isCustom)
                        {
                            MeshObject mo = FindObjectOfType<CustomMeshManager>().pedals[pm.index];
                            if (mo == null)
                            {
                                Debug.Log("Unable to find MeshObject for " + pm.partName + " at index " + pm.index);
                                continue;
                            }
                            player.partMaster.SetMesh(player.partMaster.leftPedal, mo.mesh);
                            player.partMaster.SetMesh(player.partMaster.rightPedal, mo.mesh);
                        }
                        else if (pm.isCustom && !File.Exists(fullPath))
                        {
                            player.partMaster.SetMesh(player.partMaster.leftPedal, FindObjectOfType<CustomMeshManager>().pedals[0].mesh);
                            player.partMaster.SetMesh(player.partMaster.rightPedal, FindObjectOfType<CustomMeshManager>().pedals[0].mesh);
                        }
                        break;
                    case "forks":
                        if ((pm.isCustom && File.Exists(fullPath)) || !pm.isCustom)
                        {
                            MeshObject mo = FindObjectOfType<CustomMeshManager>().forks[pm.index];
                            if (mo == null)
                            {
                                Debug.Log("Unable to find MeshObject for " + pm.partName + " at index " + pm.index);
                                continue;
                            }
                            player.partMaster.SetMesh(player.partMaster.forks, mo.mesh);
                        }
                        else if (pm.isCustom && !File.Exists(fullPath))
                            player.partMaster.SetMesh(player.partMaster.forks, FindObjectOfType<CustomMeshManager>().forks[0].mesh);
                        break;
                    case "frontPegs":
                        if ((pm.isCustom && File.Exists(fullPath)) || !pm.isCustom)
                        {
                            MeshObject mo = FindObjectOfType<CustomMeshManager>().pegs[pm.index];
                            if (mo == null)
                            {
                                Debug.Log("Unable to find MeshObject for " + pm.partName + " at index " + pm.index);
                                continue;
                            }
                            player.partMaster.SetMesh(player.partMaster.frontPegs, mo.mesh);
                        }
                        else if (pm.isCustom && !File.Exists(fullPath))
                            player.partMaster.SetMesh(player.partMaster.frontPegs, FindObjectOfType<CustomMeshManager>().pegs[0].mesh);
                        break;
                    case "rearPegs":
                        if ((pm.isCustom && File.Exists(fullPath)) || !pm.isCustom)
                        {
                            MeshObject mo = FindObjectOfType<CustomMeshManager>().pegs[pm.index];
                            if (mo == null)
                            {
                                Debug.Log("Unable to find MeshObject for " + pm.partName + " at index " + pm.index);
                                continue;
                            }
                            player.partMaster.SetMesh(player.partMaster.rearPegs, mo.mesh);
                        }
                        else if (pm.isCustom && !File.Exists(fullPath))
                            player.partMaster.SetMesh(player.partMaster.rearPegs, FindObjectOfType<CustomMeshManager>().pegs[0].mesh);
                        break;
                    case "frontHub":
                        if ((pm.isCustom && File.Exists(fullPath)) || !pm.isCustom)
                        {
                            MeshObject mo = FindObjectOfType<CustomMeshManager>().hubs[pm.index];
                            if (mo == null)
                            {
                                Debug.Log("Unable to find MeshObject for " + pm.partName + " at index " + pm.index);
                                continue;
                            }
                            player.partMaster.SetMesh(player.partMaster.frontHub, mo.mesh);
                        }
                        else if (pm.isCustom && !File.Exists(fullPath))
                            player.partMaster.SetMesh(player.partMaster.frontHub, FindObjectOfType<CustomMeshManager>().hubs[0].mesh);
                        break;
                    case "rearHub":
                        if ((pm.isCustom && File.Exists(fullPath)) || !pm.isCustom)
                        {
                            MeshObject mo = FindObjectOfType<CustomMeshManager>().hubs[pm.index];
                            if (mo == null)
                            {
                                Debug.Log("Unable to find MeshObject for " + pm.partName + " at index " + pm.index);
                                continue;
                            }
                            player.partMaster.SetMesh(player.partMaster.rearHub, mo.mesh);
                        }
                        else if (pm.isCustom && !File.Exists(fullPath))
                            player.partMaster.SetMesh(player.partMaster.rearHub, FindObjectOfType<CustomMeshManager>().hubs[0].mesh);
                        break;
                    case "seat":
                        if ((pm.isCustom && File.Exists(fullPath)) || !pm.isCustom)
                        {
                            MeshObject mo = FindObjectOfType<CustomMeshManager>().seats[pm.index];
                            if (mo == null)
                            {
                                Debug.Log("Unable to find MeshObject for " + pm.partName + " at index " + pm.index);
                                continue;
                            }
                            player.partMaster.SetMesh(player.partMaster.seat, mo.mesh);
                        }
                        else if (pm.isCustom && !File.Exists(fullPath))
                            player.partMaster.SetMesh(player.partMaster.seat, FindObjectOfType<CustomMeshManager>().seats[0].mesh);
                        break;
                    case "frontRim":
                        if ((pm.isCustom && File.Exists(fullPath)) || !pm.isCustom)
                        {
                            MeshObject mo = FindObjectOfType<CustomMeshManager>().rims[pm.index];
                            if (mo == null)
                            {
                                Debug.Log("Unable to find MeshObject for " + pm.partName + " at index " + pm.index);
                                continue;
                            }
                            player.partMaster.SetMesh(player.partMaster.frontRim, mo.mesh);
                        }
                        else if (pm.isCustom && !File.Exists(fullPath))
                            player.partMaster.SetMesh(player.partMaster.frontRim, FindObjectOfType<CustomMeshManager>().rims[0].mesh);
                        break;
                    case "rearRim":
                        if ((pm.isCustom && File.Exists(fullPath)) || !pm.isCustom)
                        {
                            MeshObject mo = FindObjectOfType<CustomMeshManager>().rims[pm.index];
                            if (mo == null)
                            {
                                Debug.Log("Unable to find MeshObject for " + pm.partName + " at index " + pm.index);
                                continue;
                            }
                            player.partMaster.SetMesh(player.partMaster.rearRim, mo.mesh);
                        }
                        else if (pm.isCustom && !File.Exists(fullPath))
                            player.partMaster.SetMesh(player.partMaster.rearRim, FindObjectOfType<CustomMeshManager>().rims[0].mesh);
                        break;
                    case "frontSpokeAccessory":
                        if ((pm.isCustom && File.Exists(fullPath)) || !pm.isCustom)
                        {
                            MeshObject mo = FindObjectOfType<CustomMeshManager>().accessories[pm.index];
                            if (mo == null)
                            {
                                Debug.Log("Unable to find MeshObject for " + pm.partName + " at index " + pm.index);
                                continue;
                            }
                            player.partMaster.SetMesh(player.partMaster.frontAcc, mo.mesh);
                            Material[] mats = new Material[1];
                            if (pm.index >= 0 && pm.index < 4)
                            {
                                mats[0] = FindObjectOfType<CustomMeshManager>().accMats[pm.index];
                                player.partMaster.frontLightsOn = pm.index == 3 ? true : false;
                            }
                            else
                            {
                                mats[0] = FindObjectOfType<MaterialManager>().defaultMat;
                            }
                            player.partMaster.SetMaterials(player.partMaster.frontAcc, mats);

                        }
                        else if (pm.isCustom && !File.Exists(fullPath))
                        {
                            player.partMaster.SetMesh(player.partMaster.frontAcc, FindObjectOfType<CustomMeshManager>().accessories[0].mesh);
                            player.partMaster.SetMaterial(player.partMaster.frontAcc, FindObjectOfType<CustomMeshManager>().accMats[0]);
                        }
                        break;
                    case "rearSpokeAccessory":
                        if ((pm.isCustom && File.Exists(fullPath)) || !pm.isCustom)
                        {
                            MeshObject mo = FindObjectOfType<CustomMeshManager>().accessories[pm.index];
                            if (mo == null)
                            {
                                Debug.Log("Unable to find MeshObject for " + pm.partName + " at index " + pm.index);
                                continue;
                            }
                            player.partMaster.SetMesh(player.partMaster.rearAcc, mo.mesh);
                            Material[] mats = new Material[1];
                            if (pm.index >= 0 && pm.index < 4)
                            {
                                mats[0] = FindObjectOfType<CustomMeshManager>().accMats[pm.index];
                                player.partMaster.rearLightsOn = pm.index == 3 ? true : false;
                            }
                            else
                            {
                                mats[0] = FindObjectOfType<MaterialManager>().defaultMat;
                            }
                            player.partMaster.SetMaterials(player.partMaster.rearAcc, mats);


                        }
                        else if (pm.isCustom && !File.Exists(fullPath))
                        {
                            player.partMaster.SetMesh(player.partMaster.rearAcc, FindObjectOfType<CustomMeshManager>().accessories[0].mesh);
                            player.partMaster.SetMaterial(player.partMaster.rearAcc, FindObjectOfType<CustomMeshManager>().accMats[0]);
                        }
                        break;
                    case "barAccessory":
                        if ((pm.isCustom && File.Exists(fullPath)) || !pm.isCustom)
                        {
                            MeshObject mo = FindObjectOfType<CustomMeshManager>().barAccessories[pm.index];
                            if (mo == null)
                            {
                                Debug.Log("Unable to find MeshObject for " + pm.partName + " at index " + pm.index);
                                continue;
                            }
                            player.partMaster.SetMesh(player.partMaster.barAcc, mo.mesh);
                        }
                        else if (pm.isCustom && !File.Exists(fullPath))
                            player.partMaster.SetMesh(player.partMaster.barAcc, FindObjectOfType<CustomMeshManager>().barAccessories[0].mesh);
                        break;
                    case "frameAccessory":
                        if ((pm.isCustom && File.Exists(fullPath)) || !pm.isCustom)
                        {
                            MeshObject mo = FindObjectOfType<CustomMeshManager>().frameAccessories[pm.index];
                            if (mo == null)
                            {
                                Debug.Log("Unable to find MeshObject for " + pm.partName + " at index " + pm.index);
                                continue;
                            }
                            player.partMaster.SetMesh(player.partMaster.frameAcc, mo.mesh);
                        }
                        else if (pm.isCustom && !File.Exists(fullPath))
                            player.partMaster.SetMesh(player.partMaster.frameAcc, FindObjectOfType<CustomMeshManager>().frameAccessories[0].mesh);
                        break;
                    case "frontHubGuard":
                        if ((pm.isCustom && File.Exists(fullPath)) || !pm.isCustom)
                        {
                            MeshObject mo = FindObjectOfType<CustomMeshManager>().frontHubGuards[pm.index];
                            if (mo == null)
                            {
                                Debug.Log("Unable to find MeshObject for " + pm.partName + " at index " + pm.index);
                                continue;
                            }
                            player.partMaster.SetMesh(player.partMaster.frontHubG, mo.mesh);
                        }
                        else if (pm.isCustom && !File.Exists(fullPath))
                            player.partMaster.SetMesh(player.partMaster.frontHubG, FindObjectOfType<CustomMeshManager>().frontHubGuards[0].mesh);
                        break;
                    case "rearHubGuard":
                        if ((pm.isCustom && File.Exists(fullPath)) || !pm.isCustom)
                        {
                            MeshObject mo = FindObjectOfType<CustomMeshManager>().rearHubGuards[pm.index];
                            if (mo == null)
                            {
                                Debug.Log("Unable to find MeshObject for " + pm.partName + " at index " + pm.index);
                                continue;
                            }
                            player.partMaster.SetMesh(player.partMaster.rearHubG, mo.mesh);
                        }
                        else if (pm.isCustom && !File.Exists(fullPath))
                            player.partMaster.SetMesh(player.partMaster.rearHubG, FindObjectOfType<CustomMeshManager>().rearHubGuards[0].mesh);
                        break;
                    case "seatPost":
                        if ((pm.isCustom && File.Exists(fullPath)) || !pm.isCustom)
                        {
                            MeshObject mo = FindObjectOfType<CustomMeshManager>().seatPosts[pm.index];
                            if (mo == null)
                            {
                                Debug.Log("Unable to find MeshObject for " + pm.partName + " at index " + pm.index);
                                continue;
                            }
                            player.partMaster.SetMesh(player.partMaster.seatPost, mo.mesh);
                        }
                        else if (pm.isCustom && !File.Exists(fullPath))
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
        int index = (id % FindObjectOfType<PartManager>().tireMats.Length);
        Material[] mats = pm.GetMaterials(pm.frontTire);
        mats[0] = FindObjectOfType<PartManager>().tireMats[index];
        pm.SetMaterials(pm.frontTire, mats);
    }

    public void SetRearTireTread(RemotePartMaster pm, int id)
    {
        int index = (id % FindObjectOfType<PartManager>().tireMats.Length);
        Material[] mats = pm.GetMaterials(pm.rearTire);
        mats[0] = FindObjectOfType<PartManager>().tireMats[index];
        pm.SetMaterials(pm.rearTire, mats);
    }

    public void SetFrontTireWall(RemotePartMaster pm, int id)
    {
        int index = (id % FindObjectOfType<PartManager>().tireWallMats.Length);
        Material[] mats = new Material[2];
        mats[0] = pm.GetMaterials(pm.frontTire)[0];
        mats[1] = FindObjectOfType<PartManager>().tireWallMats[index];
        pm.SetMaterials(pm.frontTire, mats);
    }

    public void SetRearTireWall(RemotePartMaster pm, int id)
    {
        int index = (id % FindObjectOfType<PartManager>().tireWallMats.Length);
        Material[] mats = new Material[2];
        mats[0] = pm.GetMaterials(pm.rearTire)[0];
        mats[1] = FindObjectOfType<PartManager>().tireWallMats[index];
        pm.SetMaterials(pm.rearTire, mats);
    }
    private void LoadMaterials(RemotePlayer player, SaveList loadList)
    {
        try
        {
            MaterialManager matman = FindObjectOfType<MaterialManager>();
            Material[] defaultMats = new Material[1];
            defaultMats[0] = matman.defaultMat;
            foreach (PartMaterial p in loadList.partMaterials)
            {
                List<int> keys = PartNumToKey(p.partNum);
                foreach (int k in keys)
                {
                    switch (p.matID)
                    {
                        case 0:
                            Debug.Log("Setting material " + defaultMats[0].name + " on " + player.partMaster.GetPart(k).name);
                            player.partMaster.SetMaterials(k, defaultMats);
                            break;
                        case 7:
                            Debug.Log("Skipping material on " + player.partMaster.GetPart(k).name);
                            break;
                        case 9:
                            Debug.Log("Skipping material on " + player.partMaster.GetPart(k).name);
                            break;
                        default:
                            Material[] temp = new Material[1];
                            temp[0] = matman.customMats[p.matID - 1];
                            Debug.Log("Setting material " + matman.customMats[p.matID - 1].name + " on " + player.partMaster.GetPart(k).name);
                            player.partMaster.SetMaterials(k, temp);
                            break;
                    }
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

            SetMaterialHelper(player.partMaster, player.partMaster.leftGrip, loadList.leftGripMat);
            SetMaterialHelper(player.partMaster, player.partMaster.rightGrip, loadList.rightGripMat);
            SetMaterialHelper(player.partMaster, player.partMaster.leftCrank, loadList.leftCrankMat);
            SetMaterialHelper(player.partMaster, player.partMaster.rightCrank, loadList.rightCrankMat);
            SetMaterialHelper(player.partMaster, player.partMaster.leftPedal, loadList.leftPedalMat);
            SetMaterialHelper(player.partMaster, player.partMaster.rightPedal, loadList.rightPedalMat);

            foreach (MatData matData in loadList.matData)
            {
                player.partMaster.SetMaterialData(matData.key, matData.glossiness, matData.glossMapScale, matData.metallic, matData.texTileX, matData.texTileY, matData.normTileX, matData.normTileY, matData.metTileX, matData.metTileY);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message + "\n " + e.StackTrace + "\n ");
        }
    }

    public List<int> PartNumToKey(int partNum)
    {
        List<int> keys = new List<int>();
        PartMaster pm = FindObjectOfType<PartMaster>();
        switch (partNum)
        {
            case 0:
                keys.Add(pm.frame);
                break;
            case 1:
                keys.Add(pm.forks);
                break;
            case 2:
                keys.Add(pm.frontRim);
                keys.Add(pm.rearRim);
                break;
            case 3:
                keys.Add(pm.frontTire);
                keys.Add(pm.rearTire);
                break;
            case 4:
                keys.Add(pm.frontTire);
                keys.Add(pm.rearTire);
                break;
            case 5:
                keys.Add(pm.frontSpokes);
                keys.Add(pm.rearSpokes);
                break;
            case 6:
                keys.Add(pm.frontNipples);
                keys.Add(pm.rearNipples);
                break;
            case 7:
                keys.Add(pm.bars);
                break;
            case 8:
                keys.Add(pm.leftGrip);
                keys.Add(pm.rightGrip);
                break;
            case 9:
                keys.Add(pm.barEnds);
                break;
            case 10:
                keys.Add(pm.headSet);
                break;
            case 11:
                keys.Add(pm.headSetSpacers);
                break;
            case 12:
                keys.Add(pm.stem);
                break;
            case 13:
                keys.Add(pm.stemBolts);
                break;
            case 14:
                keys.Add(pm.frontPegs);
                keys.Add(pm.rearPegs);
                break;
            case 15:
                keys.Add(pm.bottomBracket);
                break;
            case 16:
                keys.Add(pm.leftCrank);
                keys.Add(pm.rightCrank);
                break;
            case 17:
                keys.Add(pm.frontHub);
                keys.Add(pm.rearHub);
                break;
            case 18:
                keys.Add(pm.leftPedal);
                keys.Add(pm.rightPedal);
                break;
            case 19:
                keys.Add(pm.sprocket);
                break;
            default:
                break;
        }
        return keys;
    }

    public void SetMaterialHelper(RemotePartMaster pm, int key, int mat, int index = 0)
    {
        MaterialManager matman = FindObjectOfType<MaterialManager>();
        Material[] defaultMats = new Material[1];
        defaultMats[0] = matman.defaultMat;
        if (mat == 0)
            pm.SetMaterials(key, defaultMats);
        else if (mat == 9 || mat == 7)
            return;
        else
        {
            Material[] temp = index == 1 ? new Material[2] : new Material[1];
            if (index == 1)
            {
                temp[0] = pm.GetMaterials(key)[0];
            }
            temp[index] = matman.customMats[mat - 1];
            pm.SetMaterials(key, temp);
        }

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
        BLO.SetGripsID(list.gripsID % BLO.barsApplyMod.gripMats.Length);
        BLO.SetSeatCover(list.seatID % BLO.seatApplyMod.seatCovers.Length);
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
        pm.GetPart(pm.rearWheel).transform.localRotation = (Quaternion.Euler(0, 180, 0f));
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
