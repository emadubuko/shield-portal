function getCounts(id) {
    var period = $("#selected_period").val()
    $.ajax({
        url: baseUrl() + "dashboard/GetIpCounts/" + id + "/"+period,
        method: "GET",
        contentType: "application/json",
        success: function (data) {
            var values = data.split("|");
            $("#facilities").text(values[0]);
            $("#submitted").text(values[1]);
            $("#lgas").text(values[2]);
            $("#states").text(values[3]);
        }
    });
}

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

                var value = '<div class="col-lg-4"><div class="panel panel-default"><div class="panel-body"><div class="media"><div class="media-left"><div class="knob-outer"><input type="text" readonly class="knob" data-width="82" data-height="82" data-fgColor="#29B6F6" value="'+data[i].Percentage+'"></div></div><div class="media-body"><h4 class="media-heading">'+data[i].Submitted+' facilities submitted</h4><p>'+pending+'</p><button class="btn btn-sm btn-primary">'+data[i].Name+'</button></div></div></div></div></div>';
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
            var table_value = "";
            for (var i = 0; i < data.length; i++) {
                table_value += "<tr id='tr_" + data[i].Id + "'><td>" + data[i].Id + "</td>";
                table_value += "<td><a href=''><strong>" + data[i].SiteName + "</strong></a></td><td>" + data[i].Lga + "</td><td>" + data[i].State + "</td>";
                table_value += "<td>" + data[i].FacilityLevel + "</td><td>" + data[i].FacilityType + "</td>";
                //table_value += "<td><button class='btn btn-danger'  onclick='deleteDQA(" + data[i].Id + ")'><i class='fa fa-trash'></i> Delete</button></td>";
                table_value += "</tr>";
            }
            $("#output tbody").prepend(table_value);
            $('#output').DataTable();
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

