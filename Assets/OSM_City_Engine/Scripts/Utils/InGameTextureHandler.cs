using Assets.Scripts.SceneObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    class InGameTextureHandler
    {
        private const string ShaderStandartSpecular = "Standard (Specular setup)";
        private const string ShaderDiffuse = "Bumped Diffuse";
        private const string ShaderURP = "Universal Render Pipeline/Lit";

        public static Texture2D getTexture(string path)
        {
            return LoadResourceTexture(path);
            if (path == "")
                return null;

            byte[] fileData;
            Texture2D tex = new Texture2D(2, 2);
            fileData = File.ReadAllBytes(path);
            tex.LoadImage(fileData);           
            return tex;
        }

        public static Texture2D getNormalTexture(string filePath)
        {
            byte[] fileData;
            Texture2D loadedTexture;

            fileData = File.ReadAllBytes(filePath);
            loadedTexture = new Texture2D(2, 2);
            loadedTexture.LoadImage(fileData);
            return NormalMap(loadedTexture);
        }

        public static Texture2D getTextureResource(string resourcePath)
        {
            string path = resourcePath.Substring((Application.dataPath + "/Resources/").Length);
            path = path.Substring(0, path.Length - 4);
            return (Texture2D)Resources.Load(path);
        }

        public static Texture2D LoadResourceTexture(string resourcePath)
        {
            if (resourcePath.Contains("/Resources/"))
            {
                var index = resourcePath.IndexOf("/Resources/") + "/Resources/".Length;
                resourcePath = resourcePath.Substring(index);
            }
            
            var path = resourcePath.Substring(0, resourcePath.Length - 4);
            return (Texture2D)Resources.Load(path);
        }

        public static Texture2D NormalMap(Texture2D source)
        {
            
            Texture2D normalTexture = new Texture2D(source.width, source.height, TextureFormat.ARGB32, true);
            Color theColour = new Color();
            for (int x = 0; x < source.width; x++)
            {
                for (int y = 0; y < source.height; y++)
                {
                    theColour.r = 0;
                    theColour.g = source.GetPixel(x, y).g;
                    theColour.b = 0;
                    theColour.a = source.GetPixel(x, y).r;
                    normalTexture.SetPixel(x, y, theColour);
                }
            }
            normalTexture.Apply();
            return normalTexture;
                }

        public static Material createMaterial(string colorTexturePath, string normalTexturePath, string specularTexturePath)
        {
            Material mat = new Material(Shader.Find(ShaderStandartSpecular));

            Texture2D colortex;
            Texture2D normaltex;
            Texture2D speculartex;

            byte[] fileData;

            if (colorTexturePath != "")
            {
                fileData = File.ReadAllBytes(colorTexturePath);
                colortex = new Texture2D(2, 2);

                colortex.LoadImage(fileData);
                mat.SetTexture("_MainTex", colortex);
            }

            if (normalTexturePath != "")
            {
                normaltex = getNormalTexture(normalTexturePath);
                mat.SetTexture("_BumpMap", normaltex);
                mat.SetFloat("_BumpScale", 1.0f);
            }

            mat.SetFloat("_Glossiness", 0.1f);

            if (specularTexturePath != "")
            {
                fileData = File.ReadAllBytes(specularTexturePath);
                speculartex = new Texture2D(2, 2);
                speculartex.LoadImage(fileData);
                mat.SetTexture("_SpecGlossMap", speculartex);
            }

            return mat;

        }

        public static Material createMaterial(FacadeSkin skin)
        {
            Material mat = new Material(Shader.Find(ShaderStandartSpecular));

            Texture2D colortex = getTexture(skin.colorTexturePath);
            Texture2D normaltex = getTexture(skin.normalTexturePath);
            Texture2D speculartex = getTexture(skin.specularTexturePath);

            mat.mainTexture = colortex;// .SetTexture("_MainTex", colortex);
            mat.SetTexture("_BumpMap", normaltex);
            //mat.SetFloat("_BumpScale", 1.0f);
            //mat.SetFloat("_Glossiness", 0.1f);
            mat.SetTexture("_SpecGlossMap", speculartex);

            return mat;
        }

        public static Material createMaterial2(string colorTexturePath, string normalTexturePath, string specularTexturePath)
        {

            Material mat = new Material(Shader.Find(ShaderURP));
            try
            {
                mat.mainTexture = LoadResourceTexture(colorTexturePath); //.SetTexture("_MainTex", LoadResourceTexture(colorTexturePath));
                mat.SetTexture("_BumpMap", LoadResourceTexture(normalTexturePath));
                //mat.SetFloat("_BumpScale", 1.0f);
            }
            catch (Exception)
            {

            }
   

            //WWW loader;
            //Material mat = new Material(Shader.Find(ShaderDiffuse));

            //loader = new WWW("file://" + colorTexturePath);
            //mat.SetTexture("_MainTex", loader.texture);

            //loader = new WWW("file://" + normalTexturePath);
            //Texture2D normalTexture = NormalMap(loader.texture);
            //mat.SetTexture("_BumpMap", normalTexture);
            //mat.SetFloat("_BumpScale", 1.0f);
            return mat;
        }
    
    }
}
