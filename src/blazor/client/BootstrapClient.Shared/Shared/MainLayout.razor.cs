﻿using Bootstrap.Security.Blazor;
using BootstrapAdmin.Extensions;
using BootstrapAdmin.Web.Core;
using BootstrapBlazor.Components;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;

namespace BootstrapClient.Web.Shared.Shared
{
    /// <summary>
    /// 
    /// </summary>
    public sealed partial class MainLayout
    {
        private bool UseTabSet { get; set; } = true;

        private string Theme { get; set; } = "";

        private bool IsOpen { get; set; }

        private bool IsFixedHeader { get; set; } = true;

        private bool IsFixedFooter { get; set; } = true;

        private bool IsFullSide { get; set; } = true;

        private bool ShowFooter { get; set; } = true;

        private IEnumerable<MenuItem>? MenuItems { get; set; }

        [NotNull]
        private Dictionary<string, string>? TabItemTextDictionary { get; set; }

        /// <summary>
        /// 获得 当前用户登录显示名称
        /// </summary>
        [NotNull]
        public string? DisplayName { get; private set; }

        private string? Title { get; set; }

        private string? Footer { get; set; }

        /// <summary>
        /// 获得 当前用户登录名
        /// </summary>
        [NotNull]
        public string? UserName { get; private set; }

        [Inject]
        [NotNull]
        private IBootstrapAdminService? SecurityService { get; set; }

        [Inject]
        [NotNull]
        private AuthenticationStateProvider? AuthenticationStateProvider { get; set; }

        [Inject]
        [NotNull]
        private IDict? DictsService { get; set; }

        [Inject]
        [NotNull]
        private IUser? UsersService { get; set; }

        [Inject]
        [NotNull]
        private INavigation? NavigationsService { get; set; }

        [Inject]
        [NotNull]
        private IOptions<BootstrapAdminAuthenticationOptions>? AuthorizationOption { get; set; }

        [Inject]
        [NotNull]
        private NavigationManager? NavigationManager { get; set; }

        /// <summary>
        /// OnInitialized 方法
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var userName = state.User.Identity?.Name;

            if (!string.IsNullOrEmpty(userName))
            {
                UserName = userName;
                DisplayName = UsersService.GetDisplayName(userName);

                MenuItems = NavigationsService.GetAllMenus(userName).ToClientMenus();

                Title = DictsService.GetWebTitle();
                Footer = DictsService.GetWebFooter();
            }
        }

        private Task<bool> OnAuthorizing(string url) => SecurityService.AuhorizingNavigation(UserName, url);

        private string GetAuthorUrl() => $"{AuthorizationOption.Value.AuthHost}/Account/Login?ReturnUrl={NavigationManager.Uri}";
    }
}
