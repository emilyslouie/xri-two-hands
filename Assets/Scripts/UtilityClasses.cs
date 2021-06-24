// ******************************************************************************
using UnityEngine;
using System.Collections.Generic;

  // ******************************************************************************
  // ******************************************************************************
  public static class Vector3Extensions
  {
    public static Vector2 xy(this Vector3 aVector)
    {
      return new Vector2(aVector.x, aVector.y);
    }
    public static Vector2 xz(this Vector3 aVector)
    {
      return new Vector2(aVector.x, aVector.z);
    }
    public static Vector2 yz(this Vector3 aVector)
    {
      return new Vector2(aVector.y, aVector.z);
    }
    public static Vector2 yx(this Vector3 aVector)
    {
      return new Vector2(aVector.y, aVector.x);
    }
    public static Vector2 zx(this Vector3 aVector)
    {
      return new Vector2(aVector.z, aVector.x);
    }
    public static Vector2 zy(this Vector3 aVector)
    {
      return new Vector2(aVector.z, aVector.y);
    }
  }

  public static class Vector4Extensions
  {
    public static Vector2 xy(this Vector4 aVector)
    {
      return new Vector2(aVector.x, aVector.y);
    }
    public static Vector2 zw(this Vector4 aVector)
    {
      return new Vector2(aVector.z, aVector.w);
    }
  }

  // ******************************************************************************
  // ******************************************************************************
  static public class RectTransformExt
  {
    // ******************************************************************************
    /// <summary>
    /// Converts RectTransform.rect's local coordinates to world space
    /// Usage example RectTransformExt.GetWorldRect(myRect, Vector2.one);
    /// </summary>
    /// <returns>The world rect.</returns>
    /// <param name="rt">RectangleTransform we want to convert to world coordinates.</param>
    /// <param name="scale">Optional scale pulled from the CanvasScaler. Default to using Vector2.one.</param>
    static public Rect GetWorldRect(RectTransform rt, Vector2 scale)
    {
      // Convert the rectangle to world corners and grab the top left
      Vector3[] corners = new Vector3[4];
      rt.GetWorldCorners(corners);
      Vector3 topLeft = corners[0];

      // Rescale the size appropriately based on the current Canvas scale
      Vector2 scaledSize = new Vector2(scale.x * rt.rect.size.x, scale.y * rt.rect.size.y);

      return new Rect(topLeft, scaledSize);
    }
  }

  // ******************************************************************************
  // ******************************************************************************
  public class SimpleTimer
  {
    public float ElapsedTime
    {
      get { return m_ElapsedTime; }
    }
    private float m_ElapsedTime = 0;

    public float TargetTime
    {
      get { return m_TargetTime; }
    }
    private float m_TargetTime = 0;

    public float PercentageDone
    {
      get { return m_ElapsedTime / m_TargetTime; }
    }

    public bool Paused { get { return m_bPaused; } }
    private bool m_bPaused = false;

    public bool Done { get { return m_Done; } }
    private bool m_Done = false;

    private string m_TrackerName = null;
    public static Dictionary<string,SimpleTimer> TrackedTimers { get; private set; }
    //private static Dictionary<string, SimpleTimer> m_TrackedTimers = null;

    // ******************************************************************************
    public SimpleTimer(float timeInSeconds)
    {
      m_ElapsedTime = 0;
      m_TargetTime = timeInSeconds;
      m_Done = false;
      m_bPaused = false;
      m_TrackerName = null;
      //do this check for when timers are created from public values that could be negative or 0
      if (m_TargetTime < 0 || Mathf.Approximately(m_TargetTime, 0))
        ForceDone();
    }

    //Let this instance know when it's being destroyed
    ~SimpleTimer()
    {
      if(string.IsNullOrEmpty(m_TrackerName) == false)
      {
        if (TrackedTimers.ContainsKey(m_TrackerName))
          TrackedTimers.Remove(m_TrackerName);
      }
    }

    // ******************************************************************************
    public bool Update(float deltaTime)
    {
      if (m_bPaused)
        return m_Done;

      m_ElapsedTime += deltaTime;
      if (m_ElapsedTime >= m_TargetTime)
      {
        m_ElapsedTime = m_TargetTime;
        m_Done = true;
      }

      return m_Done;
    }

    // ******************************************************************************
    public void Reset(float newtargetTime = float.NegativeInfinity)
    {
      if (newtargetTime != float.NegativeInfinity)
      {
        m_TargetTime = newtargetTime;
      }
      m_ElapsedTime = 0;
      m_Done = false;
      m_bPaused = false;
      //do this check for when timers are created from public values that could be negative or 0
      if (m_TargetTime < 0 || Mathf.Approximately(m_TargetTime, 0))
        ForceDone();
    }

    // ******************************************************************************
    public void SetTargetTime(float timeInSeconds, bool resetElapsed = true)
    {
      if (resetElapsed)
      {
        m_ElapsedTime = 0;
        m_Done = false;
        m_bPaused = false;
      }

      m_TargetTime = timeInSeconds;
      if (m_ElapsedTime >= m_TargetTime)
      {
        ForceDone();
      }

      //do this check for when timers are created from public values that could be negative or 0
      if (m_TargetTime < 0 || Mathf.Approximately(m_TargetTime, 0))
        ForceDone();
    }

    // ******************************************************************************
    public void ForceDone()
    {
      m_ElapsedTime = m_TargetTime;
      m_Done = true;
      m_bPaused = false;
    }

    public void Pause()
    {
      m_bPaused = true;
    }

    public void Resume()
    {
      m_bPaused = false;
    }

    public bool TrackTimer(string timerName)
    {
      if (TrackedTimers == null)
        TrackedTimers = new Dictionary<string, SimpleTimer>();

      if (TrackedTimers.ContainsKey(timerName) == false)
      {
        TrackedTimers.Add(timerName, this);

        m_TrackerName = timerName;

        return true;
      }

      return false;
    }
  }

  // ********************************************************************
  // ********************************************************************
  public static class FlagsHelper
  {
    // ********************************************************************
    public static bool IsSet<T>(T flags, T flag) where T : struct
    {
      int flagsValue = (int)(object)flags;
      int flagValue = (int)(object)flag;

      return (flagsValue & flagValue) != 0;
    }

    // ********************************************************************
    public static void Set<T>(ref T flags, T flag) where T : struct
    {
      int flagsValue = (int)(object)flags;
      int flagValue = (int)(object)flag;

      flags = (T)(object)(flagsValue | flagValue);
    }

    // ********************************************************************
    public static void Unset<T>(ref T flags, T flag) where T : struct
    {
      int flagsValue = (int)(object)flags;
      int flagValue = (int)(object)flag;

      flags = (T)(object)(flagsValue & (~flagValue));
    }
  }

  // ********************************************************************
  // ********************************************************************
  public class CountUpMeter
  {
    private int m_StartValue = 0;
    private int m_RequestedValue = 0;

    private int m_CurrentValue = 0;

    private float m_CountUpTime = 0;
    private float m_ElapsedCountUpTime = 0;

    public int CurrentValue { get { return m_CurrentValue; } }
    public bool IsFinished { get { return m_bFinished; } }
    private bool m_bFinished = false;

    // ********************************************************************
    public CountUpMeter(int startValue, int initialRequestedValue, float time)
    {
      m_StartValue = startValue;
      m_RequestedValue = initialRequestedValue;
      m_CountUpTime = time;
      m_bFinished = false;
    }

    // ********************************************************************
    public int Update(float deltaTime)
    {
      if (m_bFinished == false)
      {
        m_ElapsedCountUpTime += deltaTime;
        if (m_ElapsedCountUpTime >= m_CountUpTime)
        {
          m_ElapsedCountUpTime = m_CountUpTime;
        }
        m_CurrentValue = Mathf.CeilToInt(Mathf.Lerp(m_StartValue, m_RequestedValue, m_ElapsedCountUpTime / m_CountUpTime));
        if (m_CurrentValue >= m_RequestedValue)
        {
          m_CurrentValue = m_RequestedValue;
          m_bFinished = true;
        }
      }
      return m_CurrentValue;
    }

    // ********************************************************************
    public int ForceFinish()
    {
      if (m_bFinished == false)
      {
        m_ElapsedCountUpTime = m_CountUpTime;
        m_CurrentValue = m_RequestedValue;
        m_bFinished = true;
      }
      return m_CurrentValue;
    }

    // ********************************************************************
    /// <summary>
    /// Set new value to count towards
    /// Passed in time is relative to the current value
    /// Current value can be reset
    /// </summary>
    /// <param name="requestedValue"></param>
    /// <param name="time"></param>
    /// <param name="resetStartValue"></param>
    public void SetRequestedValue(int requestedValue, float time, bool resetCurrentValue = false)
    {
      m_RequestedValue = requestedValue;
      m_StartValue = resetCurrentValue ? 0 : m_CurrentValue;
      m_CountUpTime = time;
      m_ElapsedCountUpTime = 0;
      m_bFinished = false;
    }

    // ********************************************************************
    public void SetRequestedValueNoReset(int requestedValue, float time)
    {
      m_RequestedValue = requestedValue;
      m_CountUpTime = time;
      m_bFinished = false;
    }

    // ********************************************************************
    public void InstantSetValue(int value)
    {
      m_RequestedValue = value;
      m_StartValue = value;
      m_CurrentValue = value;
      m_CountUpTime = 0;
      m_ElapsedCountUpTime = 0;
      m_bFinished = true;
    }
  }