
if (ReportArrayFromServer!=null && ReportArrayFromServer.length > 0) {
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
        report.Id = c + 1;
        reportArray.push(report);
        CreateReportTable(report);
    }
}
         

$("#addReportbtn").click(function (e) {
    var report = {};
    if (reportArray == undefined || reportArray.length == 0) {
        report["Id"] = 1
    }
    else {
        report["Id"] = reportArray[reportArray.length - 1].Id + 1; // reportArray.length + 1;
    }
    var dateStringArray = [];

    $("#reportdatadiv input, select").each(function(){
          if ($(this)[0].id == "ReportsType") {
            report["ReportsType"] = $(this)[0].value;
            $(this).val("");
          }

          else if ($(this)[0].id == "rptReportingLevel") {
              let reportinglevel = '';
              $("#rptReportingLevel option:selected").each(function () {
                  reportinglevel += $(this).val() + ',';
              });
              report["ReportingLevel"] = reportinglevel;
              $("#rptReportingLevel").select2('val', 'All');
          }

          else if ($(this)[0].id == "ReportedTo") {
              report["ReportedTo"] = $(this)[0].value;
              $(this).val("");
          }
          else if ($(this)[0].id == "ProgramArea") {
              report["ProgramArea"] = $(this)[0].value;
              $(this).val("");
          }
          else if ($(this)[0].id == "TimelinesForReporting") {
              if ($(this)[0].value == "") {
                  alert("please select timelines")
                  return;
              }
              dateStringArray = $(this)[0].value.split(',');
              report["TimelinesForReporting"] = dateStringArray;
              $(this).val("");
              $('#TimelinesForReporting').multiDatesPicker('resetDates', 'picked');
          }
          else if ($(this)[0].id == "FrequencyOfReporting") {
              report["FrequencyOfReporting"] = $(this)[0].value;
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
              if (duration == "") {
                  alert("please specify duration");
                  return;
              }
              report["DurationOfReporting"] = duration;
              $(this).val("");
          }
          else if ($(this)[0].id == "justdate") {
              report["justdate"] = $(this)[0].value;
              $(this).val("");
          }
    });
    if (report.TimelinesForReporting != null && report.DurationOfReporting != null && report.ReportingLevel !=null) {
        reportArray.push(report);
        CreateReportTable(report);
    }
    
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
    html += '<td>' + report.ReportingLevel + '</td>';
    html += '<td>' + report.ReportsType + '</td>';
    html += '<td>' + report.ReportedTo + '</td>';
    html += '<td>' + report.ProgramArea + '</td>';
    html += '<td>' + report.FrequencyOfReporting + '</td>';
    html += '<td>' + report.DurationOfReporting + '</td>';
    html += '<td><a class="btn btn-info btn-sm btn-outline" title=' + JSON.stringify(timelines) + '>View</a></td>';
    if (editMode) {
        html += '<td><input type="button" value="Remove" class="btn btn-danger btn-sm btn-outline" onclick="DeleteRow(' + reportId + ')"  id=' + removeId + '/></td>';
    }    
    html += '</tr>';
    $('#reportdataTable').append(html);
}
 