
if (reportingLevelArrayFromServer !=null && reportingLevelArrayFromServer.length > 0) {
    var reportingLevel = {};
    for (var c = 0; c < reportingLevelArrayFromServer.length; c++) {
        reportingLevel = reportingLevelArrayFromServer[c];
        PopulateReportingLevels(reportingLevel);
        reportingLevelArray.push(reportingLevel);
        CreatereportingLevelTable(reportingLevel);
    }
}

$("#reportingLevelbtn").click(function (e) {
    let level = $("#reportingLeveltxt")[0].value

    if (level != '') {
        $("#reportingLeveltxt").val("");

        PopulateReportingLevels(level);
        reportingLevelArray.push(level);        
        CreatereportingLevelTable(level);
    }
});

function PopulateReportingLevels(level) {
    let controls = $(".form-control.reportingleveldrpdn");

    $.each(controls, function (index, control) {
        var opt = document.createElement("option");
        opt.textContent = level;
        opt.value = level;
        control.appendChild(opt);
    });
}


function CreatereportingLevelTable(reportingLevel) {
   
    var reportingLevelId = "reportingLevelx" + reportingLevel;
    var html = '<tr id=' + reportingLevelId + '>'; 
    html += '<td>' + reportingLevel + '</td>';       
    if (editMode) {
        html += '<td><input type="button" value="Remove" class="btn btn-danger btn-sm btn-outline" onclick="DeleteLevel(' + reportingLevelId + ')"/></td>';
    }
    html += '</tr>';
    $('#reportingLevelTable').append(html);
}

function DeleteLevel(row) {

    if (row.id.indexOf("reportingLevel") !== -1) {
        var rowid = row.id.split('x')[1];
        $(row).remove();
        reportingLevelArray.splice(rowid - 1, 1);

        $(".form-control.reportingleveldrpdn option[value=" + rowid + "]").remove();                 
    }
}