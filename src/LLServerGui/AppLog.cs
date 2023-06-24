using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using ImGuiNET;
using LibreLancer.ImUI;

namespace LLServer;

public class AppLog : IDisposable
{
    private const int START_CAPACITY = 4096;
    private IntPtr buffer;
    private int bufferCapacity;
    private int bufferSize;
    private List<int> lineOffsets = new List<int>();
    private bool autoScroll = true;
    private ListClipper listClipper;
    public AppLog()
    {
        buffer = Marshal.AllocHGlobal(START_CAPACITY);
        bufferCapacity = START_CAPACITY;
        lineOffsets.Add(0);
        listClipper = new ListClipper();
    }

    public unsafe void AppendText(string s)
    {
        var byteSize = Encoding.UTF8.GetByteCount(s);
        if (bufferSize + byteSize >= bufferCapacity)
        {
            int newCapacity = Math.Max(bufferSize + byteSize + 512, bufferSize * 2);
            buffer = Marshal.ReAllocHGlobal(buffer, new IntPtr(newCapacity));
            bufferCapacity = newCapacity;
        }
        var newString = new Span<byte>((void*) (buffer + bufferSize), byteSize);
        Encoding.UTF8.GetBytes(s.AsSpan(), newString);
        for (int i = 0; i < newString.Length; i++) {
            if (newString[i] == (byte) '\n') {
                lineOffsets.Add(bufferSize + i + 1);
            }
        }
        bufferSize += byteSize;
    }

    public unsafe void Draw()
    {
        ImGui.Checkbox("Autoscroll", ref autoScroll);
        ImGui.SameLine();
        if (ImGui.Button("Clear"))
        {
            bufferSize = 0;
            lineOffsets = new List<int> {0};
        }
        ImGui.Separator();
        IntPtr bufferEnd = buffer + bufferSize;
        if (ImGui.BeginChild("scrolling", Vector2.Zero, false, ImGuiWindowFlags.HorizontalScrollbar))
        {
            ImGui.PushFont(ImGuiHelper.SystemMonospace);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);
            listClipper.Begin(lineOffsets.Count);
            while (listClipper.Step())
            {
                for (int line_no = listClipper.DisplayStart; line_no < listClipper.DisplayEnd; line_no++)
                {
                    IntPtr line_start = buffer + lineOffsets[line_no];
                    IntPtr line_end = (line_no + 1 < lineOffsets.Count) ? (buffer + lineOffsets[line_no + 1] - 1) : bufferEnd;
                    ImGuiNative.igTextUnformatted((byte*)line_start, (byte*)line_end);
                }
            }
            listClipper.End();
            ImGui.PopStyleVar();
            ImGui.PopFont();
            if(autoScroll && ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
                ImGui.SetScrollHereY(1.0f);
            ImGui.EndChild();
        }
    }

    public void Dispose()
    {
        Marshal.FreeHGlobal(buffer);
        listClipper.Dispose();
    }
    
}