namespace FrostyPipeServer.ServerFiles
{
    using System;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using System.Threading.Tasks;
    using UnityEngine;

    [Serializable]
    /// <summary>
    /// Garage's Save Class
    /// </summary>
    [XmlRoot("BikeSave")]
    [XmlInclude(typeof(PartColor))]
    public class GarageSaveList
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

        public int barBrakeIndex;
        public int frameBrakeIndex;

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
        public int leftGripMat;
        public int rightGripMat;
        public int leftCrankMat;
        public int rightCrankMat;
        public int leftPedalMat;
        public int rightPedalMat;
        public int brakeMat;
        public int brakeCableMat;

        public List<PartMesh> partMeshes;
        public List<PartColor> partColors;
        public List<PartMaterial> partMaterials;
        public List<PartTexture> partTextures;
        public List<PartPosition> partPositions;
        public List<MatData> matData;

        public GarageSaveList()
        {
            partMeshes = new List<PartMesh>();
            partColors = new List<PartColor>();
            partMaterials = new List<PartMaterial>();
            partTextures = new List<PartTexture>();
            partPositions = new List<PartPosition>();
            matData = new List<MatData>();
        }

    }

    [Serializable]
    /// <summary>
    /// Class to represent part number and color associated with the bike part
    /// </summary>
    [XmlType("PartColor")]
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

    [Serializable]
    /// <summary>
    /// Class to represent part number, texture url, and whether the texture is main, normal or metallic
    /// </summary>
    [XmlType("PartTexture")]
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

    [Serializable]
    /// <summary>
    /// Class to represent a part number and associated material
    /// </summary>
    [XmlType("PartMaterial")]
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

    [Serializable]
    [XmlType("MatData")]
    public class MatData
    {
        public int key;
        public float glossiness;
        public float glossMapScale;
        public float texTileX, texTileY;
        public float normTileX, normTileY;
        public float metTileX, metTileY;
        public float metallic;
        public bool isFade;

        public MatData(int key, float glossiness, float glossMapScale, float texTileX, float texTileY, float normTileX, float normTileY, float metTileX, float metTileY, float metallic, bool isFade)
        {
            this.key = key;
            this.glossiness = glossiness;
            this.glossMapScale = glossMapScale;
            this.texTileX = texTileX;
            this.texTileY = texTileY;
            this.normTileX = normTileX;
            this.normTileY = normTileY;
            this.metTileX = metTileX;
            this.metTileY = metTileY;
            this.metallic = metallic;
            this.isFade = isFade;
        }

        public MatData()
        {

        }

    }

    [Serializable]
    [XmlType("PartMesh")]
    public class PartMesh
    {
        public int index;
        public int key;
        public bool isCustom;
        public string fileName;
        public string partName;

        public PartMesh(int index, int key, bool isCustom, string fileName, string partName)
        {
            this.index = index;
            this.key = key;
            this.isCustom = isCustom;
            this.fileName = fileName;
            this.partName = partName;
        }

        public PartMesh()
        {

        }

    }

    [Serializable]
    [XmlType("PartPosition")]
    public class PartPosition
    {
        public int partNum;
        public float x, y, z;
        public float scaleX, scaleY, scaleZ;
        public float rotX, rotY, rotZ;
        public bool isVisible;

        public PartPosition(int key, Vector3 pos, Vector3 scale, Vector3 rot, bool visible)
        {
            partNum = key;
            x = pos.x;
            y = pos.y;
            z = pos.z;
            rotX = rot.x;
            rotY = rot.y;
            rotZ = rot.z;
            scaleX = scale.x;
            scaleY = scale.y;
            scaleZ = scale.z;
            isVisible = visible;
        }

        public PartPosition()
        {

        }

    }






}
