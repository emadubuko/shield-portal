
function percentage(num, per) {
    var result = num * (per / 100);
    return Math.round(result);
}

Highcharts.getOptions().colors = Highcharts.map(Highcharts.getOptions().colors, function (color) {
    return {
        radialGradient: {
            cx: 0.5,
            cy: 0.3,
            r: 0.7
        },
        stops: [
            [0, color],
            [1, Highcharts.Color(color).brighten(-0.3).get('rgb')] // darken
        ]
    };
});

Highcharts.setOptions({
    lang: {
        decimalPoint: '.',
        thousandsSep: ','
    }
});

var pieColors = (function () {
    var colors = [],
        i;

    for (i = 0; i < 50; i += 3) {
        // Start out with a darkened base color (negative brighten), and end
        // up with a much brighter color
        base = Highcharts.getOptions().colors[i],
            colors.push(Highcharts.Color(base).get());
    }
    return colors;
}());


function get_color_shades(start) {
    var colors = [],
        base = Highcharts.getOptions().colors[start],
        i;

    for (i = 0; i < 100; i += 1) {
        colors.push(Highcharts.Color(base).brighten((i) / 100).get());
    }
    return colors;
}

function countUnique(one_dimensional_array_item) {
    return new Set(one_dimensional_array_item).size;
}

function getRandomInt(min, max) {
    return Math.floor(Math.random() * (max - min + 1)) + min;
}


function BuildBubbleChart(id, title, xaxis_title, yaxis_title, bubble_pointFormat, data_array) {

    Highcharts.chart(id, {
        credits: {
            enabled: false
        },
        chart: {
            type: 'bubble',
            plotBorderWidth: 1,
            zoomType: 'xy'
        },

        legend: {
            enabled: true
        },

        title: {
            text: title
        },

        subtitle: {
            text: 'click the bubbles to drill down'
        },

        xAxis: {
            gridLineWidth: 1,
            title: {
                text: xaxis_title
            },
            labels: {
                format: '{value}'
            }
        },

        yAxis: {
            startOnTick: false,
            endOnTick: false,
            title: {
                text: yaxis_title
            },
            labels: {
                format: '{value} %'
            },
            max: 105,
            min: 0,
            maxPadding: 0.2,
        },

        tooltip: {
            useHTML: true,
            headerFormat: '<table>',
            pointFormat: bubble_pointFormat,
            footerFormat: '</table>',
            followPointer: true
        },

        plotOptions: {
            series: {
                dataLabels: {
                    enabled: true,
                    format: '<span style="color: white>{point.name}</span>',

                }
            }
        },
        series: [{
            name: 'State',
            data: data_array.state_data
        }],
        drilldown: {
            series: data_array.lga_drill_down_data
        }

    });

}

function BuildDonut(id, title, data_array) {

    var chart = new Highcharts.Chart({
        credits: {
            enabled: false
        },
        chart: {
            renderTo: id,
            type: 'pie'
        },
        title: {
            text: title,
            style: {
                fontSize: '12px'
            }
        },
        legend: {
            reversed: true
        },
        plotOptions: {
            pie: {
                colors: pieColors,
                shadow: false,
                dataLabels: {
                    format: '{point.y:,.0f}<br /> ({point.percentage:.1f} %)'
                }
            }
        },
        tooltip: {
            pointFormat: '{point.y} ({point.percentage:.1f}%)'
        },
        series: [{
            data: data_array,
            size: '60%',
            innerSize: '50%',
            showInLegend: true,

        }],
        responsive: {
            rules: [{
                condition: {
                    maxWidth: 400
                }
            }]
        }
    });
}

function BuildPieChart(id, title, data_array) {
    var chart = Highcharts.chart(id, {
        chart: {
            plotBackgroundColor: null,
            plotBorderWidth: null,
            plotShadow: false,
            type: 'pie'
        },
        title: {
            text: title,
            style: {
                fontSize: '12px'
            }
        },
        tooltip: {
            pointFormat: '{point.y} ({point.percentage:.1f}%)'
        },
        plotOptions: {
            pie: {
                allowPointSelect: true,
                cursor: 'pointer',
                dataLabels: {
                    format: '{point.y:,.0f} ({point.percentage:.1f} %)'
                },
                showInLegend: true,
            }
        },
        series: [{
            data: data_array,
        }],
        responsive: {
            rules: [{
                condition: {
                    maxWidth: 400
                }
            }]
        },
        colors: ['grey', '#F2784B', '#08bf0f'],//'#1ba39c'
    });
    //chart.series[0].data[0].update({
    //    color: '#00a65a'
    //});
    //chart.series[0].data[1].update({
    //    color: 'sandybrown'
    //});
}

function Build_Pos_Neg_Chart(id, title, categories, series_data, max) {

    Highcharts.chart(id, {
        chart: {
            type: 'bar'
        },
        credits: {
            enabled: false
        },
        title: {
            text: title,
            style: {
                fontSize: '12px'
            }
        },
        xAxis: [{
            categories: categories,
            reversed: true,
            labels: {
                step: 1
            }
        }, { // mirror axis on right side
            opposite: true,
            reversed: true,
            categories: categories,
            linkedTo: 0,
            labels: {
                step: 1
            }
        }],
        yAxis: {
            title: {
                text: null
            },
            labels: {
                formatter: function () {
                    return Math.abs(this.value);
                }
            },
            max: (max + percentage(max, 10)),
            min: -(max + percentage(max, 10))
        },

        plotOptions: {

            series: {
                stacking: 'normal',
            }
        },

        tooltip: {
            formatter: function () {
                return '<b>' + this.series.name + ', age ' + this.point.category + '</b><br/>' +
                    'Total: ' + Highcharts.numberFormat(Math.abs(this.point.y), 0);
            }
        },

        series: series_data

    });
}

function build_drilldown_bar_chart(id, title, yaxistitle, principal_data, drill_down_data, addSigntovalue = "") {

    var colors = get_color_shades(2);
    Highcharts.chart(id, {
        credits: {
            enabled: false
        },
        chart: {
            type: 'column'
        },
        title: {
            text: title,
            style: {
                fontSize: '12px'
            }
        },
        subtitle: {
            text: 'Click the columns to drill down'
        },
        xAxis: {
            type: 'category'
        },
        yAxis: {
            title: {
                text: yaxistitle
            }
        },
        legend: {
            enabled: false
        },
        plotOptions: {
            series: {
                colors: colors,
                borderWidth: 0,
                dataLabels: {
                    enabled: true,
                    format: '{point.y:, 1f} ' + addSigntovalue
                    //format: '{point.y:.1f}%'
                },

            }
        },

        tooltip: {
            headerFormat: '<span style="font-size:11px">{series.name}</span><br>',
            pointFormat: '<span style="color:{point.color}">{point.name}</span>: <b>{point.y}' + addSigntovalue + '</b> '
        },

        series: [{
            name: ' State',
            colorByPoint: true,
            data: principal_data
        }],
        drilldown: {
            series: drill_down_data
        }
    });
}

function build_Column_chart(id, title, yaxistitle, xaxisCategory, series_data, average_value) {
    //var colors = get_color_shades(2);  
    Highcharts.chart(id, {
        chart: {
            type: 'column',
        },
        credits: {
            enabled: false
        },
        title: {
            text: title,
            style: {
                fontSize: '12px'
            }
        },
        xAxis: {
            categories: xaxisCategory,
            crosshair: true
        },
        yAxis: {
            min: 0,
            title: {
                text: yaxistitle,
            },
            plotLines: [{
                color: 'red',
                value: average_value,
                width: '1',
                zIndex: 2
            }]
        },
        legend: {
            enabled: true
        },
        tooltip: {
            headerFormat: '<span style="font-size:10px">{point.key}</span><table>',
            pointFormat: '<tr><td style="color:{series.color};padding:0">{series.name}: </td>' +
                '<td style="padding:0"><b>{point.y:.1f}</b></td></tr>',
            footerFormat: '</table>',
            shared: true,
            useHTML: true
        },
        colors: ['#7cb5ec', '#f7a35c', 'darkturquoise', 'deeppink'],
        plotOptions: {
            column: {
                pointPadding: 0.2,
                borderWidth: 0
            },
            //series: {
            //    borderWidth: 0,
            //    dataLabels: {
            //        enabled: true,
            //        format: '{point.y:, 1f}'
            //    }
            //}
        },
        series: series_data
    });
}

function build_bar_chart_dual_axis(container_id, title, y1_title, y2_title, xaxisCategory, parent_data, parent_data_name, child_data, child_data_name, percent_data, percent_data_name, useLine = true, height) {

    Highcharts.chart(container_id, {
        chart: {
            zoomType: 'xy',
            height: height
        },
        credits: {
            enabled: false
        },
        title: {
            text: title,
            style: {
                fontSize: '12px'
            }
        },
        xAxis: [{
            categories: xaxisCategory,
            crosshair: true
        }],
        yAxis: [
            { // Secondary yAxis
                title: {
                    text: y1_title,
                    rotation: 270,
                },
                labels: {
                    format: '{value:,.0f}',
                },
                max: Math.max.apply(Math, parent_data),
                min: 0
            },
            { // Primary yAxis
                labels: {
                    format: '{value} %',
                },
                title: {
                    text: y2_title,
                },
                opposite: true,
                max: 100,
                min: 0
            }],
        tooltip: {
            pointFormat: '<span style="color:{series.color}">{series.name}</span>: ' +
                '{point.y}<br/>',
            shared: true
        },
        colors: ['steelblue', 'red', 'sandybrown'],
        legend: {
            enabled: true,
        },
        series: [{
            name: parent_data_name,
            type: 'column',
            data: parent_data

        }, {
            name: child_data_name,
            type: 'column',
            data: child_data
        },
        {
            name: percent_data_name,
            type: useLine ? 'spline' : 'scatter',
            data: percent_data,
            yAxis: 1,
            tooltip: {
                useHTML: true,
                pointFormat: '{point.y:.0f} %<br/>',
            },

            //tooltip: {
            //    useHTML: true,
            //    headerFormat: '<table>',
            //    pointFormat: '<h3>{point.name}</h3>' +
            //        '<tr><th>Total Tested:</th><td>{point.y0}</td></tr>' +
            //        '<tr><th>POS:</th><td>{point.x}</td></tr>' +                    
            //        '<tr><th>Percentage Yield:</th><td>{point.y1}%</td></tr>',
            //    footerFormat: '</table>',
            //    followPointer: true
            //}
            //tooltip: {
            //    pointFormat: '<b>{point.y:.1f}%</b>',
            //    //valueSuffix: ' %'
            //}
        }]
    });
}
//

function build_trend_chart(container_id, title, yAxistitle, xaxisCategory, series_data) {
    //var colors = get_color_shades(2); 
    Highcharts.chart(container_id, {

        title: {
            text: title,
            style: {
                fontSize: '12px'
            }
        },

        yAxis: {
            title: {
                text: yAxistitle
            }
        },
        xAxis: {
            categories: xaxisCategory,
        },
        colors: ['#808000', '#46f0f0', '#e6194b', '#3cb44b', '#f58231', '#0082c8', '#911eb4', '#000000', '#f032e6', '#008080', '#aa6e28', '#000080', '#d2f53c'],
        legend: {
            layout: 'vertical',
            align: 'right',
            verticalAlign: 'middle',
            title: {
                text: '<span style="font-size: 12px;text-decoration: underline;"> Cohorts </span>',
                style: {
                    fontStyle: 'italic'
                }
            },
            labelFormatter: function () {
                return '<span style="color:' + this.color + ';">' + this.name + '</span>';
            },
        },
        tooltip: {
            formatter: function () {
                return '<b>Report period:</b> ' + this.point.category + '<br /><b> Cohort </b>:' + this.series.name + '<br /><b> No. of patient:</b>' + this.point.y
            }
        },

        plotOptions: {
            line: {
                marker: {
                    enabled: false
                }
            }
        },

        series: series_data,

        //responsive: {
        //    rules: [{
        //        condition: {
        //            maxWidth: 500
        //        },
        //        chartOptions: {
        //            legend: {
        //                layout: 'horizontal',
        //                align: 'center',
        //                verticalAlign: 'bottom'
        //            }
        //        }
        //    }]
        //}
    });
}

function build_side_by_side_column_chart(container_id, title, yAxistitle, xaxisCategory, series_data) {
    Highcharts.chart(container_id, {
        chart: {
            type: 'column'
        },
        credits: {
            enabled: false
        },
        title: {
            text: title
        },
        xAxis: {
            categories: xaxisCategory,
            crosshair: true
        },
        yAxis: {
            min: 0,
            title: {
                text: yAxistitle
            }
        },
        colors: ['steelblue', 'sandybrown', 'green'],
        tooltip: {
            headerFormat: '<span style="font-size:10px">{point.key}</span><table>',
            pointFormat: '<tr><td style="color:{series.color};padding:0">{series.name}: </td>' +
                '<td style="padding:0"><b>{point.y}</b></td></tr>',
            footerFormat: '</table>',
            shared: true,
            useHTML: true
        },
        plotOptions: {
            column: {
                pointPadding: 0.2,
                borderWidth: 0
            }
        },
        series: series_data
    });
}

function plotMapAdvanced(container_id, main_title, state_chart_container_id, xaxisCategories, statedata, state_detail_title, show_subtitle = true) {

    var mapData = Highcharts.geojson(Highcharts.maps['countries/ng/ng-all']);

    // Initiate the map chart
    var mapChart = Highcharts.mapChart(container_id, {

        title: {
            text: main_title,
            style: {
                fontSize: '12px'
            }
        },
        subtitle: {
            text: show_subtitle ? 'Click states to view details. <br /> <small><em> (Shift + Click on map to compare different states)</em></small>' : '',
        },
        mapNavigation: {
            enabled: true,
            enableMouseWheelZoom: false,
            buttonOptions: {
                verticalAlign: 'bottom'
            }
        },

        colorAxis: {
            min: 0,
            minColor: '#E6E7E8',
            maxColor: '#005645'
        },

        tooltip: {
            footerFormat: '<span style="font-size: 10px">(Click for details)</span>'
        },

        series: [{
            data: statedata,
            mapData: mapData,
            joinBy: ['hc-key', 'code3'],
            name: 'Nigeria',
            allowPointSelect: true,
            cursor: 'pointer',
            dataLabels: {
                enabled: true,
                format: '{point.properties.name}'
            },
            states: {
                select: {
                    color: '#a4edba',
                    borderColor: 'black',
                    dashStyle: 'shortdot'
                }
            }
        }]
    });

    var stateChart;

    // Wrap point.select to get to the total selected points
    Highcharts.wrap(Highcharts.Point.prototype, 'select', function (proceed) {

        proceed.apply(this, Array.prototype.slice.call(arguments, 1));

        var points = mapChart.getSelectedPoints();
        if (points.length) {

            //$('#info .subheader').html('<small><em>Shift + Click on map to compare different states</em></small>');

            if (!stateChart) {
                stateChart = Highcharts.chart(state_chart_container_id, {
                    chart: {
                        height: 400,
                        spacingLeft: 0
                    },
                    credits: {
                        enabled: false
                    },
                    title: {
                        text: state_detail_title,
                        style: {
                            fontSize: '12px'
                        }
                    },
                    subtitle: {
                        text: '<small><em> (Shift + Click on map to compare different states)</em></small>'
                    },
                    legend: {
                        enabled: true,
                    },
                    xAxis: {
                        tickPixelInterval: 50,
                        categories: xaxisCategories,
                        crosshair: true
                    },
                    yAxis: {
                        title: null,
                        opposite: true
                    },
                    tooltip: {
                        split: true
                    },
                    plotOptions: {
                        series: {
                            animation: {
                                duration: 500
                            },
                            marker: {
                                enabled: false
                            },
                            threshold: 0,
                        }
                    }
                });
            }

            $.each(points, function (i) {
                // Update
                if (stateChart.series[i]) {
                    stateChart.series[i].update({
                        name: this.name,
                        data: statedata.find(x => x.code3 === this.code3).monthData,
                        type: points.length > 1 ? 'line' : 'area'
                    }, false);
                } else {
                    stateChart.addSeries({
                        name: this.name,
                        data: statedata.find(x => x.code3 === this.code3).monthData,
                        type: points.length > 1 ? 'line' : 'area'
                    }, false);
                }
            });
            while (stateChart.series.length > points.length) {
                stateChart.series[stateChart.series.length - 1].remove(false);
            }
            stateChart.redraw();
        }
    });

}

function build_stacked_bar_with_percent(container_id, title, xaxis_categories, y1_axis_title, y2_axis_title, smaller_data_set, smaller_data_name, larger_data_set, bigger_data_name, percent_data, useLine = true) {

    Highcharts.chart(container_id, {
        chart: {
            type: 'column'
        },
        credits: {
            enabled: false
        },
        title: {
            text: title
        },
        xAxis: {
            categories: xaxis_categories
        },
        yAxis: [{
            min: 0,
            title: {
                text: y1_axis_title
            },
        },
        {
            labels: {
                format: '{value} %',
            },
            title: {
                text: y2_axis_title,
            },
            opposite: true,
            max: 100,
            min: 0
        }
        ],
        legend: {
            align: 'middle',
            verticalAlign: 'bottom',
            //floating: true,
            y: 10,
            backgroundColor: (Highcharts.theme && Highcharts.theme.background2) || 'white',
            borderColor: '#CCC',
            borderWidth: 1,
            shadow: false
        },
        tooltip: {
            headerFormat: '<b>{point.x}</b><br/>',
            pointFormat: '{series.name}: {point.y}<br/>Total: {point.stackTotal}'
        },
        plotOptions: {
            column: {
                stacking: 'normal',
                dataLabels: {
                    enabled: true,

                }
            }
        },
        colors: ['sandybrown', 'green', 'cornflowerblue'],
        series: [{
            name: smaller_data_name,
            data: smaller_data_set
        }, {
            name: bigger_data_name,
            data: larger_data_set
        },
        {
            name: 'percentage',
            type: useLine ? 'spline' : 'scatter',
            data: percent_data,
            yAxis: 1,
        }]
    });
}

function build_stacked_bar_with_drilldown(container_id, title, subtitle, yaxis_title, xaxis_categories, parent_series_data, child_series_data) {
    Highcharts.chart(container_id, {
        credits: {
            enabled: false
        },
        chart: {
            type: 'column'
        },
        subtitle: {
            text: subtitle,
        },
        title: {
            text: title
        },

        xAxis: {
            type: 'category'
            //categories: xaxis_categories
        },

        yAxis: {
            min: 0,
            title: {
                text: yaxis_title
            }
        },
        tooltip: {
            formatter: function () {
                return '<b>' + this.x + '</b><br/>' +
                    this.series.name + ': ' + this.y + '<br/>' +
                    'Total: ' + this.point.stackTotal;
            }
        },

        plotOptions: {
            series: {
                borderWidth: 0,
                dataLabels: {
                    enabled: true,
                    style: {
                        color: 'white',
                        textShadow: '0 0 2px black, 0 0 2px black'
                    }
                },
                stacking: 'normal'
            }
        },
        colors: ['red', 'green'],

        series: parent_series_data,
        drilldown: {
            activeDataLabelStyle: {
                color: 'white',
                textShadow: '0 0 2px black, 0 0 2px black'
            },
            series: child_series_data
        }
    });
}

function build_side_by_side_bar_chart_with_DrillDown(container_id, title, y1_title, principal_data_array, child_data) {

    Highcharts.chart(container_id, {
        chart: {
            type: 'column'
        },
        title: {
            text: title,
            style: {
                fontSize: '12px'
            }
        },
        subtitle: {
            text: 'Click the bars to drill down',
        },
        legend: {
            enabled: true,
        },
        tooltip: {
            shared: true
        },
        plotOptions: {
            series: {
                borderWidth: 0,
                dataLabels: {
                    enabled: true,
                }
            }
        },
        colors: ['steelblue', 'red', 'sandybrown'],
        xAxis: {
            type: 'category'
        },
        yAxis: [
            {
                title: {
                    text: y1_title,
                },
                labels: {
                    format: '{value:,.0f}',
                },
                //max: Math.max.apply(Math, parent_data),
                min: 0
            }],
        series: principal_data_array,
        drilldown: {
            _animation: {
                duration: 2000
            },
            series: child_data
        }
    });
}


function build_bar_chart_dual_axis_with_drill_down(container_id, title, y1_title, y2_title, parent_data, drill_down_data, xaxisCategory) {
    var main_categories = xaxisCategory;
    var positions = ['Positives', 'Tx_New', 'Linkage (%)'];

    Highcharts.setOptions({
        lang: {
            drillUpText: '<< go back to {series.name}'
        }
    });

    Highcharts.chart(container_id, {
        chart: {
            zoomType: 'xy',
            events: {
                drilldown: function (e) {
                    var chart = this;
                    setChart(e.target.renderTo.id, e.seriesOptions);
                    chart.applyDrilldown();
                },
                drillup: function (e) {
                    console.log(e);
                    //if (chart1.drilldownLevels.length > 0) {
                    //    chart1.drillUp();
                    //}
                }
            }
        },
        credits: {
            enabled: false
        },
        title: {
            text: title,
            style: {
                fontSize: '12px'
            }
        },
        xAxis: [{
            categories: main_categories,
            crosshair: true
        }],
        yAxis: [
            { // Secondary yAxis
                title: {
                    text: y1_title,
                    rotation: 270,
                },
                labels: {
                    format: '{value:,.0f}',
                },
                //max: Math.max.apply(Math, parent_data),
                min: 0
            },
            { // Primary yAxis
                labels: {
                    format: '{value} %',
                },
                title: {
                    text: y2_title,
                },
                opposite: true,
                max: 100,
                min: 0
            }],
        tooltip: {
            shared: true,
            formatter: function () {
                var txt = '<b>' + this.x + '</b><br/>';
                if (this.points) {
                    $.each(this.points, function (i, v) {
                        txt = txt + "<b style='color:" + v.color + "'>" + v.series.name + '</b> : ' + v.y + ' <br/> ';
                    });
                } else {
                    txt = txt + '<b>' + this.point.series.name + '</b> :' + this.y + '<br/>';
                }
                return txt;
            }
        },
        plotOptions: {
            series: {
                borderWidth: 0,
                dataLabels: {
                    enabled: true,
                }
            }
        },
        colors: ['steelblue', 'red', 'sandybrown'],
        legend: {
            enabled: true,
        },
        series: parent_data,
        drilldown: {
            drillUpButton: {
                relativeTo: 'spacingBox',
                position: {
                    y: 0,
                    x: 0
                },
                theme: {
                    fill: 'white',
                    'stroke-width': 1,
                    stroke: 'silver',
                    r: 0,
                    states: {
                        hover: {
                            fill: '#a4edba'
                        },
                        select: {
                            stroke: '#039',
                            fill: '#a4edba'
                        }
                    }
                }

            },
            series: drill_down_data
        }
    });
}

var drilledDownChart;
function setChart(chartId, series_data) {
    var chart = $("#" + chartId).highcharts();

    drilledDownChart = chart;
    //var point = chart.series[0].data[0];
    //chart.addSingleSeriesAsDrilldown(point, series_data);
    //point.doDrilldown();
    //chart.applyDrilldown();

    //var point = chart.series[0].data[0];
    //point.doDrilldown();
    //chart.applyDrilldown();


    chart.series[0].remove();
    chart.xAxis[0].setCategories(series_data.categories, false);
    chart.addSeries(series_data, false);
    chart.redraw();

    if (series_data.name.indexOf('%') != -1) {
        $("#" + chartId).append("<a class='btn btn-sm btn-primary btn-outline drillup' style='cursor: pointer;border-color: gray;'>◁ Drill Up</a>");
            //append("<a href='#'> Drill up </a>");
        //drilledDownChart.applyDrilldown();
    }    
}

$(document).on('click', '.drillup', function () {
    console.log(this);
    if (drilledDownChart.drilldownLevels.length > 0) {
        drilledDownChart.drillUp();
    }
});
