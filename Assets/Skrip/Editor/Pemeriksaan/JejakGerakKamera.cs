using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Rendering;

// Mencatat kamera yang benar-benar dirender, termasuk frame tanpa tick fisika baru.
public sealed class JejakGerakKamera : IDisposable
{
    readonly ArthurJalurKampus actor;
    readonly Vector3 axis;
    readonly string path;
    readonly List<string> rows = new List<string>{"frame,time,fixedTime,actorX,actorY,actorZ,pathDistance,cameraX,cameraY,cameraZ,renderX,renderY,renderZ,viewportX,viewportY,deltaTime,running,cameraYaw,cameraFov"};
    int lastFrame = -1;
    bool disposed;
    Vector3 lastCamera, lastActor;
    float minY = float.PositiveInfinity, maxY = float.NegativeInfinity;
    float firstTime, lastTime, firstPosition, lastPosition;
    float runningDistance, runningTime;
    public int Frames { get; private set; }
    public float CameraReverse { get; private set; }
    public float ActorReverse { get; private set; }
    public int RunningFrames { get; private set; }
    public float MaxRunSpeedError { get; private set; }
    public float AverageRunSpeed => runningTime>0 ? runningDistance/runningTime : 0;
    public float VerticalRange => maxY-minY;
    public float Speed => lastTime>firstTime ? (lastPosition-firstPosition)/(lastTime-firstTime) : 0;
    public JejakGerakKamera(ArthurJalurKampus a, Vector3 direction, string output)
    {
        actor=a;axis=direction.normalized;path=output;
        Camera.onPreCull+=Capture;
        RenderPipelineManager.beginCameraRendering+=CaptureSRP;
    }
    void CaptureSRP(ScriptableRenderContext context,Camera camera)=>Capture(camera);
    void Capture(Camera camera)
    {
        if(disposed)return;
        if(actor==null){Dispose();return;}
        if(camera!=actor.kamera||lastFrame==Time.frameCount)return;
        var p=actor.transform.position;var c=camera.transform.position;
        if(Frames>0)
        {
            CameraReverse=Mathf.Max(CameraReverse,-Vector3.Dot(c-lastCamera,axis));ActorReverse=Mathf.Max(ActorReverse,-Vector3.Dot(p-lastActor,axis));
            var controller=actor.GetComponent<KarakterController>();
            if(controller.SedangBerlari&&Time.time>lastTime)
            {
                var delta=p-lastActor;delta.y=0;
                runningDistance+=delta.magnitude;runningTime+=Time.time-lastTime;
                MaxRunSpeedError=Mathf.Max(MaxRunSpeedError,Mathf.Abs(delta.magnitude/(Time.time-lastTime)-controller.kecepatanJalan));RunningFrames++;
            }
        }
        else {firstTime=Time.time;firstPosition=Vector3.Dot(p,axis);}
        lastTime=Time.time;lastPosition=Vector3.Dot(p,axis);
        minY=Mathf.Min(minY,c.y);maxY=Mathf.Max(maxY,c.y);
        lastFrame=Time.frameCount;lastCamera=c;lastActor=p;Frames++;
        var visual=actor.wadahArahVisual!=null?actor.wadahArahVisual.position:p;
        var viewport=camera.WorldToViewportPoint(visual);
        rows.Add(string.Join(",",new[]{(float)Time.frameCount,Time.time,Time.fixedTime,p.x,p.y,p.z,actor.JarakDiJalur,c.x,c.y,c.z,visual.x,visual.y,visual.z,viewport.x,viewport.y,Time.deltaTime,actor.GetComponent<KarakterController>().SedangBerlari?1:0,camera.transform.eulerAngles.y,camera.fieldOfView}.ConvertAllInvariant()));
    }
    public void Dispose(){if(disposed)return;disposed=true;Camera.onPreCull-=Capture;RenderPipelineManager.beginCameraRendering-=CaptureSRP;File.WriteAllLines(path,rows);}
}
static class AngkaJejakKamera
{
    public static string[] ConvertAllInvariant(this float[] values)=>Array.ConvertAll(values,v=>v.ToString("R",CultureInfo.InvariantCulture));
}
