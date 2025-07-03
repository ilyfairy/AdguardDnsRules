#!/usr/bin/env dotnet run
using System.Runtime.CompilerServices;
using System.Text;

var url = "https://raw.githubusercontent.com/Loyalsoldier/surge-rules/release/direct.txt";
var appendList = 
"""
[/cn/] {DNS}
""";

var outputDir = Environment.GetEnvironmentVariable("OUTPUT_DIR") switch
{
    { } env => env,
    _ => Environment.CurrentDirectory,
};
var appendDns = Environment.GetEnvironmentVariable("APPEND_DNS") switch
{
    { } env => env.Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries),
    _ => ["223.5.5.5"],
};

List<(string Name, string[] Dns)> list =
[
    ("AdGuard-Dns-Rules-ChinaTelecom-Anhui.txt", ["61.132.163.68", "202.102.213.68"]),
    ("AdGuard-Dns-Rules-ChinaTelecom-Fujian.txt", ["218.85.152.99", "218.85.157.99"]),
    ("AdGuard-Dns-Rules-ChinaTelecom-Guangdong.txt", ["202.96.128.86", "202.96.128.166"]),
    ("AdGuard-Dns-Rules-ChinaTelecom-Guangxi.txt", ["202.103.225.68"]),
    ("AdGuard-Dns-Rules-ChinaTelecom-Guizhou.txt", ["202.98.192.67", "202.98.198.167"]),
    ("AdGuard-Dns-Rules-ChinaTelecom-Henan.txt", ["222.88.88.88", "222.85.85.85"]),
    ("AdGuard-Dns-Rules-ChinaTelecom-Heilongjiang.txt", ["219.147.198.230"]),
    ("AdGuard-Dns-Rules-ChinaTelecom-Hubei.txt", ["202.103.0.68"]),
    ("AdGuard-Dns-Rules-ChinaTelecom-Jiangsu.txt", ["218.2.2.2", "218.4.4.4"]),
    ("AdGuard-Dns-Rules-ChinaTelecom-Shaanxi.txt", ["218.30.19.40", "61.134.1.4"]),
    ("AdGuard-Dns-Rules-ChinaTelecom-Shanghai.txt", ["202.96.209.133"]),
    ("AdGuard-Dns-Rules-ChinaTelecom-Sichuan.txt", ["61.139.2.69", "218.6.200.139"]),
    ("AdGuard-Dns-Rules-ChinaTelecom-Yunnan.txt", ["222.172.200.68", "61.166.150.123"]),

    ("AdGuard-Dns-Rules-ChinaUnicom-Beijing.txt", ["123.123.123.123", "123.123.123.124"]),
    ("AdGuard-Dns-Rules-ChinaUnicom-Chongqing.txt", ["221.5.203.98", "221.7.92.98"]),
    ("AdGuard-Dns-Rules-ChinaUnicom-Guangdong.txt", ["210.21.196.6", "221.5.88.88"]),
    ("AdGuard-Dns-Rules-ChinaUnicom-Hebei.txt", ["202.99.160.68", "202.99.166.4"]),
    ("AdGuard-Dns-Rules-ChinaUnicom-Heilongjiang.txt", ["202.97.224.69", "202.97.224.68"]),
    ("AdGuard-Dns-Rules-ChinaUnicom-Jilin.txt", ["202.98.0.68"]),
    ("AdGuard-Dns-Rules-ChinaUnicom-Jiangsu.txt", ["221.6.4.66", "221.6.4.67"]),
    ("AdGuard-Dns-Rules-ChinaUnicom-Inner-Mongolia.txt", ["202.99.224.68", "202.99.224.8"]),
    ("AdGuard-Dns-Rules-ChinaUnicom-Shandong.txt", ["202.102.128.68", "202.102.152.3"]),
    ("AdGuard-Dns-Rules-ChinaUnicom-Shanxi.txt", ["202.99.192.66", "202.99.192.68"]),
    ("AdGuard-Dns-Rules-ChinaUnicom-Sichuan.txt", ["119.6.6.6"]),
    ("AdGuard-Dns-Rules-ChinaUnicom-Zhejiang.txt", ["221.12.1.227", "221.12.33.227"]),

    ("AdGuard-Dns-Rules-ChinaMobile-Beijing.txt", ["221.130.33.60", "221.130.33.52"]),
    ("AdGuard-Dns-Rules-ChinaMobile-Guangdong.txt", ["211.136.192.6"]),
    ("AdGuard-Dns-Rules-ChinaMobile-Jiangsu.txt", ["221.131.143.69", "112.4.0.55"]),
    ("AdGuard-Dns-Rules-ChinaMobile-Anhui.txt", ["211.138.180.2", "211.138.180.3"]),
    ("AdGuard-Dns-Rules-ChinaMobile-Shandong.txt", ["218.201.96.130", "211.137.191.26"])
];


outputDir = Path.GetFullPath(outputDir);
Console.WriteLine($"输出目录: {outputDir}");

using HttpClient httpClient = new();
var bytes = await httpClient.GetByteArrayAsync(url);

Parallel.ForEach(list, (item, _) =>
{
    Write(bytes, Path.Combine(outputDir, item.Name), [.. item.Dns, .. appendDns]);
    Console.WriteLine($"{item.Name} 完成");
});
Console.WriteLine("全部完成");



[MethodImpl(MethodImplOptions.AggressiveOptimization)]
void Write(byte[] bytes, string filePath, string[] dns)
{
    using var outputStream = new FileStream(filePath, FileMode.Create);
    using var writer = new StreamWriter(outputStream, Encoding.UTF8);
    var reader = new StreamReader(new MemoryStream(bytes), Encoding.UTF8);

    Span<char> buffer = stackalloc char[1024];
    int bufferIndex = 0;

    string dnsString = string.Join(' ', dns);
    writer.WriteLine(appendList.Replace("{DNS}", dnsString));
    writer.WriteLine();

    while (true)
    {
        if (reader.EndOfStream)
            break;
        buffer[..bufferIndex].Clear();
        bufferIndex = 0;
        while (reader.Read() is int c)
        {
            if (c is '\r' or '\n' or -1)
                break;
            buffer[bufferIndex++] = (char)c;
        }
        var line = buffer[..bufferIndex].Trim(['\r', '\n', ' ', '\t', '.']);
        if (line.IsEmpty)
            continue;

        writer.Write("[/");
        writer.Write(line);
        writer.Write("/]");
        writer.Write(' ');
        writer.WriteLine(dnsString);
    }
}
