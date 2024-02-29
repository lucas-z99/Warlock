using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;


public class TEST : NetworkBehaviour
{
     // static
     public static TEST inst { get { if (_inst == null) _inst = FindObjectOfType<TEST>(); return _inst; } }
     public static Action OnConnect;
     public static Action OnDisconnect;
     public static bool isConnected { get; private set; }

     // public
     public bool quickStart = false;
     public bool clientHasDamageAutority = true; //TEST


     // private
     static TEST _inst;
     ulong clientID { get => NetworkManager.Singleton.LocalClientId; }
     PING pingClass;

     void Awake()
     {
          transform.position = new Vector3(99999, 99999, 99999); //out of sight just in case we do some stupid thing

          _inputIP = GetLocalIPv4();
          pingClass = FindObjectOfType<PING>();
          UIDamageTextMgr.Init();
     }
     void Start()
     {
          StartCoroutine(UpdateIP());

          if (quickStart)
          {
               StartHost();
               _showGUI = false;
          }
     }


     public ProjectileLauncher launcher;
     void Update()
     {
          if (Input.GetKeyDown(KeyCode.Q))
               launcher.FireProjectile_AI(new Vector3(0, 1, 0), new Vector3(1, 0, 1), CharaTeam.player_main_chara);
     }


     // set up connection ----------------------------------------------------------------------------

     UnityTransport uTransport { get => NetworkManager.Singleton.GetComponent<UnityTransport>(); }
     string serverIP { get => NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address; }
     readonly static ushort port = 1313;
     readonly static string listenToAll = "0.0.0.0";


     public void SetupConnection(string serverIPv4)
     {
          uTransport.SetConnectionData(serverIPv4, port, listenToAll);
     }
     public void StartHost()
     {
          SetupConnection(myIP); //for init other values, we don't use the ip part
          NetworkManager.Singleton.StartHost();

          isConnected = true;
          OnConnect?.Invoke();
     }
     public void StartClient(string serverIPv4)
     {
          SetupConnection(serverIPv4);
          NetworkManager.Singleton.StartClient();

          isConnected = true;
          OnConnect?.Invoke();
     }
     public void Shutdown()
     {
          NetworkManager.Singleton.Shutdown();

          isConnected = false;
          OnDisconnect?.Invoke();
     }


     // ip ----------------------------------------------------------------------------

     public static string myIP { get; private set; } //local IPv4

     IEnumerator UpdateIP()
     {
          while (true)
          {
               myIP = GetLocalIPv4();
               yield return new WaitForSeconds(1);
          }
     }

     string GetLocalIPv4()
     {
          var host = Dns.GetHostEntry(Dns.GetHostName());
          foreach (var ip in host.AddressList)
          {
               if (ip.AddressFamily == AddressFamily.InterNetwork)
               {
                    return ip.ToString();
               }
          }

          throw new Exception("Maybe no IPv4 adapters available??");
     }


     // testing ----------------------------------------------------------------------------
     bool IsUsingLatency()
     {
          var sim = NetworkManager.GetComponent<UnityTransport>().DebugSimulator;
          return sim.PacketDelayMS > 0 || sim.PacketJitterMS > 0 || sim.PacketDropRate > 0;
     }


     // OnGUI ----------------------------------------------------------------------------

     string _inputIP;
     bool _showGUI = true;

     void OnGUI()
     {
          GUILayout.BeginArea(new Rect(12, 12, 400, 5000));//-------------------------------------------------------
          var H = GUILayout.Height(25);

          if (GUILayout.Button(_showGUI ? "Hide" : "Show", H, GUILayout.Width(133)))
               _showGUI = !_showGUI;
          if (!_showGUI)
          {
               GUILayout.EndArea();//-------------------------------------------------------
               return;
          }


          // start as host or client
          GUILayout.Space(10);
          if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
          {
               if (GUILayout.Button("(A) Host  -  IP: " + myIP, H))
                    StartHost();


               GUILayout.BeginHorizontal();
               {
                    if (GUILayout.Button("(B) Conect to", GUILayout.Width(200), H))
                         StartClient(_inputIP);

                    _inputIP = GUILayout.TextField(_inputIP, H);
               }
               GUILayout.EndHorizontal();
          }
          else
          {
               if (NetworkManager.Singleton.IsHost)
                    GUILayout.Box("You are Host. IP: " + myIP, H);
               else
                    GUILayout.Box("You are Cinet. Connect to: " + serverIP, H);

               if (GUILayout.Button("Disconnect", H))
                    Shutdown();

          }

          // info display
          GUILayout.Space(12);
          GUILayout.BeginHorizontal();
          {
               // PING
               if (pingClass)
               {
                    GUILayout.Box("RTT = " + Math.Round(pingClass.RTT, 0));
                    GUILayout.Box("RTTmax = " + Math.Round(pingClass.RTTmax, 0));
               }
               else
                    GUILayout.Box("PING class N/A");

               // simulate latency
               if (IsUsingLatency())
                    GUILayout.Box("Simulate Lantency");
          }
          GUILayout.EndHorizontal();


          // log
          GUILayout.TextArea(log, GUILayout.Height(420));
          if (GUILayout.Button("Clear log"))
          {
               log = "";
               queue.Clear();
          }


          // Other TEST
          GUILayout.Space(12);




          GUILayout.EndArea();//-------------------------------------------------------
     }



     static string time { get => DateTime.Now.ToString("h:mm:ss"); }
     static string log = "";
     static Queue<string> queue = new Queue<string>();
     static int logMaxLines = 30;

     public static void GUILog(string text)
     {
          queue.Enqueue("[" + time + "] " + text);
          if (queue.Count > logMaxLines)
               queue.Dequeue();

          var _array = queue.ToArray();
          log = _array[0];

          for (int i = 1; i < _array.Length; i++)
               log += "\n" + _array[i];
     }



}