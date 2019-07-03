import { Component, OnInit } from "@angular/core";
import moment from "moment/moment";

@Component({
  selector: "detail-stage-popup",
  templateUrl: "./detail-stage-popup.component.html"
})
export class OpsModuleStageManagementDetailComponent implements OnInit {
  constructor() {
    this.keepCalendarOpeningWithRange = true;
    this.selectedDate = Date.now();
    this.selectedRange = {
      startDate: moment().startOf("month"),
      endDate: moment().endOf("month")
    };
  }

  ngOnInit() {}

  /**
   * Daterange picker
   */
  selectedRange: any;
  selectedDate: any;
  keepCalendarOpeningWithRange: true;
  maxDate: moment.Moment = moment();
  ranges: any = {
    Today: [moment(), moment()],
    Yesterday: [moment().subtract(1, "days"), moment().subtract(1, "days")],
    "Last 7 Days": [moment().subtract(6, "days"), moment()],
    "Last 30 Days": [moment().subtract(29, "days"), moment()],
    "This Month": [moment().startOf("month"), moment().endOf("month")],
    "Last Month": [
      moment()
        .subtract(1, "month")
        .startOf("month"),
      moment()
        .subtract(1, "month")
        .endOf("month")
    ]
  };

  /**
   * ng2-select
   */
  statusStage: Array<string> = [
    "In schedule",
    "Processing",
    "Done",
    "Overdue",
    "Pending",
    "Deleted"
  ];
  statusStageActive = ["In schedule"];

  private value: any = {};
  private _disabledV: string = "0";
  public disabled: boolean = false;

  private get disabledV(): string {
    return this._disabledV;
  }

  private set disabledV(value: string) {
    this._disabledV = value;
    this.disabled = this._disabledV === "1";
  }

  public selected(value: any): void {
    console.log("Selected value is: ", value);
  }

  public removed(value: any): void {
    console.log("Removed value is: ", value);
  }

  public typed(value: any): void {
    console.log("New search input: ", value);
  }

  public refreshValue(value: any): void {
    this.value = value;
  }
}
