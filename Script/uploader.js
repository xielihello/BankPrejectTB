(function ($) {
    var defaults; //全局变量 配置参数
    $.uploader = {
        //初始化表单参数
        Upload: function (options) {
            defaults = {
                avatar_sizes:"",//裁剪尺寸  100*100|50*50|32*32
                avatar_sizes_desc:"",//尺寸说明 150*150像素|50*50像素|32*32像素 
                src_upload: 2,//是否上传原图片的选项，有以下值：0-不上传；1-上传；2-显示复选框由用户选择
                sign:"",//上传路径Key
                control: "",//上传成功后保存上传路径的页面元素
                dialog: "",//调起的窗体
                showbox: "",//显示上传后图片的页面元素
            };
            $.extend(defaults, options);
            $("#" + defaults.dialog).dialog({
                title: "上传图片",
                width: 850,
                height: 650,
                closed: false,
                cashe: false,
                href: '/Upload/FAE_Upload/?avatar_sizes_desc='+defaults.avatar_sizes_desc+'&avatar_sizes='+defaults.avatar_sizes+'&src_upload=' + defaults.src_upload + '&sign=' + defaults.sign + '&control=' + defaults.control + "&dialog=" + defaults.dialog + "&showbox=" + defaults.showbox ,
                modal: true,
                buttons: [
                    {
                        text: "关闭",
                        handler: function () {
                            $("#" + defaults.dialog).dialog('close');
                        }
                    }]
            });
        }
    }

})(jQuery);
 

    
 