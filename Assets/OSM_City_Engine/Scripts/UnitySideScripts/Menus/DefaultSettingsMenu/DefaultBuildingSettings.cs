﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.ConfigHandler;
using Assets.Scripts.UnitySideScripts.MouseScripts;

namespace Assets.Scripts.UnitySideScripts.Menus
{
    class DefaultBuildingSettings : MonoBehaviour
    {

        private bool colorTextureSelect = false; 
        private bool normalTextureSelect = false; 
        private bool specularTextureSelect = false;

        private string colorTexturePath, normalTexturePath, specularTexturePath;
        private Texture2D colorTexture, normalTexture, specularTexture;
        private List<BuildingMaterial> materialList;
        private GameObject buildingMenu;


        private GameObject fileBrowser;
        private myFileBrowserDialog fbd;

        void Start()
        {
            fileBrowser = GameObject.Instantiate((GameObject)Resources.Load("Prefabs/Menu/FileBrowser"));
            fileBrowser.transform.SetParent(GameObject.Find("Canvas").transform);
            fileBrowser.SetActive(false);
            RectTransform rt = fileBrowser.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 0);
            fbd = fileBrowser.GetComponent<myFileBrowserDialog>();

            buildingMenu = GameObject.Find("Building Menu");
            loadConfig();

            colorTexturePath = "";
            normalTexturePath = "";
            specularTexturePath = "";

        }

        void Update()
        {
            if (fbd.state == myFileBrowserDialog.BrowserState.Selected)
            {
                if(colorTextureSelect)
                {
                    colorTexturePath = fbd.selectedPath;
                    GameObject createSkinComp = transform.Find("Panel").Find("CreateSkinComponentBuilding").gameObject;
                    GameObject colorTextureImageView = createSkinComp.transform.Find("Panel").gameObject.transform.Find("ColorTexture").gameObject;
                    GameObject colorTextureText = createSkinComp.transform.Find("Panel").gameObject.transform.Find("Text_Color").gameObject;

                    colorTextureText.GetComponent<Text>().text = " ";

                    byte[] fileData = File.ReadAllBytes(colorTexturePath);

                    colorTexture = new Texture2D(2, 2);
                    colorTexture.LoadImage(fileData);

                    var colorRaw = colorTextureImageView.GetComponent<RawImage>();
                    colorRaw.texture = colorTexture;
                    colorTextureSelect = false;
                    buildingMenu.transform.Find("Panel").gameObject.SetActive(true);

                }
                else if(normalTextureSelect)
                {
                    normalTexturePath = fbd.selectedPath;
                    GameObject createSkinComp = transform.Find("Panel").Find("CreateSkinComponentBuilding").gameObject;
                    GameObject normalTextureImageView = createSkinComp.transform.Find("Panel").gameObject.transform.Find("NormalTexture").gameObject;
                    GameObject normalTextureText = createSkinComp.transform.Find("Panel").gameObject.transform.Find("Text_Normal").gameObject;

                    normalTextureText.GetComponent<Text>().text = " ";

                    byte[] fileData = File.ReadAllBytes(normalTexturePath);

                    normalTexture = new Texture2D(2, 2);
                    normalTexture.LoadImage(fileData);

                    var normalRaw = normalTextureImageView.GetComponent<RawImage>();
                    normalRaw.texture = normalTexture;
                    normalTextureSelect = false;
                    buildingMenu.transform.Find("Panel").gameObject.SetActive(true);

                }

                else if(specularTextureSelect)
                {
                    specularTexturePath = fbd.selectedPath;
                    GameObject createSkinComp = transform.Find("Panel").Find("CreateSkinComponentBuilding").gameObject;
                    GameObject specularTextureImageView = createSkinComp.transform.Find("Panel").gameObject.transform.Find("SpecularTexture").gameObject;
                    GameObject specularTextureText = createSkinComp.transform.Find("Panel").gameObject.transform.Find("Text_Specular").gameObject;

                    specularTextureText.GetComponent<Text>().text = " ";

                    byte[] fileData = File.ReadAllBytes(specularTexturePath);

                    specularTexture = new Texture2D(2, 2);
                    specularTexture.LoadImage(fileData);

                    var specularRaw = specularTextureImageView.GetComponent<RawImage>();
                    specularRaw.texture = specularTexture;
                    specularTextureSelect = false;
                    buildingMenu.transform.Find("Panel").gameObject.SetActive(true);


                }

                
            }
            
            if(fbd.state == myFileBrowserDialog.BrowserState.Cancelled)
                buildingMenu.transform.Find("Panel").gameObject.SetActive(true);
        }

        public void clickColorTexture()
        {
           colorTextureSelect = true;
           DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
           fbd.draw(myFileBrowserDialog.BrowserMode.FileSelect, di, new string[] { ".png", ".bmp", ".jpg", ".tif" });
           buildingMenu.transform.Find("Panel").gameObject.SetActive(false);
        }

        public void clickNormalTexture()
        {
            normalTextureSelect = true;
            DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
            fbd.draw(myFileBrowserDialog.BrowserMode.FileSelect, di, new string[] { ".png", ".bmp", ".jpg", ".tif" });
            buildingMenu.transform.Find("Panel").gameObject.SetActive(false);
        }

        public void clickSpecularTexture()
        {
            specularTextureSelect = true;
            DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
            fbd.draw(myFileBrowserDialog.BrowserMode.FileSelect, di, new string[] { ".png", ".bmp", ".jpg", ".tif" });
            buildingMenu.transform.Find("Panel").gameObject.SetActive(false);
        }

        public void clickAddSkin()
        {
            GameObject createSkinComp = buildingMenu.transform.Find("Panel").Find("CreateSkinComponentBuilding").gameObject; 
            GameObject skinList = buildingMenu.transform.Find("Panel").Find("ScrollRect").Find("Content Panel").gameObject;
            GameObject skinItem = (GameObject)MonoBehaviour.Instantiate(Resources.Load("Prefabs/Menu/BuildingSkin"));

            Toggle skinitemToggle = skinItem.transform.Find("Panel").Find("Toggle").gameObject.GetComponent<Toggle>();
            skinitemToggle.isOn = true;

            Text skinitemName = skinItem.transform.Find("Panel").Find("Text_Type").gameObject.GetComponent<Text>();
            InputField ifnewItemName = createSkinComp.transform.Find("Panel").Find("InputField_SkinName").GetComponent<InputField>();
            skinitemName.text = ifnewItemName.text;

            Text skinitemSize = skinItem.transform.Find("Panel").Find("Text_Size").GetComponent<Text>();
            InputField ifnewItemSize = createSkinComp.transform.Find("Panel").Find("InputField_TextureWidth").GetComponent<InputField>();
            skinitemSize.text = ifnewItemSize.text;

            RawImage skinitemColor = skinItem.transform.Find("Panel").Find("Image_ColorTexture").GetComponent<RawImage>();
            skinitemColor.texture = colorTexture;
            RawImage skinitemNormal = skinItem.transform.Find("Panel").Find("Image_NormalTexture").GetComponent<RawImage>();
            skinitemNormal.texture = normalTexture;
            RawImage skinitemSpecular = skinItem.transform.Find("Panel").Find("Image_SpecularTexture").GetComponent<RawImage>();
            skinitemSpecular.texture = specularTexture;

            skinItem.transform.SetParent(skinList.transform);

            BuildingMaterial mat = new BuildingMaterial();
            mat.isActive = true;
            mat.name = skinitemName.text;
            mat.width = float.Parse(skinitemSize.text);
            mat.colorTexturePath = colorTexturePath;
            mat.normalTexturePath = normalTexturePath;
            mat.specularTexturePath = specularTexturePath;
            materialList.Add(mat);


            //Clear Menu

            ifnewItemSize.text = "";
            ifnewItemName.text = "";

            GameObject specularTextureImageView = createSkinComp.transform.Find("Panel").gameObject.transform.Find("SpecularTexture").gameObject;
            specularTextureImageView.GetComponent<RawImage>().texture = null;
            GameObject specularTextureText = createSkinComp.transform.Find("Panel").gameObject.transform.Find("Text_Specular").gameObject;
            specularTextureText.GetComponent<Text>().text = "Click to Add Specular Texture";

            GameObject colorTextureImageView = createSkinComp.transform.Find("Panel").gameObject.transform.Find("ColorTexture").gameObject;
            colorTextureImageView.GetComponent<RawImage>().texture = null;
            GameObject colorTextureText = createSkinComp.transform.Find("Panel").gameObject.transform.Find("Text_Color").gameObject;
            colorTextureText.GetComponent<Text>().text = "Click to Add Color Texture";
            
            GameObject normalTextureImageView = createSkinComp.transform.Find("Panel").gameObject.transform.Find("NormalTexture").gameObject;
            normalTextureImageView.GetComponent<RawImage>().texture = null;
            GameObject normalTextureText = createSkinComp.transform.Find("Panel").gameObject.transform.Find("Text_Normal").gameObject;
            normalTextureText.GetComponent<Text>().text = "Click to Add Normal Texture";
        
        }

        public void clickCancel()
        {
            loadConfig();

            GameObject createSkinComp = buildingMenu.transform.Find("Panel").Find("CreateSkinComponentBuilding").gameObject;
            InputField ifnewItemSize = createSkinComp.transform.Find("Panel").Find("InputField_TextureWidth").GetComponent<InputField>();
            ifnewItemSize.text = "";
            InputField ifnewItemName = createSkinComp.transform.Find("Panel").Find("InputField_SkinName").GetComponent<InputField>();  
            ifnewItemName.text = "";

            GameObject specularTextureImageView = createSkinComp.transform.Find("Panel").gameObject.transform.Find("SpecularTexture").gameObject;
            specularTextureImageView.GetComponent<RawImage>().texture = null;
            GameObject specularTextureText = createSkinComp.transform.Find("Panel").gameObject.transform.Find("Text_Specular").gameObject;
            specularTextureText.GetComponent<Text>().text = "Click to Add Specular Texture";

            GameObject colorTextureImageView = createSkinComp.transform.Find("Panel").gameObject.transform.Find("ColorTexture").gameObject;
            colorTextureImageView.GetComponent<RawImage>().texture = null;
            GameObject colorTextureText = createSkinComp.transform.Find("Panel").gameObject.transform.Find("Text_Color").gameObject;
            colorTextureText.GetComponent<Text>().text = "Click to Add Color Texture";

            GameObject normalTextureImageView = createSkinComp.transform.Find("Panel").gameObject.transform.Find("NormalTexture").gameObject;
            normalTextureImageView.GetComponent<RawImage>().texture = null;
            GameObject normalTextureText = createSkinComp.transform.Find("Panel").gameObject.transform.Find("Text_Normal").gameObject;
            normalTextureText.GetComponent<Text>().text = "Click to Add Normal Texture";

            buildingMenu.SetActive(false);

        }

        public void clickSave()
        {
            InitialConfigLoader loader = new InitialConfigLoader();
            InitialConfigurations config = loader.loadInitialConfig();

            //Create new Building Config Part
            BuildingConfigurations newbuildingconfig = new BuildingConfigurations();

            InputField min = buildingMenu.transform.Find("Panel").Find("HeightComponent").Find("InputField_Min").GetComponent<InputField>();
            newbuildingconfig.minheight = float.Parse(min.text);
            InputField max = buildingMenu.transform.Find("Panel").Find("HeightComponent").Find("InputField_Max").GetComponent<InputField>();
            newbuildingconfig.maxheight = float.Parse(max.text);
            newbuildingconfig.defaultSkins = materialList;

            Transform contentPanel = buildingMenu.transform.Find("Panel").Find("ScrollRect").Find("Content Panel");
            int index = 0;
            foreach (Transform child in contentPanel)
            {
                Toggle t=  child.Find("Panel").Find("Toggle").GetComponent<Toggle>();
                newbuildingconfig.defaultSkins[index].isActive = t.isOn;
                index++;
            }


            config.buildingConfig = newbuildingconfig;

            loader.saveInitialConfig(Path.Combine(Application.streamingAssetsPath, "ConfigFiles/initialConfig.xml"), config);

            buildingMenu.SetActive(false);
        }

        public void clickReset()
        {
            InitialConfigLoader loader = new InitialConfigLoader();
            InitialConfigurations config = loader.loadInitialConfig();
            InitialConfigurations tmpconfig = loader.fillConfig();
            config.buildingConfig = tmpconfig.buildingConfig;
            configLoadHelper(config);
        }

        private void loadConfig()
        {
            InitialConfigLoader loader = new InitialConfigLoader();
            InitialConfigurations config = loader.loadInitialConfig();
            configLoadHelper(config);
        }

        private void configLoadHelper(InitialConfigurations config)
        {
            var children = new List<GameObject>();
            var parentTransform = buildingMenu.transform.Find("Panel").Find("ScrollRect").Find("Content Panel");
            foreach (Transform child in parentTransform) children.Add(child.gameObject);
            children.ForEach(child => Destroy(child));

            materialList = config.buildingConfig.defaultSkins;

            InputField min = buildingMenu.transform.Find("Panel").Find("HeightComponent").Find("InputField_Min").GetComponent<InputField>();
            min.text = config.buildingConfig.minheight.ToString();

            InputField max = buildingMenu.transform.Find("Panel").Find("HeightComponent").Find("InputField_Max").GetComponent<InputField>();
            max.text = config.buildingConfig.maxheight.ToString();

            GameObject skinList = buildingMenu.transform.Find("Panel").Find("ScrollRect").Find("Content Panel").gameObject;

            for (int k = 0; k < config.buildingConfig.defaultSkins.Count; k++)
            {

                GameObject skinItem = (GameObject)MonoBehaviour.Instantiate(Resources.Load("Prefabs/Menu/BuildingSkin"));

                Toggle skinitemToggle = skinItem.transform.Find("Panel").Find("Toggle").gameObject.GetComponent<Toggle>();
                skinitemToggle.isOn = config.buildingConfig.defaultSkins[k].isActive;

                Text skinitemName = skinItem.transform.Find("Panel").Find("Text_Type").gameObject.GetComponent<Text>();
                skinitemName.text = config.buildingConfig.defaultSkins[k].name;

                Text skinitemSize = skinItem.transform.Find("Panel").Find("Text_Size").GetComponent<Text>();
                skinitemSize.text = config.buildingConfig.defaultSkins[k].width.ToString();

                byte[] fileData;

                RawImage skinitemColor = skinItem.transform.Find("Panel").Find("Image_ColorTexture").GetComponent<RawImage>();
                Texture2D colorText;
                if (File.Exists(config.buildingConfig.defaultSkins[k].colorTexturePath))
                {
                    fileData = File.ReadAllBytes(config.buildingConfig.defaultSkins[k].colorTexturePath);
                    colorText = new Texture2D(2, 2);
                    colorText.LoadImage(fileData);
                    skinitemColor.texture = colorText;
                }
                else
                    skinitemColor.texture = null;

                RawImage skinitemNormal = skinItem.transform.Find("Panel").Find("Image_NormalTexture").GetComponent<RawImage>();
                Texture2D normalText;
                if (File.Exists(config.buildingConfig.defaultSkins[k].normalTexturePath))
                {
                    fileData = File.ReadAllBytes(config.buildingConfig.defaultSkins[k].normalTexturePath);
                    normalText = new Texture2D(2, 2);
                    normalText.LoadImage(fileData);
                    skinitemNormal.texture = normalText;
                }
                else
                    skinitemNormal.texture = null;

                RawImage skinitemSpecular = skinItem.transform.Find("Panel").Find("Image_SpecularTexture").GetComponent<RawImage>();
                Texture2D specularText;
                if (File.Exists(config.buildingConfig.defaultSkins[k].specularTexturePath))
                {
                    fileData = File.ReadAllBytes(config.buildingConfig.defaultSkins[k].specularTexturePath);
                    specularText = new Texture2D(2, 2);
                    specularText.LoadImage(fileData);
                    skinitemSpecular.texture = specularText;
                }
                else
                    skinitemSpecular.texture = null;
                

                skinItem.transform.SetParent(skinList.transform);
            }

        }


    }
}
