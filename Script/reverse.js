/*
  反选
  1、添加class="reverse"
  2、"全部"选项上添加data-sign="all"
  3、除"全部"选项以外所有选项添加data-sign="part"
*/
$(function () {
    $(".reverse").click(function () {
        var is_check = $(this).is(':checked');
        var sign = $(this).attr("data-sign");
        if (sign == "all") {
            $(this).parent().find(".reverse[data-sign=part]").removeAttr("checked");
        } else {
            $(this).parent().find(".reverse[data-sign=all]").removeAttr("checked");
        }
    });
});