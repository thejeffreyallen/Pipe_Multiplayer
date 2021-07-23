using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Class to represent all data of a saved bike preset
/// </summary>
[XmlRoot("BikeSave")]
[XmlInclude(typeof(PartColor))]
[Serializable]
public class SaveList
{
    public bool brakes;
    public bool flanges;
    public bool LHD;

    public float seatAngle;
    public float barsAngle;
    public float bikeScale;
    public float frontTireWidth;
    public float rearTireWidth;
    public float seatHeight;

    public int seatID;
    public int gripsID;
    public int frontTreadID;
    public int frontWallID;
    public int rearTreadID;
    public int rearWallID;

    public int seatPostMat;
    public int frontTireMat;
    public int rearTireMat;
    public int frontTireWallMat;
    public int rearTireWallMat;
    public int frontRimMat;
    public int rearRimMat;
    public int frontHubMat;
    public int rearHubMat;
    public int frontSpokesMat;
    public int rearSpokesMat;
    public int frontNipplesMat;
    public int rearNipplesMat;

    public List<PartMesh> partMeshes;
    public List<PartColor> partColors;
    public List<PartMaterial> partMaterials;
    public List<PartTexture> partTextures;
    public List<PartPosition> partPositions;
    public List<MatData> matData;

    public SaveList()
    {
        partMeshes = new List<PartMesh>();
        partColors = new List<PartColor>();
        partMaterials = new List<PartMaterial>();
        partTextures = new List<PartTexture>();
        partPositions = new List<PartPosition>();
        matData = new List<MatData>();
    }

}

/// <summary>
/// Class to represent part number and color associated with the bike part
/// </summary>
[XmlType("PartColor")]
[Serializable]
public class PartColor
{
    public int partNum;
    public float r;
    public float g;
    public float b;
    public float a;

    public PartColor(int partNum, Color col)
    {
        this.partNum = partNum;
        r = col.r;
        g = col.g;
        b = col.b;
        a = col.a;
    }

    public PartColor()
    {

    }
}

/// <summary>
/// Class to represent part number, texture url, and whether the texture is main, normal or metallic
/// </summary>
[XmlType("PartTexture")]
[Serializable]
public class PartTexture
{
    public string url;
    public int partNum;
    public bool normal;
    public bool metallic;

    public PartTexture(string url, int partNum, bool normal, bool metallic)
    {
        this.url = url;
        this.partNum = partNum;
        this.normal = normal;
        this.metallic = metallic;
    }

    public PartTexture()
    {

    }

}

/// <summary>
/// Class to represent a part number and associated material
/// </summary>
[XmlType("PartMaterial")]
[Serializable]
public class PartMaterial
{
    public int matID;
    public int partNum;

    public PartMaterial(int matID, int partNum)
    {
        this.matID = matID;
        this.partNum = partNum;
    }

    public PartMaterial()
    {

    }

}

[XmlType("MatData")]
[Serializable]
public class MatData
{
    public int key;
    public float glossiness;
    public float glossMapScale;

    public MatData(int key, float glossiness, float glossMapScale)
    {
        this.key = key;
        this.glossiness = glossiness;
        this.glossMapScale = glossMapScale;
    }

    public MatData()
    {

    }

}

[XmlType("PartMesh")]
[Serializable]
public class PartMesh
{
    public int partNum;
    public bool isCustom;
    public string fileName;
    public string partName;

    public PartMesh(int partNum, bool isCustom, string fileName, string partName)
    {
        this.partNum = partNum;
        this.isCustom = isCustom;
        this.fileName = fileName;
        this.partName = partName;
    }

    public PartMesh()
    {

    }

}

[XmlType("PartPosition")]
[Serializable]
public class PartPosition
{
    public int partNum;
    public float x, y, z;
    public float scaleX, scaleY, scaleZ;
    public bool isVisible;

    public PartPosition(int key, Vector3 pos, Vector3 scale, bool visible)
    {
        partNum = key;
        x = pos.x;
        y = pos.y;
        z = pos.z;
        scaleX = scale.x;
        scaleY = scale.y;
        scaleZ = scale.z;
        isVisible = visible;
    }

    public PartPosition()
    {

    }

}





