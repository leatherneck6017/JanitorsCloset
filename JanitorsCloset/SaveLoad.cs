﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using KSP.UI;
using KSP.UI.Screens;

namespace JanitorsCloset
{
    partial class JanitorsCloset
    {

        const string CfgPath = "JanitorsCloset/PluginData";
        private static ConfigNode configFile = null;
        private static ConfigNode configBarNode = null;
        private static ConfigNode configButtonNode = null;


        public static readonly String ROOT_PATH = KSPUtil.ApplicationRootPath;
        private static readonly String CONFIG_BASE_FOLDER = ROOT_PATH + "GameData/";
        private static String JC_BASE_FOLDER = CONFIG_BASE_FOLDER + "JanitorsCloset/";
        private static String JC_NODE = "JANITORSCLOSET";
        private static String JC_CFG_FILE = JC_BASE_FOLDER + "PluginData/JanitorsCloset.cfg";
        private static String JC_DEFAULT_CFG_FILE = JC_BASE_FOLDER + "PluginData/JanitorsClosetDefault.cfg";
        private static String JC_BLACKLIST_FILE = JC_BASE_FOLDER + "PluginData/JCBlacklist.cfg";

        static string SafeLoad(string value, float oldvalue)
        {
            if (value == null)
                return oldvalue.ToString();
            return value;
        }
        static string SafeLoad(string value, int oldvalue)
        {
            if (value == null)
                return oldvalue.ToString();
            return value;
        }
        static string SafeLoad(string value, bool oldvalue)
        {
            if (value == null)
                return oldvalue.ToString();
            return value;
        }
        static string SafeLoad(string value, string oldvalue)
        {
            if (value == null)
                return oldvalue;
            return value;
        }
        static string SafeLoad(string value, GameScenes oldvalue)
        {
            if (value == null)
                return oldvalue.ToString();
            return value;
        }
        static string SafeLoad(string value, Blocktype oldvalue)
        {
            if (value == null)
                return oldvalue.ToString();
            return value;
        }

        void saveButtonData()
        {
            Log.Info("saveButtonData");
            ConfigNode janitorsClosetNode = new ConfigNode();

            for (int i = 0; i < (int)GameScenes.PSYSTEM; i++)
            {

                var bbl = buttonBarList[i];
                if (bbl.Count > 0)
                {
                    configBarNode = new ConfigNode(((GameScenes)i).ToString()); // scene

                    foreach (var bbi in bbl)
                    {
                        configButtonNode = new ConfigNode(bbi.Value.buttonHash); // button on main toolbar
                        configButtonNode.AddValue("folderIcon", bbi.Value.folderIcon);
                        foreach (var bBlockl in bbi.Value.buttonBlockList)
                        {
                            ConfigNode button = new ConfigNode();

                            button.AddValue("scene", bBlockl.Value.scene.ToString());
                            button.AddValue("blocktype", bBlockl.Value.blocktype.ToString());
                            button.AddValue("buttonHash", bBlockl.Value.buttonHash);


                            configButtonNode.AddNode(bBlockl.Value.buttonHash, button);

                        }
                        configBarNode.AddNode(bbi.Value.buttonHash, configButtonNode);
                    }
                    janitorsClosetNode.AddNode(((GameScenes)i).ToString(), configBarNode);

                }

            }

            configBarNode = new ConfigNode("Hidden");
            for (int i = 0; i < (int)GameScenes.PSYSTEM + 1; i++)
            {

                foreach (var bbi in JanitorsCloset.hiddenButtonBlockList[i])
                {
                    configButtonNode = new ConfigNode(bbi.Value.buttonHash); // button on main toolbar
                    configButtonNode.AddValue("buttonHash", bbi.Value.buttonHash);
                    configButtonNode.AddValue("scene", bbi.Value.scene);
                    configButtonNode.AddValue("blocktype", bbi.Value.blocktype);
                    configButtonNode.AddValue("active", bbi.Value.active);
                    configBarNode.AddNode(bbi.Value.buttonHash, configButtonNode);
                }
            }
            janitorsClosetNode.AddNode("Hidden", configBarNode);

            configBarNode = new ConfigNode("ButtonIDs");
            foreach (var bdi in buttonDictionary)
            {
                if (bdi.Value.identifier != "")
                    configBarNode.AddValue(bdi.Value.buttonHash, bdi.Value.identifier);
            }
            janitorsClosetNode.AddNode("ButtonIDs", configBarNode);
            configFile = new ConfigNode();
            configFile.AddNode(JC_NODE, janitorsClosetNode);
            configFile.Save(JC_CFG_FILE);
        }


        void loadButtonData()
        {
            loadedCfgs = new Dictionary<string, Cfg>();
            loadedHiddenCfgs = new Dictionary<string, ButtonSceneBlock>();
            Cfg cfg;
            foreach (string cfgFile in new List<string> { JC_DEFAULT_CFG_FILE, JC_CFG_FILE })
            {
                if (File.Exists(cfgFile))
                {
                    configFile = ConfigNode.Load(cfgFile);
                    ConfigNode janitorsClosetNode = configFile.GetNode(JC_NODE);
                    if (janitorsClosetNode != null)
                    {
                        foreach (ConfigNode n in janitorsClosetNode.GetNodes())  // n = scenes
                        {
                            Log.Info("Node.name: " + n.name);
                            if (n.name != "Hidden" && n.name != "ButtonIDs")
                            {
                                foreach (var n1 in n.GetNodes())
                                {
                                    foreach (var n2 in n1.GetNodes())
                                    {
                                        cfg = new Cfg();
                                        cfg.scene = (GameScenes)Enum.Parse(typeof(GameScenes), n2.GetValue("scene"));
                                        cfg.blocktype = (Blocktype)Enum.Parse(typeof(Blocktype), n2.GetValue("blocktype"));
                                        cfg.buttonHash = n2.GetValue("buttonHash");
                                        cfg.folderIcon = System.Int32.Parse(n1.GetValue("folderIcon"));

                                        cfg.toolbarButtonHash = n1.name;
                                        cfg.toolbarButtonIndex = cfg.folderIcon;
                                        loadedCfgs.Add(cfg.scene + cfg.buttonHash, cfg);
                                    }
                                }
                            }
                            else
                            {
                                if (n.name == "ButtonIDs")
                                {
                                    Log.Info("Loading ButtonIDs");
                                    // int cnt = 1;
                                    foreach (ConfigNode.Value n2 in n.values)
                                    {
                                        string hash = n2.name;
                                        string data = n2.value;
                                        Log.Info("hash: " + hash + "   data: " + data);
                                        var b = JanitorsCloset.Instance.buttonIdBDI(hash);
                                        if (b != null)
                                        {
                                            b.identifier = data;
                                        }
                                        else
                                        {
                                            b = new ButtonDictionaryItem();
                                            b.identifier = data;
                                            b.buttonHash = hash;
                                            b.button = new ApplicationLauncherButton();
                                            // b.button.name = cnt.ToString();
                                            //   cnt++;
                                            JanitorsCloset.buttonDictionary.Add(b.button, b);
                                        }
                                    }
                                }
                                if (n.name == "Hidden")
                                {
                                    // Hidden

                                    ButtonSceneBlock bsb;
                                    Log.Info("Loading Hidden");

                                    foreach (var n2 in n.GetNodes())
                                    {
                                        bsb = new ButtonSceneBlock();
                                        bsb.scene = (GameScenes)Enum.Parse(typeof(GameScenes), n2.GetValue("scene"));
                                        bsb.blocktype = (Blocktype)Enum.Parse(typeof(Blocktype), n2.GetValue("blocktype"));
                                        bsb.buttonHash = n2.GetValue("buttonHash");
                                        bsb.active = Boolean.Parse(n2.GetValue("active"));
                                        Log.Info("hidden button: " + bsb.buttonHash + "  blocktype: " + bsb.blocktype.ToString());
                                        if (bsb.blocktype == Blocktype.hideHere)
                                            loadedHiddenCfgs.Add(bsb.buttonHash + bsb.scene.ToString(), bsb);
                                        else
                                            loadedHiddenCfgs.Add(bsb.buttonHash, bsb);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        void saveBlacklistData(Dictionary<string,string> blacklistData)
        {
            ConfigNode blacklistNode = new ConfigNode("BLACKLIST");
            ConfigNode janitorsClosetNode = new ConfigNode(JC_NODE);
            configFile = new ConfigNode(JC_BLACKLIST_FILE);
            foreach (var bld in blacklistData)
            {
                blacklistNode.AddValue("blacklistIconHash", bld.Value);               
            }
            janitorsClosetNode.AddNode(blacklistNode);
            configFile.AddNode(janitorsClosetNode);
            configFile.Save(JC_BLACKLIST_FILE);
        }

        Dictionary<string, string> loadBlacklistData()
        {
            Dictionary<string, string> loadedBlacklist = new Dictionary<string, string>();
            Log.Info("loadBlacklistData");
            if (File.Exists(JC_BLACKLIST_FILE))
            {
                configFile = ConfigNode.Load(JC_BLACKLIST_FILE);
                if (configFile == null)
                {
                    Log.Info("Blacklist config file not loaded");
                    return loadedBlacklist;
                }
                ConfigNode janitorsClosetNode = configFile.GetNode(JC_NODE);
                if (janitorsClosetNode != null)
                {
                    ConfigNode blacklistNode = janitorsClosetNode.GetNode("BLACKLIST");
                    if (blacklistNode != null && blacklistNode.CountValues > 0)
                    {
                        foreach (var s in blacklistNode.GetValues("iconName"))
                        {
                            Log.Info("blacklistIcon: " + s);
                            loadedBlacklist.Add(s, s);
                        }
                        foreach (var s in blacklistNode.GetValues("iconHash"))
                        {
                            Log.Info("blacklistIconHash: " + s);
                            loadedBlacklist.Add(s, s);
                        }
                    }
                }
            }
            return loadedBlacklist;
        }
    }
}
