using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UserBodyData
{
    public enum USERDATAFLG : byte
    {
        POSITION_X = 0x001,
        POSITION_Y = 0x002,
        POSITION_Z = 0x004,
        ROTATION_X = 0x008,
        ROTATION_Y = 0x010,
        ROTATION_Z = 0x020,
        HP = 0x040
    }


    public Vector3 position = Vector3.zero;
    public Vector3 rotetion = Vector3.zero;
    public int animationKey = 0;
    public int hp = 0;

    private USERDATAFLG sendDataFlg = 0x000;

    public void SetData(Vector3 _position, Vector3 _rotetion, int _currentKey, int _hp)
    {
        //position
        {
            bool flg = false;
            if (Math.Abs(position.x - _position.x) > 0.001)
            {
                sendDataFlg |= USERDATAFLG.POSITION_X;
                flg = true;
            }
            if (Math.Abs(position.y - _position.y) > 0.001)
            {
                sendDataFlg |= USERDATAFLG.POSITION_Y;
                flg = true;
            }
            if (Math.Abs(position.z - _position.z) > 0.001)
            {
                sendDataFlg |= USERDATAFLG.POSITION_Z;
                flg = true;
            }
            if (flg) position = _position;

        }

        //rotation
        {
            bool flg = false;
            if (Math.Abs(rotetion.x - _rotetion.x) > 0.001)
            {
                sendDataFlg |= USERDATAFLG.ROTATION_X;
                flg = true;
            }
            if (Math.Abs(rotetion.y - _rotetion.y) > 0.001)
            {
                sendDataFlg |= USERDATAFLG.ROTATION_Y;
                flg = true;
            }
            if (Math.Abs(rotetion.y - _rotetion.y) > 0.001)
            {
                sendDataFlg |= USERDATAFLG.ROTATION_Z;
                flg = true;
            }
            if (flg) rotetion = _rotetion;

        }
        if (hp != _hp)
        {
            sendDataFlg |= USERDATAFLG.HP;
            hp = _hp;
        }
        animationKey = _currentKey;
    }

    public byte[] GetData()
    {
        List<byte> returnData = new List<byte>();

        //送信データ作成
        returnData.Add((byte)sendDataFlg);
        if (sendDataFlg.HasFlag(USERDATAFLG.POSITION_X)) returnData.AddRange(BitConverter.GetBytes(position.x));
        if (sendDataFlg.HasFlag(USERDATAFLG.POSITION_Y)) returnData.AddRange(BitConverter.GetBytes(position.y));
        if (sendDataFlg.HasFlag(USERDATAFLG.POSITION_Z)) returnData.AddRange(BitConverter.GetBytes(position.z));
        if (sendDataFlg.HasFlag(USERDATAFLG.ROTATION_X)) returnData.AddRange(BitConverter.GetBytes(rotetion.x));
        if (sendDataFlg.HasFlag(USERDATAFLG.ROTATION_Y)) returnData.AddRange(BitConverter.GetBytes(rotetion.y));
        if (sendDataFlg.HasFlag(USERDATAFLG.ROTATION_Z)) returnData.AddRange(BitConverter.GetBytes(rotetion.z));
        if (sendDataFlg.HasFlag(USERDATAFLG.HP)) returnData.AddRange(BitConverter.GetBytes(hp));
        returnData.AddRange(BitConverter.GetBytes(animationKey));
        sendDataFlg = 0;

        return returnData.ToArray();
    }

    public byte[] GetCompleteData(UserBodyData _oldData = null)
    {

        List<byte> returnData = new List<byte>();

        //送信データ作成
        sendDataFlg |= USERDATAFLG.POSITION_X;
        sendDataFlg |= USERDATAFLG.POSITION_Y;
        sendDataFlg |= USERDATAFLG.POSITION_Z;
        sendDataFlg |= USERDATAFLG.ROTATION_X;
        sendDataFlg |= USERDATAFLG.ROTATION_Y;
        sendDataFlg |= USERDATAFLG.ROTATION_Z;
        sendDataFlg |= USERDATAFLG.HP;

        returnData.Add((byte)sendDataFlg);
        returnData.AddRange(Convert.GetByteVector3(position));
        returnData.AddRange(Convert.GetByteVector3(rotetion));
        returnData.AddRange(BitConverter.GetBytes(hp));
        returnData.AddRange(BitConverter.GetBytes(animationKey));
        sendDataFlg = 0;


        return returnData.ToArray();
    }

    public int Deserialize(byte[] _data, int _index)
    {
        int nowIndex = _index;
        USERDATAFLG nowFlg = (USERDATAFLG)_data[nowIndex++];
        if (nowFlg.HasFlag(USERDATAFLG.POSITION_X)) position.x = GetFloat(_data, ref nowIndex);
        if (nowFlg.HasFlag(USERDATAFLG.POSITION_Y)) position.y = GetFloat(_data, ref nowIndex);
        if (nowFlg.HasFlag(USERDATAFLG.POSITION_Z)) position.z = GetFloat(_data, ref nowIndex);
        if (nowFlg.HasFlag(USERDATAFLG.ROTATION_X)) rotetion.x = GetFloat(_data, ref nowIndex);
        if (nowFlg.HasFlag(USERDATAFLG.ROTATION_Y)) rotetion.y = GetFloat(_data, ref nowIndex);
        if (nowFlg.HasFlag(USERDATAFLG.ROTATION_Z)) rotetion.z = GetFloat(_data, ref nowIndex);
        if (nowFlg.HasFlag(USERDATAFLG.HP)) hp = GetInt(_data, ref nowIndex);
        animationKey = GetInt(_data, ref nowIndex);
        return nowIndex;
    }
    private float GetFloat(byte[] _data, ref int _startIndex)
    {
        _startIndex += sizeof(float);
        return BitConverter.ToSingle(_data, _startIndex - sizeof(float));
    }

    private int GetInt(byte[] _data, ref int _startIndex)
    {
        _startIndex += sizeof(int);
        return BitConverter.ToInt32(_data, _startIndex - sizeof(int));
    }
}
