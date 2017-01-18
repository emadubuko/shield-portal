
if (TrainingArrayFromServer.length > 0) {
    var training = {};
    for (var c = 0; c < TrainingArrayFromServer.length; c++) {
        training = TrainingArrayFromServer[c];
        var tL = training.TimelinesForTrainings;
        var timeLines = [];
        for (var i = 0; i < tL.length; i++) {
            var dt = tL[i].toString();
            if (dt.indexOf("Date") !== -1) {
                dt = parseInt(dt.replace(/\/Date\((\d+)\)\//, '$1'));
                var dtt = new Date(dt);
                timeLines.push(dtt);
            }
        }
        training.TimelinesForTrainings = timeLines;
        trainingArray.push(training);
        CreatetrainingTable(training);
    }
}


$("#addtrainingbtn").click(function (e) {
    var training = {};
    training["Id"] = trainingArray.length + 1;
    var dateStringArray = [];

    $("#trainingForm input, select").each(function () {
        if ($(this)[0].id == "NameOfTraining") {
            training["NameOfTraining"] = $(this)[0].value;
            $(this).val("");
        }
        else if ($(this)[0].id == "TimelinesForTrainings") {
            dateStringArray = $(this)[0].value.split(',');
            training["TimelinesForTrainings"] = dateStringArray;
            $(this).val("");
            $('#TimelinesForTrainings').multiDatesPicker('resetDates', 'picked');
        }
        else if ($(this)[0].id == "FequencyOfTrainings") {
            training["FequencyOfTrainings"] = $(this)[0].value;
            $(this).val("");
        }
        else if ($(this)[0].id == "DurationOfTrainings") {
            training["DurationOfTrainings"] = $(this)[0].value;
            $(this).val("");
        }
    });

    trainingArray.push(training);
    CreatetrainingTable(training);
});

function CreatetrainingTable(training) {
    var dateArray = training.TimelinesForTrainings;
    var removeId = "remove" + training.Id;
    var timelines = '';
    var months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    for (var i = 0; i < dateArray.length; i++) {
        var dt = dateArray[i].toString();
        if (dt.indexOf("Date") !== -1) {
            dt = parseInt(dt.replace(/\/Date\((\d+)\)\//, '$1'));
        }
        var dtt = new Date(dt);
        dt = dtt.getDate() + '-' + months[dtt.getMonth()] + '-' + dtt.getFullYear();
        timelines += dt + " &#013;";
    }

    var html = '<tr id=training' + training.Id + '>';
    html += '<td>' + training.NameOfTraining + '</td>'; 
    html += '<td>' + training.FequencyOfTrainings + '</td>';
    html += '<td>' + training.DurationOfTrainings + '</td>';
    html += '<td><a class="btn btn-info btn-sm btn-outline" title=' + JSON.stringify(timelines) + '>View</a></td>';
    html += '<td><input type="button" value="Remove" class="btn btn-danger btn-sm btn-outline" onclick="DeleteRow(' + training.Id + ',"training")"  id=' + removeId + '/></td>';
    html += '</tr>';
    $('#trainingTable').append(html);
}