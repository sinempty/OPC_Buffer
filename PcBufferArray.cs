using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;

public class Buffer_class
{
    public Buffer_class(string name, string value)
    {
        Name = name;
        Value = value;
    }
    public string Name { get; set; }
    public string Value { get; set; }

}
public class PcBufferArray : MonoBehaviour
{
    //接收由HOLOLENS過來PC的UDP訊息
    //這個腳本用在接收到PLC與MR的值後陣列緩衝器
    List<Buffer_class> Buffer_Array = new List<Buffer_class>();
    public bool RenewBufferBool = false;//更新開關
    OPCReadWrite_SendUDP GetValueOPC;//call 讀取程式的腳本

    void Start()
    {
        GetValueOPC = GetComponent<OPCReadWrite_SendUDP>();
    }

    void Update()
    {
        if (RenewBufferBool == true)
        {
            GetValueOPC.ReturnReadSthFromOpc(Buffer_Array.Select(x => x.Name).ToArray());
            RenewBufferBool = false;
        }
    }
    public string BufferReTurnValue(string search)
    {
        return Buffer_Array.Find(x => x.Name == search).Value;
    }
    public bool CheckNameInBuffer(string search)
    {
        return Buffer_Array.Exists(x => x.Name == search);
    }
}