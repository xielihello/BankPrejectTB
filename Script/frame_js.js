function myAlert(msg) {
    $.messager.alert("提示", msg, "warning");
}
var tabs;
function addTab(title, url) {
    if ($('#tabs').tabs('exists', title)) {
        $('#tabs').tabs('select', title); //选中并刷新
        var currTab = $('#tabs').tabs('getSelected');
        var url = $(currTab.panel('options').content).attr('src');
        if (url != undefined && currTab.panel('options').title != 'Home') {
            $('#tabs').tabs('update', {
                tab: currTab,
                options: {
                    content: createFrame(url)
                }
            })
        }
    } else {
        var content = createFrame(url);
        $('#tabs').tabs('add', {
            title: title,
            content: content,
            closable: true
        });
    }
    tabClose();
}
function createFrame(url) {
    var s = '<iframe scrolling="auto" frameborder="0"  src="' + url + '" style="width:100%;height:100%;"></iframe>';
    return s;
}
function tabClose() {
    /*双击关闭TAB选项卡*/
    $(".tabs-inner").dblclick(function () {
        var subtitle = $(this).children(".tabs-closable").text();
        $('#tabs').tabs('close', subtitle);
    })
    /*为选项卡绑定右键*/
    $(".tabs-inner").bind('contextmenu', function (e) {
        $('#mm').menu('show', {
            left: e.pageX,
            top: e.pageY
        });
        var subtitle = $(this).children(".tabs-closable").text();
        $('#mm').data("currtab", subtitle);
        $('#tabs').tabs('select', subtitle);
        return false;
    });
}
//绑定右键菜单事件
function tabCloseEven() {
    //刷新
    $('#mm-tabupdate').click(function () {
        var currTab = $('#tabs').tabs('getSelected');
        var url = $(currTab.panel('options').content).attr('src');
        if (url != undefined && currTab.panel('options').title != 'Home') {
            $('#tabs').tabs('update', {
                tab: currTab,
                options: {
                    content: createFrame(url)
                }
            })
        }
    })
    //关闭当前
    $('#mm-tabclose').click(function () {
        var currtab_title = $('#mm').data("currtab");
        $('#tabs').tabs('close', currtab_title);
    })
    //全部关闭
    $('#mm-tabcloseall').click(function () {
        $('.tabs-inner span').each(function (i, n) {
            var t = $(n).text();
            if (t != 'Home') {
                $('#tabs').tabs('close', t);
            }
        });
    });
    //关闭除当前之外的TAB
    $('#mm-tabcloseother').click(function () {
        var prevall = $('.tabs-selected').prevAll();
        var nextall = $('.tabs-selected').nextAll();
        if (prevall.length > 0) {
            prevall.each(function (i, n) {
                var t = $('a:eq(0) span', $(n)).text();
                if (t != 'Home') {
                    $('#tabs').tabs('close', t);
                }
            });
        }
        if (nextall.length > 0) {
            nextall.each(function (i, n) {
                var t = $('a:eq(0) span', $(n)).text();
                if (t != 'Home') {
                    $('#tabs').tabs('close', t);
                }
            });
        }
        return false;
    });
    //关闭当前右侧的TAB
    $('#mm-tabcloseright').click(function () {
        var nextall = $('.tabs-selected').nextAll();
        if (nextall.length == 0) {
            //msgShow('系统提示','后边没有啦~~','error');
            alert('后边没有啦~~');
            return false;
        }
        nextall.each(function (i, n) {
            var t = $('a:eq(0) span', $(n)).text();
            $('#tabs').tabs('close', t);
        });
        return false;
    });
    //关闭当前左侧的TAB
    $('#mm-tabcloseleft').click(function () {
        var prevall = $('.tabs-selected').prevAll();
        if (prevall.length == 0) {
            alert('到头了，前边没有啦~~');
            return false;
        }
        prevall.each(function (i, n) {
            var t = $('a:eq(0) span', $(n)).text();
            $('#tabs').tabs('close', t);
        });
        return false;
    });
    //退出
    $("#mm-exit").click(function () {
        $('#mm').menu('hide');
    })
}
$(function () {
    tabs = $("#tabs");
    $('#tabs').tabs({
        onBeforeClose: function (title, index) {
            if (title.trim() == "采购入库单" || title.trim() == "采购退货单" || title.trim() == "销售出库单" || title.trim() == "销售退货单" || title.trim() == "报损单" || title.trim() == "报溢单" || title.trim() == "库存盘点单") {
                var target = this;
                $.messager.confirm('确认提示', '数据可能已经发生改变，点OK将放弃当前改变，否则取消！', function (r) {
                    if (r) {
                        var opts = $(target).tabs('options');
                        var bc = opts.onBeforeClose;
                        opts.onBeforeClose = function () { };  // allowed to close now
                        $(target).tabs('close', index);
                        opts.onBeforeClose = bc;  // restore the event function
                    }
                });
                return false;	// prevent from closing
            }
        }

    });
    tabCloseEven();
    $('.cs-navi-tab').click(function () {
        var $this = $(this);
        var href = $this.attr('src');
        var title = $this.text();
        addTab(title, href);
    });
    $(".pli").mousemove(function () {
        $(this).addClass("pli2");
    });
    $(".pli").mouseout(function () {
        $(this).removeClass("pli2");
    });
    $("#login-wrap").window({
        width: 300,
        height: 150,
        modal: true,
        collapsible: false,
        minimizable: false,
        maximizable: false,
        closable: false,
        closed: true
    });
    $("#chg-pwd").window({
        width: 280,
        height: 180,
        modal: true,
        collapsible: false,
        minimizable: false,
        maximizable: false,
        closed: true
    });
    $(".upPwd").click(function () {
        $("#chg-pwd").window("open");
    });
    $("#btn_chg_pwd").click(function () {
        var postdata = $("#pass_form").serializeArray();
        $.form.Init({
            url: "../operators/ModifyPasswordSelf/",
            postdata: postdata,
            success: function (data) {
                if (data.succ) {
                    $.messager.alert("提示", "修改成功！需要重新登录");
                    setTimeout('top.location.href = "/main/LoginOut"', 1000);
                    $("#chg-pwd").window("close");
                } else {
                    $.messager.alert("提示", data.msg);
                }
            },
            alert: function (obj,msg) {
                $.messager.alert("提示", msg);
            },
            beforeSend: function () {
                $.messager.progress({ title: "提示", msg: "请稍候..." });
            },
            complete: function () {
                $.messager.progress('close');
            }
        });
        var result = $.form.Validate();
        if (result) {
            $.form.Submit();
        }
    });
    $(".add-quick-bar").click(function () {
        $("#quick_bar_dialog").dialog("open");
    });

    $(".quick-bar-source").click(function () {
        var _this = $(this);
        var bar_path = _this.attr("data-path");
        var bar_icon = _this.attr("data-icon");
        var bar_title = _this.find(".title").text().trim();
        var bar_id = _this.attr("data-id");
        var bar_added = _this.attr("data-added");
        if (bar_added == "True") {
            return;
        }
        $.ajax({
            url: "/main/SetQuickBar/" + bar_id + "/",
            dataType: "json",
            success: function (data) {
                if (data.succ) {
                    var quick_bar_code = "<li onclick=\"addTab('" + bar_title + "','" + bar_path + "')\" class=\"quick-bar-item float-left quick-bar-quote\" data-id=\"" + bar_id + "\" data-path=\"" + bar_path + "\" data-icon=\"" + bar_icon + "\">";
                    quick_bar_code += "<div class=\"icon quick-bar-icon-" + bar_icon + "\"></div>";
                    quick_bar_code += "<div class=\"title\">" + bar_title + "</div>";
                    $(".add-quick-bar").before(quick_bar_code);
                    _this.append("<div class=\"quick-bar-cover opactity-40\"></div><div class=\"quick-bar-cover\"><p data-id=\"" + bar_id + "\" class=\"bars-del\"><img src=\"/Images/sub.png\" /></p></div>");
                    _this.attr("data-added", "True");
                    //重新绑定删除事件
                    _this.find(".bars-del").click(function () {
                        var id = $(this).attr("data-id"); 
                        bars_del(id);
                        return false;
                    });

                } else {
                    $.messager.alert("提示", data.msg);
                }
            },
            error: function () {
                //$.messager.alert("提示", "出错了！");
            },
            beforeSend: function () {
                $.messager.progress({ title: "提示", msg: "请稍候..." });
            },
            complete: function () {
                $.messager.progress('close');
            }
        });

    });

    $(".bars-del").click(function (event) { 
        var id = $(this).attr("data-id"); 
        bars_del(id);
        return false;
    });
});

var bars_del = function (id) {
    $.ajax({
        url: "/main/DeleteQuickBar/" + id + "/",
        dataType: "json",
        success: function (data) {
            if (data.succ) {
                var bar_source = $(".quick-bar-source[data-id=" + id + "]");
                bar_source.attr("data-added", "False");
                bar_source.find(".quick-bar-cover").remove();
                $(".quick-bar-quote[data-id=" + id + "]").remove();
            }
        },
        error: function () {
            //$.messager.alert("提示", "出错了！");
        },
        beforeSend: function () {
            $.messager.progress({ title: "提示", msg: "请稍候..." });
        },
        complete: function () {
            $.messager.progress('close');
        }
    });


}