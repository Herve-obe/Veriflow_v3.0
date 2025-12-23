using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Veriflow.Infrastructure.Services;

/// <summary>
/// Service for reading and writing WAV metadata (BWF and iXML)
/// </summary>
public class WavMetadataEditor
{
    // RIFF chunk IDs
    private const string RIFF_ID = "RIFF";
    private const string WAVE_ID = "WAVE";
    private const string BEXT_ID = "bext"; // Broadcast Wave Format Extension
    private const string IXML_ID = "iXML"; // iXML metadata
    private const string DATA_ID = "data";
    
    /// <summary>
    /// Read BWF metadata from WAV file
    /// </summary>
    public async Task<Dictionary<string, string>> ReadBwfMetadataAsync(string filePath, CancellationToken ct = default)
    {
        var metadata = new Dictionary<string, string>();
        
        if (!File.Exists(filePath) || !filePath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
            return metadata;
        
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new BinaryReader(stream);
        
        // Read RIFF header
        var riffId = new string(reader.ReadChars(4));
        if (riffId != RIFF_ID) return metadata;
        
        var fileSize = reader.ReadInt32();
        var waveId = new string(reader.ReadChars(4));
        if (waveId != WAVE_ID) return metadata;
        
        // Search for bext chunk
        while (stream.Position < stream.Length - 8)
        {
            var chunkId = new string(reader.ReadChars(4));
            var chunkSize = reader.ReadInt32();
            
            if (chunkId == BEXT_ID)
            {
                // Read BWF fields
                metadata["Description"] = ReadNullTerminatedString(reader, 256);
                metadata["Originator"] = ReadNullTerminatedString(reader, 32);
                metadata["OriginatorReference"] = ReadNullTerminatedString(reader, 32);
                metadata["OriginationDate"] = ReadNullTerminatedString(reader, 10);
                metadata["OriginationTime"] = ReadNullTerminatedString(reader, 8);
                
                var timeReferenceLow = reader.ReadUInt32();
                var timeReferenceHigh = reader.ReadUInt32();
                var timeReference = ((long)timeReferenceHigh << 32) | timeReferenceLow;
                metadata["TimeReference"] = timeReference.ToString();
                
                metadata["Version"] = reader.ReadUInt16().ToString();
                
                // Skip UMID (64 bytes)
                reader.ReadBytes(64);
                
                // Skip LoudnessValue, LoudnessRange, etc. (180 bytes)
                reader.ReadBytes(180);
                
                break;
            }
            else
            {
                // Skip this chunk
                stream.Seek(chunkSize + (chunkSize % 2), SeekOrigin.Current);
            }
        }
        
        return metadata;
    }
    
    /// <summary>
    /// Read iXML metadata from WAV file
    /// </summary>
    public async Task<Dictionary<string, string>> ReadIxmlMetadataAsync(string filePath, CancellationToken ct = default)
    {
        var metadata = new Dictionary<string, string>();
        
        if (!File.Exists(filePath) || !filePath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
            return metadata;
        
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new BinaryReader(stream);
        
        // Read RIFF header
        var riffId = new string(reader.ReadChars(4));
        if (riffId != RIFF_ID) return metadata;
        
        reader.ReadInt32(); // file size
        var waveId = new string(reader.ReadChars(4));
        if (waveId != WAVE_ID) return metadata;
        
        // Search for iXML chunk
        while (stream.Position < stream.Length - 8)
        {
            var chunkId = new string(reader.ReadChars(4));
            var chunkSize = reader.ReadInt32();
            
            if (chunkId == IXML_ID)
            {
                var xmlData = reader.ReadBytes(chunkSize);
                var xmlString = Encoding.UTF8.GetString(xmlData).TrimEnd('\0');
                
                try
                {
                    var doc = XDocument.Parse(xmlString);
                    
                    // Extract common iXML fields
                    metadata["Project"] = doc.Descendants("PROJECT").FirstOrDefault()?.Value ?? "";
                    metadata["Scene"] = doc.Descendants("SCENE").FirstOrDefault()?.Value ?? "";
                    metadata["Take"] = doc.Descendants("TAKE").FirstOrDefault()?.Value ?? "";
                    metadata["Tape"] = doc.Descendants("TAPE").FirstOrDefault()?.Value ?? "";
                    metadata["CircledTake"] = doc.Descendants("CIRCLED").FirstOrDefault()?.Value ?? "";
                    metadata["Note"] = doc.Descendants("NOTE").FirstOrDefault()?.Value ?? "";
                }
                catch
                {
                    // Invalid XML, skip
                }
                
                break;
            }
            else
            {
                stream.Seek(chunkSize + (chunkSize % 2), SeekOrigin.Current);
            }
        }
        
        return metadata;
    }
    
    /// <summary>
    /// Write BWF metadata to WAV file
    /// </summary>
    public async Task WriteBwfMetadataAsync(string filePath, Dictionary<string, string> metadata, CancellationToken ct = default)
    {
        if (!File.Exists(filePath) || !filePath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Invalid WAV file path");
        
        var tempFile = Path.GetTempFileName();
        
        try
        {
            await using var inputStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            await using var outputStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None);
            using var reader = new BinaryReader(inputStream);
            using var writer = new BinaryWriter(outputStream);
            
            // Read and write RIFF header
            var riffId = reader.ReadChars(4);
            writer.Write(riffId);
            var fileSize = reader.ReadInt32();
            writer.Write(fileSize); // Will update later
            var waveId = reader.ReadChars(4);
            writer.Write(waveId);
            
            bool bextWritten = false;
            
            // Copy chunks, replacing or adding bext
            while (inputStream.Position < inputStream.Length - 8)
            {
                var chunkId = new string(reader.ReadChars(4));
                var chunkSize = reader.ReadInt32();
                
                if (chunkId == BEXT_ID)
                {
                    // Replace existing bext chunk
                    WriteBextChunk(writer, metadata);
                    bextWritten = true;
                    
                    // Skip old bext data
                    inputStream.Seek(chunkSize + (chunkSize % 2), SeekOrigin.Current);
                }
                else if (chunkId == DATA_ID && !bextWritten)
                {
                    // Insert bext before data chunk
                    WriteBextChunk(writer, metadata);
                    bextWritten = true;
                    
                    // Write data chunk
                    writer.Write(chunkId.ToCharArray());
                    writer.Write(chunkSize);
                    var data = reader.ReadBytes(chunkSize);
                    writer.Write(data);
                    if (chunkSize % 2 == 1) reader.ReadByte();
                }
                else
                {
                    // Copy chunk as-is
                    writer.Write(chunkId.ToCharArray());
                    writer.Write(chunkSize);
                    var data = reader.ReadBytes(chunkSize);
                    writer.Write(data);
                    if (chunkSize % 2 == 1) reader.ReadByte();
                }
            }
            
            // Update file size
            outputStream.Seek(4, SeekOrigin.Begin);
            writer.Write((int)(outputStream.Length - 8));
            
            outputStream.Close();
            inputStream.Close();
            
            // Replace original file
            File.Delete(filePath);
            File.Move(tempFile, filePath);
        }
        catch
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
            throw;
        }
    }
    
    private void WriteBextChunk(BinaryWriter writer, Dictionary<string, string> metadata)
    {
        writer.Write(BEXT_ID.ToCharArray());
        writer.Write(602); // bext chunk size (version 1)
        
        WriteFixedString(writer, metadata.GetValueOrDefault("Description", ""), 256);
        WriteFixedString(writer, metadata.GetValueOrDefault("Originator", ""), 32);
        WriteFixedString(writer, metadata.GetValueOrDefault("OriginatorReference", ""), 32);
        WriteFixedString(writer, metadata.GetValueOrDefault("OriginationDate", ""), 10);
        WriteFixedString(writer, metadata.GetValueOrDefault("OriginationTime", ""), 8);
        
        var timeRef = long.TryParse(metadata.GetValueOrDefault("TimeReference", "0"), out var tr) ? tr : 0L;
        writer.Write((uint)(timeRef & 0xFFFFFFFF));
        writer.Write((uint)(timeRef >> 32));
        
        writer.Write((ushort)1); // Version
        
        // UMID (64 bytes) - zeros
        writer.Write(new byte[64]);
        
        // Reserved (190 bytes) - zeros
        writer.Write(new byte[190]);
    }
    
    private string ReadNullTerminatedString(BinaryReader reader, int maxLength)
    {
        var bytes = reader.ReadBytes(maxLength);
        var nullIndex = Array.IndexOf(bytes, (byte)0);
        if (nullIndex >= 0)
            return Encoding.ASCII.GetString(bytes, 0, nullIndex);
        return Encoding.ASCII.GetString(bytes);
    }
    
    private void WriteFixedString(BinaryWriter writer, string value, int length)
    {
        var bytes = new byte[length];
        if (!string.IsNullOrEmpty(value))
        {
            var valueBytes = Encoding.ASCII.GetBytes(value);
            Array.Copy(valueBytes, bytes, Math.Min(valueBytes.Length, length - 1));
        }
        writer.Write(bytes);
    }
}
