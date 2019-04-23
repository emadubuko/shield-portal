function build_clustered_column_chart(container_id, title, xAxisTitle, categories, series_data) {
    Highcharts.chart(container_id, {
        chart: {
            type: 'column'
        },
        title: {
            text: title
        },

        subtitle: {
            text: ''
        },

        xAxis: {
            title: {
                text: xAxisTitle
            },
            categories: categories,
            crosshair: true
        },

        yAxis: {
            min: 0,
            title: {
                text: 'Indicator Values'
            },
            labels: {
                formatter: function () {
                    return this.value;
                }
            }
        },
        tooltip: {
            headerFormat: '<span style="font-size:10px">{point.key}</span><table>',
            pointFormat: '<tr><td style="color:{series.color};padding:0">{series.name}: </td>' +
                '<td style="padding:0"><b>{point.y:.1f} </b></td></tr>',
            footerFormat: '</table>',
            shared: true,
            useHTML: true
        },

        plotOptions: {
            column: {
                pointPadding: 0.2,
                borderWidth: 0,

            }
        },

        lang: {
            decimalPoint: '.',
            thousandsSep: ','
        },

        series: series_data

    });

}