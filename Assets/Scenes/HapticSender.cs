using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System;
using UnityEngine.XR;

/// <summary>
/// Streams 4-channel force values over UDP every FixedUpdate.
/// Attach to any persistent GameObject (e.g. MusicPlayer).
/// </summary>
public class HapticSender : MonoBehaviour
{
    [Header("UDP")]
    public string remoteAddress = "127.0.0.1";
    public int remotePort = 7000;

    [Header("Debug")]
    public bool echoToQuest = true;      // fallback rumble

    UdpClient client;
    IPEndPoint endPoint;
    byte[] packet = new byte[20];
    byte frameId;

    InputDevice rightHand;

    void Awake()
    {
        client = new UdpClient();
        endPoint = new IPEndPoint(IPAddress.Parse(remoteAddress), remotePort);
        // find a controller for optional rumble
        var hands = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, hands);
        if (hands.Count > 0) rightHand = hands[0];        // XR API docs :contentReference[oaicite:4]{index=4}
    }

    void FixedUpdate()
    {
        if (!rightHand.isValid)              // reacquire if cable re-plugs or wake-on-motion
            rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        /* 1️-  Compute the forces – for now, fake with sword speed magnitude */
        Vector3 acc = SwordPhysicsCache.CurrentLinearAcceleration;
        float force = acc.magnitude;            // simple demo mapping

        short f = (short)Mathf.Clamp(force * 8000f, short.MinValue, short.MaxValue);

        /* 2️-  Build packet */
        packet[0] = 4;            // channel count
        packet[1] = frameId++;    // frame counter
        // little-endian writes
        System.Buffer.BlockCopy(BitConverter.GetBytes(f), 0, packet, 2, 2);
        System.Buffer.BlockCopy(BitConverter.GetBytes(f), 0, packet, 4, 2);
        System.Buffer.BlockCopy(BitConverter.GetBytes(f), 0, packet, 6, 2);
        System.Buffer.BlockCopy(BitConverter.GetBytes(f), 0, packet, 8, 2);

        byte xor = 0;
        for (int i = 0; i < 10; i++) xor ^= packet[i];
        packet[10] = xor;

        /* 3️-  Send (non-blocking) */
        client.Send(packet, packet.Length, endPoint);      // ↑ 100 µs on desktop :contentReference[oaicite:5]{index=5}

        /* 4️-  Optional: mirror to Quest vibration so testers feel something */
        if (echoToQuest && rightHand.isValid && rightHand.TryGetHapticCapabilities(out var caps) && caps.supportsImpulse)
        {
            rightHand.SendHapticImpulse(0u, Mathf.Clamp01(force * 0.002f), 0.02f);
        }
    }

    void OnDestroy() => client?.Dispose();
}
