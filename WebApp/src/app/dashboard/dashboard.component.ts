import { Component, OnInit } from '@angular/core';
import * as moment from 'moment';
import * as Highcharts from 'highcharts';

@Component({
    selector: 'app-dashboard',
    templateUrl: './dashboard.component.html',
    styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
    constructor() {
        this.keepCalendarOpeningWithRange = true;
        this.selectedDate = Date.now();
        this.selectedRange = { startDate: moment().startOf('month'), endDate: moment().endOf('month') };
    }

    ngOnInit() {
    }

    /**
     * Daterange picker
     */
    selectedRange: any;
    selectedDate: any;
    keepCalendarOpeningWithRange: true;
    maxDate: moment.Moment = moment();
    ranges: any = {
        Today: [moment(), moment()],
        Yesterday: [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
        'Last 7 Days': [moment().subtract(6, 'days'), moment()],
        'Last 30 Days': [moment().subtract(29, 'days'), moment()],
        'This Month': [moment().startOf('month'), moment().endOf('month')],
        'Last Month': [
            moment()
                .subtract(1, 'month')
                .startOf('month'),
            moment()
                .subtract(1, 'month')
                .endOf('month')
        ]
    };
    
    // highchart
    // https://www.npmjs.com/package/highcharts-angular
    Highcharts = Highcharts;
    chartOptions1 = {
        chart: {
            marginLeft: 40, // Keep all charts left aligned
            spacingTop: 20,
            spacingBottom: 20,
            className: "chart-sync-revenue"
        },
        title: {
            text: "Revenue",
            align: "left",
            margin: 0,
            x: 30
        },
        credits: {
            enabled: false
        },
        legend: {
            enabled: true,
            verticalAlign: 'top',
            y: 0
        },
        xAxis: {
            // crosshair: true,
            // events: {
            //     setExtremes: function (e) {
            //         var thisChart = this.chart;

            //         if (e.trigger !== "syncExtremes") {
            //             // Prevent feedback loop
            //             Highcharts.each(Highcharts.charts, void function (chart) {
            //                 if (
            //                     chart !== thisChart &&
            //                     chart.options.chart.className ===
            //                     thisChart.options.chart.className
            //                 ) {
            //                     if (chart.xAxis[0].setExtremes) {
            //                         // It is null while updating
            //                         chart.xAxis[0].setExtremes(e.min, e.max, undefined, false, {
            //                             trigger: "syncExtremes"
            //                         });
            //                     }
            //                 }
            //             });
            //         }
            //     }
            // },
            // labels: {
            //     format: "{value}"
            // }
            categories:["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"]
        },
        yAxis: {
            title: {
                text: null
            }
        },
        // tooltip: {
        //     positioner: function () {
        //         return {
        //             // right aligned
        //             x: this.chart.chartWidth - this.label.width,
        //             y: 10 // align to title
        //         };
        //     },
        //     borderWidth: 0,
        //     backgroundColor: "none",
        //     pointFormat: "{point.y}",
        //     headerFormat: "",
        //     shadow: false,
        //     style: {
        //         fontSize: "18px"
        //     }
        // },
        series: [
            {
                color: '#ffb822',
                data: [1, 2, 3, 4, 5, 6, 7, 8, 9],
                name: "USD"
            },
            {
                color: "#c9cdd4",
                data: [1, 3, 5, 7, 8, 9, 12, 14, 17],
                name: "VND"
            }
        ]
    };

    chartOptions2 = {
        chart: {
            marginLeft: 40, // Keep all charts left aligned
            spacingTop: 20,
            spacingBottom: 20,
            className: "chart-sync-volume"
        },
        title: {
            text: "Volume",
            align: "left",
            margin: 0,
            x: 30
        },
        credits: {
            enabled: false
        },
        legend: {
            enabled: false
        },
        xAxis: {
            // crosshair: true,
            // events: {
            //     setExtremes: function (e) {
            //         var thisChart = this.chart;

            //         if (e.trigger !== "syncExtremes") {
            //             // Prevent feedback loop
            //             Highcharts.each(Highcharts.charts, void function (chart) {
            //                 if (
            //                     chart !== thisChart &&
            //                     chart.options.chart.className ===
            //                     thisChart.options.chart.className
            //                 ) {
            //                     if (chart.xAxis[0].setExtremes) {
            //                         // It is null while updating
            //                         chart.xAxis[0].setExtremes(e.min, e.max, undefined, false, {
            //                             trigger: "syncExtremes"
            //                         });
            //                     }
            //                 }
            //             });
            //         }
            //     }
            // },
            // labels: {
            //     format: "{value}"
            // }
            categories:["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"]
        },
        yAxis: {
            title: {
                text: null
            }
        },
        // tooltip: {
        //     positioner: function () {
        //         return {
        //             // right aligned
        //             x: this.chart.chartWidth - this.label.width,
        //             y: 10 // align to title
        //         };
        //     },
        //     borderWidth: 0,
        //     backgroundColor: "none",
        //     pointFormat: "{point.y}",
        //     headerFormat: "",
        //     shadow: false,
        //     style: {
        //         fontSize: "18px"
        //     }
        // },
        series: [
            {
                color: "#3966b6",
                data: [1, 2, 3, 4, 5, 6, 7, 8, 9],
                name: "Shipment"
            },
            {
                color: "#c9cdd4",
                data: [1, 3, 5, 7, 8, 9, 12, 14, 17],
                name: "HBL"
            }
        ]
    };

    chartOptions3 = {
        chart: {
            marginLeft: 40, // Keep all charts left aligned
            spacingTop: 20,
            spacingBottom: 20,
            className: "chart-sync-weight"
        },
        title: {
            text: "Weight",
            align: "left",
            margin: 0,
            x: 30
        },
        credits: {
            enabled: false
        },
        legend: {
            enabled: false
        },
        xAxis: {
            // crosshair: true,
            // events: {
            //     setExtremes: function (e) {
            //         var thisChart = this.chart;

            //         if (e.trigger !== "syncExtremes") {
            //             // Prevent feedback loop
            //             Highcharts.each(Highcharts.charts, void function (chart) {
            //                 if (
            //                     chart !== thisChart &&
            //                     chart.options.chart.className ===
            //                     thisChart.options.chart.className
            //                 ) {
            //                     if (chart.xAxis[0].setExtremes) {
            //                         // It is null while updating
            //                         chart.xAxis[0].setExtremes(e.min, e.max, undefined, false, {
            //                             trigger: "syncExtremes"
            //                         });
            //                     }
            //                 }
            //             });
            //         }
            //     }
            // },
            // labels: {
            //     format: "{value}"
            // }
            categories:["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"]
        },
        yAxis: {
            title: {
                text: null
            }
        },
        // tooltip: {
        //     positioner: function () {
        //         return {
        //             // right aligned
        //             x: this.chart.chartWidth - this.label.width,
        //             y: 10 // align to title
        //         };
        //     },
        //     borderWidth: 0,
        //     backgroundColor: "none",
        //     pointFormat: "{point.y}",
        //     headerFormat: "",
        //     shadow: false,
        //     style: {
        //         fontSize: "18px"
        //     }
        // },
        series: [
            {
                color: "#41bfa3",
                data: [1, 2, 3, 4, 5, 6, 7, 8, 9],
                name: "KG"
            },
            {
                color: "#c9cdd4",
                data: [1, 3, 5, 7, 8, 9, 12, 14, 17],
                name: "CBM"
            }
        ]
    };
    
}
