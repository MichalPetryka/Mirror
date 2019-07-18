using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using UnityEngine;
#if UNITY_EDITOR && !UNITY_2020_1_OR_NEWER
#if UNITY_2018_3_OR_NEWER && !UNITY_2019_1_OR_NEWER
using System.Reflection;
#else
using UnityEditor;
#endif
#endif

namespace Mirror
{
#if UNITY_EDITOR && !UNITY_2020_1_OR_NEWER
#pragma warning disable 618
    internal static class NetworkProfiler
    {
#if UNITY_2018_3_OR_NEWER && !UNITY_2019_1_OR_NEWER
        private static readonly bool failedToLoad;
        private static readonly Type networkDetailStats;

        private static readonly MethodInfo newProfilerTick;
        private static readonly MethodInfo setStat;
        private static readonly MethodInfo incrementStat;
        private static readonly MethodInfo resetAll;

        private static readonly object[] newProfilerTickParameters = new object[1];
        private static readonly object[] setStatParameters = new object[4];
        private static readonly object[] incrementStatParameters = new object[4];

        static NetworkProfiler()
        {
            networkDetailStats = Type.GetType("UnityEditor.NetworkDetailStats");
            if (networkDetailStats != null)
            {
                newProfilerTick = networkDetailStats.GetMethod("NewProfilerTick", BindingFlags.Static | BindingFlags.Public);
                setStat = networkDetailStats.GetMethod("SetStat", BindingFlags.Static | BindingFlags.Public);
                incrementStat = networkDetailStats.GetMethod("IncrementStat", BindingFlags.Static | BindingFlags.Public);
                resetAll = networkDetailStats.GetMethod("ResetAll", BindingFlags.Static | BindingFlags.Public);
            }

            failedToLoad = networkDetailStats == null || newProfilerTick == networkDetailStats || setStat == null || incrementStat == null || resetAll == null;
        }
#endif

        internal enum NetworkDirection
        {
            Incoming,
            Outgoing
        }

        internal static void NewProfilerTick(float newTime)
        {
#if UNITY_2018_3_OR_NEWER && !UNITY_2019_1_OR_NEWER
            if (!failedToLoad)
            {
                newProfilerTickParameters[0] = newTime;
                newProfilerTick.Invoke(null, newProfilerTickParameters);
            }
#else
            NetworkDetailStats.NewProfilerTick(newTime);
#endif
        }

        internal static void SetStat(NetworkDirection direction, short msgId, string entryName, int amount)
        {
#if UNITY_2018_3_OR_NEWER && !UNITY_2019_1_OR_NEWER
            if (!failedToLoad)
            {
                setStatParameters[0] = direction;
                setStatParameters[1] = msgId;
                setStatParameters[2] = entryName;
                setStatParameters[3] = amount;
                setStat.Invoke(null, setStatParameters);
            }
#else
            NetworkDetailStats.SetStat((NetworkDetailStats.NetworkDirection)direction, msgId, entryName, amount);
#endif
        }

        internal static void IncrementStat(NetworkDirection direction, short msgId, string entryName, int amount)
        {
#if UNITY_2018_3_OR_NEWER && !UNITY_2019_1_OR_NEWER
            if (!failedToLoad)
            {
                incrementStatParameters[0] = direction;
                incrementStatParameters[1] = msgId;
                incrementStatParameters[2] = entryName;
                incrementStatParameters[3] = amount;
                incrementStat.Invoke(null, incrementStatParameters);
            }
#else
            NetworkDetailStats.IncrementStat((NetworkDetailStats.NetworkDirection)direction, msgId, entryName, amount);
#endif
        }

        internal static void ResetAll()
        {
#if UNITY_2018_3_OR_NEWER && !UNITY_2019_1_OR_NEWER
            if (!failedToLoad)
            {
                resetAll.Invoke(null, null);
            }
#else
            NetworkDetailStats.ResetAll();
#endif
        }
    }
#pragma warning restore 618
#endif

    // Handles network messages on client and server
    public delegate void NetworkMessageDelegate(NetworkMessage netMsg);

    // Handles requests to spawn objects on the client
    public delegate GameObject SpawnDelegate(Vector3 position, Guid assetId);

    // Handles requests to unspawn objects on the client
    public delegate void UnSpawnDelegate(GameObject spawned);

    // invoke type for Cmd/Rpc/SyncEvents
    public enum MirrorInvokeType
    {
        Command,
        ClientRpc,
        SyncEvent
    }

    // built-in system network messages
    // original HLAPI uses short, so let's keep short to not break packet header etc.
    // => use .ToString() to get the field name from the field value
    // => we specify the short values so it's easier to look up opcodes when debugging packets
    [EditorBrowsable(EditorBrowsableState.Never), Obsolete("Use Send<T>  with no message id instead")]
    public enum MsgType : short
    {
        // internal system messages - cannot be replaced by user code
        ObjectDestroy = 1,
        Rpc = 2,
        Owner = 4,
        Command = 5,
        SyncEvent = 7,
        UpdateVars = 8,
        SpawnPrefab = 3,
        SpawnSceneObject = 10,
        SpawnStarted = 11,
        SpawnFinished = 12,
        ObjectHide = 13,
        LocalClientAuthority = 15,

        // public system messages - can be replaced by user code
        Connect = 32,
        Disconnect = 33,
        Error = 34,
        Ready = 35,
        NotReady = 36,
        AddPlayer = 37,
        RemovePlayer = 38,
        Scene = 39,

        // time synchronization
        Ping = 43,
        Pong = 44,

        Highest = 47
    }

    public enum Version
    {
        Current = 1
    }

    public static class Channels
    {
        public const int DefaultReliable = 0;
        public const int DefaultUnreliable = 1;
    }

    // -- helpers for float conversion without allocations --
    [StructLayout(LayoutKind.Explicit)]
    internal struct UIntFloat
    {
        [FieldOffset(0)]
        public float floatValue;

        [FieldOffset(0)]
        public uint intValue;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct UIntDouble
    {
        [FieldOffset(0)]
        public double doubleValue;

        [FieldOffset(0)]
        public ulong longValue;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct UIntDecimal
    {
        [FieldOffset(0)]
        public ulong longValue1;

        [FieldOffset(8)]
        public ulong longValue2;

        [FieldOffset(0)]
        public decimal decimalValue;
    }
}
