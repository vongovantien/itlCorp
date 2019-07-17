import { Component, OnInit, OnDestroy } from "@angular/core";
import moment from "moment/moment";
import { BaseService } from "src/app/shared/services/base.service";
import { API_MENU } from "src/constants/api-menu.const";
import { OpsTransaction } from "src/app/shared/models/document/OpsTransaction.mode";
import * as shipmentHelper from "src/helper/shipment.helper";
import * as dataHelper from "src/helper/data.helper";
import { PartnerGroupEnum } from "src/app/shared/enums/partnerGroup.enum";
import { NgForm } from "@angular/forms";
import { Router } from "@angular/router";
import { PlaceTypeEnum } from "src/app/shared/enums/placeType-enum";
import { JobRepo } from "src/app/shared/repositories";
import { PopupBase } from "src/app/popup.base";
import { takeUntil, catchError, finalize } from "rxjs/operators";
import { NgxSpinnerService } from "ngx-spinner";
import { ToastrService } from "ngx-toastr";
import { Subscription } from "rxjs";
@Component({
  selector: "app-ops-module-billing-job-create",
  templateUrl: "./ops-module-billing-job-create.component.html"
})
export class OpsModuleBillingJobCreateComponent extends PopupBase implements OnInit, OnDestroy {
  DataStorage: Object = null;
  productServices: any[] = [];
  serviceModes: any[] = [];
  shipmentModes: any[] = [];
  listCustomers: any[] = [];
  listSuppliers: any[] = [];
  listAgents: any[] = [];
  listPort: any[] = [];
  listBillingOps: any[] = [];
  OpsTransactionToAdd: OpsTransaction = new OpsTransaction();
  constructor(
    private baseServices: BaseService,
    private api_menu: API_MENU,
    private router: Router,
    private jobRepo: JobRepo,
    private spinner: NgxSpinnerService,
    private _toaster: ToastrService
  ) {
    super();
    this.keepCalendarOpeningWithRange = true;
    this.selectedRange = {
      startDate: moment().startOf("month"),
      endDate: moment().endOf("month")
    };

    this.baseServices.dataStorage.subscribe(data => {
      this.DataStorage = data;
    });
  }

  ngOnInit() {
    this.getShipmentCommonData();
    this.getListCustomers();
    this.getListPorts();
    this.getListSupplier();
    this.getListAgent();
    this.getListBillingOps();
  }

  ngOnDestroy(): void {
    // this.baseServices.dataStorage.unsubscribe();
  }
  async getShipmentCommonData() {
    const data = await shipmentHelper.getOPSShipmentCommonData(
      this.baseServices,
      this.api_menu
    );
    this.productServices = dataHelper.prepareNg2SelectData(
      data.productServices,
      "value",
      "displayName"
    );
    this.serviceModes = dataHelper.prepareNg2SelectData(
      data.serviceModes,
      "value",
      "displayName"
    );
    this.shipmentModes = dataHelper.prepareNg2SelectData(
      data.shipmentModes,
      "value",
      "displayName"
    );
  }

  private getListCustomers() {
    this.baseServices
      .post(this.api_menu.Catalogue.PartnerData.query, {
        partnerGroup: PartnerGroupEnum.CUSTOMER,
        inactive: false,
        all: null
      })
      .subscribe((res: any) => {
        this.listCustomers = res;
      });
  }

  private getListPorts() {
    this.baseServices
      .post(this.api_menu.Catalogue.CatPlace.query, {
        placeType: PlaceTypeEnum.Port,
        inactive: false
      })
      .subscribe((res: any) => {
        this.listPort = res;
        console.log(this.listPort);
      });
  }

  private getListSupplier() {
    this.baseServices
      .post(this.api_menu.Catalogue.PartnerData.query, {
        partnerGroup: PartnerGroupEnum.CARRIER,
        inactive: false,
        all: null
      })
      .subscribe((res: any) => {
        this.listSuppliers = res;
        console.log({ Supplier: this.listSuppliers });
      });
  }
  private getListAgent() {
    this.baseServices
      .post(this.api_menu.Catalogue.PartnerData.query, {
        partnerGroup: PartnerGroupEnum.AGENT,
        inactive: false,
        all: null
      })
      .subscribe((res: any) => {
        this.listAgents = res;
        console.log({ Agents: this.listAgents });
      });
  }

  private getListBillingOps() {
    this.baseServices
      .get(this.api_menu.System.User_Management.getAll)
      .subscribe((res: any) => {
        this.listBillingOps = res;
        console.log({ "Billing Ops": this.listBillingOps });
      });
  }

  public submitNewOps(form: NgForm) {
    console.log(this.OpsTransactionToAdd);
    setTimeout(async () => {
      if (form.submitted) {
        const error = $("#add-new-ops-job-form").find("div.has-danger");
        if (error.length === 0) {
          this.OpsTransactionToAdd.serviceDate = this.selectedDate.startDate != null ? dataHelper.dateTimeToUTC(this.selectedDate.startDate) : null;
          this.jobRepo.addJob(this.OpsTransactionToAdd).pipe(
            takeUntil(this.ngUnsubscribe),
            catchError(this.catchError),
            finalize(() => { this.spinner.hide(); })
          ).subscribe(
            (res: any) => {
              if (!res.status) {
                this._toaster.error(res.message, '', { positionClass: 'toast-bottom-right' });
              } else {
                this.OpsTransactionToAdd = new OpsTransaction();
                this.resetDisplay();
                form.onReset();
                this._toaster.success(res.message, '', { positionClass: 'toast-bottom-right' });
                this.router.navigate([
                  "/home/operation/job-edit/", res.data
                ]);
              }
            }
          );
          // this._jobRepo.getDetailStageOfJob(id).pipe(
          //   takeUntil(this.ngUnsubscribe),
          //   catchError(this.catchError),
          //   finalize(() => { this._spinner.hide() }),
          // ).subscribe(
          //   (res: any[]) => {
          //     if (res instanceof Error) {

          //     } else {
          //       this.selectedStage = new Stage(res);
          //       this.openPopupDetail();
          //     }
          //   },
          //   // error
          //   (errs: any) => {
          //     // this.handleErrors(errs)
          //   },
          //   // complete
          //   () => { }
          // )
          // if (res.status) {//job-edit/:id
          //   console.log(res);
          //   this.router.navigate([
          //     "/home/operation/job-edit/", res.data
          //   ]);
          //   this.OpsTransactionToAdd = new OpsTransaction();
          //   this.resetDisplay();
          //   form.onReset();
          // }
        }
      }
    }, 300);
  }
  isDisplay: boolean = true;
  resetDisplay() {
    this.isDisplay = false;
    setTimeout(() => {
      this.isDisplay = true;
    }, 300);
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
  public items: Array<string> = [
    "option 1",
    "option 2",
    "option 3",
    "option 4",
    "option 5",
    "option 6",
    "option 7"
  ];

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
