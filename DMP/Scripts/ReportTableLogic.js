
if(ReportArrayFromServer.length > 0){
    var report = {};
    for(var c =0; c< ReportArrayFromServer.length; c++){
        report = ReportArrayFromServer[c];
        var tL = report.TimelinesForReporting;
        var timeLines = [];
        for(var i=0; i< tL.length; i++)
        {
            var dt = tL[i].toString();
            if(dt.indexOf("Date") !== -1){
                dt = parseInt(dt.replace(/\/Date\((\d+)\)\//, '$1'));
                var dtt = new Date(dt);
                timeLines.push(dtt);
            }
        }
        report.TimelinesForReporting = timeLines;
        reportArray.push(report);
        CreateReportTable(report);
    }
}
         

$("#addReportbtn").click(function (e) {
    var report={};
    report["Id"] = reportArray.length + 1;
    var dateStringArray = [];

    $("#reportdatadiv input, select").each(function(){
        if($(this)[0].id == "NameOfReport"){
            report["NameOfReport"] =  $(this)[0].value;
            $(this).val("");
        }
        else if($(this)[0].id == "ThematicArea"){
            report["ThematicArea"] =  $(this)[0].value;
            $(this).val("");
        }
        else if($(this)[0].id == "TimelinesForReporting"){
            dateStringArray = $(this)[0].value.split(',');
            report["TimelinesForReporting"]  = dateStringArray;
            $(this).val("");
            $('#TimelinesForReporting').multiDatesPicker('resetDates', 'picked'); 
        }
        else if($(this)[0].id == "FrequencyOfReporting"){
            report["FrequencyOfReporting"] =  $(this)[0].value;
            $(this).val("");
        }
        else if ($(this)[0].id == "rptDurationdrpdwn") {
            var durationDrpDwn = $(this)[0].value;
            var inputValue = $("#rptDurationField")[0].value;
            var duration = inputValue;
            switch (durationDrpDwn) {
                case "Weeks":
                    duration = inputValue * 7; break;
                case "Months":
                    duration = inputValue * 30; break;
                case "Years":
                    duration = inputValue * 365; break;
            }
            report["DurationOfReporting"] = duration; //  $(this)[0].value;
            $(this).val("");
        }
        else if($(this)[0].id == "justdate"){
            report["justdate"] =  $(this)[0].value;
            $(this).val("");
        }
    });

    reportArray.push(report);
    CreateReportTable(report);
});

function CreateReportTable(report){
    var dateArray = report.TimelinesForReporting;
    var removeId = "remove"+report.Id;
    var timelines='';
    if (dateArray != null) {
        for (var i = 0; i < dateArray.length; i++) {
            var dt = dateArray[i].toString();
            if (dt.indexOf("Date") !== -1) {
                dt = parseInt(dt.replace(/\/Date\((\d+)\)\//, '$1'));
            }
            var dtt = new Date(dt);
            dt = dtt.getDate() + '-' + months[dtt.getMonth()] + '-' + dtt.getFullYear();
            timelines += dt + " &#013;";
        }
    }

    var reportId = "reportx" + report.Id;
    var html = '<tr id=' + reportId + '>';
    html += '<td>' + report.NameOfReport + '</td>';
    html += '<td>' + report.ThematicArea + '</td>';
    html += '<td>' + report.FrequencyOfReporting + '</td>';
    html += '<td>' + report.DurationOfReporting + '</td>';
    html += '<td><a class="btn btn-info btn-sm btn-outline" title=' + JSON.stringify(timelines) + '>View</a></td>';
    if (editMode) {
        html += '<td><input type="button" value="Remove" class="btn btn-danger btn-sm btn-outline" onclick="DeleteRow(' + reportId + ')"  id=' + removeId + '/></td>';
    }    
    html += '</tr>';
    $('#reportdataTable').append(html);
}
 