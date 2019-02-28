//supplementary info codes here
$(".sumbit-button").click(function (e) {
    var title = $("#title-info").val();
    var mesg = $('div.note-editable.panel-body').html();

    var model = {};
    var info = {};
    info["Title"] = title;
    info["Content"] = mesg;
    info["evaluationId"] = window.location.href.split('?id=')[1];
    model["info"] = info;

    $.ajax({
        type: "POST",
        url: "/EvaluationPlan/addsupplementaryinfo",
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify(model),
        cache: false,
    }).done(function (response) {
        $('div.note-editable.panel-body').html('');
        $("#title-info").val('');
        if (response != undefined) {
            info["PostedDate"] = response.PostedDate;
            info["id"] = response.id;
            buildInfoPanel(info);
        }
    }).error(function (xhr, status, err) {
        alert(err);
        console.log(err);
    });
    return false;
});

$.each(info_array, function (i, info) {
    buildInfoPanel(info);
});

//build supplemntary info here
function buildInfoPanel(info) {
    var html = "<div class='panel panel-default minimal'>";
    html += "<a data-toggle='collapse' data-parent='#accordion' href='#colps" + info.id + "'>";
    html += "<div class='tab-panel-heading' style='padding-top: 10px;'><span class='panel-title'>";
    html += "<small class='col-sm-3' style='color: #000;'>" + info.PostedDate + "</small> &emsp;";
    html += "<span class='col-sm-push-1'><strong>" + info.Title + "</strong></span>";
    html += "</span><i class='icon-down-open' style='float: right;'></i></div></a>";
    html += "<div id='colps" + info.id + "' class='panel-collapse collapse'>" + info.Content + "</div></div>";
    $("#otherinfopanel").append(html);
}
 
//activity

$(".status-option").click(function (e) {

    var selected_option = e.currentTarget.id.split('|');
    var status = selected_option[0];
    var activityId = selected_option[1];
    $.ajax({
        type: "POST",
        url: "/EvaluationPlan/UpdateActivityStatus?status=" + status + "&&activityId="+activityId,
        contentType: "application/json; charset=utf-8",
        cache: false,
    }).done(function (response) {
        if (response != undefined) {
            console.log(response);
            $("#status-button_" + activityId).html('(' + response + ')');
        }
    }).error(function (xhr, status, err) {
        alert(err);
        console.log(err);
    });
    return false;

});

//comments
$(".comment-button").click(function (e) {
    var activityId = $(this).closest('div').parent('div')[0].id.split('_')[1];
    var commentbox = "#comment_box_" + activityId;
    var mesg = $(commentbox).val();
    $(commentbox).val('');

    var data = JSON.stringify({ "message": mesg, "activityId": activityId });
    $.ajax({
        type: "POST",
        url: "/EvaluationPlan/Addcomment?message=" + mesg + "&&activityId=" + activityId.split('_')[1],
        contentType: "application/json; charset=utf-8",
        data: data,
        cache: false,
    }).done(function (response) {
        if (response != undefined)
            refreshComments(response);
    }).error(function (xhr, status, err) {
        alert(err);
        console.log(err);
    });
    return false;
});

function refreshComments(c) {
    var html = "<tr id='comment_" + c.id + "'><td><p>" + c.message + "</p>";
    html += "&nbsp;&nbsp;&nbsp;-<i>" + c.commenter + "</i>&nbsp;&nbsp;<span dir='ltr' class='comment-label'>" + c.dateadded + "</span>";
    html += "<a class='edit-comment' onclick='MakeEditable(comment_" + c.id + ")'>Edit comment</a></td></tr>";
    var table = "#comment_table_" + c.activityId;
    $(table).append(html);
}



//comment code here
//$.each(activities, function (index, value) {
//    var html = "<div class='panel panel-default minimal'>";
//    html += "<div class='tab-panel-heading'>";
//    html += "<a data-toggle='collapse' data-parent='#accordion' href='#collapse_" + value.id + "'>";
//    html += "<i class='icon-down-open' style='float:left;'></i>";
//    html += "<span class='panel-title'>";
//    html += "<small class='col-sm-2' style='color: #000;'>" + value.StartDate + " -- " + value.EndDate + "</small> &emsp;";
//    html += "<span class='col-sm-push-1' style='color:steelblue;'><strong>" + value.Name + "</strong></span>&nbsp;&nbsp;<span style='color: deeppink;'>(" + value.Status + ")</span>";
//    html += "</span></a>";
//    html += "<input type='button' class='btn btn-default btn-sm status-button' value='change activity status'/></div>";
//    html += "<div id='collapse_" + value.id + "' class='panel-collapse collapse'>";
//    html += "<div class='panel-body'>";
//    html += "<p class='col-sm-offset-2'>" + value.ExpectedOutcome + "</p>";
//    html += "<span class='col-sm-offset-2 comment-label'>Comments</span>";
//    html += "<table id='comment_table_" + value.id + "'><tbody>";
//    $.each(value.comments, function (i, c) {
//        html += "<tr id='comment_" + c.id + "'><td><p>" + c.message + "</p>";
//        html += "&nbsp;&nbsp;&nbsp;-<i>" + c.commenter + "</i>&nbsp;&nbsp;<span dir='ltr' class='comment-label'>" + c.dateadded + "</span>";
//        html += "<a class='edit-comment' onclick='MakeEditable(comment_" + c.id + ")'>Edit</a></td></tr>";
//    });
//    html += "</tbody></table>";
//    html += "<label class='col-sm-2 comment-label'>add a comment </label>";
//    html += "<span> <textarea class='comment-link' id='comment_box_" + value.id + "' cols='92' tabindex='101' data-min-length='' ></textarea></span>";
//    html += "<span><a class='btn btn-default comment-button'>Add</a></span>";
//    html += "</div></div></div>";
//    $("#activitypanel").append(html);
//});