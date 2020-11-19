import { Component, ViewChild } from '@angular/core';
import { AppList } from "src/app/app.list";
import { ReportPreviewComponent } from "@common";
import { CommonEnum } from "@enums";
import { ICrystalReport, ReportInterface } from "@interfaces";

@Component({
  selector: 'app-commission-incentive-report',
  templateUrl: './commission-incentive-report.component.html'
})
export class CommissionIncentiveReportComponent extends AppList implements ICrystalReport {
  @ViewChild(ReportPreviewComponent, { static: false }) reportPopup: ReportPreviewComponent;
  shipmentInput: OperationInteface.IInputShipment;
  numberOfShipment: number = 0;

  showReport(): void {
    this.reportPopup.frm.nativeElement.submit();
    this.reportPopup.show();
  }

  ngOnInit() {
  }

  onSearchSaleReport(data: ReportInterface.ISaleReportCriteria) {
    console.log(data);
    switch (data.typeReport) {
      case CommonEnum.COMMISSION_INCENTIVE_TYPE.COMMISSION_PR_AS:
        break;
      case CommonEnum.COMMISSION_INCENTIVE_TYPE.COMMISSION_PR_OPS:
        break;
      case CommonEnum.COMMISSION_INCENTIVE_TYPE.INCENTIVE_RPT:
        break;
    }
  }

  previewComPROpsReport(data: ReportInterface.ISaleReportCriteria) {

  }

  onShipmentList(data: any) {
    this.shipmentInput = data;
    if (data) {
        this.numberOfShipment = this.shipmentInput.keyword.split(/\n/).filter(item => item.trim() !== '').map(item => item.trim()).length;
    } else {
        this.numberOfShipment = 0;
    }
}
}
