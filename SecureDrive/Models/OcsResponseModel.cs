﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SecureDrive.Models
{
    public class OcsResponseModel
    {
        [XmlRoot("ocs")]
        public class OcsResponse
        {
            [XmlElement("meta")]
            public Meta Meta { get; set; }
            [XmlElement("data")]
            public Data Data { get; set; }
        }

        public class Meta
        {
            [XmlElement("status")]
            public string Status { get; set; }
            [XmlElement("statuscode")]
            public int StatusCode { get; set; }
            [XmlElement("message")]
            public string Message { get; set; }
        }

        public class Data
        {
            [XmlElement("element")]
            public List<GroupElements> Elements { get; set; }
        }

        public class GroupElements
        {
            [XmlElement("id")]
            public int Id { get; set; }
            [XmlElement("mount_point")]
            public string MountPoint { get; set; }
            [XmlElement("groups")]
            public Groups Groups { get; set; }
            [XmlElement("quota")]
            public long Quota { get; set; }
            [XmlElement("size")]
            public long Size { get; set; }
            [XmlElement("acl")]
            public int Acl { get; set; }
            [XmlElement("parentspath")]
            public string ParentsPath { get; set; }
            [XmlArray("manage")]
            [XmlArrayItem("element")]
            public List<ManageEntry> Manage { get; set; }
        }

        public class ManageEntry
        {
            [XmlElement("type")]
            public string Type { get; set; }
            [XmlElement("id")]
            public string Id { get; set; }
            [XmlElement("displayname")]
            public string DisplayName { get; set; }
        }

        public class Groups
        {
            [XmlAnyElement]
            public XmlElement[] AnyElements { get; set; }

            // Method เพื่อแปลงเป็น Dictionary ที่มี Group details
            public Dictionary<string, GroupInfo> ToGroupDictionary()
            {
                var dict = new Dictionary<string, GroupInfo>();
                if (AnyElements != null)
                {
                    foreach (var el in AnyElements)
                    {
                        var groupInfo = new GroupInfo
                        {
                            Name = el.Name,
                            DisplayName = el["displayName"]?.InnerText,
                            Type = el["type"]?.InnerText
                        };

                        // Parse permissions
                        if (int.TryParse(el["permissions"]?.InnerText, out int permissions))
                        {
                            groupInfo.Permissions = permissions;
                        }

                        // Parse is_drive
                        if (int.TryParse(el["is_drive"]?.InnerText, out int isDrive))
                        {
                            groupInfo.IsDrive = isDrive;
                        }

                        dict[el.Name] = groupInfo;
                    }
                }
                return dict;
            }

            // Method เดิมสำหรับ backward compatibility (ถ้ามีการใช้งานอยู่)
            public Dictionary<string, int> ToDictionary()
            {
                var dict = new Dictionary<string, int>();
                if (AnyElements != null)
                {
                    foreach (var el in AnyElements)
                    {
                        if (int.TryParse(el["permissions"]?.InnerText, out int permissions))
                        {
                            dict[el.Name] = permissions;
                        }
                    }
                }
                return dict;
            }
        }

        // Class สำหรับเก็บข้อมูล Group
        public class GroupInfo
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public int Permissions { get; set; }
            public int IsDrive { get; set; }
            public string Type { get; set; }
        }
    }
}
