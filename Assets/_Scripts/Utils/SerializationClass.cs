using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ExitGames.Client.Photon;
using UnityEngine;

public static class CustomStream
{
    /// <summary>
    /// CALL THIS IN START OR AWAKE IN YOUR NETWORK MANAGER CLASS ONCE
    /// BEFORE YOU TRY TO SEND YOUR OBJECT DATA. YOU WILL GET AN
    /// EXCEPTION!
    /// </summary>
    public static void Register()
    {
        // Register with photon your custom data type to be serialized and deserialized.
        PhotonPeer.RegisterType(typeof(PlayerProfile), (byte)'Z', SerializePlayerProfile, DeserializePlayerProfile);
    }

    /// <summary>
    /// byte memory stream for damage info. The size of the array must be equal to
    /// the size of the sum of all the data type byte sizes.
    ///
    ///
    /// </summary>
    public static readonly byte[] memPlayerProfile = new byte[INTEGER_INT * 3];

    private static short SerializePlayerProfile(StreamBuffer outStream, object customObject)
    {
        PlayerProfile playerProfile = (PlayerProfile)customObject; // Cast to object type.

        // Lock the memory byte stream for damage info
        // this prevents the byte[] from being changed while we are
        // changing it.
        lock (memPlayerProfile)
        {
            int index = 0; // byte stream starting index.

            byte[] bytes = memPlayerProfile;

            // Serialize each value in player profile
            Protocol.Serialize((float)playerProfile.coins, bytes, ref index);
            Protocol.Serialize(playerProfile.roomID, bytes, ref index);
            Protocol.Serialize(playerProfile.slot, bytes, ref index);

            outStream.Write(bytes, 0, memPlayerProfile.Length);
        }

        return (short)memPlayerProfile.Length;
    }

    private static object DeserializePlayerProfile(StreamBuffer inStream, short length)
    {
        // Temperary holders for each member in DamageInfo
        int coins = 0;
        int roomID = 0;
        int slot = 0;

        lock (memPlayerProfile)
        {
            int index = 0;

            // Deserailize in the same order the object was serialized!
            Protocol.Deserialize(out coins, memPlayerProfile, ref index);
            Protocol.Deserialize(out roomID, memPlayerProfile, ref index);
            Protocol.Deserialize(out slot, memPlayerProfile, ref index);
        }

        PlayerProfile playerProfile = new PlayerProfile();
        playerProfile.coins = coins;
        playerProfile.roomID = roomID;
        playerProfile.slot = slot;

        // Return a new instance of DamageInfo with all the data.
        return playerProfile;
    }

    const int INTEGER_BYTE = 1; // Unsigned (0 to 255)
    const int INTEGER_SBYTE = 2; // Signed (-128 to 127)
    const int INTEGER_SHORT = 2; // Signed (-32,768 to 32,767)
    const int INTEGER_USHORT = 2; // Unsigned (0 to 65,535)
    const int INTEGER_INT = 4; // Singed (-2,147,483,648 to 2,147,483,647)
    const int INTEGER_UINT = 4; // Unsigned (0 to 4,294,967,295)
    const int INTEGER_LONG = 8; //  Signed (-9,223,372,036,854,775,808 to 9,223,372,036,854,775,807)
    const int INTEGER_ULONG = 8; // Unsinged (0 to 18,446,744,073,709,551,615)
    const int FLOAT_FLOAT = 4; // ±1.5e−45 to ±3.4e38  (Precision:7 digits)
    const int FLOAT_DOUBLE = 8; // ±5.0e−324 to ±1.7e308 (Precision:15-16 digits)
    const int FLOAT_DECIMAL = 16; // (-7.9 x 1028 to 7.9 x 1028) / (100 to 28) (Precision:28-29 digits)
    const int CHARACTER_CHAR = 2;
    const int OTHER_DATETIME = 8;
    const int OTHER_BOOL = 1;
}


public static class SerializationClass
{
    public static byte[] SerializeObject(object obj)
    {
        if (obj == null)
            return null;

        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        bf.Serialize(ms, obj);

        return ms.ToArray();
    }

    public static object DeserializeObject(byte[] arrBytes)
    {
        MemoryStream memStream = new MemoryStream();
        BinaryFormatter binForm = new BinaryFormatter();
        memStream.Write(arrBytes, 0, arrBytes.Length);
        memStream.Seek(0, SeekOrigin.Begin);
        object obj = (object)binForm.Deserialize(memStream);

        return obj;
    }
}