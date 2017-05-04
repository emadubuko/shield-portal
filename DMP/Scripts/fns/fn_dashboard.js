function loadDashboardData(id, quarter, year) {
    //var period = $("#selected_period").val()
   // var period = getUrlVars()['period'];
    $.ajax({
        url: baseUrl() + "dashboard/GetIpCounts/?reporting_period=" + quarter + "&year="+year+"&partner_id=" + id,
        method: "GET",
        contentType: "application/json",
        success: function (data) {

            //load state summary
            $("#state_summary").empty();
            var state_data = data.Table2;
            for (var i = 0; i < state_data.length; i++) {
               
                var dt = state_data[i];
                var pending = "";
                if (dt.Pending > 0) {
                    pending = dt.Pending + " are yet to submit";
                }
                else if (dt.Pending == 0) {
                    pending = "All facilites in the state have submitted"
                }
                var percentage = 0;
                if (dt.Total > 0) {
                    percentage = ((dt.Submitted / dt.Total) * 100).toFixed(2);
                }

                var value = '<div class="col-lg-4"><div class="panel panel-default"><div class="panel-body"><div class="media"><div class="media-left"><div class="knob-outer"><input type="text" readonly class="knob" data-width="82" data-height="82" data-fgColor="#29B6F6" value="' + percentage + '"></div></div><div class="media-body"><h4 class="media-heading">' + dt.Submitted + ' facilities submitted</h4><p>' + pending + '</p><button class="btn btn-sm btn-success">' + dt.Name + '</button></div></div></div></div></div>';
                $("#state_summary").append(value);
            }

            $(".knob").knob();
              
            if (data.Table1[0] != "") {
                var values = data.Table1[0].Column1.split("|");
                $("#facilities").text(values[0]);
                $("#submitted").text(values[1]);
                $("#lgas").text(values[2]);
                $("#states").text(values[3]);
            }
           
        }
    });
}


//function loadHomeData() {
//    var period = $("#selected_period").val()
//    $.ajax({
//        url: baseUrl() + "dashboard/gethome/" + period ,
//        method: "GET",
//        contentType: "application/json",
//        success: function (data) {

//            //load state summary
//            $("#state_summary").empty();
//            var state_data = data.Table2;
//            for (var i = 0; i < state_data.length; i++) {
               
//                var dt = state_data[i];
//                var pending = "";
//                if (dt.Pending > 0) {
//                    pending = dt.Pending + " are yet to submit";
//                }
//                else if (dt.Pending <= 0) {
//                    pending = "All facilites in the state have submitted"
//                }
//                var percentage = 0;
//                if (dt.Total > 0) {
//                    percentage = ((dt.Submitted / dt.Total) * 100).toFixed(2);;
//                }

//                var value = '<div class="col-lg-4"><div class="panel panel-default"><div class="panel-body"><div class="media"><div class="media-left"><div class="knob-outer"><input type="text" readonly class="knob" data-width="82" data-height="82" data-fgColor="#29B6F6" value="' + percentage + '"></div></div><div class="media-body"><h4 class="media-heading">' + dt.Submitted + ' facilities submitted</h4><p>' + pending + '</p><a href="/dqa/IpHome/' + dt.Id + '" class="btn btn-sm btn-success">' + dt.Name + '</a></div></div></div></div></div>';
//                $("#state_summary").append(value);
//            }

//            $(".knob").knob();

//            var jsonHtmlTable = ConvertJsonToTable(data.Table, 'table', "table table-bordered table-hover dataTables-example", 'Download');
//            $("#divTable").empty();
//            $("#divTable").html(jsonHtmlTable);

//            $('.dataTables-example').DataTable({});


//            if (data.Table1[0] != "") {
//                var values = data.Table1[0].Column1.split("|");
//                $("#facilities").text(values[0]);
//                $("#submitted").text(values[1]);
//                $("#lgas").text(values[2]);
//                $("#states").text(values[3]);
//            }
           
//        }
//    });
//}


function getStateSummary(id) {
    var period = $("#selected_period").val()
    $.ajax({
        url: baseUrl() + "dashboard/GetStateSummary/" + id+"/"+period,
        method: "GET",
        contentType: "application/json",
        success: function (data) {
            $("#state_summary").empty();

            for (var i = 0; i < data.length; i++) {

                var pending = "";
                if (data[i].Pending > 0) {
                    pending = data[i].Pending + " are yet to submit";
                }
                else if (data[i].Pending == 0) {
                    pending="All facilites in the state have submitted"
                }

                var value = '<div class="col-lg-4"><div class="panel panel-default"><div class="panel-body"><div class="media"><div class="media-left"><div class="knob-outer"><input type="text" readonly class="knob" data-width="82" data-height="82" data-fgColor="#29B6F6" value="'+data[i].Percentage+'"></div></div><div class="media-body"><h4 class="media-heading">'+data[i].Submitted+' facilities submitted</h4><p>'+pending+'</p><button class="btn btn-sm btn-success">'+data[i].Name+'</button></div></div></div></div></div>';
                $("#state_summary").append(value);
            }

            $(".knob").knob();
           
        }
    });
}


function getPending(ip) {
    var period = $("#selected_period").val()
    $.ajax({
        url: baseUrl() + "dashboard/GetPendingFacilities/" + ip + "/"+period,
        method: "GET",
        contentType: "application/json",
        success: function (data) {
             
        }
    })
}

function loadSettings() {
    var d = new Date();
    var n = d.getMonth();

    $("#selected_year").val(d.getFullYear());
    
    if ($.inArray(n, [9,10,11])==0) {
        $("#selected_period").val("Q1 (Oct-Dec)");
    }
    else if ($.inArray(n, [0, 1, 2])==0) {
        $("#selected_period").val("Q2 (Jan-Mar)");
    }
    else if ($.inArray(n, [3, 4, 5])==0) {
        $("#selected_period").val("Q3 (Apr-Jun)");
    }
    else if ($.inArray(n, [6, 7, 8])==0) {
        $("#selected_period").val("Q4 (Jul-Sep)");
    }
}

function getUrlVars() {
    var vars = [], hash;
    var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < hashes.length; i++) {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0]] = hash[1];
    }
    return vars;
}

