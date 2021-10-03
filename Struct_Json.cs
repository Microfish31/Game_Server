using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System;


namespace Struct_Json
{
    public class StructDewrapper<T>
    {
        public T[] items { get; set; }   // 不能改 items?
    }

    public class StructWrapper<T>
    {
        public T[] items { get; set; }  // 不能改 items?
    }

    public class StructJson
    {

        public T BytesToStruct<T>(byte[] data_byte)
        {
            Utf8JsonReader utf8Reader = new Utf8JsonReader(data_byte);
            var data_struct = JsonSerializer.Deserialize<T>(ref utf8Reader);
            return data_struct;
        }

        public T[] BytesToStructArray<T>(byte[] data_byte)
        {
            StructDewrapper<T> dewrap = new StructDewrapper<T>();
            var utf8Reader = new Utf8JsonReader(data_byte);
            dewrap = JsonSerializer.Deserialize<StructDewrapper<T>>(ref utf8Reader);
            return dewrap.items;
        }

        public byte[] StructToBytes<T>(T data_struct)
        {
            byte[] jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(data_struct);
            return jsonUtf8Bytes;
        }

        public byte[] StructToBytes<T>(T[] data_struct)
        {
            StructWrapper<T> wrap = new StructWrapper<T>();
            wrap.items = data_struct;
            byte[] jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(wrap);
            return jsonUtf8Bytes;
        }

        public void StructFileWrite<T>(T[] data_struct, string path)
        {
            StructWrapper<T> wrap = new StructWrapper<T>();
            wrap.items = data_struct;
            byte[] jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(wrap);
            File.WriteAllBytes(path, jsonUtf8Bytes);
        }

        public void StructFileWrite<T>(T data_struct, string path)
        {
            byte[] jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(data_struct);
            File.WriteAllBytes(path, jsonUtf8Bytes);
        }

        public void BytesFileWrite(byte[] get_byte_array, string path) 
        {
            File.WriteAllBytes(path, get_byte_array);
        }

        public T FileReadStruct<T>(string path)
        {
            T type_data;

            byte[] jsonUtf8Bytes = File.ReadAllBytes(path);
            var utf8Reader = new Utf8JsonReader(jsonUtf8Bytes);
            type_data = JsonSerializer.Deserialize<T>(ref utf8Reader);
            return type_data;
        }

        public T[] FileReadStructArray<T>(string path)
        {
            StructDewrapper<T> dewrap = new StructDewrapper<T>();
            byte[] jsonUtf8Bytes = File.ReadAllBytes(path);
            var utf8Reader = new Utf8JsonReader(jsonUtf8Bytes);
            dewrap = JsonSerializer.Deserialize<StructDewrapper<T>>(ref utf8Reader);
            return dewrap.items;
        }
    }
}
