using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class Packager
{
    public static string platform = string.Empty;
    static List<string> paths = new List<string>();
    static List<string> files = new List<string>();

    ///-----------------------------------------------------------
    static string[] exts = { ".txt", ".xml", ".lua", ".assetbundle" };
    static bool CanCopy(string ext)
    {   //�ܲ��ܸ���
        foreach (string e in exts)
        {
            if (ext.Equals(e)) return true;
        }
        return false;
    }

    [MenuItem("ME Tools/�������°�����dataĿ¼ѹ��Ϊһ��zip��������StreamingAssetsĿ¼")]
    static void PackFiles()
    {   
  
        string assetPath = Application.dataPath + "/StreamingAssets/";
        string srcPath = Application.dataPath + "/data/";

        cleanMeta(srcPath);

        //ѹ���ļ�    
        API.PackFiles(assetPath + "/data.zip", srcPath);
        AssetDatabase.Refresh();

        Debug.Log("dataĿ¼ѹ������ɹ����ļ���" + assetPath + "/data.zip");
        Debug.Log("������ļ��ϴ���web���·�����Ŀ¼��");

    }
    /*
    [MenuItem("Game/UnZIP Data folder ")] 
    static void UnpackFiles()
    {
        //��ѹ�ļ�
        Util.UnpackFiles(Application.dataPath + "/data.zip", Application.dataPath + "/data/");
        AssetDatabase.Refresh();
    }
    */
    /// <summary>
    /// �����ز�
    /// </summary>
    static UnityEngine.Object LoadAsset(string file)
    {
        if (file.EndsWith(".lua")) file += ".txt";
        return AssetDatabase.LoadMainAssetAtPath("Assets/Builds/" + file);
    }

    //�������
    [MenuItem("ME Tools/�������ѡ��Ŀ¼�µĸ������󲢷���dataĿ¼")]
    static void CreateAssetBunldesMain()
    {
        //��ȡ��Project��ͼ��ѡ���������Ϸ����
        Object[] SelectedAsset = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        BuildTarget target = GetTargetPlatform();
        string assetPath = Application.dataPath + "/data/asset/" + target + "/";
        //�������е���Ϸ���� 
        foreach (Object obj in SelectedAsset)
        {            
            //���ز��ԣ��������Assetbundle����StreamingAssets�ļ����£����û�оʹ���һ������Ϊ�ƶ�ƽ̨��ֻ�ܶ�ȡ���·��
            //StreamingAssets��ֻ��·��������д��
            //���������أ��Ͳ���Ҫ��������������Ͽͻ�����www��������ء�
            if (obj is GameObject)
            {
                string targetPath = assetPath + obj.name + ".assetbundle";
                if (BuildPipeline.BuildAssetBundle(obj, null, targetPath, BuildAssetBundleOptions.CollectDependencies, target))
                {
                    Debug.Log(obj.name + " ==>��Դ����ɹ�");
                }
                else
                {
                    Debug.Log(obj.name + " ==>��Դ���ʧ��");
                }
            }
        }
        //ˢ�±༭�� 
        AssetDatabase.Refresh();
    }
    /// <summary>
    /// ���ɰ��ز�
    /// </summary>
    [MenuItem("ME Tools/��BuildsĿ¼�µ���Դ�����������������dataĿ¼")]
    public static void BuildAssetResource()
    {
        Object mainAsset = null;        //���ز���������
        Object[] addis = null;     //�����ز��������
        string assetfile = string.Empty;  //�ز��ļ���

        BuildAssetBundleOptions options = BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.CollectDependencies |
                                          BuildAssetBundleOptions.CompleteAssets | BuildAssetBundleOptions.DeterministicAssetBundle;

        BuildTarget target = GetTargetPlatform();

        string assetPath = Application.dataPath + "/data/asset/" + target + "/";
        if (!Directory.Exists(assetPath)) Directory.CreateDirectory(assetPath);

        ///-----------------------------���ɹ���Ĺ������زİ�-------------------------------------
        BuildPipeline.PushAssetDependencies();

        assetfile = assetPath + "shared.assetbundle";
        mainAsset = LoadAsset("Shared/Atlas/Dialog.prefab");
        BuildPipeline.BuildAssetBundle(mainAsset, null, assetfile, options,target);

        ///------------------------------����PromptPanel�زİ�-----------------------------------
        BuildPipeline.PushAssetDependencies();
        mainAsset = LoadAsset("Prompt/Prefabs/PromptPanel.prefab");
        addis = new Object[1];
        addis[0] = LoadAsset("Prompt/Prefabs/PromptItem.prefab");
        assetfile = assetPath + "prompt.assetbundle";
        BuildPipeline.BuildAssetBundle(mainAsset, addis, assetfile, options, target);
        BuildPipeline.PopAssetDependencies();

        ///------------------------------����MessagePanel�زİ�-----------------------------------
        BuildPipeline.PushAssetDependencies();
        mainAsset = LoadAsset("Message/Prefabs/MessagePanel.prefab");
        assetfile = assetPath + "message.assetbundle";
        BuildPipeline.BuildAssetBundle(mainAsset, null, assetfile, options, target);
        BuildPipeline.PopAssetDependencies();

        ///-------------------------------ˢ��---------------------------------------
        BuildPipeline.PopAssetDependencies();
        AssetDatabase.Refresh();
    }


    [MenuItem("ME Tools/��AtlasĿ¼�µ�.pngͼƬ��Ϊͼ����Դ������dataĿ¼")]
    static private void BuildUnityGUIAssetBundle()
    {
       // string dir = Application.dataPath + "/StreamingAssets";
        BuildTarget target = GetTargetPlatform();
        string assetPath = Application.dataPath + "/data/asset/" + target + "/Atlas/";

        if (!Directory.Exists(assetPath))
        {
            Directory.CreateDirectory(assetPath);
        }
        DirectoryInfo rootDirInfo = new DirectoryInfo(Application.dataPath + "/Atlas");
        foreach (DirectoryInfo dirInfo in rootDirInfo.GetDirectories())
        {
            List<Sprite> assets = new List<Sprite>();
            string path = assetPath + "/" + dirInfo.Name + ".assetbundle";
            foreach (FileInfo pngFile in dirInfo.GetFiles("*.png", SearchOption.AllDirectories))
            {

                string allPath = pngFile.FullName; Debug.Log(allPath);
                string dir = allPath.Substring(allPath.IndexOf("Assets"));
                assets.Add(Resources.LoadAssetAtPath<Sprite>(dir));
            }
            if (BuildPipeline.BuildAssetBundle(null, assets.ToArray(), path, BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.CollectDependencies, target))
            {
            }
        }	
    }


    /// <summary>
    /// ����Ŀ¼
    /// </summary>
    static string AppDataPath
    {
        get { return Application.dataPath.ToLower(); }
    }

    static private BuildTarget GetBuildTarget()
    {
        BuildTarget target = BuildTarget.WebPlayer;
#if UNITY_STANDALONE
			target = BuildTarget.StandaloneWindows;
#elif UNITY_IPHONE
			target = BuildTarget.iPhone;
#elif UNITY_ANDROID
			target = BuildTarget.Android;
#endif
        return target;
    }

    //��ԴĿ��ƽ̨
    static BuildTarget GetTargetPlatform()
    {
         BuildTarget target;
        if (Application.platform == RuntimePlatform.OSXEditor)
        {
            target = BuildTarget.iPhone;
        }
        else
        {
            target = BuildTarget.Android;
        }
        return target;

    }

    /// <summary>
    /// ����Ŀ¼������Ŀ¼
    /// </summary>
    static void Recursive(string path)
    {
        string[] names = Directory.GetFiles(path);
        string[] dirs = Directory.GetDirectories(path);
        foreach (string filename in names)
        {
            string ext = Path.GetExtension(filename);
            if (ext.Equals(".meta")) continue;
            files.Add(filename.Replace('\\', '/'));
        }
        foreach (string dir in dirs)
        {
            paths.Add(dir.Replace('\\', '/'));
            Recursive(dir);
        }
    }

    static void cleanMeta(string path)
    {
        string[] names = Directory.GetFiles(path);
        string[] dirs = Directory.GetDirectories(path);
        foreach (string filename in names)
        {
            string ext = Path.GetExtension(filename);
            if (ext.Equals(".meta"))
            {
                //Debug.Log(filename);
                File.Delete(@filename);
            }

            foreach (string dir in dirs)
            {
                cleanMeta(dir);
            }

        }        
    }
}