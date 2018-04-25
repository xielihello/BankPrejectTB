    /***********************************************************************************************************
    @author chenzhen
    @date 2015-11-16
    @verson 1.0.2
    说明：
    1、必须先调用Init()，再调用Validate()（根据需求调用），最后调用Submit()。
    2、class=cz-validate 为表单验证项的标记，需要验证的表单项必须加上
    3、表单验证类型(属性：data-cz-type)，非空：not-null、数字：number、邮箱：email 、单选：radio、复选：checkbox、自定义正则表达式：regex，属性data-cz-regex包含自定义正则表达式
    4、属性：data-cz-tip 为错误提示信息。 例如：data-cz-tip="请输入内容" 
    5、单选复选框使用<section>标签包括起来，其上三条所述属性需加在<section>标签上
    6、新增了fill()函数用来自动填充表单，参数为json对象。可以和以上函数结合使用，但并不依赖以上函数
    7、新增了支持定义正则表达式验证表单
    8、修复了必须验证表单项的bug
 ***********************************************************************************************************/
(function ($) {
    var defaults; //全局变量 配置参数
    $.form = {
        //初始化表单参数
        Init: function (options) {
            defaults = {
                url: "",
                type: "post",
                dataType: "json",
                postdata: {},
                success: function (data) {
                    if (data.succ) {
                        alert("表单提交成功");
                    }
                },
                error: function (error) {
                    alert("服务器出错,错误未知!详细信息参考error参数");
                },
                beforeSend: function () { },
                complete: function () { },
                alert: function (obj,msg) {
                    alert(msg);
                }
            };
            $.extend(defaults, options);
        },
        //表单验证
        Validate: function () {
            var validates = $(".cz-validate");
            var result = true;
            for (var i = 0; i < validates.length; i++) {
                var tag = $(validates[i]).get(0).tagName;
                switch (tag) {
                    case "INPUT":
                    case "SECTION":
                    case "TEXTAREA":
                        result = ValidateInput(validates[i]);
                        if (!result) {
                            return result;
                        }
                        break;
                    case "SELECT":
                        result = ValidateCombo(validates[i]);
                        if (!result) {
                            return result;
                        }
                        break;
                    default:
                        alert("插件暂时不支持[" + tag + "]标签的验证");
                        break;
                }
            }
            return result;

            //文本框、文本域、单选、复选
            function ValidateInput(obj) {
                var v_type = $(obj).attr("data-cz-type");
                if (!v_type) {
                    //不添加验证类型标记时 不进行验证
                    return true;
                }
                var result = true;
                var v_val = $(obj).val();
                var v_tip = $(obj).attr("data-cz-tip");
                switch (v_type) {
                    case "not-null":
                        //非空  
                        if (v_val == "" || v_val == null) {
                            result = false;
                            defaults.alert(obj, (!v_tip ? "" : v_tip));
                        }
                        break;
                    case "number":
                        //数字 
                        if (isNaN(v_val) || v_val == "" || v_val == null) {
                            result = false;
                            defaults.alert(obj, (!v_tip ? "" : v_tip));
                        }
                        break;
                    case "email":
                        //邮箱
                        var regex = /^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/;
                        if (regex.test(v_val) == false) {
                            result = false;
                            defaults.alert(obj, (!v_tip ? "" : v_tip));
                        }
                        break;
                    case "radio":
                        //单选
                        var radio = $(obj).find("input:checked");
                        if (!radio || radio.length == 0) {
                            result = false;
                            defaults.alert(obj, (!v_tip ? "" : v_tip));
                        }
                        break;
                    case "checkbox":
                        //复选
                        var checkbox = $(obj).find("input:checked");
                        if (!checkbox || checkbox.length == 0) {
                            result = false;
                            defaults.alert(obj, (!v_tip ? "" : v_tip));
                        }
                        break;
                    case "regex":
                        //正则表达式 
                        var regex = eval($(obj).attr("data-cz-regex")); 
                        if (regex.test(v_val) == false) {
                            result = false;
                            defaults.alert(obj, (!v_tip ? "" : v_tip));
                        }
                        break;
                    default:
                        alert("插件暂时不支持[" + v_type + "]格式的验证");
                        break;
                }
                if (!result) {
                    $(obj).focus();
                }
                return result;
            }

            //下拉框验证
            function ValidateCombo(obj) {
                var result = true;
                var v_tip = $(obj).attr("data-cz-tip");
               
                var v_name = $(obj).attr("name");
                var v_first_val = $($(obj).get(0)[0]).attr("value");
                var v_val = $(obj).val();
                if (v_val == v_first_val) {
                    result = false;
                    defaults.alert(obj, (!v_tip ? (v_name + "：data-cz-tip属性未配置") : v_tip));
                }
                return result;
            }
        },
        //提交表单
        Submit: function () {
            $.ajax({
                type: defaults.type,
                url: defaults.url,
                data: defaults.postdata,
                dataType: defaults.dataType,
                success: defaults.success,
                error: defaults.error,
                beforeSend: defaults.beforeSend,
                complete: defaults.complete
            });
        },
        Clear: function () {
            var controls = $("[data-cz-clear=true]");
            for (var i = 0; i < controls.length; i++) {
                var tag = $(controls[i]).get(0).tagName;
                switch (tag) {
                    case "INPUT": 
                    case "TEXTAREA":
                        $(controls[i]).val("");
                        break;
                    case "SELECT":
                        $(controls[i]).find("option:first").prop("selected", 'selected');
                        break;
                    case "SECTION":
                        //单选、复选
                        $(controls[i]).find("input[type=radio],input[type=checkbox]").attr("checked", false);
                        break;
                    default:
                        alert("插件暂时不支持[" + tag + "]标签的验证");
                        break;
                }
            } 
        }
    }

    //填充表单
    //data 要填充的数据
    //callback回调函数
    $.fn.fill = function (data, callback) {
        var tagName = $(this).get(0).tagName;
        if (tagName != "FORM") {
            alert("只有FORM表单才允许调用此函数");
            return;
        }
        if (!data) {
            return;
        }
        for (var key in data) { 
            var control = $(document.getElementsByName(key));
            var dom_control = control.get(0);
            if (!dom_control) { 
                continue;
            }
            var C_tagName = dom_control.tagName; 
            if (C_tagName == "INPUT") {
                var C_Type = control.attr("type");
                switch (C_Type) {
                    case "text":
                        control.val(data[key]);
                        break;
                    case "checkbox":
                        if (data[key]) {
                            for (var j = 0; j < data[key].length; j++) {
                                control.each(function (i) {
                                    var val = $(control[i]).val();
                                    if (val == data[key][j]) {
                                        $(control[i]).attr("checked", "checked");
                                    }
                                });
                            }
                        }
                        break;
                    case "radio":
                        control.each(function (i) {
                            var val = $(control[i]).val();
                            if (val == data[key]) {
                                $(control[i]).attr("checked", "checked");
                            }
                        });
                        break;
                }
            } else if (C_tagName == "TEXTAREA") {
                control.val(data[key]);
            } else if (C_tagName == "SELECT") { 
                control.val(data[key]);
            }
        }
        if (callback) {
            callback();
        }
    }
     
})(jQuery);