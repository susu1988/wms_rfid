﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using THOK.Authority.Bll.Interfaces;
using THOK.Authority.Bll.Models;
using THOK.Authority.Dal.Interfaces;
using THOK.Authority.DbModel;

namespace THOK.Authority.Bll.Service
{
    public class HelpContentService : ServiceBase<HelpContent>, IHelpContentService
    {
        [Dependency]
        public IHelpContentRepository HelpContenRepository { get; set; }
        [Dependency]
        public ISystemRepository SystemRepository { get; set; }
        [Dependency]
        public IModuleRepository ModuleRepository { get; set; }
        [Dependency]
        public IHelpContentRepository HelpContentRepository { get; set; }

        protected override Type LogPrefix
        {
            get { return this.GetType(); }
        }


        public bool Add(HelpContent helpContent, out string strResult)
        {
            strResult = string.Empty;
            bool result = false;
            var help = new HelpContent();
                if (helpContent != null)
                {
                    try
                    {
                        help.ID = Guid.NewGuid();
                        help.ContentCode = helpContent.ContentCode;
                        help.ContentName = helpContent.ContentName;
                        help.ContentPath = helpContent.ContentPath;
                        help.NodeType = helpContent.NodeType;
                        help.FatherNodeID = helpContent.FatherNodeID != null ? helpContent.FatherNodeID : help.ID;
                        help.ModuleID = helpContent.ModuleID;
                        help.NodeOrder = helpContent.NodeOrder;
                        help.IsActive = helpContent.IsActive;
                        help.UpdateTime = DateTime.Now;

                        HelpContentRepository.Add(help);
                        HelpContentRepository.SaveChanges();
                        result = true;
                    }
                    catch (Exception ex)
                    {
                        strResult = "原因：" + ex.Message;
                    }
                }
            return result;
        }

        public string WhatType(string nodeType)
        {
            string typeStr = "";
            switch (nodeType)
            {
                case "1":
                    typeStr = "第一节点";
                    break;
                case "2":
                    typeStr = "中间节点";
                    break;
                case "3":
                    typeStr = "末端节点";
                    break;
            }
            return typeStr;
        }
        public object GetDetails(int page, int rows, string QueryString, string Value)
        {
            string ContentName = "";
            string ContentCode = "";
            if (QueryString == "ContentName")
            {
                ContentName = Value;
            }
            else
            {
                ContentCode = Value;
            }
            IQueryable<HelpContent> HelpContentQuery = HelpContentRepository.GetQueryable();
            var HelpContent = HelpContentQuery.Where(c => c.ContentName.Contains(ContentName) && c.ContentCode.Contains(ContentCode))
                .OrderBy(c => c.ContentCode)
                .Select(c => c);
            if (!ContentName.Equals(string.Empty))
            {
                HelpContent = HelpContent.Where(p => p.ContentName == ContentName);
            }
            int total = HelpContent.Count();
            HelpContent = HelpContent.Skip((page - 1) * rows).Take(rows);

            var temp = HelpContent.ToArray().Select(c => new
            {
                ID = c.ID,
                ContentCode = c.ContentCode,
                ContentName = c.ContentName,
                FatherNode = c.ID ==c.FatherNodeID? "":  c.FatherNode.ContentCode + " " + c.FatherNode.ContentName,
                NodeType = WhatType(c.NodeType),
                NodeOrder = c.NodeOrder,
                IsActive = c.IsActive == "1" ? "可用" : "不可用"
            });
            return new { total, rows = temp.ToArray() };
        }

        public bool Save(string ID, string ContentCode, string ContentName, string ContentPath, string FatherNodeID, string ModuleID, int NodeOrder, string IsActive, out string strResult)
        {
            strResult = string.Empty;
            try
            {
                Guid new_ID = new Guid(ID);
                var help = HelpContentRepository.GetQueryable()
                    .FirstOrDefault(i => i.ID == new_ID);
                help.ContentCode = ContentCode;
                help.ContentName = ContentName;
                help.ContentPath = ContentPath;
                help.FatherNodeID = new Guid(FatherNodeID);
                help.ModuleID = new Guid(ModuleID);
                help.NodeOrder = NodeOrder;
                help.IsActive = IsActive;
                HelpContentRepository.SaveChanges();
               
            }
            catch (Exception ex)
            {
                strResult = "原因：" + ex.Message;
            }
            return true;
        }

        public object GetDetails2(int page, int rows, string ContentCode, string ContentName, string NodeType, string FatherNodeID, string ModuleID, string IsActive)
        {
            IQueryable<HelpContent> HelpContentQuery = HelpContentRepository.GetQueryable();
            var HelpContent = HelpContentQuery.Where(c => c.ContentCode.Contains(ContentCode) &&
                                                          c.ContentName.Contains(ContentName) &&
                                                          c.IsActive.Contains(IsActive) &&
                                                          c.NodeType.Contains(NodeType));
            if (!FatherNodeID.Equals(string.Empty) && FatherNodeID != null)
            {
                Guid Father_NodeID = new Guid(FatherNodeID);
                HelpContent = HelpContent.Where(h => h.FatherNodeID == Father_NodeID);
            }
            if (!ModuleID.Equals(string.Empty) && ModuleID != null)
            {
                Guid Module_ID = new Guid(ModuleID);
                HelpContent = HelpContent.Where(h => h.ModuleID == Module_ID);
            }
            HelpContent = HelpContent.OrderBy(h => h.ContentCode);
            int total = HelpContent.Count();
            HelpContent = HelpContent.Skip((page - 1) * rows).Take(rows);

            var temp = HelpContent.ToArray().Select(c => new
            {
               ID=c.ID,
               ContentCode= c.ContentCode,
               ContentName= c.ContentName,
               ContentPath= c.ContentPath,
               FatherNodeName =c.ID ==c.FatherNodeID? "": c.FatherNode.ContentName,
               ModuleID= c.ModuleID,
               ModuleName = c.Module.ModuleName,
               FatherNodeID= c.FatherNodeID,
               NodeType= WhatType(c.NodeType),
               NodeOrder= c.NodeOrder,
               IsActive = c.IsActive == "1" ? "可用" : "不可用",
               UpdateTime=c.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss")
            });
            return new { total, rows = temp.ToArray() };
        }

        public bool Delete(string ID)
        {
            Guid new_ID = new Guid(ID);
            var help = HelpContentRepository.GetQueryable()
                .FirstOrDefault(i => i.ID == new_ID);
            if (ID != null)
            {
                HelpContentRepository.Delete(help);
                HelpContentRepository.SaveChanges();
            }
            else
                return false;
            return true;
        }
        public object GetHelpContentTree(string sysId)
        {
            IQueryable<THOK.Authority.DbModel.System> querySystem = SystemRepository.GetQueryable();
            IQueryable<THOK.Authority.DbModel.Module> queryModule = ModuleRepository.GetQueryable();
            IQueryable<THOK.Authority.DbModel.HelpContent> queryHelpContent = HelpContentRepository.GetQueryable();
            var systems = querySystem.AsEnumerable();
            if (sysId != null && sysId != string.Empty)
            {
                Guid gsystemid = new Guid(sysId);
                systems = querySystem.Where(i => i.SystemID == gsystemid)
                                     .Select(i => i);
            }

            HashSet<Tree> systemTreeSet = new HashSet<Tree>();
            foreach (var system in systems)
            {
                Tree systemTree = new Tree();
                systemTree.id = system.SystemID.ToString();
                systemTree.text = system.SystemName;
                var helpContent = queryHelpContent.Where(m => m.Module.System.SystemID == system.SystemID && m.ID == m.FatherNodeID)
                                         .OrderBy(m => m.NodeOrder)
                                         .Select(m => m);
                var systemAttribute = new
                {
                    AttributeId =system.SystemID,
                    AttributeTxt =system.SystemName
                };
                systemTree.attributes = systemAttribute;
                HashSet<Tree> contentTreeSet = new HashSet<Tree>();
                foreach (var item in helpContent)
                {
                    Tree helpContentTree = new Tree();
                    helpContentTree.id = item.ContentCode;
                    helpContentTree.text = item.ContentCode + item.ContentName;
                    var helpAttribute = new
                    {
                        AttributeId = item.ID,
                        AttributeTxt = item.ContentText
                    };
                    helpContentTree.attributes = helpAttribute;
                    contentTreeSet.Add(helpContentTree);
                    GetChildTree(helpContentTree, item);
                    contentTreeSet.Add(helpContentTree);
                }
                systemTree.children = contentTreeSet.ToArray();
                systemTreeSet.Add(systemTree);
            }
            return systemTreeSet.ToArray();
        }
        private void GetChildTree(Tree helpContentTree, HelpContent helpContent)
        {
            HashSet<Tree> childContentSet = new HashSet<Tree>();
            var helpContents = from m in helpContent.HelpContents
                          orderby m.NodeOrder
                               where m.FatherNodeID == helpContent.FatherNodeID
                          select m;
            foreach (var item in helpContents)
            {
                if (item.ID != helpContent.ID)
                {
                    Tree childContentTree = new Tree();
                    childContentTree.id = item.ContentCode;
                    childContentTree.text = item.ContentCode + item.ContentName;
                    var childAttribute = new
                    {
                        AttributeId = item.ID,
                        AttributeTxt = item.ContentText
                    };
                    childContentTree.attributes = childAttribute;
                    childContentSet.Add(childContentTree);
                }
            }
            helpContentTree.children = childContentSet.ToArray();
        }
        public bool EditSave(string helpId, string contentText, out string strResult)
        {
            strResult = string.Empty;
            try
            {
                Guid new_ID = new Guid(helpId);
                var help = HelpContentRepository.GetQueryable()
                    .FirstOrDefault(i => i.ID == new_ID);
                help.ContentText = contentText;
                HelpContentRepository.SaveChanges();
            }
            catch (Exception ex)
            {
                strResult = "原因：" + ex.Message;
            }
            return true;
        }

        public object GetContentTxt(string helpId)
        {
            string content_text = "";
            Guid new_ID = new Guid(helpId);
            var system = SystemRepository.GetQueryable().FirstOrDefault(i => i.SystemID == new_ID);
            if (system != null)
            {
               var help = HelpContentRepository.GetQueryable().Where(i => i.Module.System_SystemID == new_ID).OrderBy(h=>h.ContentCode);
               foreach (var text in help)
               {
                   content_text = content_text + text.ContentText;
               }
               var helper = HelpContentRepository.GetQueryable().FirstOrDefault(h => h.Module.System_SystemID == new_ID);
               helper.ContentText = content_text;
               return new { helper.ContentText };
            }
            else
            {
                var help = HelpContentRepository.GetQueryable().FirstOrDefault(i => i.ID == new_ID);
                if (help.NodeType == "1")
                {
                    var helpChild = HelpContentRepository.GetQueryable().Where(i => i.FatherNodeID == help.FatherNodeID).OrderBy(h => h.ContentCode);
                    foreach (var text in helpChild)
                    {
                        content_text = content_text + text.ContentText;
                    }
                    help.ContentText = content_text;
                    return new { help.ContentText };
                }
                else
                {
                    return new { help.ContentText };
                }
            }
        }

        public object Help(string helpId)
        {
            Guid new_ID = new Guid(helpId);
            var help = HelpContentRepository.GetQueryable().FirstOrDefault(i => i.ModuleID == new_ID);
            if (help != null)
            {
                return new { help.ContentText };
            }
            return new { ContentText =@"<p>当前索引未找到相关文档内容！</p> "};
        }


        public object GetSingleContentTxt(string helpId)
        {
            Guid newId = new Guid(helpId);
            var system=SystemRepository.GetQueryable().FirstOrDefault(i=>i.SystemID==newId);
            var help = HelpContentRepository.GetQueryable().FirstOrDefault(i => i.ID == newId);
            if (system!= null)
            {
                help = HelpContentRepository.GetQueryable().FirstOrDefault(i => i.Module.System_SystemID == newId);
            }
            return help != null ? new { help.ContentText } : new { ContentText = "" };
        }


        public System.Data.DataTable GetHelpConten(int page, int rows, string contentCode, string contentName, string nodeType, string fatherNodeID, string moduleID, string isActive)
        {
            IQueryable<HelpContent> contenQuery = HelpContenRepository.GetQueryable();
            var conten = contenQuery.Where(c => c.ContentCode.Contains(contentCode)
                && c.ContentName.Contains(contentName)
                && c.NodeType.Contains(nodeType));
                if(!fatherNodeID.Equals(string.Empty))
                {
                    Guid fID=new Guid (fatherNodeID);
                    conten = conten.Where(c => c.FatherNode.ID == fID);
                }
                if(!moduleID.Equals(string.Empty))
                {
                    Guid mID=new Guid(moduleID);
                    conten = conten.Where(c => c.Module.ModuleID == mID);
                }
                var temp = conten.AsEnumerable().OrderByDescending(c => c.UpdateTime).AsEnumerable()
                .Select(c => new
                {
                    c.ContentCode,
                    c.ContentName,
                    c.ContentPath,
                    NodeType = c.NodeType == "1" ? "第一节点" : c.NodeType == "2" ? "中间节点" : "末级节点",
                    FatherNodeID = c.FatherNode.ContentName,
                    ModuleID=c.Module.ModuleID,
                    c.Module.ModuleName,
                    c.NodeOrder,
                    IsActive = c.IsActive == "1" ? "可用" : "不可用",
                    UpdateTime = c.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss")
                });
            if (!isActive.Equals(""))
            {
                conten = contenQuery.Where(c => c.ContentCode.Contains(contentCode)
                && c.ContentName.Contains(contentName)
                && c.NodeType.Contains(nodeType));
                if (!fatherNodeID.Equals(string.Empty))
                {
                    Guid fID = new Guid(fatherNodeID);
                    conten = conten.Where(d => d.FatherNode.ID == fID);
                }
                if (!moduleID.Equals(string.Empty))
                {
                    Guid mID = new Guid(moduleID);
                    conten = conten.Where(d => d.Module.ModuleID == mID);
                }
                temp = conten.AsEnumerable().OrderByDescending(c => c.UpdateTime).AsEnumerable()
                .Select(c => new
                {
                    c.ContentCode,
                    c.ContentName,
                    c.ContentPath,
                    NodeType = c.NodeType == "1" ? "第一节点" : c.NodeType == "2" ? "中间节点" : "末级节点",
                    FatherNodeID = c.FatherNode.ContentName,
                    ModuleID = c.Module.ModuleID,
                    c.Module.ModuleName,
                    c.NodeOrder,
                    IsActive = c.IsActive == "1" ? "可用" : "不可用",
                    UpdateTime = c.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Columns.Add("目录编码", typeof(string));
            dt.Columns.Add("目录名称", typeof(string));
            dt.Columns.Add("目录路径", typeof(string));
            dt.Columns.Add("节点类型", typeof(string));
            dt.Columns.Add("父节点", typeof(string));
            dt.Columns.Add("模块ID", typeof(string));
            dt.Columns.Add("节点顺序", typeof(string));
            dt.Columns.Add("状态", typeof(string));
            dt.Columns.Add("更新时间", typeof(string));
            foreach (var item in temp)
            {
                dt.Rows.Add
                    (
                        item.ContentCode,
                        item.ContentName,
                        item.ContentPath,
                        item.NodeType,
                        item.FatherNodeID,
                        item.ModuleName,
                        item.NodeOrder,
                        item.IsActive,
                        item.UpdateTime
                    );

            }
            return dt;
        }
    }
}
