using System.IO;
using System.Xml;
using UnityEditor.Android;

public sealed class ManifestLandscapeAndroid : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => 1000;
    public void OnPostGenerateGradleAndroidProject(string path)
    {
        var file = Path.Combine(path, "src/main/AndroidManifest.xml");
        var document = new XmlDocument();
        document.Load(file);
        const string android = "http://schemas.android.com/apk/res/android";
        foreach (XmlElement activity in document.SelectNodes("/manifest/application/activity"))
            if (activity.GetAttribute("name", android).StartsWith("com.unity3d.player."))
                activity.SetAttribute("screenOrientation", android, "sensorLandscape");
        document.Save(file);
    }
}
