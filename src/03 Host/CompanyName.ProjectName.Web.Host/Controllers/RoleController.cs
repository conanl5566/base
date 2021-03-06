﻿using CompanyName.ProjectName.ICommonServer;
using CompanyName.ProjectName.ICommonServer.Sys;
using CompanyName.ProjectName.Core;
using CompanyName.ProjectName.Web.Host.Framework;
using CompanyName.ProjectName.Web.Host.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CompanyName.ProjectName.Web.Host.Controllers
{
    public class RoleController : CustomController
    {
        public IRoleApp RoleApp { get; set; }
        public IRoleAuthorizeApp RoleAuthorizeApp { get; set; }
        public IModuleApp ModuleApp { get; set; }
        public IModuleButtonApp ModuleButtonApp { get; set; }
        public SiteConfig Config;

        public RoleController(IOptions<SiteConfig> option)
        {
            Config = option.Value;
            DefaultPageSize = ZConvert.StrToInt(Config.Configlist.FirstOrDefault(o => o.Key == "pagesize")?.Values);
        }

        //
        // GET: /<controller>/
        public async Task<IActionResult> Index(RoleOption filter, int? page)
        {
            ViewBag.filter = filter;
            var currentPageNum = page ?? 1;
            var result = await RoleApp.GetPageAsync(currentPageNum, DefaultPageSize, filter);
            var model = new BaseListViewModel<Role>
            {
                list = result.Data,
                Paging =
                {
                    CurrentPage = currentPageNum,
                    ItemsPerPage = DefaultPageSize,
                    TotalItems = result.ItemCount
                }
            };
            return View(model);
        }

        #region 添加

        /// <summary>
        /// 添加
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Create()
        {
            var m = new Rolemodel { GoBackUrl = SetingBackUrl(this.HttpContext.Request) };
            ViewBag.GoBackUrl = SetingBackUrl(this.HttpContext.Request);
            //菜单数据
            ViewData["tree"] = TreeModel.ToJson(await GetPermissionTree(null)); //json long 转成 string, 名称用驼峰结构输出
            return View(m);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Rolemodel model, List<long> permissionIds)
        {
            if (!ModelState.IsValid)
            {
                return Operation(false, "数据验证失败" + GetErrorFromModelStateStr());
            }
            var m = model.MapTo<Role>();
            var r = await RoleApp.CreateAsync(m, permissionIds, await CurrentUser());
            return Json(r);
        }

        #endregion 添加

        #region 删除

        [HttpPost]
        public async Task<IActionResult> Delete(long Id)
        {
            var r = await RoleApp.DeleteAsync(Id, await CurrentUser());
            return Json(r);
        }

        #endregion 删除

        #region 修改

        public async Task<IActionResult> Edit(long Id)
        {
            var role = await RoleApp.GetAsync(Id);
            if (role == null)
            {
                return NotFind();
            }
            var model = new Rolemodel();
            model = role.MapTo<Rolemodel>();
            model.GoBackUrl = SetingBackUrl(this.HttpContext.Request);
            ViewBag.GoBackUrl = SetingBackUrl(this.HttpContext.Request);
            ViewData["tree"] = TreeModel.ToJson(await GetPermissionTree(Id));
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Rolemodel model, List<long> permissionIds)
        {
            if (!ModelState.IsValid)
            {
                return Operation(false, GetErrorFromModelStateStr());
            }

            var m = await RoleApp.GetAsync(model.Id);
            if (m == null)
            {
                return Operation(false, "数据不存在或已被删除");
            }

            m = model.MapToMeg<Rolemodel, Role>(m);
            var r = await RoleApp.UpdateAsync(m, permissionIds, await CurrentUser());

            return Json(r);
        }

        #endregion 修改

        /// <summary>
        ///
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        private async Task<List<TreeModel>> GetPermissionTree(long? roleId)
        {
            var moduledata = (await ModuleApp.GetSaasModuleListAsync()).OrderBy(o => o.SortCode).ToList();
            var buttondata = (await ModuleButtonApp.GetSaasModuleListAsync()).OrderBy(o => o.SortCode).ToList();
            var authorizedata = new List<RoleAuthorize>();
            if (roleId.HasValue)
            {
                authorizedata = (await RoleAuthorizeApp.GetListAsync(roleId.Value, 1)).ToList();
            }
            var treeList = new List<TreeModel>();
            foreach (var item in moduledata)
            {
                var tree = new TreeModel
                {
                    Id = item.Id,
                    Text = item.FullName,
                    Value = item.Id.ToString(),
                    Parentnodes = item.ParentId,
                    Isexpand = true,
                    Complete = false,
                    Showcheck = true,
                    Checkstate = authorizedata.Count(t => t.ItemId == item.Id),
                    HasChildren = false,
                    Img = item.Icon == "" ? "" : item.Icon
                };
                treeList.Add(tree);
            }

            foreach (var item in buttondata)
            {
                var tree = new TreeModel
                {
                    Id = item.Id,
                    Text = item.FullName,
                    Value = item.Id.ToString(),
                    Parentnodes = item.ParentId == 0 ? item.ModuleId : item.ParentId,
                    Isexpand = true,
                    Complete = false,
                    Showcheck = true,
                    Checkstate = authorizedata.Count(t => t.ItemId == item.Id),
                    HasChildren = false,
                    Img = item.Icon == "" ? "" : item.Icon
                };
                treeList.Add(tree);
            }
            return treeList;
        }
    }
}