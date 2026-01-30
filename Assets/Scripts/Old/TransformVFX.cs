
using UnityEngine.VFX;

public class TransformVFX
{
    VisualEffect _vfx;
    public TransformVFX(VisualEffect vfx)
    {
        _vfx = vfx;
    }
    public void SetTransform(TransformData data)
    {
        _vfx.SetVector3("Position", data.Center);
        _vfx.SetVector3("Scale", data.Size);
        _vfx.SetVector3("Rotation", data.EulerAngles);
    }
    public void Play()
    {
        _vfx.Play();
    }
}

public class CircleVFX
{
    VisualEffect _vfx;
    public CircleVFX(VisualEffect vfx)
    {
        _vfx = vfx;
    }
    public void SetCircle(Circle circle)
    {
        _vfx.SetVector3("Position", circle.Center);
        _vfx.SetFloat("Radius", circle.Radius);
        _vfx.SetVector3("Rotation", circle.EulerAngles);
        _vfx.SetVector3("Scale", circle.Size);
    }
    public void Play()
    {
        _vfx.Play();
    }
}