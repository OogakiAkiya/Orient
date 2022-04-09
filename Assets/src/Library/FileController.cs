using System.Collections;
using System.Collections.Generic;
using System.IO;
public class FileController
{
    private static FileController _instance=new FileController();

    public static FileController GetInstance()
    {
        return _instance;
    }

    public void Write(string _fileName,string _writeData,bool _newLine=true)
    {
        string writeData=_writeData;
        if (_newLine) writeData=_writeData + "\n";
        File.AppendAllText(@_fileName + ".log", writeData);
    }

}
