﻿using System.Web.Mvc;
using System.Web.Script.Serialization;
using THOK.Common.WebUtil;
using Microsoft.Practices.Unity;
using THOK.Security;
using THOK.Authority.Bll.Interfaces;
using THOK.Authority.Bll.Models;

namespace Authority.Controllers
{
    public class HomeController : Controller
    {
        [Dependency]
        public IModuleService ModuleService { get; set; }
        [Dependency]
        public ICityService CityService { get; set; }
        [Dependency]
        public IServerService ServerService { get; set; }
        [Dependency]
        public ISystemService SystemService { get; set; }
        [Dependency]
        public IFormsAuthenticationService FormsService { get; set; }
        [Dependency]
        public IUserService UserService { get; set; }
        public ActionResult Index()
        {
            string userName = this.GetCookieValue("username");
            string cityId = this.GetCookieValue("cityid");
            string serverId = this.GetCookieValue("serverid");
            string systemId = this.GetCookieValue("systemid");
            string localip = this.ControllerContext.HttpContext.Request.UserHostAddress;
            if (!cityId.Equals(string.Empty) && !serverId.Equals(string.Empty) && !systemId.Equals(string.Empty) && UserService.CheckAdress(userName, localip))
            {

                ViewBag.CityName = CityService.GetCityByCityID(cityId).ToString();
                ViewBag.ServerName = ServerService.GetServerById(serverId).ToString();
                ViewBag.SystemName = SystemService.GetSystemById(systemId).ToString();
                ViewBag.userName = userName;
                ViewBag.localip = localip;
            }
            else
            {
                this.RemoveCookie(cityId);
                this.RemoveCookie(serverId);
                this.RemoveCookie(systemId);
                this.RemoveCookie(userName);
                FormsService.SignOut();
                if (this.ControllerContext.HttpContext.Request.IsAuthenticated)
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            Session["userName"] = userName;
            return View();
        }
        public ActionResult GetUser()
        {
            return Json(new { Identity = new { User.Identity.IsAuthenticated, User.Identity.Name } }, "text", JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult GetMenu()
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            var jmenus = serializer.Deserialize<Menu[]>(JsonHelper.getJsonMenu());

            var menus = ModuleService.GetUserMenus(User.Identity.Name,this.GetCookieValue("cityid"),this.GetCookieValue("systemid"));          
            return Json(menus,"text",JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult GetFun(string moduleId)
        {
            Fun fun = new Fun()
            {
                funs = new Fun[] { 
                new Fun() { funname = "search", iconCls = "icon-search", funid = "EEB02601-9BF6-412F-A63E-92857BF38638", isActive = true },
                new Fun() { funname = "add", iconCls = "icon-search", funid = "EEB02601-9BF6-412F-A63E-92857BF38638" , isActive = true},
                new Fun() { funname = "edit", iconCls = "icon-search", funid = "EEB02601-9BF6-412F-A63E-92857BF38638" , isActive = true},
                new Fun() { funname = "delete", iconCls = "icon-search", funid = "EEB02601-9BF6-412F-A63E-92857BF38638" , isActive = true},
                 new Fun() { funname = "functionadmin", iconCls = "icon-search", funid = "EEB02601-9BF6-412F-A63E-92857BF38638" , isActive = true},
                new Fun() { funname = "permissionadmin", iconCls = "icon-search", funid = "EEB02601-9BF6-412F-A63E-92857BF38638" , isActive = true},
                new Fun() { funname = "useradmin", iconCls = "icon-search", funid = "EEB02601-9BF6-412F-A63E-92857BF38638" , isActive = true},
                new Fun() { funname = "roleadmin", iconCls = "icon-search", funid = "EEB02601-9BF6-412F-A63E-92857BF38638" , isActive = true},
                new Fun() { funname = "authorize", iconCls = "icon-search", funid = "EEB02601-9BF6-412F-A63E-92857BF38638" , isActive = true},                
                new Fun() { funname = "print", iconCls = "icon-search", funid = "EEB02601-9BF6-412F-A63E-92857BF38638" , isActive = true},
                new Fun() { funname = "help", iconCls = "icon-search", funid = "EEB02601-9BF6-412F-A63E-92857BF38638", isActive = true }
                }
            };
            var funs = ModuleService.GetModuleFuns(User.Identity.Name, this.GetCookieValue("cityid"), moduleId);
            return Json(funs,"text",JsonRequestBehavior.AllowGet);
        }

        public ActionResult PageNotFound(string PageNotFoundLog)
        {
            ViewBag.PageNotFoundLog = PageNotFoundLog;
            return View();
        }

        public ActionResult ServerError(string ServerErrorLog)
        {
            ViewBag.ServerErrorLog = ServerErrorLog;
            return View();
        }

        public ActionResult Error(string errorLog)
        {
            ViewBag.ErrorLog = errorLog;
            return View();
        }

        public ActionResult AjaxPageNotFound(string AjaxPageNotFoundLog)
        {
            this.ControllerContext.HttpContext.Response.StatusCode = 404;
            return Json(JsonMessageHelper.getJsonMessage(false, AjaxPageNotFoundLog, ""), "text", JsonRequestBehavior.AllowGet);
        }

        public ActionResult AjaxServerError(string AjaxServerErrorLog)
        {
            this.ControllerContext.HttpContext.Response.StatusCode = 500;
            return Json(JsonMessageHelper.getJsonMessage(false, AjaxServerErrorLog, ""), "text", JsonRequestBehavior.AllowGet);
        }

        public ActionResult AjaxError(int errorCode, string AjaxErrorLog)
        {
            this.ControllerContext.HttpContext.Response.StatusCode = errorCode;
            return Json(JsonMessageHelper.getJsonMessage(false, AjaxErrorLog, ""), "text", JsonRequestBehavior.AllowGet);
        }
    }
}
