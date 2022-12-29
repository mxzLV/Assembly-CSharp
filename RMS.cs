using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine;

public class RMS
{
    public static int status;

    public static sbyte[] data;

    public static string filename;

    private const int INTERVAL = 5;

    private const int MAXTIME = 500;

    [DllImport("kernel32")]
    private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);




    [DllImport("kernel32")]
    private static extern int WritePrivateProfileString(string lpApplicationName, string lpKeyName, string lpString, string lpFileName);




    public static string Read(string section, string key, string def)
    {
        try
        {
            int ind = Application.dataPath.LastIndexOf("/");
            string re = Application.dataPath.Substring(0, ind);
            re = re.Replace("/", "\\");


            StringBuilder sb = new StringBuilder(1024);
            GetPrivateProfileString(section, key, def, sb, 1024, re + "\\config.ini");
            return sb.ToString();
        }
        catch (Exception ex)
        {
            RMS.writeLog(ex.Message);
            return "";
        }
    }




    public static int Write(string section, string key, string value)
    {
        try
        {
            int ind = Application.dataPath.LastIndexOf("/");
            string re = Application.dataPath.Substring(0, ind);
            re = re.Replace("/", "\\");


            return WritePrivateProfileString(section, key, value, re + "\\config.ini");
        }
        catch (Exception ex)
        {
            RMS.writeLog(ex.Message);
            return 0;
        }
    }
    public static void writeLog(string sLog)
    {
        try
        {
            string text = DateTime.Now.ToString("h:mm:ss tt");
            sLog = text + "---" + sLog;
            int length = Application.dataPath.LastIndexOf("/");
            string text2 = Application.dataPath.Substring(0, length);
            text2 = text2.Replace("/", "\\");
            string path = text2 + "\\Log.txt";
            if (File.Exists(path))
            {
                using (FileStream stream = new FileStream(path, FileMode.Append, FileAccess.Write))
                {
                    using StreamWriter streamWriter = new StreamWriter(stream);
                    streamWriter.WriteLine(sLog);
                    return;
                }
            }
            using FileStream fileStream = File.Create(path);
            byte[] bytes = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes(sLog);
            fileStream.Write(bytes, 0, bytes.Length);
            byte[] array = new byte[1];
            fileStream.Write(array, 0, array.Length);
        }
        catch
        {
        }
    }
    public static void saveRMSImage(string idImage, sbyte[] data)
    {
        if (Thread.CurrentThread.Name == Main.mainThreadName)
        {
            __saveRMS(filename, data);
        }
        else
        {
            _saveRMS(filename, data);
        }
    }

    public static void saveRMS(string filename, sbyte[] data)
    {
        if (Thread.CurrentThread.Name == Main.mainThreadName)
        {
            __saveRMS(filename, data);
        }
        else
        {
            _saveRMS(filename, data);
        }
    }

    public static sbyte[] loadRMS(string filename)
    {
        if (Thread.CurrentThread.Name == Main.mainThreadName)
        {
            return __loadRMS(filename);
        }
        return _loadRMS(filename);
    }

    public static string loadRMSString(string fileName)
    {
        sbyte[] array = loadRMS(fileName);
        if (array == null)
        {
            return null;
        }
        DataInputStream dataInputStream = new DataInputStream(array);
        try
        {
            string result = dataInputStream.readUTF();
            dataInputStream.close();
            return result;
        }
        catch (Exception ex)
        {
            Out.println(ex.StackTrace);
        }
        return null;
    }

    public static byte[] convertSbyteToByte(sbyte[] var)
    {
        byte[] array = new byte[var.Length];
        for (int i = 0; i < var.Length; i++)
        {
            if (var[i] > 0)
            {
                array[i] = (byte)var[i];
            }
            else
            {
                array[i] = (byte)(var[i] + 256);
            }
        }
        return array;
    }

    public static void saveRMSString(string filename, string data)
    {
        DataOutputStream dataOutputStream = new DataOutputStream();
        try
        {
            dataOutputStream.writeUTF(data);
            saveRMS(filename, dataOutputStream.toByteArray());
            dataOutputStream.close();
        }
        catch (Exception ex)
        {
            Out.println(ex.StackTrace);
        }
    }

    private static void _saveRMS(string filename, sbyte[] data)
    {
        if (status != 0)
        {
            Debug.LogError("Cannot save RMS " + filename + " because current is saving " + RMS.filename);
            return;
        }
        RMS.filename = filename;
        RMS.data = data;
        status = 2;
        int i;
        for (i = 0; i < 500; i++)
        {
            Thread.Sleep(5);
            if (status == 0)
            {
                break;
            }
        }
        if (i == 500)
        {
            Debug.LogError("TOO LONG TO SAVE RMS " + filename);
        }
    }

    private static sbyte[] _loadRMS(string filename)
    {
        if (status != 0)
        {
            Debug.LogError("Cannot load RMS " + filename + " because current is loading " + RMS.filename);
            return null;
        }
        RMS.filename = filename;
        data = null;
        status = 3;
        int i;
        for (i = 0; i < 500; i++)
        {
            Thread.Sleep(5);
            if (status == 0)
            {
                break;
            }
        }
        if (i == 500)
        {
            Debug.LogError("TOO LONG TO LOAD RMS " + filename);
        }
        return data;
    }

    public static void update()
    {
        if (status == 2)
        {
            status = 1;
            __saveRMS(filename, data);
            status = 0;
        }
        else if (status == 3)
        {
            status = 1;
            data = __loadRMS(filename);
            status = 0;
        }
    }

    public static int loadRMSInt(string file)
    {
        sbyte[] array = loadRMS(file);
        if (array == null)
        {
            return -1;
        }
        return array[0];
    }

    public static void saveRMSInt(string file, int x)
    {
        try
        {
            saveRMS(file, new sbyte[1] { (sbyte)x });
        }
        catch (Exception)
        {
        }
    }

    private static void __saveRMS(string filename, sbyte[] data)
    {
        string value = ByteArrayToString(ArrayCast.cast(data));
        PlayerPrefs.SetString(filename, value);
    }

    private static sbyte[] __loadRMS(string filename)
    {
        string @string = PlayerPrefs.GetString(filename);
        byte[] array;
        try
        {
            array = StringToByteArray(@string);
        }
        catch (Exception ex)
        {
            Debug.LogError("PARSE RMS STRING FAIL " + ex.StackTrace);
            return null;
        }
        if (array.Length == 0)
        {
            return null;
        }
        return ArrayCast.cast(array);
    }

    public static void deleteAll()
    {
        Debug.LogWarning("ALL RMS CLEAR");
        PlayerPrefs.DeleteAll();
    }

    public static string ByteArrayToString(byte[] ba)
    {
        string text = BitConverter.ToString(ba);
        return text.Replace("-", string.Empty);
    }

    public static byte[] StringToByteArray(string hex)
    {
        int length = hex.Length;
        byte[] array = new byte[length / 2];
        for (int i = 0; i < length; i += 2)
        {
            array[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }
        return array;
    }

    public static void deleteRecord(string name)
    {
        try
        {
            PlayerPrefs.DeleteKey(name);
        }
        catch (Exception ex)
        {
            Debug.Log("Xoa RMS Error");
            Out.println("loi xoa RMS --------------------------" + ex.ToString());
        }
    }

    public static void clearRMS()
    {
        deleteRecord("nj_arrow");
        deleteRecord("nj_effect");
        deleteRecord("nj_image");
        deleteRecord("nj_part");
        deleteRecord("nj_skill");
        deleteRecord("data");
        deleteRecord("dataVersion");
        deleteRecord("map");
        deleteRecord("mapVersion");
        deleteRecord("skill");
        deleteRecord("killVersion");
        deleteRecord("item");
        deleteRecord("itemVersion");
    }

    public static void SaveRMSBool(string NameSave, bool Value)
    {
        saveRMSInt(NameSave, Code.ToInt32(Value));
    }

    public static bool loadRMSBool(string NameSave)
    {
        return Code.ToBoolean(loadRMSInt(NameSave));
    }
}
