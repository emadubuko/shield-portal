
if (DataVerificationArrayFromServer !=null && DataVerificationArrayFromServer.length > 0) {
    var dataverification = {};
    for (var c = 0; c < DataVerificationArrayFromServer.length; c++) {
        dataverification = DataVerificationArrayFromServer[c];
        var tL = dataverification.TimelinesForDataVerification;
        var timeLines = [];
        if (tL != null) {
            for (var i = 0; i < tL.length; i++) {
                var dt = tL[i].toString();
                if (dt.indexOf("Date") !== -1) {
                    dt = parseInt(dt.replace(/\/Date\((\d+)\)\//, '$1'));
                    var dtt = new Date(dt);
                    timeLines.push(dtt);
                }
            }
            dataverification.TimelinesForDataVerification = timeLines;
            dataverification.Id = c + 1;
        }
        
        DataVerificationArray.push(dataverification);
        CreatedataverificationTable(dataverification);
    }
}


$("#addDataVerificationbtn").click(function (e) {
    var dataverification = {};
    if (DataVerificationArray == undefined || DataVerificationArray.length == 0) {
        dataverification["Id"] = 1;
    } else {
        dataverification["Id"] = DataVerificationArray[DataVerificationArray.length - 1].Id + 1;  //DataVerificationArray.length + 1;
    }
    
    var dateStringArray = [];

    $("#dataVerificationForm input, select").each(function () {
        if ($(this)[0].id == "DataVerificationApproach") {
            dataverification["DataVerificationApproach"] = $(this)[0].value;
            $(this).val("");
        }
        else if ($(this)[0].id == "dvReportingLevel") {
            dataverification["ReportingLevel"] = $(this)[0].value;
            $(this).val("");
        }
        else if ($(this)[0].id == "dvThematicArea") {
            dataverification["ThematicArea"] = $(this)[0].value;
            $(this).val("");
        }

        else if ($(this)[0].id == "TypesOfDataVerification") {
            dataverification["TypesOfDataVerification"] = $(this)[0].value;
            $(this).val("");
        }
        else if ($(this)[0].id == "TimelinesForDataVerification") {
            if ($(this)[0].value == "") {
                alert("please select timelines")
                return;
            }
            dateStringArray = $(this)[0].value.split(',');
            dataverification["TimelinesForDataVerification"] = dateStringArray;
            $(this).val("");
            $('#TimelinesForDataVerification').multiDatesPicker('resetDates', 'picked');
        }
        else if ($(this)[0].id == "FrequencyOfDataVerification") {
            dataverification["FrequencyOfDataVerification"] = $(this)[0].value;
            $(this).val("");
        }
        else if ($(this)[0].id == "dvDurationdrpdwn") {
            var durationDrpDwn = $(this)[0].value;
            var inputValue = $("#dvDurationField")[0].value;
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
            dataverification["DurationOfDataVerificaion"] = duration; // $(this)[0].value;
            $(this).val("");
        }        
    });

    if (dataverification.TimelinesForDataVerification != null && dataverification.DurationOfDataVerificaion != null && dataverification.ReportingLevel !=null) {
        DataVerificationArray.push(dataverification);
        CreatedataverificationTable(dataverification);
    }
    
});

function CreatedataverificationTable(dataverification) {
    var dateArray = dataverification.TimelinesForDataVerification;
    var removeId = "remove" + dataverification.Id;
    var timelines = '';
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

   
    var dataverificationId = "dataverificationx" + dataverification.Id;
    var html = '<tr id=' + dataverificationId + '>';
    html += '<td>' + dataverification.ReportingLevel + '</td>';
    html += '<td>' + dataverification.ThematicArea + '</td>';
    html += '<td>' + dataverification.DataVerificationApproach + '</td>';
    html += '<td>' + dataverification.TypesOfDataVerification + '</td>';
    html += '<td>' + dataverification.FrequencyOfDataVerification + '</td>';
    html += '<td>' + dataverification.DurationOfDataVerificaion + '</td>';
    html += '<td><a class="btn btn-info btn-sm btn-outline" title=' + JSON.stringify(timelines) + '>View</a></td>';
    if (editMode) {
        html += '<td><input type="button" value="Remove" class="btn btn-danger btn-sm btn-outline" onclick="DeleteRow(' + dataverificationId + ')"  id=' + removeId + '/></td>';
    }
    html += '</tr>';
    $('#dataVerificationTable').append(html);
}