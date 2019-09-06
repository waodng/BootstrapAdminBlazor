$(function () {
    var $dialogUser = $("#dialogUser");
    var $dialogUserHeader = $('#myUserModalLabel');
    var $dialogUserForm = $('#userForm');
    var $dialogGroup = $("#dialogGroup");
    var $dialogGroupHeader = $('#myGroupModalLabel');
    var $dialogGroupForm = $('#groupForm');
    var $dialogMenu = $('#dialogMenu');
    var $dialogMenuHeader = $('#myMenuModalLabel');
    var $dialogSubMenu = $('#dialogSubMenu').find('.modal-content');
    var $btnSubmitMenu = $('#btnSubmitMenu');
    var $nestMenu = $('#nestable_menu');
    var $nestMenuInput = $nestMenu.find('div.dd3-content');
    var $dialogApp = $("#dialogApp");
    var $dialogAppHeader = $('#myAppModalLabel');
    var $dialogAppForm = $('#appForm');

    $('table').lgbTable({
        url: Role.url,
        dataBinder: {
            map: {
                Id: "#roleID",
                RoleName: "#roleName",
                Description: "#roleDesc"
            },
            events: {
                '#btn_assignUser': function (row) {
                    $.bc({
                        id: row.Id, url: User.url, query: { type: "role" }, method: "post", htmlTemplate: CheckboxHtmlTemplate,
                        callback: function (result) {
                            var htmlTemplate = this.htmlTemplate;
                            var html = $.map(result, function (element, index) {
                                return $.format(htmlTemplate, element.Id, element.DisplayName, element.Checked, element.UserName);
                            }).join('');
                            $dialogUserHeader.text($.format('{0}-用户授权窗口', row.RoleName));
                            $dialogUserForm.html(html).find('[data-toggle="tooltip"]').each(function (index, label) {
                                if (label.title === "") label.title = "未设置";
                            }).tooltip();
                            $dialogUserForm.find('.form-checkbox').lgbCheckbox();
                            $dialogUser.modal('show');
                        }
                    });
                },
                '#btn_assignGroup': function (row) {
                    $.bc({
                        id: row.Id, url: Group.url, query: { type: "role" }, method: "post", htmlTemplate: CheckboxHtmlTemplate,
                        callback: function (result) {
                            var htmlTemplate = this.htmlTemplate;
                            var html = $.map(result, function (element, index) {
                                return $.format(htmlTemplate, element.Id, element.GroupName, element.Checked, element.Description);
                            }).join('');
                            $dialogGroupHeader.text($.format('{0}-部门授权窗口', row.RoleName));
                            $dialogGroupForm.html(html).find('[data-toggle="tooltip"]').each(function (index, label) {
                                if (label.title === "") label.title = "未设置";
                            }).tooltip();
                            $dialogGroupForm.find('.form-checkbox').lgbCheckbox();
                            $dialogGroup.modal('show');
                        }
                    });
                },
                '#btn_assignMenu': function (row) {
                    $.bc({
                        id: row.Id, url: Menu.url, query: { type: "role" }, method: "post", htmlTemplate: CheckboxHtmlTemplate,
                        callback: function (result) {
                            $dialogMenuHeader.text($.format('{0}-菜单授权窗口', row.RoleName));
                            $btnSubmitMenu.data('type', 'menu');
                            // set checkbox status
                            var menus = $nestMenu.find('input:checkbox');
                            menus.prop('checked', false);
                            $.each(result, function (index, item) {
                                var selector = $.format('[value={0}]', item);
                                menus.filter(selector).prop('checked', true);
                            });
                            $dialogSubMenu.show();
                            $dialogMenu.modal('show');
                        }
                    });
                },
                '#btn_assignApp': function (row) {
                    $.bc({
                        id: row.Id, url: App.url, method: "get", htmlTemplate: CheckboxHtmlTemplate,
                        callback: function (result) {
                            var htmlTemplate = this.htmlTemplate;
                            var html = $.map(result, function (element, index) {
                                return $.format(htmlTemplate, element.Id, element.AppName, element.Checked, "应用程序名称");
                            }).join('');
                            $dialogAppHeader.text($.format('{0}-应用授权窗口', row.RoleName));
                            $dialogAppForm.html(html).find('[data-toggle="tooltip"]').each(function (index, label) {
                                if (label.title === "") label.title = "未设置";
                            }).tooltip();
                            $dialogAppForm.find('.form-checkbox').lgbCheckbox();
                            $dialogApp.modal('show');
                        }
                    });
                },
                '#btnSubmitUser': function (row) {
                    var roleId = row.Id;
                    var userIds = $dialogUser.find(':checked').map(function (index, element) {
                        return $(element).val();
                    }).toArray();
                    $.bc({ id: roleId, url: Role.url, method: "put", data: userIds, query: { type: "user" }, modal: '#dialogUser', title: User.title });
                },
                '#btnSubmitGroup': function (row) {
                    var roleId = row.Id;
                    var groupIds = $dialogGroup.find(':checked').map(function (index, element) {
                        return $(element).val();
                    }).toArray();
                    $.bc({ id: roleId, url: Role.url, method: "put", data: groupIds, query: { type: "group" }, modal: '#dialogGroup', title: Group.title });
                },
                '#btnSubmitMenu': function (row) {
                    var roleId = row.Id;
                    var menuIds = $nestMenuInput.find('input:checkbox:checked').map(function (index, element) {
                        return $(element).val();
                    }).toArray();
                    $.bc({ id: roleId, url: Role.url, method: "put", data: menuIds, query: { type: "menu" }, modal: '#dialogMenu', title: Menu.title });
                },
                '#btnSubmitApp': function (row) {
                    var roleId = row.Id;
                    var appIds = $dialogApp.find(':checked').map(function (index, element) {
                        return $(element).val();
                    }).toArray();
                    $.bc({ id: roleId, url: Role.url, method: "put", data: appIds, query: { type: "app" }, modal: '#dialogApp', title: App.title });
                }
            }
        },
        smartTable: {
            sortName: 'RoleName',
            queryParams: function (params) { return $.extend(params, { roleName: $("#txt_search_name").val(), description: $("#txt_role_desc").val() }); },           //传递参数（*）
            columns: [
                { title: "角色名称", field: "RoleName", sortable: true },
                { title: "角色描述", field: "Description", sortable: false }
            ],
            exportOptions: {
                fileName: "角色数据",
                ignoreColumn: [0, 3]
            }
        }
    });

    $nestMenu.nestMenu(function () {
        $nestMenuInput = $nestMenu.find('div.dd3-content');
        $nestMenuInput.on('click', ':checkbox', function () {
            var val = $(this).prop('checked');
            var child = $(this).parents('.dd3-content').next();
            if (child.hasClass('dd-list')) {
                child.find(':checkbox').prop('checked', val);
            }
        }).children('.radio').hide();
    });
});