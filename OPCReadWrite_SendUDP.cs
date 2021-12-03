using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using OpcUaHelper;
using Opc.Ua;
using Opc.Ua.Client;
using System.Net;
using System.Net.Sockets;

public class OPCReadWrite_SendUDP : MonoBehaviour
{
    //放在PC端
    //1.可以直接讀寫OPC的數值
    //2.讀取利用訂閱制 數值變化會觸發副程式**
    //3.把讀取到的值用UDP發送到HOLOLENS上
    //4.抓其他腳本中的共用變數 並發送給OPC
    OpcUaClient opcUaClient = new OpcUaClient();
    #region//anyValues
    /*public string anyViaUDP;
    public string anyNameViaUDP;
    public float anyValuesViaUDP;*/
    Addr SearchName;
    PcBufferArray BufferArray; //21/12/03更新
    #endregion

    public GameObject DebugSend;
    private static string SendDebug;
    public string NowAckIP;

    public bool EMS_MR;

    public GameObject[] GyroTestText;
    public GameObject[] TankTestText;

    public string[] TestModeString;//Use For Test Code:TestToPLC

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            opcUaClient.ConnectServer("opc.tcp://127.0.0.1:49320");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Connect failed : " + ex.Message);
            return;
        }
        SearchName = GetComponent<Addr>();
        BufferArray = GetComponent<PcBufferArray>();

        TestModeString = new string[2];
    }

    // Update is called once per frame
    void Update()
    {
        #region//readNode
        //Tank_01 = opcUaClient.ReadNode<float>("ns=2;s=Channel1.Device1.DemoTest.Tank01");
        #endregion
        DebugSend.GetComponent<Text>().text = SendDebug;
        if (EMS_MR)
        {
            opcUaClient.WriteNode<bool>(SearchName.SearchAddr("EMS_MR")[0], true);
        }
    }

    //===============================================================================================================11/23
    public void ReturnReadSthFromOpc(string[] Name)
    {
        string Result = null;

        for (int i = 0; i < Name.Length - 1; i++)
        {
            if (SearchName.SearchAddr(Name[i]) != null)
            {
                if (SearchName.SearchAddr(Name[i])[1] == "1" || SearchName.SearchAddr(Name[i])[1] == "7")
                {
                    bool Value;
                    Debug.Log(Name[i] + "Bool");
                    Value = opcUaClient.ReadNode<bool>(SearchName.SearchAddr(Name[i])[0]);
                    Result = Result + Value.ToString();
                }
                else if (SearchName.SearchAddr(Name[i])[1] == "3")
                {
                    float Value;
                    if (SearchName.SearchAddr(Name[i])[2] != "NoLiner")
                    {
                        Debug.Log("show Liner" + SearchName.SearchAddr(Name[i])[2]);
                        float LinerHigh = float.Parse("0" + SearchName.SearchAddr(Name[i])[2]);
                        float OrgValues = opcUaClient.ReadNode<float>(SearchName.SearchAddr(Name[i])[0]);
                        Value = OrgValues / LinerHigh * 100;
                    }
                    else
                    {
                        Value = opcUaClient.ReadNode<float>(SearchName.SearchAddr(Name[i])[0]);
                    }
                   
                    Debug.Log(Result + Value.ToString());
                    Result = Result + Value.ToString();
                }
                else if (SearchName.SearchAddr(Name[i])[1] == "5")
                {
                    float Value;
                    Value = opcUaClient.ReadNode<UInt16>(SearchName.SearchAddr(Name[i])[0]);
                    Result = Result + Value.ToString();
                }
                Result = Result + ":";
            }
            else
            {
                Debug.Log(Name[i] + "___Fail Name___");
                Result = Result + "Fail Name";
            }
        }
        BufferArray.RenewBufferBool = true; //21/12/03 更新
        Debug.Log("New1123Test" + Result);
        SendToMR(System.Text.Encoding.UTF8.GetBytes(Result));
    }
    public void WriteSthFromPc(string[] Name)
    {
        //In write case Name[0] is Name, Name[1] is Values and Name[2]'s infomation is "Write".
        Debug.Log("Write_NAME:" + Name[0] + Name[1]);
        if (SearchName.SearchAddr(Name[0]) != null)
        {
            Debug.Log("Write_NAME:" + Name[0] + ":" + SearchName.SearchAddr(Name[0]));
            if (SearchName.SearchAddr(Name[0])[1] == "1" || SearchName.SearchAddr(Name[0])[1] == "7")
            {
                if (Name[1] == "True" || Name[1] == "true")
                {
                    Debug.Log("Write_" + "true");
                    opcUaClient.WriteNode<bool>(SearchName.SearchAddr(Name[0])[0], true);
                }
                else if (Name[1] == "False" || Name[1] == "false")
                {
                    Debug.Log("Write_" + "false");
                    opcUaClient.WriteNode<bool>(SearchName.SearchAddr(Name[0])[0], false);
                }

            }
            else if (SearchName.SearchAddr(Name[0])[1] == "3")
            {
                float writeFloat = float.Parse("0" + Name[1]);
                Debug.Log("Write_" + writeFloat);
                opcUaClient.WriteNode<float>(SearchName.SearchAddr(Name[0])[0], writeFloat);
            }
            else if (SearchName.SearchAddr(Name[0])[1] == "5")
            {
                ushort writeInt = UInt16.Parse("0" + Name[1]);
                Debug.Log("Write_" + writeInt);
                opcUaClient.WriteNode<UInt16>(SearchName.SearchAddr(Name[0])[0], writeInt);
            }
        }
        else
        {
            Debug.Log("Can't find");
        }
    }
    //===============================================================================================================11/23

    public void SendToMR(byte[] b)
    {
        string[] AckIp = NowAckIP.Split(':');
        #region //socketConnect
        /*if (opcUaClient.ReadNode<bool>(SearchName.SearchAddr("EMS_MR")[0]) || EMS_MR)
        {
            //SendToMR(System.Text.Encoding.UTF8.GetBytes("EMS" + ":" + "True"));
            EMS_MR = true;
            b = System.Text.Encoding.UTF8.GetBytes("EMS" + ":" + "True");
        }*/
        try
        {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(AckIp[0]), 5555);
            UdpClient uc = new UdpClient();
            SendDebug = AckIp[0] + ":5555 : " + System.Text.Encoding.UTF8.GetString(b);
            uc.Send(b, b.Length, ipep);
        }
        catch (Exception Ex)
        {
            Debug.LogWarning("Error Message :" + Ex);
        }
        #endregion
    }
}