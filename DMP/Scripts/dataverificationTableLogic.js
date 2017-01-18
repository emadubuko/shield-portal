
if (DataVerificationArrayFromServer.length > 0) {
    var dataverification = {};
    for (var c = 0; c < DataVerificationArrayFromServer.length; c++) {
        dataverification = DataVerificationArrayFromServer[c];
        var tL = dataverification.TimelinesForDataVerification;
        var timeLines = [];
        for (var i = 0; i < tL.length; i++) {
            var dt = tL[i].toString();
            if (dt.indexOf("Date") !== -1) {
                dt = parseInt(dt.replace(/\/Date\((\d+)\)\//, '$1'));
                var dtt = new Date(dt);
                timeLines.push(dtt);
            }
        }
        dataverification.TimelinesForDataVerification = timeLines;
        DataVerificationArray.push(dataverification);
        CreateReportTable(dataverification);
    }
}


$("#addDataVerificationbtn").click(function (e) {
    var dataverification = {};
    dataverification["Id"] = DataVerificationArray.length + 1;
    var dateStringArray = [];

    $("#dataVerificationForm input, select").each(function () {
        if ($(this)[0].id == "DataVerificationApproach") {
            dataverification["DataVerificationApproach"] = $(this)[0].value;
            $(this).val("");
        }
        else if ($(this)[0].id == "TypesOfDataVerification") {
            dataverification["TypesOfDataVerification"] = $(this)[0].value;
            $(this).val("");
        }
        else if ($(this)[0].id == "TimelinesForDataVerification") {
            dateStringArray = $(this)[0].value.split(',');
            dataverification["TimelinesForDataVerification"] = dateStringArray;
            $(this).val("");
            $('#TimelinesForDataVerification').multiDatesPicker('resetDates', 'picked');
        }
        else if ($(this)[0].id == "FrequencyOfDataVerification") {
            dataverification["FrequencyOfDataVerification"] = $(this)[0].value;
            $(this).val("");
        }
        else if ($(this)[0].id == "DurationOfDataVerificaion") {
            dataverification["DurationOfDataVerificaion"] = $(this)[0].value;
            $(this).val("");
        }        
    });

    DataVerificationArray.push(dataverification);
    CreatedataverificationTable(dataverification);
});

function CreatedataverificationTable(dataverification) {
    var dateArray = dataverification.TimelinesForDataVerification;
    var removeId = "remove" + dataverification.Id;
    var timelines = '';
    for (var i = 0; i < dateArray.length; i++) {
        var dt = dateArray[i].toString();
        if (dt.indexOf("Date") !== -1) {
            dt = parseInt(dt.replace(/\/Date\((\d+)\)\//, '$1'));
        }
        var dtt = new Date(dt);
        dt = dtt.getDate() + '-' + months[dtt.getMonth()] + '-' + dtt.getFullYear();
        timelines += dt + " &#013;";
    }

    var html = '<tr id=dataverification' + dataverification.Id + '>';
    html += '<td>' + dataverification.DataVerificationApproach + '</td>';
    html += '<td>' + dataverification.TypesOfDataVerification + '</td>';
    html += '<td>' + dataverification.FrequencyOfDataVerification + '</td>';
    html += '<td>' + dataverification.DurationOfDataVerificaion + '</td>';
    html += '<td><a class="btn btn-info btn-sm btn-outline" title=' + JSON.stringify(timelines) + '>View</a></td>';
    html += '<td><input type="button" value="Remove" class="btn btn-danger btn-sm btn-outline" onclick="DeleteRow(' + dataverification.Id + ',"dataverification")"  id=' + removeId + '/></td>';
    html += '</tr>';
    $('#dataVerificationTable').append(html);
}