using System.Collections.Generic;
using ProtoBuf;

namespace woopmotd;

[ProtoContract]
public class MotdPacket
{
    [ProtoMember(1)] public List<string> Vtmls { get; set; }
}