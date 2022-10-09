using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
public class FileController
{
    private static FileController _instance=new FileController();
    private DateTime dateTime = new DateTime();

    public static FileController GetInstance()
    {
        return _instance;
    }

    public void Write(string _fileName,string _writeData,bool _newLine=true)
    {
        string writeData=_writeData;
        if (_newLine) writeData=_writeData + "\n";
        string date = DateTime.Now.ToString("[ yyyy/MM/dd HH: mm:ss:fff ]");
        File.AppendAllText(@_fileName + ".log", date +  writeData);
    }

}
